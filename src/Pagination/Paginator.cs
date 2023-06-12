using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Fergun.Interactive.Extensions;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents an abstract immutable paginator.
/// </summary>
public abstract class Paginator : IInteractiveElement<KeyValuePair<IEmote, PaginatorAction>>
{
    private readonly Lazy<string> _lazyJumpInputTextLabel;
    private readonly Lazy<string> _lazyInvalidJumpInputMessage;
    private readonly Lazy<TimeoutTaskCompletionSource<IMessage?>> _lazyMessageTcs;
    private readonly Lazy<TimeoutTaskCompletionSource<IModalInteraction?>> _lazyModalTcs;
    private readonly object _waitLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Paginator"/> class.
    /// </summary>
    /// <param name="properties">The builder properties to copy from.</param>
    protected Paginator(IBasePaginatorBuilderProperties properties)
    {
        InteractiveGuards.NotNull(properties);
        InteractiveGuards.NotNull(properties.Users);
        InteractiveGuards.NotNull(properties.Options);
        InteractiveGuards.NotNull(properties.ButtonFactories);
        InteractiveGuards.NotEmpty(properties.ButtonFactories);
        InteractiveGuards.SupportedInputType(properties.InputType, false);

        if (properties.InputType.HasFlag(InputType.Reactions))
        {
            InteractiveGuards.NotEmpty(properties.Options);
        }

        Users = properties.Users.ToArray();
        Emotes = properties.Options.AsReadOnly();
        ButtonFactories = new ReadOnlyCollection<Func<IButtonContext, IPaginatorButton>>(properties.ButtonFactories);
        CanceledPage = properties.CanceledPage?.Build();
        TimeoutPage = properties.TimeoutPage?.Build();
        Deletion = properties.Deletion;
        InputType = properties.InputType;
        ActionOnCancellation = properties.ActionOnCancellation;
        ActionOnTimeout = properties.ActionOnTimeout;
        CurrentPageIndex = properties.StartPageIndex;
        JumpInputTimeout = properties.JumpInputTimeout;
        JumpInputPrompt = properties.JumpInputPrompt ?? "Enter a page number";
        JumpInputInUseMessage = properties.JumpInputInUseMessage ?? "Another user is currently using this action. Try again later.";
        ExpiredJumpInputMessage = properties.ExpiredJumpInputMessage ?? $"Expired modal interaction. You must respond within {JumpInputTimeout.TotalSeconds} seconds.";

        _lazyJumpInputTextLabel = new Lazy<string>(() => properties.JumpInputTextLabel ?? $"Page number (1-{MaxPageIndex + 1})");
        _lazyInvalidJumpInputMessage = new Lazy<string>(() => properties.InvalidJumpInputMessage ?? $"Invalid input. The number must be in the range of 1 to {MaxPageIndex + 1}, excluding the current page.");
        _lazyMessageTcs = new Lazy<TimeoutTaskCompletionSource<IMessage?>>(() => new TimeoutTaskCompletionSource<IMessage?>(JumpInputTimeout));
        _lazyModalTcs = new Lazy<TimeoutTaskCompletionSource<IModalInteraction?>>(() => new TimeoutTaskCompletionSource<IModalInteraction?>(JumpInputTimeout));
    }

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
    public IReadOnlyCollection<IUser> Users { get; }

    /// <summary>
    /// Gets the emotes and their related actions of this paginator.
    /// </summary>
    public IReadOnlyDictionary<IEmote, PaginatorAction> Emotes { get; }

    /// <summary>
    /// Gets the button factories.
    /// </summary>
    /// <remarks>This property is only used when <see cref="InputType"/> contains <see cref="Fergun.Interactive.InputType.Buttons"/>.</remarks>
    public IReadOnlyList<Func<IButtonContext, IPaginatorButton>> ButtonFactories { get; }

    /// <inheritdoc/>
    public IPage? CanceledPage { get; }

    /// <inheritdoc/>
    public IPage? TimeoutPage { get; }

    /// <summary>
    /// Gets what type of inputs this paginator should delete.
    /// </summary>
    /// <remarks>This property is ignored in button-based paginators.</remarks>
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
    /// Gets the <see cref="TimeoutTaskCompletionSource{TResult}"/> used to receive modal interactions in <see cref="JumpToPageAsync(SocketReaction)"/>.
    /// </summary>
    protected TimeoutTaskCompletionSource<IMessage?> MessageTaskCompletionSource => _lazyMessageTcs.Value;

    /// <summary>
    /// Gets the <see cref="TimeoutTaskCompletionSource{TResult}"/> used to receive modal interactions in <see cref="JumpToPageAsync(SocketMessageComponent)"/>.
    /// </summary>
    protected TimeoutTaskCompletionSource<IModalInteraction?> ModalTaskCompletionSource => _lazyModalTcs.Value;

    /// <inheritdoc/>
    IReadOnlyCollection<KeyValuePair<IEmote, PaginatorAction>> IInteractiveElement<KeyValuePair<IEmote, PaginatorAction>>.Options => Emotes;

    /// <summary>
    /// Sets the <see cref="CurrentPageIndex"/> of this paginator.
    /// </summary>
    /// <param name="pageIndex">The index of the page to set.</param>
    /// <returns>A task representing the asynchronous operation. The result contains whether the operation succeeded.</returns>
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
    public virtual ValueTask<bool> ApplyActionAsync(PaginatorAction action) =>
        action switch
        {
            PaginatorAction.Backward => SetPageAsync(CurrentPageIndex - 1),
            PaginatorAction.Forward => SetPageAsync(CurrentPageIndex + 1),
            PaginatorAction.SkipToStart => SetPageAsync(0),
            PaginatorAction.SkipToEnd => SetPageAsync(MaxPageIndex),
            _ => new ValueTask<bool>(false)
        };

    /// <summary>
    /// Responds <paramref name="reaction"/> with a message, waits for a message with a valid page number and jumps (skips) to that page.
    /// </summary>
    /// <param name="reaction">The reaction.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains whether the action succeeded.</returns>
    public virtual async ValueTask<bool> JumpToPageAsync(SocketReaction reaction)
    {
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
                await reaction.Channel.SendMessageAsync($"{MentionUtils.MentionUser(reaction.UserId)}, {JumpInputInUseMessage}",
                    allowedMentions: AllowedMentions.None, messageReference: new MessageReference(reaction.MessageId)).ConfigureAwait(false);
            }

            return false;
        }

        var promptMessage = await reaction.Channel.SendMessageAsync($"{MentionUtils.MentionUser(reaction.UserId)}, {JumpInputPrompt}", allowedMentions: AllowedMentions.None, messageReference: new MessageReference(reaction.MessageId)).ConfigureAwait(false);

        lock (_waitLock)
        {
            JumpInputUserId = reaction.UserId;
        }

        MessageTaskCompletionSource.ResetTaskSource();
        MessageTaskCompletionSource.TryReset();
        var message = await MessageTaskCompletionSource.Task.ConfigureAwait(false);

        lock (_waitLock)
        {
            JumpInputUserId = 0;
        }

        try
        {
            await promptMessage.DeleteAsync().ConfigureAwait(false);
        }
        catch
        {
            // We want to delete the message so we don't care if the message has been already deleted.
        }

        if (message is null)
            return false;

        string? rawInput = message.Content;
        if (rawInput is null || !int.TryParse(rawInput, out int pageNumber) || !await SetPageAsync(pageNumber - 1).ConfigureAwait(false))
        {
            if (!string.IsNullOrEmpty(InvalidJumpInputMessage))
            {
                await message.Channel.SendMessageAsync(InvalidJumpInputMessage, allowedMentions: AllowedMentions.None,
                    messageReference: new MessageReference(message.Id)).ConfigureAwait(false);
            }

            return false;
        }

        bool manageMessages = await message.Channel.CurrentUserHasManageMessagesAsync().ConfigureAwait(false);

        if (manageMessages && Deletion.HasFlag(DeletionOptions.Valid))
        {
            await message.DeleteAsync().ConfigureAwait(false);
        }

        return true;
    }

    /// <summary>
    /// Responds <paramref name="interaction"/> with a modal, waits for a valid page number and jumps (skips) to that page.
    /// </summary>
    /// <param name="interaction">The component interaction.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains whether the action succeeded.</returns>
    public virtual async ValueTask<bool> JumpToPageAsync(SocketMessageComponent interaction)
    {
        if (JumpInputUserId != 0)
        {
            // The user canceled the modal and then pressed the "jump to page" button again
            if (JumpInputUserId == interaction.User.Id)
            {
                ModalTaskCompletionSource.TryCancel();
            }
            else
            {
                // We don't know if the user is viewing the modal or they just dismissed it, the former is assumed
                if (!string.IsNullOrEmpty(JumpInputInUseMessage))
                {
                    await interaction.RespondAsync(JumpInputInUseMessage, ephemeral: true).ConfigureAwait(false);
                }
                else
                {
                    await interaction.DeferAsync().ConfigureAwait(false);
                }

                return false;
            }
        }

        var modal = new ModalBuilder()
            .WithCustomId(interaction.Message.Id.ToString())
            .WithTitle(JumpInputPrompt)
            .AddTextInput(JumpInputTextLabel, "text_input", minLength: 1, maxLength: (int)Math.Floor(Math.Log10(MaxPageIndex + 1) + 1))
            .Build();

        await interaction.RespondWithModalAsync(modal).ConfigureAwait(false);

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

        string? rawInput = res.Data.Components.FirstOrDefault(x => x.CustomId == "text_input")?.Value;
        if (rawInput is null || !int.TryParse(rawInput, out int pageNumber) || !await SetPageAsync(pageNumber - 1).ConfigureAwait(false))
        {
            if (!string.IsNullOrEmpty(InvalidJumpInputMessage))
            {
                await res.RespondAsync(InvalidJumpInputMessage, ephemeral: true).ConfigureAwait(false);
            }
            else
            {
                await res.DeferAsync().ConfigureAwait(false);
            }

            return false;
        }

        await res.DeferAsync().ConfigureAwait(false);

        return true;
    }

    /// <inheritdoc/>
    public virtual ComponentBuilder GetOrAddComponents(bool disableAll, ComponentBuilder? builder = null)
    {
        builder ??= new ComponentBuilder();
        for (int i = 0; i < ButtonFactories.Count; i++)
        {
            var context = new ButtonContext(i, CurrentPageIndex, MaxPageIndex, disableAll);
            var properties = ButtonFactories[i].Invoke(context);

            if (properties is null || properties.IsHidden)
                continue;

            var style = properties.Style ?? (properties.Action == PaginatorAction.Exit ? ButtonStyle.Danger : ButtonStyle.Primary);
            var button = new ButtonBuilder();

            if (style == ButtonStyle.Link)
            {
                button.WithUrl(properties.Url);
            }
            else
            {
                button.WithCustomId(string.IsNullOrEmpty(properties.CustomId) ? $"{i}_{(int)properties.Action}" : properties.CustomId);
            }

            button.WithStyle(style)
                .WithEmote(properties.Emote)
                .WithDisabled(properties.IsDisabled ?? context.ShouldDisable(properties.Action));

            if (!string.IsNullOrEmpty(properties.Text))
                button.WithLabel(properties.Text);

            builder.WithButton(button);
        }

        return builder;
    }

    /// <inheritdoc cref="IInteractiveInputHandler.HandleMessageAsync(IMessage, IUserMessage)"/>
    /// <remarks>By default, paginators only accept a message input for the "jump to page" action.</remarks>
    public virtual Task<InteractiveInputResult> HandleMessageAsync(IMessage input, IUserMessage message)
    {
        InteractiveGuards.NotNull(input);
        InteractiveGuards.NotNull(message);

        if (!this.CanInteract(input.Author))
        {
            return Task.FromResult(new InteractiveInputResult(InteractiveInputStatus.Ignored));
        }

        if (JumpInputUserId == 0)
        {
            return Task.FromResult(new InteractiveInputResult(InteractiveInputStatus.Ignored));
        }

        MessageTaskCompletionSource.TrySetResult(input);
        return Task.FromResult(new InteractiveInputResult(InteractiveInputStatus.Ignored)); // ignore this because we only handle messages for the "jump to page" action.
    }

    /// <inheritdoc cref="IInteractiveInputHandler.HandleReactionAsync"/>
    public virtual async Task<InteractiveInputResult> HandleReactionAsync(SocketReaction input, IUserMessage message)
    {
        InteractiveGuards.NotNull(input);
        InteractiveGuards.NotNull(message);

        if (!InputType.HasFlag(InputType.Reactions) || input.MessageId != message.Id)
        {
            return InteractiveInputStatus.Ignored;
        }

        bool valid = Emotes.TryGetValue(input.Emote, out var action)
                     && this.CanInteract(input.UserId);

        bool manageMessages = await message.Channel.CurrentUserHasManageMessagesAsync().ConfigureAwait(false);

        if (manageMessages)
        {
            switch (valid)
            {
                case false when Deletion.HasFlag(DeletionOptions.Invalid):
                case true when Deletion.HasFlag(DeletionOptions.Valid):
                    await message.RemoveReactionAsync(input.Emote, input.UserId).ConfigureAwait(false);
                    break;
            }
        }

        if (!valid)
        {
            return InteractiveInputStatus.Ignored;
        }

        if (action == PaginatorAction.Exit)
        {
            return InteractiveInputStatus.Canceled;
        }

        if (action == PaginatorAction.Jump && await JumpToPageAsync(input).ConfigureAwait(false))
        {
            var currentPage = await GetOrLoadCurrentPageAsync().ConfigureAwait(false);
            var attachments = currentPage.AttachmentsFactory is null ? null : await currentPage.AttachmentsFactory().ConfigureAwait(false);

            await message.ModifyAsync(x =>
            {
                x.Embeds = currentPage.GetEmbedArray();
                x.Content = currentPage.Text;
                x.AllowedMentions = currentPage.AllowedMentions;
                x.Attachments = attachments is null ? new Optional<IEnumerable<FileAttachment>>() : new Optional<IEnumerable<FileAttachment>>(attachments);
            }).ConfigureAwait(false);
        }

        bool refreshPage = await ApplyActionAsync(action).ConfigureAwait(false);
        if (refreshPage)
        {
            var currentPage = await GetOrLoadCurrentPageAsync().ConfigureAwait(false);
            var attachments = currentPage.AttachmentsFactory is null ? null : await currentPage.AttachmentsFactory().ConfigureAwait(false);

            await message.ModifyAsync(x =>
            {
                x.Embeds = currentPage.GetEmbedArray();
                x.Content = currentPage.Text;
                x.AllowedMentions = currentPage.AllowedMentions;
                x.Attachments = attachments is null ? new Optional<IEnumerable<FileAttachment>>() : new Optional<IEnumerable<FileAttachment>>(attachments);
            }).ConfigureAwait(false);
        }

        return InteractiveInputStatus.Success;
    }

    /// <inheritdoc cref="IInteractiveInputHandler.HandleInteractionAsync"/>
    public virtual async Task<InteractiveInputResult> HandleInteractionAsync(SocketMessageComponent input, IUserMessage message)
    {
        InteractiveGuards.NotNull(input);
        InteractiveGuards.NotNull(message);

        if (!InputType.HasFlag(InputType.Buttons))
        {
            return new(InteractiveInputStatus.Ignored);
        }

        if (input.Message.Id != message.Id || !this.CanInteract(input.User))
        {
            return new(InteractiveInputStatus.Ignored);
        }

        // Get last character of custom Id, convert it to a number and cast it to PaginatorAction
        var action = (PaginatorAction)(input.Data.CustomId?[^1] - '0' ?? -1);
        if (!Enum.IsDefined(typeof(PaginatorAction), action))
        {
            // Old way to get the action for backward compatibility
            var emote = (input
                    .Message
                    .Components
                    .SelectMany(x => x.Components)
                    .FirstOrDefault(x => x is ButtonComponent button && button.CustomId == input.Data.CustomId) as ButtonComponent)?
                .Emote;

            if (emote is null || !Emotes.TryGetValue(emote, out action))
            {
                return InteractiveInputStatus.Ignored;
            }
        }

        if (action == PaginatorAction.Exit)
        {
            return InteractiveInputStatus.Canceled;
        }

        if (action == PaginatorAction.Jump && await JumpToPageAsync(input).ConfigureAwait(false))
        {
            var currentPage = await GetOrLoadCurrentPageAsync().ConfigureAwait(false);
            var attachments = currentPage.AttachmentsFactory is null ? null : await currentPage.AttachmentsFactory().ConfigureAwait(false);
            var buttons = GetOrAddComponents(false).Build();

            await input.ModifyOriginalResponseAsync(x =>
            {
                x.Content = currentPage.Text ?? ""; // workaround for d.net bug
                x.Embeds = currentPage.GetEmbedArray();
                x.Components = buttons;
                x.AllowedMentions = currentPage.AllowedMentions;
                x.Attachments = attachments is null ? new Optional<IEnumerable<FileAttachment>>() : new Optional<IEnumerable<FileAttachment>>(attachments);
            }).ConfigureAwait(false);
        }

        bool refreshPage = await ApplyActionAsync(action).ConfigureAwait(false);
        if (refreshPage)
        {
            var currentPage = await GetOrLoadCurrentPageAsync().ConfigureAwait(false);
            var attachments = currentPage.AttachmentsFactory is null ? null : await currentPage.AttachmentsFactory().ConfigureAwait(false);
            var buttons = GetOrAddComponents(false).Build();

            await input.UpdateAsync(x =>
            {
                x.Content = currentPage.Text ?? ""; // workaround for d.net bug
                x.Embeds = currentPage.GetEmbedArray();
                x.Components = buttons;
                x.AllowedMentions = currentPage.AllowedMentions;
                x.Attachments = attachments is null ? new Optional<IEnumerable<FileAttachment>>() : new Optional<IEnumerable<FileAttachment>>(attachments);
            }).ConfigureAwait(false);
        }

        return InteractiveInputStatus.Success;
    }

    /// <summary>
    /// Handles a modal interaction.
    /// </summary>
    /// <param name="input">The modal interaction to handle.</param>
    /// <param name="message">The message containing the interactive element.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing the result.</returns>
    public virtual async ValueTask<InteractiveInputResult> HandleModalAsync(IModalInteraction input, IUserMessage message)
    {
        InteractiveGuards.NotNull(input);
        InteractiveGuards.NotNull(message);

        if (!ulong.TryParse(input.Data.CustomId, out var messageId) || messageId != message.Id || !this.CanInteract(input.User))
        {
            return new(InteractiveInputStatus.Ignored);
        }

        // Expired modal
        if (JumpInputUserId == 0)
        {
            if (!string.IsNullOrEmpty(ExpiredJumpInputMessage))
            {
                await input.RespondAsync(ExpiredJumpInputMessage, ephemeral: true).ConfigureAwait(false);
            }
            else
            {
                await input.DeferAsync().ConfigureAwait(false);
            }

            return new(InteractiveInputStatus.Ignored);
        }

        ModalTaskCompletionSource.TrySetResult(input);
        return new(InteractiveInputStatus.Success);
    }

    /// <inheritdoc />
    async Task<IInteractiveResult<InteractiveInputStatus>> IInteractiveInputHandler.HandleMessageAsync(IMessage input, IUserMessage message)
        => await HandleMessageAsync(input, message).ConfigureAwait(false);

    /// <inheritdoc />
    async Task<IInteractiveResult<InteractiveInputStatus>> IInteractiveInputHandler.HandleReactionAsync(IReaction input, IUserMessage message)
    {
        InteractiveGuards.ExpectedType<IReaction, SocketReaction>(input, out var socketReaction);
        return await HandleReactionAsync(socketReaction, message).ConfigureAwait(false);
    }

    /// <inheritdoc />
    async Task<IInteractiveResult<InteractiveInputStatus>> IInteractiveInputHandler.HandleInteractionAsync(IComponentInteraction input, IUserMessage message)
    {
        InteractiveGuards.ExpectedType<IComponentInteraction, SocketMessageComponent>(input, out var socketMessageComponent);
        return await HandleInteractionAsync(socketMessageComponent, message).ConfigureAwait(false);
    }

    /// <summary>
    /// Initializes a message based on this paginator.
    /// </summary>
    /// <remarks>By default this method adds the reactions to a message when <see cref="InputType"/> has <see cref="InputType.Reactions"/>.</remarks>
    /// <param name="message">The message to initialize.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel this request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    internal virtual async Task InitializeMessageAsync(IUserMessage message, CancellationToken cancellationToken = default)
    {
        if (!InputType.HasFlag(InputType.Reactions)) return;

        foreach (var emote in Emotes.Keys)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await message.AddReactionAsync(emote).ConfigureAwait(false);
        }
    }
}