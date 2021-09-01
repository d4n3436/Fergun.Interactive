using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Discord;

namespace Fergun.Interactive.Pagination
{
    /// <summary>
    /// Represents a builder class for making a <see cref="StaticPaginator"/>.
    /// </summary>
    public sealed class StaticPaginatorBuilder : PaginatorBuilder<StaticPaginator, StaticPaginatorBuilder>
    {
        /// <summary>
        /// Gets or sets the pages of the <see cref="Paginator"/>.
        /// </summary>
        public IList<PageBuilder> Pages { get; set; } = new List<PageBuilder>();

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticPaginatorBuilder"/> class.
        /// </summary>
        public StaticPaginatorBuilder()
        {
        }

        /// <inheritdoc/>
        public override StaticPaginator Build(int startPageIndex = 0)
        {
            if (Options.Count == 0)
            {
                WithDefaultEmotes();
            }

            return new StaticPaginator(
                Users.ToArray(),
                new ReadOnlyDictionary<IEmote, PaginatorAction>(Options), // TODO: Find a way to create an ImmutableDictionary without getting the contents reordered.
                CanceledPage?.Build(),
                TimeoutPage?.Build(),
                Deletion,
                InputType,
                ActionOnCancellation,
                ActionOnTimeout,
                Pages.Select((x, i) => x.WithPaginatorFooter(Footer, i, Pages.Count - 1, Users).Build()).ToArray(),
                startPageIndex);
        }

        /// <summary>
        /// Sets the pages of the paginator.
        /// </summary>
        /// <param name="pages">The pages.</param>
        /// <returns>This builder.</returns>
        public StaticPaginatorBuilder WithPages(params PageBuilder[] pages)
        {
            Pages = pages?.ToList() ?? throw new ArgumentNullException(nameof(pages));
            return this;
        }

        /// <summary>
        /// Sets the pages of the paginator.
        /// </summary>
        /// <param name="pages">The pages.</param>
        /// <returns>This builder.</returns>
        public StaticPaginatorBuilder WithPages(IEnumerable<PageBuilder> pages)
        {
            Pages = pages?.ToList() ?? throw new ArgumentNullException(nameof(pages));
            return this;
        }

        /// <summary>
        /// Adds a page to the paginator.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns>This builder.</returns>
        public StaticPaginatorBuilder AddPage(PageBuilder page)
        {
            Pages.Add(page ?? throw new ArgumentNullException(nameof(page)));
            return this;
        }
    }
}