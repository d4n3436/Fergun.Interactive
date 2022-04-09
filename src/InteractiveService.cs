using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Fergun.Interactive.Extensions;
using Fergun.Interactive.Pagination;
using Fergun.Interactive.Selection;

namespace Fergun.Interactive;
// Based on Discord.InteractivityAddon
// https://github.com/Playwo/Discord.InteractivityAddon

/// <summary>
/// Represents a service containing methods for interactivity purposes.
/// </summary>
public class InteractiveService
{
    private readonly BaseSocketClient _client;
    private readonly ConcurrentDictionary<ulong, IInteractiveCallback> _callbacks = new();
    private readonly ConcurrentDictionary<Guid, IInteractiveCallback> _filteredCallbacks = new();
    private readonly InteractiveConfig _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractiveService"/> class using the default timeout.
    /// </summary>
    /// <param name="client">An instance of <see cref="BaseSocketClient"/>.</param>
    public InteractiveService(BaseSocketClient client)
        : this(client, new InteractiveConfig())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractiveService"/> class using a specified default timeout.
    /// </summary>
    /// <param name="client">An instance of <see cref="BaseSocketClient"/>.</param>
    /// <param name="defaultTimeout">The default timeout for the interactive actions.</param>
    [Obsolete("This constructor is deprecated and it will be removed in a future version. Use InteractiveService(BaseSocketClient, InteractiveConfig) instead.")]
    public InteractiveService(BaseSocketClient client, TimeSpan defaultTimeout)
        : this(client, new InteractiveConfig { DefaultTimeout = defaultTimeout })
    {
        DefaultTimeout = defaultTimeout;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractiveService"/> class using the default timeout.
    /// </summary>
    /// <param name="client">An instance of <see cref="DiscordSocketClient"/>.</param>
    public InteractiveService(DiscordSocketClient client)
        : this((BaseSocketClient)client)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractiveService"/> class using a specified default timeout.
    /// </summary>
    /// <param name="client">An instance of <see cref="DiscordSocketClient"/>.</param>
    /// <param name="defaultTimeout">The default timeout for the interactive actions.</param>
    [Obsolete("This constructor is deprecated and will be removed in a future version. Use InteractiveService(DiscordSocketClient, InteractiveConfig) instead.")]
    public InteractiveService(DiscordSocketClient client, TimeSpan defaultTimeout)
        : this((BaseSocketClient)client, defaultTimeout)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractiveService"/> class using the default timeout.
    /// </summary>
    /// <param name="client">An instance of <see cref="DiscordShardedClient"/>.</param>
    public InteractiveService(DiscordShardedClient client)
        : this((BaseSocketClient)client)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractiveService"/> class using a specified default timeout.
    /// </summary>
    /// <param name="client">An instance of <see cref="DiscordShardedClient"/>.</param>
    /// <param name="defaultTimeout">The default timeout for the interactive actions.</param>
    [Obsolete("This constructor is deprecated and will be removed in a future version. Use InteractiveService(DiscordShardedClient, InteractiveConfig) instead.")]
    public InteractiveService(DiscordShardedClient client, TimeSpan defaultTimeout)
        : this((BaseSocketClient)client, defaultTimeout)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractiveService"/> class using a specified configuration.
    /// </summary>
    /// <param name="client">An instance of <see cref="BaseSocketClient"/>.</param>
    /// <param name="config">The configuration.</param>
    public InteractiveService(BaseSocketClient client, InteractiveConfig config)
    {
        InteractiveGuards.NotNull(client);
        InteractiveGuards.NotNull(config);

        _client = client;
        _config = config;
        _client.MessageReceived += MessageReceived;
        _client.ReactionAdded += ReactionAdded;
        _client.InteractionCreated += InteractionCreated;

        Log = LogMessage;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractiveService"/> class using a specified configuration.
    /// </summary>
    /// <param name="client">An instance of <see cref="DiscordSocketClient"/>.</param>
    /// <param name="config">The configuration.</param>
    public InteractiveService(DiscordSocketClient client, InteractiveConfig config)
        : this((BaseSocketClient)client, config)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractiveService"/> class using a specified configuration.
    /// </summary>
    /// <param name="client">An instance of <see cref="DiscordShardedClient"/>.</param>
    /// <param name="config">The configuration.</param>
    public InteractiveService(DiscordShardedClient client, InteractiveConfig config)
        : this((BaseSocketClient)client, config)
    {
    }

    /// <summary>
    /// Occurs when an interactive-related information is received.
    /// </summary>
    public event Func<LogMessage, Task> Log;

    /// <summary>
    /// Gets a dictionary of active callbacks.
    /// </summary>
    public IDictionary<ulong, IInteractiveCallback> Callbacks => _callbacks;

    /// <summary>
    /// Gets the default timeout for interactive actions provided by this service.
    /// </summary>
    [Obsolete("This property is deprecated and will be removed in a future version.")]
    public TimeSpan DefaultTimeout { get; }

    /// <summary>
    /// Attempts to remove and return a callback.
    /// </summary>
    /// <param name="id">The Id of the callback.</param>
    /// <param name="callback">The callback, if found.</param>
    /// <returns>Whether the callback was removed.</returns>
    public bool TryRemoveCallback(ulong id, out IInteractiveCallback callback)
        => _callbacks.TryRemove(id, out callback);

    /// <summary>
    /// Sends a message to a channel (after an optional delay) and deletes it after another delay.
    /// </summary>
    /// <remarks>Discard the returning task if you don't want to wait it for completion.</remarks>
    /// <param name="channel">The target message channel.</param>
    /// <param name="sendDelay">The time to wait before sending the message.</param>
    /// <param name="deleteDelay">The time to wait between sending and deleting the message.</param>
    /// <param name="message">An existing message to modify.</param>
    /// <param name="text">The message to be sent.</param>
    /// <param name="isTTS">Determines whether the message should be read aloud by Discord or not.</param>
    /// <param name="embed">The <see cref="EmbedType.Rich"/> <see cref="Embed"/> to be sent.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <param name="allowedMentions">
    ///     Specifies if notifications are sent for mentioned users and roles in the message <paramref name="text"/>.
    ///     If <c>null</c>, all mentioned roles and users will be notified.
    /// </param>
    /// <param name="messageReference">The message references to be included. Used to reply to specific messages.</param>
    /// <returns>A task that represents the asynchronous delay, send message operation, delay and delete message operation.</returns>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentNullException"/>
    public async Task DelayedSendMessageAndDeleteAsync(IMessageChannel channel, TimeSpan? sendDelay = null, TimeSpan? deleteDelay = null,
        IUserMessage? message = null, string? text = null, bool isTTS = false, Embed? embed = null, RequestOptions? options = null,
        AllowedMentions? allowedMentions = null, MessageReference? messageReference = null)
    {
        InteractiveGuards.NotNull(channel);
        InteractiveGuards.MessageFromCurrentUser(_client, message);

        await Task.Delay(sendDelay ?? TimeSpan.Zero).ConfigureAwait(false);

        if (message is null)
        {
            message = await channel.SendMessageAsync(text, isTTS, embed, options, allowedMentions, messageReference).ConfigureAwait(false);
        }
        else
        {
            await message.ModifyAsync(x =>
            {
                x.Content = text;
                x.Embed = embed;
                x.AllowedMentions = allowedMentions;
            }, options).ConfigureAwait(false);
        }

        await DelayedDeleteMessageAsync(message, deleteDelay).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a file to a channel delayed and deletes it after another delay.
    /// </summary>
    /// <remarks>Discard the returning task if you don't want to wait it for completion.</remarks>
    /// <param name="channel">The target message channel.</param>
    /// <param name="sendDelay">The time to wait before sending the message.</param>
    /// <param name="deleteDelay">The time to wait between sending and deleting the message.</param>
    /// <param name="filePath">The file path of the file.</param>
    /// <param name="text">The message to be sent.</param>
    /// <param name="isTTS">Whether the message should be read aloud by Discord or not.</param>
    /// <param name="embed">The <see cref="EmbedType.Rich"/> <see cref="Embed"/> to be sent.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <param name="isSpoiler">Whether the message attachment should be hidden as a spoiler.</param>
    /// <param name="allowedMentions">
    ///     Specifies if notifications are sent for mentioned users and roles in the message <paramref name="text"/>.
    ///     If <c>null</c>, all mentioned roles and users will be notified.
    /// </param>
    /// <param name="messageReference">The message references to be included. Used to reply to specific messages.</param>
    /// <returns>A task that represents the asynchronous delay, send message operation, delay and delete message operation.</returns>
    /// <exception cref="ArgumentNullException"/>
    public async Task DelayedSendFileAndDeleteAsync(IMessageChannel channel, TimeSpan? sendDelay = null, TimeSpan? deleteDelay = null,
        string? filePath = null, string? text = null, bool isTTS = false, Embed? embed = null, RequestOptions? options = null,
        bool isSpoiler = false, AllowedMentions? allowedMentions = null, MessageReference? messageReference = null)
    {
        InteractiveGuards.NotNull(channel);

        await Task.Delay(sendDelay ?? TimeSpan.Zero).ConfigureAwait(false);
        var msg = await channel.SendFileAsync(filePath, text, isTTS, embed, options, isSpoiler, allowedMentions, messageReference)
            .ConfigureAwait(false);
        await DelayedDeleteMessageAsync(msg, deleteDelay).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a file to a channel delayed and deletes it after another delay.
    /// </summary>
    /// <remarks>Discard the returning task if you don't want to wait it for completion.</remarks>
    /// <param name="channel">The target message channel.</param>
    /// <param name="sendDelay">The time to wait before sending the message.</param>
    /// <param name="deleteDelay">The time to wait between sending and deleting the message.</param>
    /// <param name="stream">The <see cref="Stream"/> of the file to be sent.</param>
    /// <param name="filename">The name of the attachment.</param>
    /// <param name="text">The message to be sent.</param>
    /// <param name="isTTS">Whether the message should be read aloud by Discord or not.</param>
    /// <param name="embed">The <see cref="EmbedType.Rich"/> <see cref="Embed"/> to be sent.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <param name="isSpoiler">Whether the message attachment should be hidden as a spoiler.</param>
    /// <param name="allowedMentions">
    ///     Specifies if notifications are sent for mentioned users and roles in the message <paramref name="text"/>.
    ///     If <c>null</c>, all mentioned roles and users will be notified.
    /// </param>
    /// <param name="messageReference">The message references to be included. Used to reply to specific messages.</param>
    /// <returns>A task that represents the asynchronous delay, send message operation, delay and delete message operation.</returns>
    /// <exception cref="ArgumentNullException"/>
    public async Task DelayedSendFileAndDeleteAsync(IMessageChannel channel, TimeSpan? sendDelay = null, TimeSpan? deleteDelay = null,
        Stream? stream = null, string? filename = null, string? text = null, bool isTTS = false, Embed? embed = null, RequestOptions? options = null,
        bool isSpoiler = false, AllowedMentions? allowedMentions = null, MessageReference? messageReference = null)
    {
        InteractiveGuards.NotNull(channel);

        await Task.Delay(sendDelay ?? TimeSpan.Zero).ConfigureAwait(false);
        var msg = await channel.SendFileAsync(stream, filename, text, isTTS, embed, options, isSpoiler, allowedMentions, messageReference)
            .ConfigureAwait(false);
        await DelayedDeleteMessageAsync(msg, deleteDelay).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a message after a delay.
    /// </summary>
    /// <remarks>Discard the returning task if you don't want to wait it for completion.</remarks>
    /// <param name="message">The message to delete.</param>
    /// <param name="deleteDelay">The time to wait before deleting the message.</param>
    /// <returns>A task that represents the asynchronous delay and delete message operation.</returns>
    /// <exception cref="ArgumentNullException"/>
    public async Task DelayedDeleteMessageAsync(IMessage message, TimeSpan? deleteDelay = null)
    {
        InteractiveGuards.NotNull(message);

        await Task.Delay(deleteDelay ?? _config.DefaultTimeout).ConfigureAwait(false);

        try
        {
            await message.DeleteAsync().ConfigureAwait(false);
        }
        catch (HttpException e) when (e.HttpCode == HttpStatusCode.NotFound)
        {
            // We want to delete the message so we don't care if the message has been already deleted.
        }
    }

    /// <summary>
    /// Gets the next incoming message that passes the <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A filter which the message has to pass.</param>
    /// <param name="action">
    /// An action which gets executed to incoming messages,
    /// where <see cref="SocketMessage"/> is the incoming message and <see cref="bool"/>
    /// is whether the message passed the <paramref name="filter"/>.
    /// </param>
    /// <param name="timeout">The time to wait before the methods returns a timeout result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the request.</param>
    /// <returns>
    /// A task that represents the asynchronous wait operation for the next message.
    /// The task result contains an <see cref="InteractiveResult{T}"/> with the
    /// message (if successful), the elapsed time and the status.
    /// </returns>
    public async Task<InteractiveResult<SocketMessage?>> NextMessageAsync(Func<SocketMessage, bool>? filter = null,
        Func<SocketMessage, bool, Task>? action = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        => await NextEntityAsync(filter, action, timeout, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Gets the next incoming reaction that passes the <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A filter which the reaction has to pass.</param>
    /// <param name="action">
    /// An action which gets executed to incoming reactions, where <see cref="SocketReaction"/>
    /// is the incoming reaction and <see cref="bool"/> is whether the interaction passed the <paramref name="filter"/>.
    /// </param>
    /// <param name="timeout">The time to wait before the methods returns a timeout result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the request.</param>
    /// <returns>
    /// A task that represents the asynchronous wait operation for the next reaction.
    /// The task result contains an <see cref="InteractiveResult{T}"/> with the
    /// reaction (if successful), the elapsed time and the status.
    /// </returns>
    public async Task<InteractiveResult<SocketReaction?>> NextReactionAsync(Func<SocketReaction, bool>? filter = null,
        Func<SocketReaction, bool, Task>? action = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        => await NextEntityAsync(filter, action, timeout, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Gets the next interaction that passes the <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A filter which the interaction has to pass.</param>
    /// <param name="action">
    /// An action which gets executed to incoming interactions,
    /// where <see cref="SocketInteraction"/> is the incoming interaction and <see cref="bool"/>
    /// is whether the interaction passed the <paramref name="filter"/>.
    /// </param>
    /// <param name="timeout">The time to wait before the methods returns a timeout result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the request.</param>
    /// <returns>
    /// A task that represents the asynchronous wait operation for the next interaction.
    /// The task result contains an <see cref="InteractiveResult{T}"/> with the
    /// interaction (if successful), the elapsed time and the status.
    /// </returns>
    public async Task<InteractiveResult<SocketInteraction?>> NextInteractionAsync(Func<SocketInteraction, bool>? filter = null,
        Func<SocketInteraction, bool, Task>? action = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        => await NextEntityAsync(filter, action, timeout, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Gets the next message component that passes the <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A filter which the message component has to pass.</param>
    /// <param name="action">
    /// An action which gets executed to incoming interactions,
    /// where <see cref="SocketMessageComponent"/> is the incoming message component and <see cref="bool"/>
    /// is whether the message component passed the <paramref name="filter"/>.
    /// </param>
    /// <param name="timeout">The time to wait before the methods returns a timeout result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the request.</param>
    /// <returns>
    /// A task that represents the asynchronous wait operation for the next message component.
    /// The task result contains an <see cref="InteractiveResult{T}"/> with the
    /// message component (if successful), the elapsed time and the status.
    /// </returns>
    public async Task<InteractiveResult<SocketMessageComponent?>> NextMessageComponentAsync(Func<SocketMessageComponent, bool>? filter = null,
        Func<SocketMessageComponent, bool, Task>? action = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        => await NextEntityAsync(filter, action, timeout, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Gets the next slash command that passes the <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A filter which the slash command has to pass.</param>
    /// <param name="action">
    /// An action which gets executed to incoming interactions,
    /// where <see cref="SocketSlashCommand"/> is the incoming slash command and <see cref="bool"/>
    /// is whether the slash command passed the <paramref name="filter"/>.
    /// </param>
    /// <param name="timeout">The time to wait before the methods returns a timeout result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the request.</param>
    /// <returns>
    /// A task that represents the asynchronous wait operation for the next slash command.
    /// The task result contains an <see cref="InteractiveResult{T}"/> with the
    /// slash command (if successful), the elapsed time and the status.
    /// </returns>
    public async Task<InteractiveResult<SocketSlashCommand?>> NextSlashCommandAsync(Func<SocketSlashCommand, bool>? filter = null,
        Func<SocketSlashCommand, bool, Task>? action = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        => await NextEntityAsync(filter, action, timeout, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Gets the next user command that passes the <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A filter which the user command has to pass.</param>
    /// <param name="action">
    /// An action which gets executed to incoming interactions,
    /// where <see cref="SocketUserCommand"/> is the incoming user command and <see cref="bool"/>
    /// is whether the user command passed the <paramref name="filter"/>.
    /// </param>
    /// <param name="timeout">The time to wait before the methods returns a timeout result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the request.</param>
    /// <returns>
    /// A task that represents the asynchronous wait operation for the next user command.
    /// The task result contains an <see cref="InteractiveResult{T}"/> with the
    /// user command (if successful), the elapsed time and the status.
    /// </returns>
    public async Task<InteractiveResult<SocketUserCommand?>> NextUserCommandAsync(Func<SocketUserCommand, bool>? filter = null,
        Func<SocketUserCommand, bool, Task>? action = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        => await NextEntityAsync(filter, action, timeout, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Gets the next message command that passes the <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A filter which the message command has to pass.</param>
    /// <param name="action">
    /// An action which gets executed to incoming interactions,
    /// where <see cref="SocketMessageCommand"/> is the incoming message command and <see cref="bool"/>
    /// is whether the message command passed the <paramref name="filter"/>.
    /// </param>
    /// <param name="timeout">The time to wait before the methods returns a timeout result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the request.</param>
    /// <returns>
    /// A task that represents the asynchronous wait operation for the next message command.
    /// The task result contains an <see cref="InteractiveResult{T}"/> with the
    /// message command (if successful), the elapsed time and the status.
    /// </returns>
    public async Task<InteractiveResult<SocketMessageCommand?>> NextMessageCommandAsync(Func<SocketMessageCommand, bool>? filter = null,
        Func<SocketMessageCommand, bool, Task>? action = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        => await NextEntityAsync(filter, action, timeout, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Gets the next autocomplete interaction that passes the <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A filter which the autocomplete interaction has to pass.</param>
    /// <param name="action">
    /// An action which gets executed to incoming interactions,
    /// where <see cref="SocketAutocompleteInteraction"/> is the incoming autocomplete interaction and <see cref="bool"/>
    /// is whether the autocomplete interaction passed the <paramref name="filter"/>.
    /// </param>
    /// <param name="timeout">The time to wait before the methods returns a timeout result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the request.</param>
    /// <returns>
    /// A task that represents the asynchronous wait operation for the next autocomplete interaction.
    /// The task result contains an <see cref="InteractiveResult{T}"/> with the
    /// autocomplete interaction (if successful), the elapsed time and the status.
    /// </returns>
    public async Task<InteractiveResult<SocketAutocompleteInteraction?>> NextAutocompleteInteractionAsync(Func<SocketAutocompleteInteraction, bool>? filter = null,
        Func<SocketAutocompleteInteraction, bool, Task>? action = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
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
    /// will contain the sent message and a <see cref="InteractiveStatus.Success"/> status.
    /// </returns>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="NotSupportedException"/>
    public async Task<InteractiveMessageResult> SendPaginatorAsync(Paginator paginator, IMessageChannel channel, TimeSpan? timeout = null,
        Action<IUserMessage>? messageAction = null, bool resetTimeoutOnInput = false, CancellationToken cancellationToken = default)
        => await SendPaginatorInternalAsync(paginator, channel, timeout, null, messageAction, resetTimeoutOnInput, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Modifies a message to a paginator with pages which the user can change through via reactions or buttons.
    /// </summary>
    /// <param name="paginator">The paginator to send.</param>
    /// <param name="message">An existing message to modify to display the <see cref="Paginator"/>.</param>
    /// <param name="timeout">The time until the <see cref="Paginator"/> times out.</param>
    /// <param name="messageAction">A method that gets executed once when a message containing the paginator is modified.</param>
    /// <param name="resetTimeoutOnInput">Whether to reset the internal timeout timer when a valid input is received.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the paginator.</param>
    /// <returns>
    /// A task that represents the asynchronous operation for modifying the message to a paginator and waiting for a timeout or cancellation.<br/>
    /// The task result contains an <see cref="InteractiveMessageResult"/> with the message used for pagination
    /// (which may not be valid if the message has been deleted), the elapsed time and the status.<br/>
    /// If the paginator only contains one page, the task will return when the message has been sent and the result
    /// will contain the sent message and a <see cref="InteractiveStatus.Success"/> status.
    /// </returns>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="NotSupportedException"/>
    public async Task<InteractiveMessageResult> SendPaginatorAsync(Paginator paginator, IUserMessage message, TimeSpan? timeout = null,
        Action<IUserMessage>? messageAction = null, bool resetTimeoutOnInput = false, CancellationToken cancellationToken = default)
        => await SendPaginatorInternalAsync(paginator, null, timeout, message, messageAction, resetTimeoutOnInput, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Responds an interaction with a paginator.
    /// </summary>
    /// <param name="paginator">The paginator to send.</param>
    /// <param name="interaction">The interaction to respond.</param>
    /// <param name="timeout">The time until the <see cref="Paginator"/> times out.</param>
    /// <param name="responseType">The response type. When using the "Deferred" response types, you must pass an interaction that has already been deferred.</param>
    /// <param name="ephemeral">
    /// Whether the response message should be ephemeral. Ignored if modifying a non-ephemeral message.<br/><br/>
    /// Ephemeral paginators have several limitations:<br/>
    /// - <see cref="ActionOnStop.DeleteMessage"/> won't work (they cannot be deleted through the API).<br/>
    /// - <see cref="InputType.Reactions"/> won't work (they can't have reactions).<br/><br/>
    /// Ephemeral paginators require an interaction to be modified, which causes the following problems:<br/>
    /// - <see cref="BaseSelection{TOption}.ActionOnTimeout"/> will only work if a least one interaction for changing the page has been received in the last 15 minutes.<br/>
    /// - <see cref="BaseSelection{TOption}.ActionOnCancellation"/> won't work if the selection is cancelled using a <paramref name="cancellationToken"/>, unless the requisites above are satisfied.<br/>
    /// </param>
    /// <param name="messageAction">A method that gets executed once when a message containing the paginator is sent or modified.</param>
    /// <param name="resetTimeoutOnInput">Whether to reset the internal timeout timer when a valid input is received.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the paginator.</param>
    /// <returns>
    /// A task that represents the asynchronous operation for sending the paginator and waiting for a timeout or cancellation.<br/>
    /// The task result contains an <see cref="InteractiveMessageResult"/> with the message used for pagination
    /// (which may not be valid if the message has been deleted), the elapsed time and the status.<br/>
    /// If the paginator only contains one page, the task will return when the message has been sent and the result
    /// will contain the sent message and a <see cref="InteractiveStatus.Success"/> status.
    /// </returns>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="NotSupportedException"/>
    public async Task<InteractiveMessageResult> SendPaginatorAsync(Paginator paginator, IDiscordInteraction interaction, TimeSpan? timeout = null,
        InteractionResponseType responseType = InteractionResponseType.ChannelMessageWithSource, bool ephemeral = false,
        Action<IUserMessage>? messageAction = null, bool resetTimeoutOnInput = false, CancellationToken cancellationToken = default)
    {
        InteractiveGuards.NotNull(paginator);
        InteractiveGuards.NotNull(interaction);
        InteractiveGuards.DeleteAndDisableInputNotSet(paginator.ActionOnTimeout);
        InteractiveGuards.DeleteAndDisableInputNotSet(paginator.ActionOnCancellation);
        InteractiveGuards.SupportedInputType(paginator, ephemeral);
        InteractiveGuards.ValidResponseType(responseType);
        InteractiveGuards.NotCanceled(cancellationToken);

        var message = await SendOrModifyMessageAsync(paginator, interaction, responseType, ephemeral).ConfigureAwait(false);
        messageAction?.Invoke(message);

        if (paginator.MaxPageIndex == 0)
        {
            return new InteractiveMessageResultBuilder()
                .WithMessage(message)
                .Build();
        }

        var timeoutTaskSource = new TimeoutTaskCompletionSource<InteractiveStatus>(timeout ?? _config.DefaultTimeout,
            resetTimeoutOnInput, InteractiveStatus.Timeout, InteractiveStatus.Canceled, cancellationToken);

        if (_config.ReturnAfterSendingPaginator)
        {
            _ = WaitForPaginatorResultUsingCallbackAsync().ConfigureAwait(false);

            return new InteractiveMessageResultBuilder()
                .WithMessage(message)
                .Build();
        }

        return await WaitForPaginatorResultUsingCallbackAsync().ConfigureAwait(false);

        async Task<InteractiveMessageResult> WaitForPaginatorResultUsingCallbackAsync()
        {
            var initialInteraction = responseType is InteractionResponseType.DeferredUpdateMessage or InteractionResponseType.UpdateMessage ? interaction : null;
            using var callback = new PaginatorCallback(paginator, message, timeoutTaskSource, DateTimeOffset.UtcNow, initialInteraction);
            return await WaitForPaginatorResultAsync(callback).ConfigureAwait(false);
        }
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
    /// The task result contains an <see cref="InteractiveMessageResult{T}"/> with the selected value (if valid), the message used for the selection
    /// (which may not be valid if the message has been deleted), the elapsed time and the status.
    /// </returns>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="NotSupportedException"/>
    public async Task<InteractiveMessageResult<TOption?>> SendSelectionAsync<TOption>(BaseSelection<TOption> selection, IMessageChannel channel,
        TimeSpan? timeout = null, Action<IUserMessage>? messageAction = null, CancellationToken cancellationToken = default)
        => await SendSelectionInternalAsync(selection, channel, timeout, null, messageAction, cancellationToken).ConfigureAwait(false);

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
    /// The task result contains an <see cref="InteractiveMessageResult{T}"/> with the selected value (if valid), the message used for the selection
    /// (which may not be valid if the message has been deleted), the elapsed time and the status.
    /// </returns>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="NotSupportedException"/>
    public async Task<InteractiveMessageResult<TOption?>> SendSelectionAsync<TOption>(BaseSelection<TOption> selection, IUserMessage message,
        TimeSpan? timeout = null, Action<IUserMessage>? messageAction = null, CancellationToken cancellationToken = default)
        => await SendSelectionInternalAsync(selection, null, timeout, message, messageAction, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Responds an interaction with a selection.
    /// </summary>
    /// <typeparam name="TOption">The type of the options the selection contains.</typeparam>
    /// <param name="selection">The selection to send.</param>
    /// <param name="interaction">The interaction to respond.</param>
    /// <param name="timeout">The time until the selection times out.</param>
    /// <param name="responseType">The response type. When using the "Deferred" response types, you must pass an interaction that has already been deferred.</param>
    /// <param name="ephemeral">
    /// Whether the response message should be ephemeral. Ignored if modifying a non-ephemeral message.<br/><br/>
    /// Ephemeral selections have several limitations:<br/>
    /// - <see cref="ActionOnStop.DeleteMessage"/> won't work (they cannot be deleted via through the API).<br/>
    /// - <see cref="InputType.Reactions"/> won't work (they can't have reactions).<br/><br/>
    /// Ephemeral selections require an interaction to be modified, which causes the following problems:<br/>
    /// - <see cref="BaseSelection{TOption}.ActionOnTimeout"/> won't work.<br/>
    /// - <see cref="BaseSelection{TOption}.ActionOnCancellation"/> won't work if the selection is cancelled using a <paramref name="cancellationToken"/>.<br/>
    /// </param>
    /// <param name="messageAction">A method that gets executed once when a message containing the selection is sent or modified.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the selection.</param>
    /// <returns>
    /// A task that represents the asynchronous operation for sending the selection and waiting for a valid input, a timeout or a cancellation.<br/>
    /// The task result contains an <see cref="InteractiveMessageResult{T}"/> with the selected value (if valid), the message used for the selection
    /// (which may not be valid if the message has been deleted), the elapsed time and the status.
    /// </returns>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="NotSupportedException"/>
    public async Task<InteractiveMessageResult<TOption?>> SendSelectionAsync<TOption>(BaseSelection<TOption> selection, IDiscordInteraction interaction,
        TimeSpan? timeout = null, InteractionResponseType responseType = InteractionResponseType.ChannelMessageWithSource, bool ephemeral = false,
        Action<IUserMessage>? messageAction = null, CancellationToken cancellationToken = default)
    {
        InteractiveGuards.NotNull(selection);
        InteractiveGuards.NotNull(interaction);
        InteractiveGuards.DeleteAndDisableInputNotSet(selection.ActionOnTimeout);
        InteractiveGuards.DeleteAndDisableInputNotSet(selection.ActionOnCancellation);
        InteractiveGuards.DeleteAndDisableInputNotSet(selection.ActionOnSuccess);
        InteractiveGuards.SupportedInputType(selection, ephemeral);
        InteractiveGuards.ValidResponseType(responseType);
        InteractiveGuards.NotCanceled(cancellationToken);

        var message = await SendOrModifyMessageAsync(selection, interaction, responseType, ephemeral).ConfigureAwait(false);
        messageAction?.Invoke(message);

        var timeoutTaskSource = new TimeoutTaskCompletionSource<(TOption?, InteractiveStatus)>(timeout ?? _config.DefaultTimeout,
            false, (default, InteractiveStatus.Timeout), (default, InteractiveStatus.Canceled), cancellationToken);

        var initialInteraction = responseType is InteractionResponseType.DeferredUpdateMessage or InteractionResponseType.UpdateMessage ? interaction : null;
        using var callback = new SelectionCallback<TOption>(selection, message, timeoutTaskSource, DateTimeOffset.UtcNow, initialInteraction);

        return await WaitForSelectionResultAsync(callback).ConfigureAwait(false);
    }

    private async Task<InteractiveMessageResult> SendPaginatorInternalAsync(Paginator paginator, IMessageChannel? channel, TimeSpan? timeout = null,
        IUserMessage? message = null, Action<IUserMessage>? messageAction = null, bool resetTimeoutOnInput = false, CancellationToken cancellationToken = default)
    {
        InteractiveGuards.NotNull(paginator);
        InteractiveGuards.MessageFromCurrentUser(_client, message);
        InteractiveGuards.DeleteAndDisableInputNotSet(paginator.ActionOnTimeout);
        InteractiveGuards.DeleteAndDisableInputNotSet(paginator.ActionOnCancellation);
        InteractiveGuards.SupportedInputType(paginator, false);
        InteractiveGuards.NotCanceled(cancellationToken);

        message = await SendOrModifyMessageAsync(paginator, message, channel).ConfigureAwait(false);
        messageAction?.Invoke(message);

        if (paginator.MaxPageIndex == 0)
        {
            return new InteractiveMessageResultBuilder()
                .WithMessage(message)
                .Build();
        }

        var timeoutTaskSource = new TimeoutTaskCompletionSource<InteractiveStatus>(timeout ?? _config.DefaultTimeout,
            resetTimeoutOnInput, InteractiveStatus.Timeout, InteractiveStatus.Canceled, cancellationToken);

        if (_config.ReturnAfterSendingPaginator)
        {
            _ = WaitForPaginatorResultUsingCallbackAsync().ConfigureAwait(false);

            return new InteractiveMessageResultBuilder()
                .WithMessage(message)
                .Build();
        }

        return await WaitForPaginatorResultUsingCallbackAsync().ConfigureAwait(false);

        async Task<InteractiveMessageResult> WaitForPaginatorResultUsingCallbackAsync()
        {
            using var callback = new PaginatorCallback(paginator, message, timeoutTaskSource, DateTimeOffset.UtcNow);
            return await WaitForPaginatorResultAsync(callback).ConfigureAwait(false);
        }
    }

    private async Task<InteractiveMessageResult<TOption?>> SendSelectionInternalAsync<TOption>(BaseSelection<TOption> selection, IMessageChannel? channel,
        TimeSpan? timeout = null, IUserMessage? message = null, Action<IUserMessage>? messageAction = null, CancellationToken cancellationToken = default)
    {
        InteractiveGuards.NotNull(selection);
        InteractiveGuards.MessageFromCurrentUser(_client, message);
        InteractiveGuards.DeleteAndDisableInputNotSet(selection.ActionOnTimeout);
        InteractiveGuards.DeleteAndDisableInputNotSet(selection.ActionOnCancellation);
        InteractiveGuards.DeleteAndDisableInputNotSet(selection.ActionOnSuccess);
        InteractiveGuards.SupportedInputType(selection, false);
        InteractiveGuards.NotCanceled(cancellationToken);

        message = await SendOrModifyMessageAsync(selection, message, channel).ConfigureAwait(false);
        messageAction?.Invoke(message);

        var timeoutTaskSource = new TimeoutTaskCompletionSource<(TOption?, InteractiveStatus)>(timeout ?? _config.DefaultTimeout,
            false, (default, InteractiveStatus.Timeout), (default, InteractiveStatus.Canceled), cancellationToken);

        using var callback = new SelectionCallback<TOption>(selection, message, timeoutTaskSource, DateTimeOffset.UtcNow);

        return await WaitForSelectionResultAsync(callback).ConfigureAwait(false);
    }

    private async Task<InteractiveResult<T?>> NextEntityAsync<T>(Func<T, bool>? filter = null, Func<T, bool, Task>? action = null,
        TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        InteractiveGuards.NotCanceled(cancellationToken);

        filter ??= _ => true;
        action ??= (_, _) => Task.CompletedTask;

        var guid = Guid.NewGuid();

        var timeoutTaskSource = new TimeoutTaskCompletionSource<(T?, InteractiveStatus)>(timeout ?? _config.DefaultTimeout,
            false, (default, InteractiveStatus.Timeout), (default, InteractiveStatus.Canceled), cancellationToken);

        var callback = new FilteredCallback<T>(filter, action, timeoutTaskSource, DateTimeOffset.UtcNow);

        _filteredCallbacks[guid] = callback;

        var (result, status) = await callback.TimeoutTaskSource.Task.ConfigureAwait(false);

        _filteredCallbacks.TryRemove(guid, out _);

        return new InteractiveResult<T?>(result, callback.GetElapsedTime(status), status);
    }

    private async Task<InteractiveMessageResult> WaitForPaginatorResultAsync(PaginatorCallback callback)
    {
        _callbacks[callback.Message.Id] = callback;
        bool hasJumpAction = callback.Paginator.Emotes.Values.Any(x => x == PaginatorAction.Jump);

        // A CancellationTokenSource is used here for 2 things:
        // 1. To stop WaitForMessagesAsync() to avoid memory leaks
        // 2. To cancel InitializeMessageAsync() to avoid adding reactions after TimeoutTaskSource.Task has returned.
        using var cts = callback.Paginator.InputType.HasFlag(InputType.Reactions) || hasJumpAction
            ? new CancellationTokenSource()
            : null;

        _ = callback.Paginator.InitializeMessageAsync(callback.Message, cts?.Token ?? default).ConfigureAwait(false);

        if (callback.Paginator.InputType.HasFlag(InputType.Reactions) && hasJumpAction)
        {
            _ = WaitForMessagesAsync().ConfigureAwait(false);
        }

        var status = await callback.TimeoutTaskSource.Task.ConfigureAwait(false);
        cts?.Cancel();
        cts?.Dispose();

        var result = InteractiveMessageResultBuilder.FromCallback(callback, status).Build();

        if (_callbacks.TryRemove(callback.Message.Id, out _))
        {
            await ApplyActionOnStopAsync(callback.Paginator, result, callback.LastInteraction, callback.StopInteraction, _config.DeferStopPaginatorInteractions).ConfigureAwait(false);
        }

        return result;

        async Task WaitForMessagesAsync()
        {
            if (cts is null)
            {
                return;
            }

            while (!cts.IsCancellationRequested)
            {
                var messageResult = await NextMessageAsync(msg => msg.Channel.Id == callback.Message.Channel.Id && msg.Source == MessageSource.User,
                    null, callback.TimeoutTaskSource.Delay, cts.Token).ConfigureAwait(false);
                if (messageResult.IsSuccess)
                {
                    await callback.ExecuteAsync(messageResult.Value).ConfigureAwait(false);
                }
            }
        }
    }

    private async Task<InteractiveMessageResult<TOption?>> WaitForSelectionResultAsync<TOption>(SelectionCallback<TOption> callback)
    {
        _callbacks[callback.Message.Id] = callback;

        // A CancellationTokenSource is used here for 2 things:
        // 1. To cancel NextMessageAsync() to avoid memory leaks
        // 2. To cancel InitializeMessageAsync() to avoid adding reactions after TimeoutTaskSource.Task has returned.
        using var cts = callback.Selection.InputType.HasFlag(InputType.Messages) || callback.Selection.InputType.HasFlag(InputType.Reactions)
            ? new CancellationTokenSource()
            : null;

        _ = callback.Selection.InitializeMessageAsync(callback.Message, cts?.Token ?? default).ConfigureAwait(false);

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
        cts?.Dispose();

        var result = InteractiveMessageResultBuilder<TOption?>.FromCallback(callback, selected, status).Build();

        if (_callbacks.TryRemove(callback.Message.Id, out _))
        {
            await ApplyActionOnStopAsync(callback.Selection, result, callback.LastInteraction, callback.StopInteraction, _config.DeferStopSelectionInteractions).ConfigureAwait(false);
        }

        return result;
    }

    private static async Task<IUserMessage> SendOrModifyMessageAsync<TOption>(IInteractiveElement<TOption> element,
        IUserMessage? message, IMessageChannel? channel)
    {
        var page = await element.GetCurrentPageAsync().ConfigureAwait(false);

        MessageComponent? component = null;
        bool moreThanOnePage = element is not Paginator pag || pag.MaxPageIndex > 0;
        if ((element.InputType.HasFlag(InputType.Buttons) || element.InputType.HasFlag(InputType.SelectMenus)) && moreThanOnePage)
        {
            component = element.GetOrAddComponents(false).Build();
        }

        if (message is not null)
        {
            await message.ModifyAsync(x =>
            {
                x.Content = page.Text;
                x.Embeds = page.GetEmbedArray();
                x.Components = component;
                x.AllowedMentions = page.AllowedMentions;
            }).ConfigureAwait(false);
        }
        else
        {
            InteractiveGuards.NotNull(channel);
            message = await channel!.SendMessageAsync(page.Text, page.IsTTS, null, null, page.AllowedMentions, page.MessageReference, component, page.Stickers.ToArray(), page.GetEmbedArray()).ConfigureAwait(false);
        }

        return message;
    }

    private static async Task<IUserMessage> SendOrModifyMessageAsync<TOption>(IInteractiveElement<TOption> element, IDiscordInteraction interaction,
        InteractionResponseType responseType, bool ephemeral)
    {
        var page = await element.GetCurrentPageAsync().ConfigureAwait(false);

        MessageComponent? component = null;
        bool moreThanOnePage = element is not Paginator pag || pag.MaxPageIndex > 0;
        if ((element.InputType.HasFlag(InputType.Buttons) || element.InputType.HasFlag(InputType.SelectMenus)) && moreThanOnePage)
        {
            component = element.GetOrAddComponents(false).Build();
        }

        var embeds = page.GetEmbedArray();

        switch (responseType)
        {
            case InteractionResponseType.ChannelMessageWithSource:
                await interaction.RespondAsync(page.Text, embeds, page.IsTTS, ephemeral, page.AllowedMentions, component).ConfigureAwait(false);
                return await interaction.GetOriginalResponseAsync().ConfigureAwait(false);

            case InteractionResponseType.DeferredChannelMessageWithSource:
                return await interaction.FollowupAsync(page.Text, embeds, page.IsTTS, ephemeral, page.AllowedMentions, component).ConfigureAwait(false);

            case InteractionResponseType.DeferredUpdateMessage:
                InteractiveGuards.ValidResponseType(responseType, interaction);
                return await interaction.ModifyOriginalResponseAsync(UpdateMessage).ConfigureAwait(false);

            case InteractionResponseType.UpdateMessage:
                InteractiveGuards.ValidResponseType(responseType, interaction);
                await ((IComponentInteraction)interaction).UpdateAsync(UpdateMessage).ConfigureAwait(false);
                return await interaction.GetOriginalResponseAsync().ConfigureAwait(false);

            default:
                throw new ArgumentException("Unknown interaction response type.", nameof(responseType));
        }

        void UpdateMessage(MessageProperties props)
        {
            props.Content = page.Text;
            props.Embeds = embeds;
            props.Components = component;
            props.AllowedMentions = page.AllowedMentions;
        }
    }

    private static async Task ApplyActionOnStopAsync<TOption>(IInteractiveElement<TOption> element, IInteractiveMessageResult result,
        IDiscordInteraction? lastInteraction, SocketMessageComponent? stopInteraction, bool deferInteraction)
    {
        bool ephemeral = result.Message.Flags.GetValueOrDefault().HasFlag(MessageFlags.Ephemeral);

        var action = result.Status switch
        {
            InteractiveStatus.Timeout => element.ActionOnTimeout,
            InteractiveStatus.Canceled => element.ActionOnCancellation,
            InteractiveStatus.Success when element is BaseSelection<TOption> selection => selection.ActionOnSuccess,
            InteractiveStatus.Unknown => throw new ArgumentException("Unknown action.", nameof(result)),
            _ => throw new ArgumentException("Unknown action.", nameof(result))
        };

        if (action == ActionOnStop.None)
        {
            if (deferInteraction && stopInteraction is not null)
            {
                await stopInteraction.DeferAsync().ConfigureAwait(false);
            }

            return;
        }

        if (action.HasFlag(ActionOnStop.DeleteMessage))
        {
            // Ephemeral messages cannot be deleted through the API
            // https://github.com/discord/discord-api-docs/discussions/3806
            if (!ephemeral)
            {
                try
                {
                    await result.Message.DeleteAsync().ConfigureAwait(false);
                }
                catch (HttpException e) when (e.HttpCode == HttpStatusCode.NotFound)
                {
                    // We want to delete the message so we don't care if the message has been already deleted.
                }
            }
            else if (deferInteraction && stopInteraction is not null)
            {
                await stopInteraction.DeferAsync().ConfigureAwait(false);
            }

            return;
        }

        IPage? page = null;
        if (action.HasFlag(ActionOnStop.ModifyMessage))
        {
            page = result.Status switch
            {
                InteractiveStatus.Timeout => element.TimeoutPage,
                InteractiveStatus.Canceled => element.CanceledPage,
                InteractiveStatus.Success when element is BaseSelection<TOption> selection => selection.SuccessPage,
                _ => throw new ArgumentException("Unknown action.", nameof(result))
            };
        }

        MessageComponent? components = null;
        if (element.InputType.HasFlag(InputType.Buttons) || element.InputType.HasFlag(InputType.SelectMenus))
        {
            if (action.HasFlag(ActionOnStop.DisableInput))
            {
                components = element.GetOrAddComponents(true).Build();
            }
            else if (action.HasFlag(ActionOnStop.DeleteInput))
            {
                components = new ComponentBuilder().Build();
            }
        }

        bool modifyMessage = page?.Text is not null || page?.Embeds.Count > 0 || components is not null;

        if (modifyMessage)
        {
            try
            {
                if (stopInteraction is not null)
                {
                    // An interaction to stop the element has been received
                    await stopInteraction.UpdateAsync(UpdateMessage).ConfigureAwait(false);
                }
                else if (lastInteraction is not null && (DateTimeOffset.UtcNow - lastInteraction.CreatedAt).TotalMinutes <= 15.0)
                {
                    // The element is from a message that was updated using an interaction, and its token is still valid
                    await lastInteraction.ModifyOriginalResponseAsync(UpdateMessage).ConfigureAwait(false);
                }
                else if (!ephemeral)
                {
                    // Fallback for normal messages that don't use interactions or the token is no longer valid, only works for non-ephemeral messages
                    await result.Message.ModifyAsync(UpdateMessage).ConfigureAwait(false);
                }
            }
            catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.UnknownMessage)
            {
                // Ignore 10008 (Unknown Message) error.
            }
        }
        else if (deferInteraction && stopInteraction is not null)
        {
            await stopInteraction.DeferAsync().ConfigureAwait(false);
        }

        if (action.HasFlag(ActionOnStop.DeleteInput) && element.InputType.HasFlag(InputType.Reactions))
        {
            Debug.Assert(!ephemeral, "Ephemeral messages cannot have InputType.Reactions");

            bool manageMessages = await result.Message.Channel.CurrentUserHasManageMessagesAsync().ConfigureAwait(false);

            if (manageMessages)
            {
                await result.Message.RemoveAllReactionsAsync().ConfigureAwait(false);
            }
        }

        void UpdateMessage(MessageProperties props)
        {
            props.Content = page?.Text ?? new Optional<string>();
            props.Embeds = page?.GetEmbedArray() ?? new Optional<Embed[]>();
            props.Components = components ?? new Optional<MessageComponent>();
            props.AllowedMentions = page?.AllowedMentions ?? new Optional<AllowedMentions>();
        }
    }

    private Task MessageReceived(SocketMessage message)
    {
        if (message.Author.Id == _client.CurrentUser.Id)
        {
            return Task.CompletedTask;
        }

        foreach (var pair in _filteredCallbacks)
        {
            if (pair.Value is FilteredCallback<SocketMessage> filteredCallback)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await filteredCallback.ExecuteAsync(message).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        LogError("InteractiveService", "Failed to execute filtered message callback", ex);
                    }
                });
            }
        }

        return Task.CompletedTask;
    }

    private Task ReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
    {
        if (reaction.UserId != _client.CurrentUser.Id
            && _callbacks.TryGetValue(reaction.MessageId, out var callback))
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await callback.ExecuteAsync(reaction).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    LogError("InteractiveService", $"Failed to execute reaction callback (message Id: {reaction.MessageId})", ex);
                }
            });
        }

        foreach (var pair in _filteredCallbacks)
        {
            if (pair.Value is FilteredCallback<SocketReaction> filteredCallback)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await filteredCallback.ExecuteAsync(reaction).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        LogError("InteractiveService", "Failed to execute filtered reaction callback", ex);
                    }
                });
            }
        }

        return Task.CompletedTask;
    }

    private Task InteractionCreated(SocketInteraction interaction)
    {
        ulong messageId = 0;

        if (interaction is SocketMessageComponent componentInteraction
            && _callbacks.TryGetValue(componentInteraction.Message.Id, out var callback)
            || interaction is SocketModal modal
            && ulong.TryParse(modal.Data.CustomId, out messageId)
            && _callbacks.TryGetValue(messageId, out callback))
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await callback.ExecuteAsync(interaction).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    LogError("InteractiveService", $"Failed to execute interaction callback (message Id: {(interaction as SocketMessageComponent)?.Message?.Id ?? messageId})", ex);
                }
            });
        }

        foreach (var pair in _filteredCallbacks)
        {
            // Ugly but works
            if (pair.Value is FilteredCallback<SocketInteraction> or FilteredCallback<SocketMessageComponent>
                or FilteredCallback<SocketSlashCommand> or FilteredCallback<SocketUserCommand>
                or FilteredCallback<SocketMessageCommand> or FilteredCallback<SocketAutocompleteInteraction>)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await pair.Value.ExecuteAsync(interaction).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        LogError("InteractiveService", "Failed to execute filtered interaction callback", ex);
                    }
                });
            }
        }

        return Task.CompletedTask;
    }

    private void LogError(string source, string message, Exception? exception = null)
        => Log(new LogMessage(LogSeverity.Error, source, message, exception));

    private Task LogMessage(LogMessage message) =>
        _config.LogLevel >= message.Severity
            ? Task.FromResult(message)
            : Task.CompletedTask;
}