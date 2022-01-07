using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Fergun.Interactive.Extensions;

namespace Fergun.Interactive.Pagination
{
    /// <summary>
    /// Represents an abstract immutable paginator.
    /// </summary>
    public abstract class Paginator : IInteractiveElement<KeyValuePair<IEmote, PaginatorAction>>
    {
        /// <summary>
        /// Gets whether this paginator is restricted to <see cref="Users"/>.
        /// </summary>
        public bool IsUserRestricted => Users.Count > 0;

        /// <summary>
        /// Gets the index of the current page of this paginator.
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

        /// <inheritdoc/>
        IReadOnlyCollection<KeyValuePair<IEmote, PaginatorAction>> IInteractiveElement<KeyValuePair<IEmote, PaginatorAction>>.Options => Emotes;

        /// <summary>
        /// Initializes a new instance of the <see cref="Paginator"/> class.
        /// </summary>
        /// <param name="builder">The builder to copy the properties from.</param>
        protected Paginator(PaginatorBuilderProperties builder)
        {
            InteractiveGuards.NotNull(builder, nameof(builder));
            InteractiveGuards.NotNull(builder.Users, nameof(builder.Users));
            InteractiveGuards.NotNull(builder.Options, nameof(builder.Options));
            InteractiveGuards.NotEmpty(builder.Options, nameof(builder.Options));
            InteractiveGuards.SupportedInputType(builder.InputType, false);

            Users = builder.Users.ToArray();
            Emotes = builder.Options.AsReadOnly();
            CanceledPage = builder.CanceledPage?.Build();
            TimeoutPage = builder.TimeoutPage?.Build();
            Deletion = builder.Deletion;
            InputType = builder.InputType;
            ActionOnCancellation = builder.ActionOnCancellation;
            ActionOnTimeout = builder.ActionOnTimeout;
            CurrentPageIndex = builder.StartPageIndex;
        }

        /// <summary>
        /// Initializes a message based on this paginator.
        /// </summary>
        /// <remarks>By default this method adds the reactions to a message when <see cref="InputType"/> has <see cref="InputType.Reactions"/>.</remarks>
        /// <param name="message">The message to initialize.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel this request.</param>
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
        /// <param name="action">The paginator action</param>
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

        /// <inheritdoc/>
        public virtual MessageComponent BuildComponents(bool disableAll)
        {
            var builder = new ComponentBuilder();
            foreach (var pair in Emotes)
            {
                bool isDisabled = disableAll || pair.Value switch
                {
                    PaginatorAction.SkipToStart => CurrentPageIndex == 0,
                    PaginatorAction.Backward => CurrentPageIndex == 0,
                    PaginatorAction.Forward => CurrentPageIndex == MaxPageIndex,
                    PaginatorAction.SkipToEnd => CurrentPageIndex == MaxPageIndex,
                    _ => false
                };

                var button = new ButtonBuilder()
                    .WithCustomId(pair.Key.ToString())
                    .WithStyle(pair.Value == PaginatorAction.Exit ? ButtonStyle.Danger : ButtonStyle.Primary)
                    .WithEmote(pair.Key)
                    .WithDisabled(isDisabled);

                builder.WithButton(button);
            }

            return builder.Build();
        }

        /// <inheritdoc cref="IInteractiveInputHandler.HandleMessageAsync(IMessage, IUserMessage)"/>
        /// <remarks>If this method is not overriden, it will throw a <see cref="NotSupportedException"/> since paginators don't support message input by default.</remarks>
        public virtual Task<InteractiveInputResult> HandleMessageAsync(IMessage input, IUserMessage message)
            => throw new NotSupportedException("Cannot handle a message input.");

        /// <inheritdoc cref="IInteractiveInputHandler.HandleReactionAsync"/>
        public virtual async Task<InteractiveInputResult> HandleReactionAsync(SocketReaction input, IUserMessage message)
        {
            InteractiveGuards.NotNull(input, nameof(input));
            InteractiveGuards.NotNull(message, nameof(message));

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

            bool refreshPage = await ApplyActionAsync(action).ConfigureAwait(false);
            if (refreshPage)
            {
                var currentPage = await GetOrLoadCurrentPageAsync().ConfigureAwait(false);
                await message.ModifyAsync(x =>
                {
                    x.Embeds = currentPage.GetEmbedArray();
                    x.Content = currentPage.Text;
                }).ConfigureAwait(false);
            }

            return InteractiveInputStatus.Success;
        }

        /// <inheritdoc cref="IInteractiveInputHandler.HandleInteractionAsync"/>
        public virtual async Task<InteractiveInputResult> HandleInteractionAsync(SocketInteraction input, IUserMessage message)
        {
            InteractiveGuards.NotNull(input, nameof(input));
            InteractiveGuards.NotNull(message, nameof(message));

            if (!InputType.HasFlag(InputType.Buttons) || input is not SocketMessageComponent interaction)
            {
                return new(InteractiveInputStatus.Ignored);
            }

            if (interaction.Message.Id != message.Id || !this.CanInteract(interaction.User))
            {
                return new(InteractiveInputStatus.Ignored);
            }

            var emote = (interaction
                    .Message
                    .Components
                    .FirstOrDefault()?
                    .Components?
                    .FirstOrDefault(x => x is ButtonComponent button && button.CustomId == interaction.Data.CustomId) as ButtonComponent)?
                .Emote;

            if (emote is null || !Emotes.TryGetValue(emote, out var action))
            {
                return InteractiveInputStatus.Ignored;
            }

            if (action == PaginatorAction.Exit)
            {
                return InteractiveInputStatus.Canceled;
            }

            bool refreshPage = await ApplyActionAsync(action).ConfigureAwait(false);
            if (refreshPage)
            {
                var currentPage = await GetOrLoadCurrentPageAsync().ConfigureAwait(false);
                var buttons = BuildComponents(false);

                await interaction.UpdateAsync(x =>
                {
                    x.Content = currentPage.Text ?? ""; // workaround for d.net bug
                    x.Embeds = currentPage.GetEmbedArray();
                    x.Components = buttons;
                }).ConfigureAwait(false);
            }

            return InteractiveInputStatus.Success;
        }

        /// <inheritdoc />
        async Task<IInteractiveResult<InteractiveInputStatus>> IInteractiveInputHandler.HandleMessageAsync(IMessage input, IUserMessage message)
            => await HandleMessageAsync(input, message).ConfigureAwait(false);

        /// <inheritdoc />
        async Task<IInteractiveResult<InteractiveInputStatus>> IInteractiveInputHandler.HandleReactionAsync(IReaction input, IUserMessage message)
        {
            InteractiveGuards.ExpectedType<IReaction, SocketReaction>(input, nameof(input), out var socketReaction);
            return await HandleReactionAsync(socketReaction, message).ConfigureAwait(false);
        }

        /// <inheritdoc />
        async Task<IInteractiveResult<InteractiveInputStatus>> IInteractiveInputHandler.HandleInteractionAsync(IDiscordInteraction input, IUserMessage message)
        {
            InteractiveGuards.ExpectedType<IDiscordInteraction, SocketInteraction>(input, nameof(input), out var socketInteraction);
            return await HandleInteractionAsync(socketInteraction, message).ConfigureAwait(false);
        }
    }
}