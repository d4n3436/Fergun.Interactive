using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;

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
        protected Paginator(IReadOnlyCollection<IUser> users, IReadOnlyDictionary<IEmote, PaginatorAction> emotes,
            IPage? canceledPage, IPage? timeoutPage, DeletionOptions deletion, InputType inputType,
            ActionOnStop actionOnCancellation, ActionOnStop actionOnTimeout, int startPageIndex)
        {
            Users = users ?? throw new ArgumentNullException(nameof(users));

            if (inputType == 0)
            {
                throw new ArgumentException("At least one input type must be set.", nameof(inputType));
            }

            if (emotes is null)
            {
                throw new ArgumentNullException(nameof(emotes));
            }

            if (emotes.Count == 0)
            {
                throw new ArgumentException($"{nameof(emotes)} must contain at least one element.", nameof(emotes));
            }

            Emotes = emotes;
            CanceledPage = canceledPage;
            TimeoutPage = timeoutPage;
            Deletion = deletion;
            InputType = inputType;
            ActionOnCancellation = actionOnCancellation;
            ActionOnTimeout = actionOnTimeout;
            CurrentPageIndex = startPageIndex;
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
    }
}