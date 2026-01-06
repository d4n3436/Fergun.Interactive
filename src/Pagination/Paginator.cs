using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fergun.Interactive.Extensions;
using JetBrains.Annotations;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents an abstract immutable paginator.
/// </summary>
[PublicAPI]
public abstract class Paginator : IInteractiveElement<KeyValuePair<EmojiProperties, PaginatorAction>>
{
    private readonly Lazy<string> _lazyJumpInputTextLabel;
    private readonly Lazy<string> _lazyInvalidJumpInputMessage;
    private readonly Lazy<TimeoutTaskCompletionSource<Message?>> _lazyMessageTcs;
    private readonly Lazy<TimeoutTaskCompletionSource<ModalInteraction?>> _lazyModalTcs;
    private readonly Lock _waitLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Paginator"/> class.
    /// </summary>
    /// <param name="properties">The builder properties to copy from.</param>
    protected Paginator(IBasePaginatorBuilderProperties properties)
    {
        ArgumentNullException.ThrowIfNull(properties);
        ArgumentNullException.ThrowIfNull(properties.Users);
        ArgumentNullException.ThrowIfNull(properties.Options);
        ArgumentNullException.ThrowIfNull(properties.ButtonFactories);
#pragma warning disable CS0618 // Type or member is obsolete
        ArgumentNullException.ThrowIfNull(properties.SelectMenuFactories);
#pragma warning restore CS0618 // Type or member is obsolete
        InteractiveGuards.NotEmpty(properties.ButtonFactories);
        InteractiveGuards.SupportedInputType(properties.InputType, ephemeral: false);

        if (properties.InputType.HasFlag(InputType.Reactions))
        {
            InteractiveGuards.NotEmpty(properties.Options);
        }

        if (properties.RestrictedInputBehavior == RestrictedInputBehavior.SendMessage)
        {
            ArgumentNullException.ThrowIfNull(properties.RestrictedPageFactory);
        }

        Users = properties.Users.ToArray().AsReadOnly();
        Emotes = properties.Options.ToDictionary().AsReadOnly();
        ButtonFactories = properties.ButtonFactories.ToArray().AsReadOnly();
#pragma warning disable CS0618 // Type or member is obsolete
        SelectMenuFactories = properties.SelectMenuFactories.ToArray().AsReadOnly();
#pragma warning restore CS0618 // Type or member is obsolete
        CanceledPage = properties.CanceledPage?.Build();
        TimeoutPage = properties.TimeoutPage?.Build();
        RestrictedPage = properties.RestrictedPageFactory?.Invoke(Users);
        Deletion = properties.Deletion;
        InputType = properties.InputType;
        ActionOnCancellation = properties.ActionOnCancellation;
        ActionOnTimeout = properties.ActionOnTimeout;
        RestrictedInputBehavior = properties.RestrictedInputBehavior;
        CurrentPageIndex = properties.StartPageIndex;
        JumpInputTimeout = properties.JumpInputTimeout;
        JumpInputPrompt = properties.JumpInputPrompt ?? "Enter a page number";
        JumpInputInUseMessage = properties.JumpInputInUseMessage ?? "Another user is currently using this action. Try again later.";
        ExpiredJumpInputMessage = properties.ExpiredJumpInputMessage ?? $"Expired modal interaction. You must respond within {JumpInputTimeout.TotalSeconds} seconds.";

        _lazyJumpInputTextLabel = new Lazy<string>(() => properties.JumpInputTextLabel ?? $"Page number (1-{MaxPageIndex + 1})");
        _lazyInvalidJumpInputMessage = new Lazy<string>(() => properties.InvalidJumpInputMessage ?? $"Invalid input. The number must be in the range of 1 to {MaxPageIndex + 1}, excluding the current page.");
        _lazyMessageTcs = new Lazy<TimeoutTaskCompletionSource<Message?>>(() => new TimeoutTaskCompletionSource<Message?>(JumpInputTimeout));
        _lazyModalTcs = new Lazy<TimeoutTaskCompletionSource<ModalInteraction?>>(() => new TimeoutTaskCompletionSource<ModalInteraction?>(JumpInputTimeout));
    }

    /// <inheritdoc/>
    IReadOnlyCollection<KeyValuePair<EmojiProperties, PaginatorAction>> IInteractiveElement<KeyValuePair<EmojiProperties, PaginatorAction>>.Options => Emotes;

    /// <summary>
    /// Gets a value indicating whether this paginator is restricted to <see cref="Users"/>.
    /// </summary>
    public bool IsUserRestricted => Users.Count > 0;

    /// <summary>
    /// Gets or sets the index of the current page of this paginator.
    /// </summary>
    public int CurrentPageIndex { get; protected set; }

    /// <summary>
    /// Gets the maximum page index of this paginator.
    /// </summary>
    public abstract int MaxPageIndex { get; }

    /// <summary>
    /// Gets a read-only collection of users who can interact with this paginator.
    /// </summary>
    public IReadOnlyCollection<User> Users { get; }

    /// <summary>
    /// Gets the emotes and their related actions of this paginator.
    /// </summary>
    /// <remarks>This property has been replaced by <see cref="ButtonFactories"/> and it shouldn't be used on button-based paginators.</remarks>
    public IReadOnlyDictionary<EmojiProperties, PaginatorAction> Emotes { get; }

    /// <summary>
    /// Gets the button factories.
    /// </summary>
    public IReadOnlyList<Func<IButtonContext, IPaginatorButton>> ButtonFactories { get; }

    /// <summary>
    /// Gets the select menu factories.
    /// </summary>
    /// <remarks>Paginator select menus are detached from the paginator and their interactions must be manually handled.</remarks>
    [Obsolete("Paginator select menus are obsolete and its functionality has been replaced by component paginators, which offer better control of select menus.")]
    public IReadOnlyList<Func<ISelectMenuContext, IPaginatorSelectMenu>> SelectMenuFactories { get; }

    /// <inheritdoc/>
    public IPage? CanceledPage { get; }

    /// <inheritdoc/>
    public IPage? TimeoutPage { get; }

    /// <summary>
    /// Gets the <see cref="IPage"/> that will be displayed ephemerally to a user when they are not allowed to interact with this paginator.
    /// </summary>
    public IPage? RestrictedPage { get; }

    /// <summary>
    /// Gets what type of inputs this paginator should delete.
    /// </summary>
    /// <remarks>This property is ignored on button-based paginators.</remarks>
    public DeletionOptions Deletion { get; }

    /// <summary>
    /// Gets the input type, that is, what the paginator uses to change pages.
    /// </summary>
    public InputType InputType { get; }

    /// <summary>
    /// Gets the action that will be done after a cancellation.
    /// </summary>
    public ActionOnStop ActionOnCancellation { get; }

    /// <summary>
    /// Gets the action that will be done after a timeout.
    /// </summary>
    public ActionOnStop ActionOnTimeout { get; }

    /// <summary>
    /// Gets the behavior the paginator should exhibit when a user is not allowed to interact with it.
    /// </summary>
    public RestrictedInputBehavior RestrictedInputBehavior { get; }

    /// <summary>
    /// Gets the maximum time to wait for a "jump to page" input.
    /// </summary>
    public TimeSpan JumpInputTimeout { get; }

    /// <summary>
    /// Gets the "jump to page" prompt that is displayed to the user.
    /// </summary>
    /// <remarks>
    /// In button inputs, this is the title of the modal that is displayed.<br/>
    /// In reaction inputs, this is the content of the temporary message that is sent.
    /// </remarks>
    public string JumpInputPrompt { get; }

    /// <summary>
    /// Gets the "jump to page" text label that is displayed in the modal.
    /// </summary>
    public string JumpInputTextLabel => _lazyJumpInputTextLabel.Value;

    /// <summary>
    /// Gets the message to display when receiving an invalid "jump to page" input.
    /// </summary>
    /// <remarks>An invalid input may be one that isn't a number, or a number that is outside the valid range.</remarks>
    public string InvalidJumpInputMessage => _lazyInvalidJumpInputMessage.Value;

    /// <summary>
    /// Gets the message to display when a user attempts to use the "jump to page" action while other user is using it.
    /// </summary>
    public string JumpInputInUseMessage { get; }

    /// <summary>
    /// Gets the message to display when receiving an expired "jump to page" input.
    /// </summary>
    public string ExpiredJumpInputMessage { get; }

    /// <summary>
    /// Gets or sets the ID of the user that is currently using the "jump to page" action.
    /// </summary>
    /// <remarks>If the value is <c>0</c>, then no one is currently using the action.</remarks>
    protected ulong JumpInputUserId { get; set; }

    /// <summary>
    /// Gets the <see cref="TimeoutTaskCompletionSource{TResult}"/> used to receive modal interactions in <see cref="JumpToPageAsync(MessageReactionAddEventArgs, RestMessage)"/>.
    /// </summary>
    protected TimeoutTaskCompletionSource<Message?> MessageTaskCompletionSource => _lazyMessageTcs.Value;

    /// <summary>
    /// Gets the <see cref="TimeoutTaskCompletionSource{TResult}"/> used to receive modal interactions in <see cref="JumpToPageAsync(MessageComponentInteraction)"/>.
    /// </summary>
    protected TimeoutTaskCompletionSource<ModalInteraction?> ModalTaskCompletionSource => _lazyModalTcs.Value;

    /// <summary>
    /// Sets the <see cref="CurrentPageIndex"/> of this paginator.
    /// </summary>
    /// <param name="pageIndex">The index of the page to set.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains whether the operation succeeded.</returns>
    public virtual async ValueTask<bool> SetPageAsync(int pageIndex)
    {
        if (pageIndex < 0 || CurrentPageIndex == pageIndex || pageIndex > MaxPageIndex)
        {
            return false;
        }

        var page = await GetOrLoadPageAsync(pageIndex).ConfigureAwait(false);

        if (page is null)
        {
            return false;
        }

        CurrentPageIndex = pageIndex;

        return true;
    }

    /// <summary>
    /// Gets or loads a specific page of this paginator.
    /// </summary>
    /// <param name="pageIndex">The index of the page to get or load.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the requested page.</returns>
    public abstract Task<IPage> GetOrLoadPageAsync(int pageIndex);

    /// <summary>
    /// Gets or loads the current page of this paginator.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The task result contains the current page.</returns>
    public virtual Task<IPage> GetOrLoadCurrentPageAsync()
        => GetOrLoadPageAsync(CurrentPageIndex);

    /// <summary>
    /// Applies a <see cref="PaginatorAction"/> to this paginator.
    /// </summary>
    /// <param name="action">The paginator action.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains whether the action succeeded.</returns>
    public virtual ValueTask<bool> ApplyActionAsync(PaginatorAction action)
        => action switch
        {
            PaginatorAction.Backward => SetPageAsync(CurrentPageIndex - 1),
            PaginatorAction.Forward => SetPageAsync(CurrentPageIndex + 1),
            PaginatorAction.SkipToStart => SetPageAsync(0),
            PaginatorAction.SkipToEnd => SetPageAsync(MaxPageIndex),
            _ => new ValueTask<bool>(result: false)
        };

    /// <summary>
    /// Responds <paramref name="reaction"/> with a message, waits for a message with a valid page number and jumps (skips) to that page.
    /// </summary>
    /// <param name="reaction">The reaction.</param>
    /// <param name="message"></param>
    /// <returns>A task representing the asynchronous operation. The task result contains whether the action succeeded.</returns>
    public virtual async ValueTask<bool> JumpToPageAsync(MessageReactionAddEventArgs reaction, RestMessage message)
    {
        ArgumentNullException.ThrowIfNull(reaction);

        if (JumpInputUserId != 0)
        {
            // The user pressed the "jump to page" reaction again
            if (JumpInputUserId == reaction.UserId)
            {
                // Do nothing
                return false;
            }

            if (!string.IsNullOrEmpty(JumpInputInUseMessage))
            {
                var props = new ReplyMessageProperties()
                    .WithContent($"{reaction.UserId.Mention}, {JumpInputInUseMessage}")
                    .WithAllowedMentions(AllowedMentionsProperties.None);

                await message.ReplyAsync(props).ConfigureAwait(false);
            }

            return false;
        }

        var promptProperties = new ReplyMessageProperties()
            .WithContent($"{reaction.UserId.Mention}, {JumpInputPrompt}")
            .WithAllowedMentions(AllowedMentionsProperties.None);

        var promptMessage = await message.ReplyAsync(promptProperties).ConfigureAwait(false);

        lock (_waitLock)
        {
            JumpInputUserId = reaction.UserId;
        }

        MessageTaskCompletionSource.ResetTaskSource();
        MessageTaskCompletionSource.TryReset();
        var response = await MessageTaskCompletionSource.Task.ConfigureAwait(false);

        lock (_waitLock)
        {
            JumpInputUserId = 0;
        }

        try
        {
            await promptMessage.DeleteAsync().ConfigureAwait(false);
        }
        catch (RestException ex) when (ex.Error?.Code == 10008)
        {
            // We want to delete the message so we don't care if the message has been already deleted.
        }

        if (response is null)
            return false;

        string? rawInput = response.Content;
        if (rawInput is null || !int.TryParse(rawInput, NumberStyles.Integer, CultureInfo.InvariantCulture, out int pageNumber) || !await SetPageAsync(pageNumber - 1).ConfigureAwait(false))
        {
            if (!string.IsNullOrEmpty(InvalidJumpInputMessage))
            {
                var props = new ReplyMessageProperties()
                    .WithContent(InvalidJumpInputMessage)
                    .WithAllowedMentions(AllowedMentionsProperties.None);

                await response.ReplyAsync(props).ConfigureAwait(false);
            }

            return false;
        }

        return true;
    }

    /// <summary>
    /// Responds <paramref name="interaction"/> with a modal, waits for a valid page number and jumps (skips) to that page.
    /// </summary>
    /// <param name="interaction">The component interaction.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains whether the action succeeded.</returns>
    public virtual async ValueTask<bool> JumpToPageAsync(MessageComponentInteraction interaction)
    {
        ArgumentNullException.ThrowIfNull(interaction);

        if (JumpInputUserId != 0)
        {
            // The user canceled the modal and then pressed the "jump to page" button again
            if (JumpInputUserId == interaction.User.Id)
            {
                ModalTaskCompletionSource.TryCancel();
            }
            else
            {
                // We don't know if the user is viewing the modal, or they just dismissed it, the former is assumed
                if (!string.IsNullOrEmpty(JumpInputInUseMessage))
                {
                    var props = new InteractionMessageProperties()
                        .WithContent(JumpInputInUseMessage)
                        .WithFlags(MessageFlags.Ephemeral);

                    await interaction.SendResponseAsync(InteractionCallback.Message(props)).ConfigureAwait(false);
                }
                else
                {
                    await interaction.SendResponseAsync(InteractionCallback.DeferredModifyMessage).ConfigureAwait(false);
                }

                return false;
            }
        }

        var label = new LabelProperties(JumpInputTextLabel, new TextInputProperties("text_input", TextInputStyle.Short)
        {
            MinLength = 1,
            MaxLength = (int)Math.Floor(Math.Log10(MaxPageIndex + 1) + 1),
            Required = true
        });

        var modal = new ModalProperties(interaction.Message.Id.ToString(), JumpInputPrompt)
            .AddComponents(label);

        await interaction.SendResponseAsync(InteractionCallback.Modal(modal)).ConfigureAwait(false);

        lock (_waitLock)
        {
            JumpInputUserId = interaction.User.Id;
        }

        ModalTaskCompletionSource.ResetTaskSource();
        ModalTaskCompletionSource.TryReset();
        var res = await ModalTaskCompletionSource.Task.ConfigureAwait(false);

        lock (_waitLock)
        {
            JumpInputUserId = 0;
        }

        if (res is null)
            return false;

        string? rawInput = res.Data.Components
            .OfType<Label>()
            .Select(x => x.Component)
            .OfType<TextInput>()
            .FirstOrDefault(x => x.CustomId == "text_input")?
            .Value;

        if (rawInput is null || !int.TryParse(rawInput, NumberStyles.Integer, CultureInfo.InvariantCulture, out int pageNumber) || !await SetPageAsync(pageNumber - 1).ConfigureAwait(false))
        {
            if (!string.IsNullOrEmpty(InvalidJumpInputMessage))
            {
                var props = new InteractionMessageProperties()
                    .WithContent(InvalidJumpInputMessage)
                    .WithFlags(MessageFlags.Ephemeral);

                await res.SendResponseAsync(InteractionCallback.Message(props)).ConfigureAwait(false);
            }
            else
            {
                await res.SendResponseAsync(InteractionCallback.DeferredModifyMessage).ConfigureAwait(false);
            }

            return false;
        }

        await res.SendResponseAsync(InteractionCallback.DeferredModifyMessage).ConfigureAwait(false);

        return true;
    }

    /// <inheritdoc/>
    public virtual List<IMessageComponentProperties> GetOrAddComponents(bool disableAll, List<IMessageComponentProperties>? builder = null)
    {
        builder ??= [];

        var buttons = new List<ButtonProperties>();
        for (int i = 0; i < ButtonFactories.Count; i++)
        {
            var context = new ButtonContext(i, CurrentPageIndex, MaxPageIndex, disableAll);
            var properties = ButtonFactories[i].Invoke(context);

            if (properties?.IsHidden is not false)
                continue;

            var style = properties.Style ?? (properties.Action == PaginatorAction.Exit ? ButtonStyle.Danger : ButtonStyle.Primary);
            var button = new ButtonProperties(string.IsNullOrEmpty(properties.CustomId) ? $"{i}_{(int)properties.Action}" : properties.CustomId, properties.Text!, properties.Emote!, style)
                .WithDisabled(properties.IsDisabled ?? context.ShouldDisable(properties.Action));

            buttons.Add(button);
        }

        builder.AddRange(buttons.Chunk(5).Select(x => new ActionRowProperties(x)));

#pragma warning disable CS0618 // Type or member is obsolete
        for (int i = 0; i < SelectMenuFactories.Count; i++)
        {
            var context = new SelectMenuContext(i, CurrentPageIndex, MaxPageIndex, disableAll);
            var properties = SelectMenuFactories[i]?.Invoke(context);

            if (properties?.IsHidden is not false)
                continue;

            var selectMenu = new StringMenuProperties(properties.CustomId).WithOptions(properties.Options).WithPlaceholder(properties.Placeholder).WithMaxValues(properties.MaxValues)
                .WithMinValues(properties.MinValues).WithDisabled(properties.IsDisabled ?? context.ShouldDisable());

            builder.Add(selectMenu);
        }
#pragma warning restore CS0618 // Type or member is obsolete

        return builder;
    }

    /// <inheritdoc cref="IInteractiveInputHandler.HandleMessageAsync(Message, RestMessage)"/>
    /// <remarks>By default, paginators only accept a message input for the "jump to page" action.</remarks>
    public virtual Task<InteractiveInputResult> HandleMessageAsync(Message input, RestMessage message)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(message);

        if (!this.CanInteract(input.Author) || JumpInputUserId == 0)
        {
            return Task.FromResult(new InteractiveInputResult(InteractiveInputStatus.Ignored));
        }

        MessageTaskCompletionSource.TrySetResult(input);
        return Task.FromResult(new InteractiveInputResult(InteractiveInputStatus.Ignored)); // ignore this because we only handle messages for the "jump to page" action.
    }

    /// <inheritdoc cref="IInteractiveInputHandler.HandleReactionAsync"/>
    public virtual async Task<InteractiveInputResult> HandleReactionAsync(MessageReactionAddEventArgs input, RestMessage message)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(message);

        if (!InputType.HasFlag(InputType.Reactions) || input.MessageId != message.Id)
        {
            return InteractiveInputStatus.Ignored;
        }

        bool valid = Emotes.TryGetValue(input.Emoji.ToEmojiProperties(), out var action)
                     && this.CanInteract(input.UserId);

        if (!valid)
        {
            return InteractiveInputStatus.Ignored;
        }

        if (action == PaginatorAction.Exit)
        {
            return InteractiveInputStatus.Canceled;
        }

        int previousPageIndex = CurrentPageIndex;

        if ((action == PaginatorAction.Jump && await JumpToPageAsync(input, message).ConfigureAwait(false)) || await ApplyActionAsync(action).ConfigureAwait(false))
        {
            await TryUpdateMessageAsync(message.ModifyAsync, previousPageIndex, includeComponents: false).ConfigureAwait(false);
        }

        return InteractiveInputStatus.Success;
    }

    /// <inheritdoc cref="IInteractiveInputHandler.HandleInteractionAsync"/>
    public virtual async Task<InteractiveInputResult> HandleInteractionAsync(MessageComponentInteraction input, RestMessage message)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(message);

        if (!InputType.HasFlag(InputType.Buttons) || input.Message.Id != message.Id)
        {
            return InteractiveInputStatus.Ignored;
        }

        if (!this.CanInteract(input.User))
        {
            return RestrictedInputBehavior switch
            {
                RestrictedInputBehavior.Ignore or RestrictedInputBehavior.Auto when RestrictedPage is null => InteractiveInputStatus.Ignored,
                RestrictedInputBehavior.SendMessage or RestrictedInputBehavior.Auto when RestrictedPage is not null => await SendRestrictedMessageAsync(input).ConfigureAwait(false),
                RestrictedInputBehavior.Defer => await DeferInteractionAsync(input).ConfigureAwait(false),
                _ => InteractiveInputStatus.Ignored
            };
        }

        if (!TryGetAction(input, out var action))
        {
            return InteractiveInputStatus.Ignored;
        }

        if (action == PaginatorAction.Exit)
        {
            return InteractiveInputStatus.Canceled;
        }

        int previousPageIndex = CurrentPageIndex;

        Func<Action<MessageOptions>, RestRequestProperties?, CancellationToken, Task> updateMethod;
        if (action == PaginatorAction.Jump && await JumpToPageAsync(input).ConfigureAwait(false))
            updateMethod = input.ModifyResponseAsync;
        else if (await ApplyActionAsync(action).ConfigureAwait(false))
            updateMethod = SendResponseAsync;
        else return InteractiveInputStatus.Success;

        await TryUpdateMessageAsync(updateMethod, previousPageIndex).ConfigureAwait(false);

        return InteractiveInputStatus.Success;

        
        async Task SendResponseAsync(Action<MessageOptions> messageAction, RestRequestProperties? properties, CancellationToken cancellationToken)
            => await input.SendResponseAsync(InteractionCallback.ModifyMessage(messageAction), false, properties, cancellationToken);
    }

    /// <summary>
    /// Handles a modal interaction.
    /// </summary>
    /// <param name="input">The modal interaction to handle.</param>
    /// <param name="message">The message containing the interactive element.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing the result.</returns>
    public virtual async ValueTask<InteractiveInputResult> HandleModalAsync(ModalInteraction input, RestMessage message)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(message);

        if (!ulong.TryParse(input.Data.CustomId, out ulong messageId) || messageId != message.Id || !this.CanInteract(input.User))
        {
            return new InteractiveInputResult(InteractiveInputStatus.Ignored);
        }

        // Expired modal
        if (JumpInputUserId == 0)
        {
            if (!string.IsNullOrEmpty(ExpiredJumpInputMessage))
            {
                var props = new InteractionMessageProperties()
                    .WithContent(ExpiredJumpInputMessage)
                    .WithFlags(MessageFlags.Ephemeral);

                await input.SendResponseAsync(InteractionCallback.Message(props)).ConfigureAwait(false);
            }
            else
            {
                await input.SendResponseAsync(InteractionCallback.DeferredModifyMessage).ConfigureAwait(false);
            }

            return new InteractiveInputResult(InteractiveInputStatus.Ignored);
        }

        ModalTaskCompletionSource.TrySetResult(input);
        return new InteractiveInputResult(InteractiveInputStatus.Success);
    }

    /// <inheritdoc />
    async Task<IInteractiveResult<InteractiveInputStatus>> IInteractiveInputHandler.HandleMessageAsync(Message input, RestMessage message)
        => await HandleMessageAsync(input, message).ConfigureAwait(false);

    /// <inheritdoc />
    async Task<IInteractiveResult<InteractiveInputStatus>> IInteractiveInputHandler.HandleReactionAsync(MessageReactionAddEventArgs input, RestMessage message)
    {
        return await HandleReactionAsync(input, message).ConfigureAwait(false);
    }

    /// <inheritdoc />
    async Task<IInteractiveResult<InteractiveInputStatus>> IInteractiveInputHandler.HandleInteractionAsync(MessageComponentInteraction input, RestMessage message)
    {
        return await HandleInteractionAsync(input, message).ConfigureAwait(false);
    }

    /// <summary>
    /// Initializes a message based on this paginator.
    /// </summary>
    /// <remarks>By default, this method adds the reactions to a message when <see cref="InputType"/> has <see cref="InputType.Reactions"/>.</remarks>
    /// <param name="message">The message to initialize.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel this request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    internal virtual async Task InitializeMessageAsync(RestMessage message, CancellationToken cancellationToken = default)
    {
        if (!InputType.HasFlag(InputType.Reactions)) return;

        foreach (var emote in Emotes.Keys)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await message.AddReactionAsync(emote.ToReactionEmojiProperties(), null, cancellationToken).ConfigureAwait(false);
        }
    }

    internal static bool TryGetAction(MessageComponentInteraction input, out PaginatorAction action)
    {
        // Get last character of custom ID, convert it to a number and cast it to PaginatorAction
        action = (PaginatorAction)(input.Data.CustomId?[^1] - '0' ?? -1);

        return Enum.IsDefined(action);
    }

    private static async Task<InteractiveInputResult> DeferInteractionAsync(MessageComponentInteraction input)
    {
        await input.SendResponseAsync(InteractionCallback.DeferredModifyMessage).ConfigureAwait(false);
        return InteractiveInputStatus.Success;
    }

    // Attempts to update the message and reverts the page index to the previous one if an exception occurs
    private async Task TryUpdateMessageAsync(Func<Action<MessageOptions>, RestRequestProperties?, CancellationToken, Task> updateMethod, int previousPageIndex, bool includeComponents = true)
    {
        var currentPage = await GetOrLoadCurrentPageAsync().ConfigureAwait(false);
        var attachments = currentPage.AttachmentsFactory is null ? null : await currentPage.AttachmentsFactory().ConfigureAwait(false);
        var components = GetOrAddComponents(disableAll: false);

        try
        {
            await updateMethod(x =>
            {
                x.Content = currentPage.Text;
                x.Embeds = currentPage.Embeds;
                x.Components = includeComponents ? components : null;
                x.AllowedMentions = currentPage.AllowedMentions;
                x.Attachments = attachments;
                x.Flags = currentPage.MessageFlags;
            }, null, CancellationToken.None).ConfigureAwait(false);
        }
        catch
        {
            CurrentPageIndex = previousPageIndex;
            throw; // InteractiveService will handle and log the exception
        }
    }

    private async Task<InteractiveInputResult> SendRestrictedMessageAsync(MessageComponentInteraction input)
    {
        var page = RestrictedPage ?? throw new InvalidOperationException($"Expected {nameof(RestrictedPage)} to be non-null.");
        var attachments = page.AttachmentsFactory is null ? null : await page.AttachmentsFactory().ConfigureAwait(false);

        var message = new InteractionMessageProperties
        {
            Content = page.Text,
            Tts = page.IsTTS,
            Embeds = page.Embeds,
            Components = GetOrAddComponents(false),
            AllowedMentions = page.AllowedMentions,
            Attachments = attachments,
            Flags = page.MessageFlags
        };

        await input.SendResponseAsync(InteractionCallback.Message(message)).ConfigureAwait(false);

        return InteractiveInputStatus.Success;
    }
}