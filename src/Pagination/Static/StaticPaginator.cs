using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace Fergun.Interactive.Pagination
{
    /// <summary>
    /// Represents a static paginator.
    /// </summary>
    public sealed class StaticPaginator : Paginator
    {
        /// <summary>
        /// Gets the pages of the <see cref="Paginator"/>.
        /// </summary>
        public IReadOnlyCollection<IPage> Pages { get; }

        /// <summary>
        /// Gets the maximum page index of the <see cref="Paginator"/>.
        /// </summary>
        public override int MaxPageIndex => Pages.Count - 1;

        internal StaticPaginator(IReadOnlyCollection<IUser> users, IReadOnlyDictionary<IEmote, PaginatorAction> emotes,
            IPage? canceledPage, IPage? timeoutPage, DeletionOptions deletion, InputType inputType,
            ActionOnStop actionOnCancellation, ActionOnStop actionOnTimeout, IReadOnlyCollection<IPage> pages, int startPageIndex)
            : base(users, emotes, canceledPage, timeoutPage, deletion, inputType, actionOnCancellation, actionOnTimeout, startPageIndex)
        {
            if (pages is null)
            {
                throw new ArgumentNullException(nameof(pages));
            }

            if (pages.Count == 0)
            {
                throw new ArgumentException("A paginator needs at least one page.", nameof(pages));
            }

            Pages = pages;
        }

        /// <inheritdoc/>
        public override Task<IPage> GetOrLoadPageAsync(int pageIndex)
            => Task.FromResult(Pages.ElementAt(pageIndex));
    }
}