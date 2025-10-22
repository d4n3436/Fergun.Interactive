using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fergun.Interactive.Extensions;
using Fergun.Interactive.Pagination;
using Fergun.Interactive.Selection;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace Fergun.Interactive;

/// <summary>
/// Represents a service containing methods for interactivity purposes.
/// </summary>
[PublicAPI]
public class InteractiveService
{
    private readonly ShardedGatewayClient _client;
    private readonly ILogger<InteractiveService> _logger;
    private readonly ConcurrentDictionary<ulong, IInteractiveCallback> _callbacks = new();
    private readonly ConcurrentDictionary<Guid, IFilteredCallback> _filteredCallbacks = new();
    private readonly InteractiveConfig _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractiveService"/> class using the default configuration.
    /// </summary>
    /// <param name="client">An instance of <see cref="BaseSocketClient"/>.</param>
    /// <param name="logger"></param>
    public InteractiveService(ShardedGatewayClient client, ILogger<InteractiveService> logger)
        : this(client, logger, new InteractiveConfig())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractiveService"/> class using a specified configuration.
    /// </summary>
    /// <param name="client">An instance of <see cref="BaseSocketClient"/>.</param>
    /// <param name="config">The configuration.</param>
    public InteractiveService(ShardedGatewayClient client, ILogger<InteractiveService> logger, InteractiveConfig config)
    {
        InteractiveGuards.NotNull(client);
        InteractiveGuards.NotNull(config);

        _client = client;
        _logger = logger;
        _config = config;

        _client.MessageCreate += MessageCreated;
        _client.MessageReactionAdd += ReactionAdded;
    }

    /// <summary>
    /// Gets a dictionary of active callbacks.
    /// </summary>
    public IDictionary<ulong, IInteractiveCallback> Callbacks => _callbacks;

    /// <summary>
    /// Attempts to remove and return a callback.
    /// </summary>
    /// <param name="id">The ID of the callback.</param>
    /// <param name="callback">The callback, if found.</param>
    /// <returns><see langword="true"/> if the callback was removed; otherwise, <see langword="false"/>.</returns>
    public bool TryRemoveCallback(ulong id, [MaybeNullWhen(false)] out IInteractiveCallback callback)
        => _callbacks.TryRemove(id, out callback);

    /// <summary>
    /// Gets the next incoming message that passes the <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A filter which the message has to pass.</param>
    /// <param name="action">
    /// An action which gets executed to incoming messages,
    /// where <see cref="Message"/> is the incoming message and <see cref="bool"/>
    /// is whether the message passed the <paramref name="filter"/>.
    /// </param>
    /// <param name="timeout">The time to wait before the methods returns a timeout result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the request.</param>
    /// <returns>
    /// A task that represents the asynchronous wait operation for the next message.
    /// The task result contains an <see cref="InteractiveResult{T}"/> with the
    /// message (if successful), the elapsed time and the status.
    /// </returns>
    public async Task<InteractiveResult<Message?>> NextMessageAsync(Func<Message, bool>? filter = null,
        Func<Message, bool, Task>? action = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        => await NextEntityAsync(filter, action, timeout, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Gets the next incoming reaction that passes the <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A filter which the reaction has to pass.</param>
    /// <param name="action">
    /// An action which gets executed to incoming reactions, where <see cref="MessageReactionAddEventArgs"/>
    /// is the incoming reaction and <see cref="bool"/> is whether the interaction passed the <paramref name="filter"/>.
    /// </param>
    /// <param name="timeout">The time to wait before the methods returns a timeout result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the request.</param>
    /// <returns>
    /// A task that represents the asynchronous wait operation for the next reaction.
    /// The task result contains an <see cref="InteractiveResult{T}"/> with the
    /// reaction (if successful), the elapsed time and the status.
    /// </returns>
    public async Task<InteractiveResult<MessageReactionAddEventArgs?>> NextReactionAsync(Func<MessageReactionAddEventArgs, bool>? filter = null,
        Func<MessageReactionAddEventArgs, bool, Task>? action = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        => await NextEntityAsync(filter, action, timeout, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Gets the next interaction that passes the <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A filter which the interaction has to pass.</param>
    /// <param name="action">
    /// An action which gets executed to incoming interactions,
    /// where <see cref="Interaction"/> is the incoming interaction and <see cref="bool"/>
    /// is whether the interaction passed the <paramref name="filter"/>.
    /// </param>
    /// <param name="timeout">The time to wait before the methods returns a timeout result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the request.</param>
    /// <returns>
    /// A task that represents the asynchronous wait operation for the next interaction.
    /// The task result contains an <see cref="InteractiveResult{T}"/> with the
    /// interaction (if successful), the elapsed time and the status.
    /// </returns>
    public async Task<InteractiveResult<Interaction?>> NextInteractionAsync(Func<Interaction, bool>? filter = null,
        Func<Interaction, bool, Task>? action = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        => await NextEntityAsync(filter, action, timeout, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Gets the next component interaction that passes the <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A filter which the component interaction has to pass.</param>
    /// <param name="action">
    /// An action which gets executed to incoming interactions,
    /// where <see cref="MessageComponentInteraction"/> is the incoming component interaction and <see cref="bool"/>
    /// is whether the component interaction passed the <paramref name="filter"/>.
    /// </param>
    /// <param name="timeout">The time to wait before the methods returns a timeout result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the request.</param>
    /// <returns>
    /// A task that represents the asynchronous wait operation for the next component interaction.
    /// The task result contains an <see cref="InteractiveResult{T}"/> with the
    /// component interaction (if successful), the elapsed time and the status.
    /// </returns>
    public async Task<InteractiveResult<MessageComponentInteraction?>> NextComponentInteractionAsync(Func<MessageComponentInteraction, bool>? filter = null,
        Func<MessageComponentInteraction, bool, Task>? action = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        => await NextEntityAsync(filter, action, timeout, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Gets the next slash command interaction that passes the <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A filter which the slash command interaction has to pass.</param>
    /// <param name="action">
    /// An action which gets executed to incoming interactions,
    /// where <see cref="SlashCommandInteraction"/> is the incoming slash command interaction and <see cref="bool"/>
    /// is whether the slash command interaction passed the <paramref name="filter"/>.
    /// </param>
    /// <param name="timeout">The time to wait before the methods returns a timeout result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the request.</param>
    /// <returns>
    /// A task that represents the asynchronous wait operation for the next slash command interaction.
    /// The task result contains an <see cref="InteractiveResult{T}"/> with the
    /// slash command interaction (if successful), the elapsed time and the status.
    /// </returns>
    public async Task<InteractiveResult<SlashCommandInteraction?>> NextSlashCommandAsync(Func<SlashCommandInteraction, bool>? filter = null,
        Func<SlashCommandInteraction, bool, Task>? action = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        => await NextEntityAsync(filter, action, timeout, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Gets the next user command interaction that passes the <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A filter which the user command interaction has to pass.</param>
    /// <param name="action">
    /// An action which gets executed to incoming interactions,
    /// where <see cref="UserCommandInteraction"/> is the incoming user command interaction and <see cref="bool"/>
    /// is whether the user command interaction passed the <paramref name="filter"/>.
    /// </param>
    /// <param name="timeout">The time to wait before the methods returns a timeout result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the request.</param>
    /// <returns>
    /// A task that represents the asynchronous wait operation for the next user command interaction.
    /// The task result contains an <see cref="InteractiveResult{T}"/> with the
    /// user command interaction (if successful), the elapsed time and the status.
    /// </returns>
    public async Task<InteractiveResult<UserCommandInteraction?>> NextUserCommandAsync(Func<UserCommandInteraction, bool>? filter = null,
        Func<UserCommandInteraction, bool, Task>? action = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        => await NextEntityAsync(filter, action, timeout, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Gets the next message command interaction that passes the <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A filter which the message command interaction has to pass.</param>
    /// <param name="action">
    /// An action which gets executed to incoming interactions,
    /// where <see cref="MessageCommandInteraction"/> is the incoming message command interaction and <see cref="bool"/>
    /// is whether the message command interaction passed the <paramref name="filter"/>.
    /// </param>
    /// <param name="timeout">The time to wait before the methods returns a timeout result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the request.</param>
    /// <returns>
    /// A task that represents the asynchronous wait operation for the next message command interaction.
    /// The task result contains an <see cref="InteractiveResult{T}"/> with the
    /// message command interaction (if successful), the elapsed time and the status.
    /// </returns>
    public async Task<InteractiveResult<MessageCommandInteraction?>> NextMessageCommandAsync(Func<MessageCommandInteraction, bool>? filter = null,
        Func<MessageCommandInteraction, bool, Task>? action = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        => await NextEntityAsync(filter, action, timeout, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Gets the next autocomplete interaction that passes the <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A filter which the autocomplete interaction has to pass.</param>
    /// <param name="action">
    /// An action which gets executed to incoming interactions,
    /// where <see cref="AutocompleteInteraction"/> is the incoming autocomplete interaction and <see cref="bool"/>
    /// is whether the autocomplete interaction passed the <paramref name="filter"/>.
    /// </param>
    /// <param name="timeout">The time to wait before the methods returns a timeout result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the request.</param>
    /// <returns>
    /// A task that represents the asynchronous wait operation for the next autocomplete interaction.
    /// The task result contains an <see cref="InteractiveResult{T}"/> with the
    /// autocomplete interaction (if successful), the elapsed time and the status.
    /// </returns>
    public async Task<InteractiveResult<AutocompleteInteraction?>> NextAutocompleteInteractionAsync(Func<AutocompleteInteraction, bool>? filter = null,
        Func<AutocompleteInteraction, bool, Task>? action = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        => await NextEntityAsync(filter, action, timeout, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Sends a paginator with pages which the user can change through via reactions or buttons.
    /// </summary>
    /// <param name="paginator">The paginator to send.</param>
    /// <param name="channel">The channel to send the <see cref="Paginator"/> to.</param>
    /// <param name="timeout">The time until the <see cref="Paginator"/> times out.</param>
    /// <param name="messageAction">A method that gets executed once when a message containing the paginator is sent or modified.</param>
    /// <param name="resetTimeoutOnInput">Whether to reset the internal timeout timer when a valid input is received.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the paginator.</param>
    /// <returns>
    /// A task that represents the asynchronous operation for sending the paginator and waiting for a timeout or cancellation.<br/>
    /// The task result contains an <see cref="InteractiveMessageResult"/> with the message used for pagination
    /// (which may not be valid if the message has been deleted), the elapsed time and the status.<br/>
    /// If the paginator only contains one page, the task will return when the message has been sent and the result
    /// will contain the message sent and a <see cref="InteractiveStatus.Success"/> status.
    /// </returns>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="NotSupportedException"/>
    public async Task<InteractiveMessageResult> SendPaginatorAsync(Paginator paginator, TextChannel channel, TimeSpan? timeout = null,
        Action<Message>? messageAction = null, bool resetTimeoutOnInput = false, CancellationToken cancellationToken = default)
        => await SendPaginatorInternalAsync(paginator, channel, timeout, message: null, messageAction, resetTimeoutOnInput, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Modifies a message to a paginator with pages which the user can change through via reactions or buttons.
    /// </summary>
    /// <param name="paginator">The paginator to send.</param>
    /// <param name="message">An existing message to modify to display the <see cref="Paginator"/>.</param>
    /// <param name="timeout">The time until the <see cref="Paginator"/> times out.</param>
    /// <param name="messageAction">A method that gets executed once when a message containing the paginator is modified.</param>
    /// <param name="resetTimeoutOnInput">Whether to reset the internal timeout timer when a valid input is received.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the paginator.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that represents the asynchronous operation for modifying the message to a paginator and waiting for a timeout or cancellation.<br/>
    /// The task result contains an <see cref="InteractiveMessageResult"/> with the message used for pagination
    /// (which may not be valid if the message has been deleted), the elapsed time and the status.<br/>
    /// If the paginator only contains one page, the task will return when the message has been sent and the result
    /// will contain the message sent and a <see cref="InteractiveStatus.Success"/> status.
    /// </returns>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="NotSupportedException"/>
    public async Task<InteractiveMessageResult> SendPaginatorAsync(Paginator paginator, Message message, TimeSpan? timeout = null,
        Action<Message>? messageAction = null, bool resetTimeoutOnInput = false, CancellationToken cancellationToken = default)
        => await SendPaginatorInternalAsync(paginator, channel: null, timeout, message, messageAction, resetTimeoutOnInput, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Responds to an interaction with a paginator.
    /// </summary>
    /// <param name="paginator">The paginator to send.</param>
    /// <param name="interaction">The interaction to respond to.</param>
    /// <param name="timeout">The amount of time until the <see cref="Paginator"/> times out.</param>
    /// <param name="responseType">The response type. When using the "Deferred" response types, you must pass an interaction that has already been deferred.</param>
    /// <param name="ephemeral">
    /// Whether the response message should be ephemeral. Ignored if modifying a non-ephemeral message.<br/><br/>
    /// Ephemeral paginators have the following limitations:<br/>
    /// - <see cref="InputType.Reactions"/> won't work (they can't have reactions).
    /// </param>
    /// <param name="messageAction">A method that gets executed once when a message containing the paginator is sent or modified.</param>
    /// <param name="resetTimeoutOnInput">Whether to reset the internal timeout timer when a valid input is received.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the paginator.</param>
    /// <returns>
    /// A task that represents the asynchronous operation for sending the paginator and waiting for a timeout or cancellation.<br/>
    /// The task result contains an <see cref="InteractiveMessageResult"/> with the message used for pagination
    /// (which may not be valid if the message has been deleted), the elapsed time and the status.<br/>
    /// If the paginator only contains one page, the task will return when the message has been sent and the result
    /// will contain the message sent and a <see cref="InteractiveStatus.Success"/> status.
    /// </returns>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="NotSupportedException"/>
    public async Task<InteractiveMessageResult> SendPaginatorAsync(Paginator paginator, Interaction interaction, TimeSpan? timeout = null,
        InteractionCallbackType responseType = InteractionCallbackType.Message, bool ephemeral = false,
        Action<RestMessage>? messageAction = null, bool resetTimeoutOnInput = false, CancellationToken cancellationToken = default)
    {
        InteractiveGuards.NotNull(paginator);
        InteractiveGuards.NotNull(interaction);
        InteractiveGuards.ValidActionOnStop(paginator.ActionOnTimeout);
        InteractiveGuards.ValidActionOnStop(paginator.ActionOnCancellation);
        InteractiveGuards.SupportedInputType(paginator, ephemeral);
        InteractiveGuards.ValidResponseType(responseType);
        cancellationToken.ThrowIfCancellationRequested();

        var message = await SendOrModifyMessageAsync(paginator, interaction, responseType, ephemeral).ConfigureAwait(false);
        messageAction?.Invoke(message);
        

        if (!_config.ProcessSinglePagePaginators && paginator.MaxPageIndex == 0)
        {
            return new InteractiveMessageResultBuilder()
                .WithMessage(message)
                .Build();
        }

        timeout ??= _config.DefaultTimeout;

        if (!_config.ReturnAfterSendingPaginator)
        {
            return await WaitForPaginatorResultUsingCallbackAsync().ConfigureAwait(false);
        }

        _ = WaitForPaginatorResultUsingCallbackAsync().ConfigureAwait(false);

        return new InteractiveMessageResultBuilder()
            .WithMessage(message)
            .Build();

        async Task<InteractiveMessageResult> WaitForPaginatorResultUsingCallbackAsync()
        {
            var timeoutTaskSource = new TimeoutTaskCompletionSource<InteractiveStatus>(timeout.Value,
                resetTimeoutOnInput, InteractiveStatus.Timeout, InteractiveStatus.Canceled, cancellationToken);

            using var callback = new PaginatorCallback(paginator, message, timeoutTaskSource, DateTimeOffset.UtcNow, interaction);
            return await WaitForPaginatorResultAsync(callback).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Responds to an interaction with a component paginator.
    /// </summary>
    /// <remarks>Component paginators are a new type of paginators that offer more flexibility and support components V2.</remarks>
    /// <param name="paginator">The paginator to send.</param>
    /// <param name="interaction">The interaction to respond to.</param>
    /// <param name="timeout">The amount of time until the paginator times out.</param>
    /// <param name="responseType">
    /// The response type to use. This is used to determine how the interaction should be responded. Here's a list explaining the available response types:
    /// <list type="bullet">
    ///     <item>
    ///         <term><see cref="InteractionCallbackType.Message"/></term>
    ///         <description>Sends a new message using <c>Interaction.RespondWithFilesAsync</c> (requires a non-deferred interaction).</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="InteractionCallbackType.DeferredMessage"/></term>
    ///         <description>Sends a new message using <c>Interaction.FollowupWithFilesAsync</c> (requires a deferred interaction).</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="InteractionCallbackType.ModifyMessage"/></term>
    ///         <description>Updates the message where the interaction comes from using <c>MessageComponentInteraction.UpdateAsync</c> (requires a non-deferred <see cref="MessageComponentInteraction"/>).</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="InteractionCallbackType.DeferredModifyMessage"/></term>
    ///         <description>Updates the message where the interaction comes from using <c>MessageComponentInteraction.ModifyOriginalResponseAsync</c> (requires a deferred interaction).</description>
    ///     </item>
    /// </list>
    /// </param>
    /// <param name="ephemeral">Whether the response message should be ephemeral. Ignored if responding to a non-ephemeral interaction.</param>
    /// <param name="messageAction">The action to perform once when the message containing the paginator is sent or modified.</param>
    /// <param name="resetTimeoutOnInput">Whether to reset the internal timeout timer when a valid input is received.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the paginator.</param>
    /// <returns>
    /// <para>
    ///     A <see cref="Task{TResult}"/> that represents the asynchronous operation of sending the paginator and waiting for a timeout or cancellation.<br/>
    ///     The result contains an <see cref="InteractiveMessageResult"/> with the message used for pagination
    ///     (which may not be valid if the message has been deleted), the elapsed time and the status.
    /// </para>
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when a required argument is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is already cancelled.</exception>
    public async Task<InteractiveMessageResult> SendPaginatorAsync(IComponentPaginator paginator, Interaction interaction, TimeSpan? timeout = null,
        InteractionCallbackType responseType = InteractionCallbackType.Message, bool ephemeral = false,
        Func<Message, Task>? messageAction = null, bool resetTimeoutOnInput = false, CancellationToken cancellationToken = default)
    {
        InteractiveGuards.NotNull(paginator);
        InteractiveGuards.NotNull(interaction);
        cancellationToken.ThrowIfCancellationRequested();

        var message = await paginator.RenderPageAsync(interaction, responseType, ephemeral).ConfigureAwait(false);
        return await SendPaginatorInternalAsync(paginator, message, timeout, messageAction, resetTimeoutOnInput, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a component paginator to the given message channel.
    /// </summary>
    /// <remarks>Component paginators are a new type of paginators that offer more flexibility and support components V2.</remarks>
    /// <param name="paginator">The paginator to send.</param>
    /// <param name="channel">The channel where the paginator will be sent.</param>
    /// <param name="timeout">The amount of time until the paginator times out.</param>
    /// <param name="messageAction">The action to perform once when the message containing the paginator is sent or modified.</param>
    /// <param name="resetTimeoutOnInput">Whether to reset the internal timeout timer when a valid input is received.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the paginator.</param>
    /// <returns>
    /// <para>
    ///     A <see cref="Task{TResult}"/> that represents the asynchronous operation of sending the paginator and waiting for a timeout or cancellation.<br/>
    ///     The result contains an <see cref="InteractiveMessageResult"/> with the message used for pagination
    ///     (which may not be valid if the message has been deleted), the elapsed time and the status.
    /// </para>
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when a required argument is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is already cancelled.</exception>
    public async Task<InteractiveMessageResult> SendPaginatorAsync(IComponentPaginator paginator, TextChannel channel, TimeSpan? timeout = null,
        Func<Message, Task>? messageAction = null, bool resetTimeoutOnInput = false, CancellationToken cancellationToken = default)
    {
        InteractiveGuards.NotNull(paginator);
        InteractiveGuards.NotNull(channel);
        cancellationToken.ThrowIfCancellationRequested();

        var message = await paginator.RenderPageAsync(channel).ConfigureAwait(false);
        return await SendPaginatorInternalAsync(paginator, message, timeout, messageAction, resetTimeoutOnInput, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Modifies a message to display a component paginator.
    /// </summary>
    /// <remarks>Component paginators are a new type of paginators that offer more flexibility and support components V2.</remarks>
    /// <param name="paginator">The paginator to send.</param>
    /// <param name="message">The channel where the paginator will be sent.</param>
    /// <param name="timeout">The amount of time until the paginator times out.</param>
    /// <param name="messageAction">The action to perform once when the message containing the paginator is sent or modified.</param>
    /// <param name="resetTimeoutOnInput">Whether to reset the internal timeout timer when a valid input is received.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the paginator.</param>
    /// <returns>
    /// <para>
    ///     A <see cref="Task{TResult}"/> that represents the asynchronous operation of sending the paginator and waiting for a timeout or cancellation.<br/>
    ///     The result contains an <see cref="InteractiveMessageResult"/> with the message used for pagination
    ///     (which may not be valid if the message has been deleted), the elapsed time and the status.
    /// </para>
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is not owned by the current user.</exception>
    /// <exception cref="ArgumentNullException">Thrown when a required argument is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is already cancelled.</exception>
    public async Task<InteractiveMessageResult> SendPaginatorAsync(IComponentPaginator paginator, Message message, TimeSpan? timeout = null,
        Func<Message, Task>? messageAction = null, bool resetTimeoutOnInput = false, CancellationToken cancellationToken = default)
    {
        InteractiveGuards.NotNull(paginator);
        InteractiveGuards.NotNull(message);
        InteractiveGuards.MessageFromCurrentUser(_client, message);
        cancellationToken.ThrowIfCancellationRequested();

        await paginator.RenderPageAsync(message).ConfigureAwait(false);
        return await SendPaginatorInternalAsync(paginator, message, timeout, messageAction, resetTimeoutOnInput, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a selection to the given message channel.
    /// </summary>
    /// <typeparam name="TOption">The type of the options the selection contains.</typeparam>
    /// <param name="selection">The selection to send.</param>
    /// <param name="channel">The channel to send the selection to.</param>
    /// <param name="timeout">The time until the selection times out.</param>
    /// <param name="messageAction">A method that gets executed once when a message containing the selection is sent or modified.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the selection.</param>
    /// <returns>
    /// A task that represents the asynchronous operation for sending the selection and waiting for a valid input, a timeout or a cancellation.<br/>
    /// The task result contains an <see cref="InteractiveMessageResult{T}"/> with the selected values (if valid), the message used for the selection
    /// (which may not be valid if the message has been deleted), the elapsed time and the status.
    /// </returns>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="NotSupportedException"/>
    public async Task<InteractiveMessageResult<TOption>> SendSelectionAsync<TOption>(BaseSelection<TOption> selection, TextChannel channel,
        TimeSpan? timeout = null, Action<Message>? messageAction = null, CancellationToken cancellationToken = default)
        => await SendSelectionInternalAsync(selection, channel, timeout, message: null, messageAction, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Modifies a message to a selection.
    /// </summary>
    /// <typeparam name="TOption">The type of the options the selection contains.</typeparam>
    /// <param name="selection">The selection to send.</param>
    /// <param name="message">An existing message to modify to display the <see cref="BaseSelection{TOption}"/>.</param>
    /// <param name="timeout">The time until the selection times out.</param>
    /// <param name="messageAction">A method that gets executed once when a message containing the selection is sent or modified.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the selection.</param>
    /// <returns>
    /// A task that represents the asynchronous operation for modifying the message to a selection and waiting for a valid input, a timeout or a cancellation.<br/>
    /// The task result contains an <see cref="InteractiveMessageResult{T}"/> with the selected values (if valid), the message used for the selection
    /// (which may not be valid if the message has been deleted), the elapsed time and the status.
    /// </returns>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="NotSupportedException"/>
    public async Task<InteractiveMessageResult<TOption>> SendSelectionAsync<TOption>(BaseSelection<TOption> selection, Message message,
        TimeSpan? timeout = null, Action<Message>? messageAction = null, CancellationToken cancellationToken = default)
        => await SendSelectionInternalAsync(selection, channel: null, timeout, message, messageAction, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Responds to an interaction with a selection.
    /// </summary>
    /// <typeparam name="TOption">The type of the options the selection contains.</typeparam>
    /// <param name="selection">The selection to send.</param>
    /// <param name="interaction">The interaction to respond to.</param>
    /// <param name="timeout">The time until the selection times out.</param>
    /// <param name="responseType">The response type. When using the "Deferred" response types, you must pass an interaction that has already been deferred.</param>
    /// <param name="ephemeral">
    /// Whether the response message should be ephemeral. Ignored if modifying a non-ephemeral message.<br/><br/>
    /// Ephemeral selections have the following limitations:<br/>
    /// - <see cref="InputType.Reactions"/> won't work (they can't have reactions).
    /// </param>
    /// <param name="messageAction">A method that gets executed once when a message containing the selection is sent or modified.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the selection.</param>
    /// <returns>
    /// A task that represents the asynchronous operation for sending the selection and waiting for a valid input, a timeout or a cancellation.<br/>
    /// The task result contains an <see cref="InteractiveMessageResult{T}"/> with the selected values (if valid), the message used for the selection
    /// (which may not be valid if the message has been deleted), the elapsed time and the status.
    /// </returns>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="NotSupportedException"/>
    public async Task<InteractiveMessageResult<TOption>> SendSelectionAsync<TOption>(BaseSelection<TOption> selection, Interaction interaction,
        TimeSpan? timeout = null, InteractionCallbackType responseType = InteractionCallbackType.Message, bool ephemeral = false,
        Action<RestMessage>? messageAction = null, CancellationToken cancellationToken = default)
    {
        InteractiveGuards.NotNull(selection);
        InteractiveGuards.NotNull(interaction);
        InteractiveGuards.ValidActionOnStop(selection.ActionOnTimeout);
        InteractiveGuards.ValidActionOnStop(selection.ActionOnCancellation);
        InteractiveGuards.ValidActionOnStop(selection.ActionOnSuccess);
        InteractiveGuards.SupportedInputType(selection, ephemeral);
        InteractiveGuards.ValidResponseType(responseType);
        cancellationToken.ThrowIfCancellationRequested();

        var message = await SendOrModifyMessageAsync(selection, interaction, responseType, ephemeral).ConfigureAwait(false);
        messageAction?.Invoke(message);

        var timeoutTaskSource = new TimeoutTaskCompletionSource<(IReadOnlyList<TOption>, InteractiveStatus)>(timeout ?? _config.DefaultTimeout,
            canReset: false, ([], InteractiveStatus.Timeout), ([], InteractiveStatus.Canceled), cancellationToken);

        using var callback = new SelectionCallback<TOption>(selection, message, timeoutTaskSource, DateTimeOffset.UtcNow, interaction);

        return await WaitForSelectionResultAsync(callback).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns a value indicating whether the <paramref name="interaction"/> targets a component or modal a paginator or selection owns.
    /// </summary>
    /// <param name="interaction">The incoming interaction.</param>
    /// <returns><see langword="true"/> if the interaction targets a component or modal a paginator or selection owns; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="interaction"/> is <see langword="null"/>.</exception>
    public bool IsManaged(Interaction interaction)
    {
        InteractiveGuards.NotNull(interaction);

        if (interaction is MessageComponentInteraction componentInteraction)
        {
            if (TryGetPaginator(componentInteraction.Message, out var paginator) && paginator.TryGetAction(componentInteraction, out _))
            {
                return true;
            }

            if (TryGetComponentPaginator(componentInteraction.Message, out var componentPaginator) && componentPaginator.OwnsComponent(componentInteraction.Data.CustomId))
            {
                return true;
            }
        }

        string? customId = (interaction as ModalInteraction)?.Data?.CustomId ?? (interaction as MessageComponentInteraction)?.Data?.CustomId;

        return ulong.TryParse(customId, out ulong messageId)
            && _callbacks.ContainsKey(messageId);
    }

    /// <summary>
    /// Returns a value indicating whether the <paramref name="message"/> is managed by an active paginator or selection.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns><see langword="true"/> if the message is managed by an active paginator or selection; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is <see langword="null"/>.</exception>
    public bool IsManaged(Message message)
    {
        InteractiveGuards.NotNull(message);
        return IsManaged(message.Id);
    }

    /// <summary>
    /// Returns a value indicating whether the specified ID belongs to a message that is managed by an active paginator or selection.
    /// </summary>
    /// <param name="messageId">The message ID.</param>
    /// <returns><see langword="true"/> if the ID of the message is managed by an active paginator or selection; otherwise, <see langword="false"/>.</returns>
    public bool IsManaged(ulong messageId) => _callbacks.ContainsKey(messageId);

    /// <summary>
    /// Attempts to get a paginator from the message it is currently managing.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="paginator">The paginator, if found.</param>
    /// <returns><see langword="true"/> if the paginator was found; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is <see langword="null"/>.</exception>
    public bool TryGetPaginator(Message message, [MaybeNullWhen(false)] out Paginator paginator)
    {
        InteractiveGuards.NotNull(message);
        return TryGetPaginator(message.Id, out paginator);
    }

    /// <summary>
    /// Attempts to get a paginator from the ID of the message it is currently managing.
    /// </summary>
    /// <param name="messageId">The ID of the message.</param>
    /// <param name="paginator">The paginator, if found.</param>
    /// <returns><see langword="true"/> if the paginator was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetPaginator(ulong messageId, [MaybeNullWhen(false)] out Paginator paginator)
    {
        paginator = null;
        if (!_callbacks.TryGetValue(messageId, out var callback) || callback is not PaginatorCallback paginatorCallback)
            return false;

        paginator = paginatorCallback.Paginator;
        return true;
    }

    /// <summary>
    /// Attempts to get a component paginator from the message it is currently managing.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="paginator">The paginator, if found.</param>
    /// <returns><see langword="true"/> if the paginator was found; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is <see langword="null"/>.</exception>
    public bool TryGetComponentPaginator(Message message, [MaybeNullWhen(false)] out IComponentPaginator paginator)
    {
        InteractiveGuards.NotNull(message);
        return TryGetComponentPaginator(message.Id, out paginator);
    }

    /// <summary>
    /// Attempts to get a component paginator typed as <typeparamref name="TPaginator"/> from the message it is currently managing.
    /// </summary>
    /// <typeparam name="TPaginator">The type of the paginator.</typeparam>
    /// <param name="message">The message.</param>
    /// <param name="paginator">The paginator, if found.</param>
    /// <returns><see langword="true"/> if the paginator was found and is of type <typeparamref name="TPaginator"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is <see langword="null"/>.</exception>
    public bool TryGetComponentPaginator<TPaginator>(Message message, [MaybeNullWhen(false)] out TPaginator paginator)
        where TPaginator : class, IComponentPaginator
    {
        InteractiveGuards.NotNull(message);
        return TryGetComponentPaginator(message.Id, out paginator);
    }

    /// <summary>
    /// Attempts to get a component paginator from the ID of the message it is currently managing.
    /// </summary>
    /// <param name="messageId">The ID of the message.</param>
    /// <param name="paginator">The paginator, if found.</param>
    /// <returns><see langword="true"/> if the paginator was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetComponentPaginator(ulong messageId, [MaybeNullWhen(false)] out IComponentPaginator paginator)
    {
        paginator = null;
        if (!_callbacks.TryGetValue(messageId, out var callback) || callback is not ComponentPaginatorCallback paginatorCallback)
            return false;

        paginator = paginatorCallback.Paginator;
        return true;
    }

    /// <summary>
    /// Attempts to get a component paginator typed as <typeparamref name="TPaginator"/> from the ID of the message it is currently managing.
    /// </summary>
    /// <typeparam name="TPaginator">The type of the paginator.</typeparam>
    /// <param name="messageId">The ID of the message.</param>
    /// <param name="paginator">The paginator, if found.</param>
    /// <returns><see langword="true"/> if the paginator was found and is of type <typeparamref name="TPaginator"/>; otherwise, <see langword="false"/>.</returns>
    public bool TryGetComponentPaginator<TPaginator>(ulong messageId, [MaybeNullWhen(false)] out TPaginator paginator)
        where TPaginator : class, IComponentPaginator
    {
        paginator = null;
        if (!TryGetComponentPaginator(messageId, out var basePaginator) || basePaginator is not TPaginator temp)
            return false;

        paginator = temp;
        return true;
    }

    /// <summary>
    /// Attempts to get a selection from the message it is currently managing.
    /// </summary>
    /// <typeparam name="TOption">The type of the options the selection contains.</typeparam>
    /// <param name="message">The message.</param>
    /// <param name="selection">The selection, if found.</param>
    /// <returns><see langword="true"/> if the selection was found; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is <see langword="null"/>.</exception>
    public bool TryGetSelection<TOption>(Message message, [MaybeNullWhen(false)] out BaseSelection<TOption> selection)
    {
        InteractiveGuards.NotNull(message);
        return TryGetSelection(message.Id, out selection);
    }

    /// <summary>
    /// Attempts to get a selection from the ID of the message it is currently managing.
    /// </summary>
    /// <typeparam name="TOption">The type of the options the selection contains.</typeparam>
    /// <param name="messageId">The ID of the message.</param>
    /// <param name="selection">The selection, if found.</param>
    /// <returns><see langword="true"/> if the selection was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetSelection<TOption>(ulong messageId, [MaybeNullWhen(false)] out BaseSelection<TOption> selection)
    {
        selection = null;
        if (!_callbacks.TryGetValue(messageId, out var callback) || callback is not SelectionCallback<TOption> selectionCallback)
            return false;

        selection = selectionCallback.Selection;
        return true;
    }

    /// <summary>
    /// Returns a value indicating whether an incoming object (such as a message, reaction or interaction) triggers at least one of the filters registered by the <c>Next{Entity}Async()</c> methods.
    /// </summary>
    /// <typeparam name="T">The type of the incoming object.</typeparam>
    /// <param name="obj">The incoming object.</param>
    /// <returns><see langword="true"/> if specified object triggers at least one of the filters; otherwise, <see langword="false"/>.</returns>
    public bool TriggersAnyFilter<T>(T obj) => _filteredCallbacks.Values.Any(x => x.TriggersFilter(obj));

    private async Task ApplyActionOnStopAsync<TOption>(IInteractiveElement<TOption> element, IInteractiveMessageResult result,
        Interaction? lastInteraction, ComponentInteraction? stopInteraction, bool deferInteraction)
    {
        bool ephemeral = result.Message.Flags.HasFlag(MessageFlags.Ephemeral);

        var action = result.Status switch
        {
            InteractiveStatus.Timeout => element.ActionOnTimeout,
            InteractiveStatus.Canceled => element.ActionOnCancellation,
            InteractiveStatus.Success when element is BaseSelection<TOption> selection => selection.ActionOnSuccess,
            _ => throw new ArgumentException("Unknown status.", nameof(result))
        };

        if (action == ActionOnStop.None)
        {
            if (deferInteraction && stopInteraction is not null)
            {
                await stopInteraction.SendResponseAsync(InteractionCallback.DeferredModifyMessage).ConfigureAwait(false);
            }

            return;
        }

        if (action.HasFlag(ActionOnStop.DeleteMessage))
        {
            try
            {
                if (lastInteraction is not null && (DateTimeOffset.UtcNow - lastInteraction.CreatedAt).TotalMinutes <= 15.0)
                {
                    await lastInteraction.DeleteResponseAsync().ConfigureAwait(false);
                }
                else if (!ephemeral)
                {
                    await result.Message.DeleteAsync().ConfigureAwait(false);
                }
                else if (deferInteraction && stopInteraction is not null)
                {
                    await stopInteraction.SendResponseAsync(InteractionCallback.DeferredModifyMessage).ConfigureAwait(false);
                }
            }
            catch (RestException ex) when (ex.Error?.Code == 10008)
            {
                // We want to delete the message so we don't care if the message has been already deleted.
            }

            return;
        }

        IPage? page = null;
        IEnumerable<AttachmentProperties>? attachments = null;
        if (action.HasFlag(ActionOnStop.ModifyMessage))
        {
            page = result.Status switch
            {
                InteractiveStatus.Timeout => element.TimeoutPage,
                InteractiveStatus.Canceled => element.CanceledPage,
                InteractiveStatus.Success when element is BaseSelection<TOption> selection => selection.SuccessPage,
                _ => throw new ArgumentException("Unknown status.", nameof(result))
            };

            attachments = page?.AttachmentsFactory is null ? null : await page.AttachmentsFactory().ConfigureAwait(false);
        }

        List<IMessageComponentProperties>? components = null;
        if (element.InputType.HasFlag(InputType.Buttons) || element.InputType.HasFlag(InputType.SelectMenus))
        {
            if (action.HasFlag(ActionOnStop.DisableInput))
            {
                components = element.GetOrAddComponents(disableAll: true);
            }
            else if (action.HasFlag(ActionOnStop.DeleteInput))
            {
                components = [];
            }
        }

        bool modifyMessage = page?.Text is not null || page?.Embeds.Count > 0 || components is not null || attachments is not null;

        if (modifyMessage)
        {
            try
            {
                if (stopInteraction is not null)
                {
                    // An interaction to stop the element has been received
                    await stopInteraction.SendResponseAsync(InteractionCallback.ModifyMessage(UpdateMessage)).ConfigureAwait(false);
                }
                else if (lastInteraction is not null && (DateTimeOffset.UtcNow - lastInteraction.CreatedAt).TotalMinutes <= 15.0)
                {
                    // The element is from a message that was updated using an interaction, and its token is still valid
                    await lastInteraction.ModifyResponseAsync(UpdateMessage).ConfigureAwait(false);
                }
                else if (!ephemeral)
                {
                    // Fallback for normal messages that don't use interactions or the token is no longer valid, only works for non-ephemeral messages
                    await result.Message.ModifyAsync(UpdateMessage).ConfigureAwait(false);
                }
            }
            catch (RestException ex) when (ex.Error?.Code == 10008)
            {
                // Ignore 10008 (Unknown Message) error.
            }
        }
        else if (deferInteraction && stopInteraction is not null)
        {
            await stopInteraction.SendResponseAsync(InteractionCallback.DeferredModifyMessage).ConfigureAwait(false);
        }

        if (!action.HasFlag(ActionOnStop.DeleteInput) || !element.InputType.HasFlag(InputType.Reactions))
            return;

        Debug.Assert(!ephemeral, "Ephemeral messages cannot have InputType.Reactions");

        return;

        void UpdateMessage(MessageOptions props)
        {
            props.Content = page?.Text;
            props.Embeds = page?.Embeds;
            props.Components = components;
            props.AllowedMentions = page?.AllowedMentions;
            props.Attachments = attachments;
        }
    }

    private async Task<InteractiveMessageResult> SendPaginatorInternalAsync(Paginator paginator, TextChannel? channel, TimeSpan? timeout = null,
        Message? message = null, Action<Message>? messageAction = null, bool resetTimeoutOnInput = false, CancellationToken cancellationToken = default)
    {
        InteractiveGuards.NotNull(paginator);
        InteractiveGuards.MessageFromCurrentUser(_client, message);
        InteractiveGuards.ValidActionOnStop(paginator.ActionOnTimeout);
        InteractiveGuards.ValidActionOnStop(paginator.ActionOnCancellation);
        InteractiveGuards.SupportedInputType(paginator, ephemeral: false);
        cancellationToken.ThrowIfCancellationRequested();

        message = await SendOrModifyMessageAsync(paginator, message, channel).ConfigureAwait(false);
        messageAction?.Invoke(message);

        if (!_config.ProcessSinglePagePaginators && paginator.MaxPageIndex == 0)
        {
            return new InteractiveMessageResultBuilder()
                .WithMessage(message)
                .Build();
        }

        timeout ??= _config.DefaultTimeout;

        if (!_config.ReturnAfterSendingPaginator)
        {
            return await WaitForPaginatorResultUsingCallbackAsync().ConfigureAwait(false);
        }

        _ = WaitForPaginatorResultUsingCallbackAsync().ConfigureAwait(false);

        return new InteractiveMessageResultBuilder()
            .WithMessage(message)
            .Build();

        async Task<InteractiveMessageResult> WaitForPaginatorResultUsingCallbackAsync()
        {
            using var timeoutTaskSource = new TimeoutTaskCompletionSource<InteractiveStatus>(timeout.Value,
                resetTimeoutOnInput, InteractiveStatus.Timeout, InteractiveStatus.Canceled, cancellationToken);

            using var callback = new PaginatorCallback(paginator, message, timeoutTaskSource, DateTimeOffset.UtcNow);
            return await WaitForPaginatorResultAsync(callback).ConfigureAwait(false);
        }
    }

    private async Task<InteractiveMessageResult> SendPaginatorInternalAsync(IComponentPaginator paginator, Message message, TimeSpan? timeout = null,
        Func<Message, Task>? messageAction = null, bool resetTimeoutOnInput = false, CancellationToken cancellationToken = default)
    {
        if (messageAction is not null)
        {
            await messageAction(message).ConfigureAwait(false);
        }

        timeout ??= _config.DefaultTimeout;

        if (!_config.ReturnAfterSendingPaginator)
        {
            return await WaitForPaginatorResultUsingCallbackAsync().ConfigureAwait(false);
        }

        _ = WaitForPaginatorResultUsingCallbackAsync();
        return new InteractiveMessageResultBuilder()
            .WithMessage(message)
            .Build();

        async Task<InteractiveMessageResult> WaitForPaginatorResultUsingCallbackAsync()
        {
            using var timeoutTaskSource = new TimeoutTaskCompletionSource<InteractiveStatus>(timeout.Value,
                resetTimeoutOnInput, InteractiveStatus.Timeout, InteractiveStatus.Canceled, cancellationToken);

            using var callback = new ComponentPaginatorCallback(paginator, message, timeoutTaskSource, DateTimeOffset.UtcNow);
            return await WaitForPaginatorResultAsync(callback).ConfigureAwait(false);
        }
    }

    private async Task<InteractiveMessageResult<TOption>> SendSelectionInternalAsync<TOption>(BaseSelection<TOption> selection, TextChannel? channel,
        TimeSpan? timeout = null, Message? message = null, Action<Message>? messageAction = null, CancellationToken cancellationToken = default)
    {
        InteractiveGuards.NotNull(selection);
        InteractiveGuards.MessageFromCurrentUser(_client, message);
        InteractiveGuards.ValidActionOnStop(selection.ActionOnTimeout);
        InteractiveGuards.ValidActionOnStop(selection.ActionOnCancellation);
        InteractiveGuards.ValidActionOnStop(selection.ActionOnSuccess);
        InteractiveGuards.SupportedInputType(selection, ephemeral: false);
        cancellationToken.ThrowIfCancellationRequested();

        message = await SendOrModifyMessageAsync(selection, message, channel).ConfigureAwait(false);
        messageAction?.Invoke(message);

        using var timeoutTaskSource = new TimeoutTaskCompletionSource<(IReadOnlyList<TOption>, InteractiveStatus)>(timeout ?? _config.DefaultTimeout,
            canReset: false, ([], InteractiveStatus.Timeout), ([], InteractiveStatus.Canceled), cancellationToken);

        using var callback = new SelectionCallback<TOption>(selection, message, timeoutTaskSource, DateTimeOffset.UtcNow);

        return await WaitForSelectionResultAsync(callback).ConfigureAwait(false);
    }

    private async Task<InteractiveResult<T?>> NextEntityAsync<T>(Func<T, bool>? filter = null, Func<T, bool, Task>? action = null,
        TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        filter ??= _ => true;
        action ??= (_, _) => Task.CompletedTask;

        var guid = Guid.NewGuid();

        var timeoutTaskSource = new TimeoutTaskCompletionSource<(T?, InteractiveStatus)>(timeout ?? _config.DefaultTimeout,
            canReset: false, (default, InteractiveStatus.Timeout), (default, InteractiveStatus.Canceled), cancellationToken);

        var callback = new FilteredCallback<T>(filter, action, timeoutTaskSource, DateTimeOffset.UtcNow);

        _filteredCallbacks[guid] = callback;

        var (result, status) = await callback.TimeoutTaskSource.Task.ConfigureAwait(false);

        _filteredCallbacks.TryRemove(guid, out _);

        return new InteractiveResult<T?>(result, callback.GetElapsedTime(status), status);
    }

    private async Task<InteractiveMessageResult> WaitForPaginatorResultAsync(PaginatorCallback callback)
    {
        _callbacks[callback.Message.Id] = callback;
        bool hasReactions = callback.Paginator.InputType.HasFlag(InputType.Reactions);

        // A CancellationTokenSource is used here for 2 things:
        // 1. To stop WaitForMessagesAsync() to avoid memory leaks
        // 2. To cancel InitializeMessageAsync() to avoid adding reactions after TimeoutTaskSource.Task has returned.
        using var cts = hasReactions ? new CancellationTokenSource() : null;

        if (hasReactions)
        {
            _ = callback.Paginator.InitializeMessageAsync(callback.Message, cts!.Token).ConfigureAwait(false);

            if (callback.Paginator.Emotes.Values.Any(x => x == PaginatorAction.Jump))
            {
                _ = WaitForMessagesAsync(cts).ConfigureAwait(false);
            }
        }

        var status = await callback.TimeoutTaskSource.Task.ConfigureAwait(false);
        cts?.Cancel();

        var result = InteractiveMessageResultBuilder.FromCallback(callback, status).Build();

        if (_callbacks.TryRemove(callback.Message.Id, out _))
        {
            await ApplyActionOnStopAsync(callback.Paginator, result, callback.LastInteraction, callback.StopInteraction, _config.DeferStopPaginatorInteractions).ConfigureAwait(false);
        }

        return result;

        async Task WaitForMessagesAsync(CancellationTokenSource cancellationTokenSource)
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                var messageResult = await NextMessageAsync(msg => msg.Channel.Id == callback.Message.Channel.Id && msg.Source == MessageSource.User,
                    action: null, callback.TimeoutTaskSource.Delay, cancellationTokenSource.Token).ConfigureAwait(false);
                if (messageResult.IsSuccess)
                {
                    await callback.ExecuteAsync(messageResult.Value).ConfigureAwait(false);
                }
            }
        }
    }

    private async Task<InteractiveMessageResult> WaitForPaginatorResultAsync(ComponentPaginatorCallback callback)
    {
        _callbacks[callback.Message.Id] = callback;

        var status = await callback.TimeoutTaskSource.Task.ConfigureAwait(false);

        var result = InteractiveMessageResultBuilder.FromCallback(callback, status).Build();

        if (!_callbacks.TryRemove(callback.Message.Id, out _))
            return result;

        callback.Paginator.Status = result.Status switch
        {
            InteractiveStatus.Canceled => PaginatorStatus.Canceled,
            InteractiveStatus.Timeout => PaginatorStatus.TimedOut,
            _ => throw new InvalidOperationException($"Invalid {nameof(InteractiveStatus)} {result.Status}.")
        };

        await callback.Paginator.ApplyActionOnStopAsync(result.Message, result.StopInteraction, _config.DeferStopPaginatorInteractions).ConfigureAwait(false);

        return result;
    }

    private async Task<InteractiveMessageResult<TOption>> WaitForSelectionResultAsync<TOption>(SelectionCallback<TOption> callback)
    {
        _callbacks[callback.Message.Id] = callback;

        // A CancellationTokenSource is used here for 2 things:
        // 1. To cancel NextMessageAsync() to avoid memory leaks
        // 2. To cancel InitializeMessageAsync() to avoid adding reactions after TimeoutTaskSource.Task has returned.
        using var cts = callback.Selection.InputType.HasFlag(InputType.Messages) || callback.Selection.InputType.HasFlag(InputType.Reactions)
            ? new CancellationTokenSource()
            : null;

        _ = callback.Selection.InitializeMessageAsync(callback.Message, cts?.Token ?? CancellationToken.None).ConfigureAwait(false);

        if (callback.Selection.InputType.HasFlag(InputType.Messages))
        {
            _ = NextMessageAsync(_ => false, async (msg, _) =>
            {
                if (msg.Channel.Id == callback.Message.Channel.Id && msg.Source == MessageSource.User)
                {
                    await callback.ExecuteAsync(msg).ConfigureAwait(false);
                }
            }, callback.TimeoutTaskSource.Delay, cts!.Token).ConfigureAwait(false);
        }

        var (selected, status) = await callback.TimeoutTaskSource.Task.ConfigureAwait(false);
        cts?.Cancel();

        var result = InteractiveMessageResultBuilder<TOption>.FromCallback(callback, selected, status).Build();

        if (_callbacks.TryRemove(callback.Message.Id, out _))
        {
            await ApplyActionOnStopAsync(callback.Selection, result, callback.LastInteraction, callback.StopInteraction, _config.DeferStopSelectionInteractions).ConfigureAwait(false);
        }

        return result;
    }

    private async Task<Message> SendOrModifyMessageAsync<TOption>(IInteractiveElement<TOption> element, Message? message, TextChannel? channel)
    {
        var page = await element.GetCurrentPageAsync().ConfigureAwait(false);

        List<IMessageComponentProperties>? component = null;
        bool addComponents = element is not Paginator pag || _config.ProcessSinglePagePaginators || pag.MaxPageIndex > 0;
        if ((element.InputType.HasFlag(InputType.Buttons) || element.InputType.HasFlag(InputType.SelectMenus)) && addComponents)
        {
            component = element.GetOrAddComponents(disableAll: false);
        }

        var attachments = page.AttachmentsFactory is null ? null : await page.AttachmentsFactory().ConfigureAwait(false);

        if (message is not null)
        {
            await message.ModifyAsync(x =>
            {
                x.Content = page.Text;
                x.Embeds = page.Embeds;
                x.Components = component;
                x.AllowedMentions = page.AllowedMentions;
                x.Attachments = attachments;
                x.Flags = page.MessageFlags;
            }).ConfigureAwait(false);
        }
        else if (channel is not null)
        {
            message = await channel.SendFilesAsync(attachments ?? [], page.Text, page.IsTTS, embed: null, options: null,
                page.AllowedMentions, page.MessageReference, component, page.StickerIds.ToArray(), page.Embeds, flags: page.MessageFlags ?? MessageFlags.None).ConfigureAwait(false);
        }
        else
        {
            throw new InvalidOperationException($"Expected at least one of {nameof(message)} or {nameof(channel)} to not be null");
        }

        return message;
    }

    private async Task<RestMessage> SendOrModifyMessageAsync<TOption>(IInteractiveElement<TOption> element, Interaction interaction,
        InteractionCallbackType responseType, bool ephemeral)
    {
        var page = await element.GetCurrentPageAsync().ConfigureAwait(false);

        List<IMessageComponentProperties>? component = null;
        bool addComponents = element is not Paginator pag || _config.ProcessSinglePagePaginators || pag.MaxPageIndex > 0;
        if ((element.InputType.HasFlag(InputType.Buttons) || element.InputType.HasFlag(InputType.SelectMenus)) && addComponents)
        {
            component = element.GetOrAddComponents(disableAll: false).Build();
        }

        var embeds = page.Embeds;
        var attachments = page.AttachmentsFactory is null ? null : await page.AttachmentsFactory().ConfigureAwait(false);

        switch (responseType)
        {
            case InteractionCallbackType.Message:
                await interaction.RespondWithFilesAsync(attachments ?? [],
                    page.Text, embeds, page.IsTTS, ephemeral, page.AllowedMentions, component, flags: page.MessageFlags ?? MessageFlags.None).ConfigureAwait(false);
                return await interaction.GetOriginalResponseAsync().ConfigureAwait(false);

            case InteractionCallbackType.DeferredMessage:
                return await interaction.FollowupWithFilesAsync(attachments ?? [],
                    page.Text, embeds, page.IsTTS, ephemeral, page.AllowedMentions, component, flags: page.MessageFlags ?? MessageFlags.None).ConfigureAwait(false);

            case InteractionCallbackType.DeferredModifyMessage:
                return await interaction.ModifyOriginalResponseAsync(UpdateMessage).ConfigureAwait(false);

            case InteractionCallbackType.ModifyMessage:
                InteractiveGuards.ValidResponseType(responseType, interaction);
                return await ((MessageComponentInteraction)interaction).ModifyResponseAsync(UpdateMessage).ConfigureAwait(false);

            default:
                throw new ArgumentException("Unknown interaction response type.", nameof(responseType));
        }

        void UpdateMessage(MessageOptions props)
        {
            props.Content = page.Text;
            props.Embeds = embeds;
            props.Components = component;
            props.AllowedMentions = page.AllowedMentions;
            props.Attachments = attachments.AsOptional();
            props.Flags = page.MessageFlags;
        }
    }

    private ValueTask MessageCreated(GatewayClient client, Message message)
    {
        if (message.Author.Id == client.Id)
        {
            return ValueTask.CompletedTask;
        }

        foreach (var pair in _filteredCallbacks)
        {
            if (pair.Value.IsCompatible(message))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await pair.Value.ExecuteAsync(message).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to execute filtered message callback");
                    }
                });
            }
        }

        return ValueTask.CompletedTask;
    }

    private Task InteractionCreated(GatewayClient client, Interaction interaction)
    {
        if ((interaction is MessageComponentInteraction componentInteraction
            && _callbacks.TryGetValue(componentInteraction.Message.Id, out var callback))
            || (interaction is ModalInteraction { Message: not null } modal
                && _callbacks.TryGetValue(modal.Message.Id, out callback)))
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await callback.ExecuteAsync(interaction).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    ulong? messageId = (interaction as MessageComponentInteraction)?.Message?.Id ?? (interaction as ModalInteraction)?.Message?.Id;
                    _logger.LogError(ex, "Failed to execute interaction callback (message Id: {MessageId})", messageId?.ToString() ?? "?");
                }
            });
        }

        foreach (var pair in _filteredCallbacks)
        {
            if (pair.Value.IsCompatible(interaction))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await pair.Value.ExecuteAsync(interaction).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to execute filtered interaction callback");
                    }
                });
            }
        }

        return Task.CompletedTask;
    }
}

internal class InteractionCreateHandler : IInteractionCreateShardedGatewayHandler
{
    /// <inheritdoc />
    public ValueTask HandleAsync(GatewayClient client, Interaction interaction)
    {
            
        return ValueTask.CompletedTask;
    }
}