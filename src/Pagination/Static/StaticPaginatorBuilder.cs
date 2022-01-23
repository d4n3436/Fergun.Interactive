using System;
using System.Collections.Generic;
using System.Linq;

namespace Fergun.Interactive.Pagination
{
    /// <summary>
    /// Represents a builder class for making a <see cref="StaticPaginator"/>.
    /// </summary>
    public sealed class StaticPaginatorBuilder : PaginatorBuilder<StaticPaginator, StaticPaginatorBuilder>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StaticPaginatorBuilder"/> class.
        /// </summary>
        public StaticPaginatorBuilder()
        {
        }

        /// <summary>
        /// Gets or sets the pages of the paginator.
        /// </summary>
        public IList<IPageBuilder> Pages { get; set; } = new List<IPageBuilder>();

        /// <inheritdoc/>
        public override StaticPaginator Build()
        {
            if (Options.Count == 0)
            {
                WithDefaultEmotes();
            }

            return new StaticPaginator(this);
        }

        /// <summary>
        /// Sets the pages of the paginator.
        /// </summary>
        /// <param name="pages">The pages.</param>
        /// <returns>This builder.</returns>
        public StaticPaginatorBuilder WithPages(params IPageBuilder[] pages)
        {
            InteractiveGuards.NotNull(pages, nameof(pages));
            Pages = pages.ToList();
            return this;
        }

        /// <summary>
        /// Sets the pages of the paginator.
        /// </summary>
        /// <param name="pages">The pages.</param>
        /// <returns>This builder.</returns>
        public StaticPaginatorBuilder WithPages(IEnumerable<IPageBuilder> pages)
        {
            InteractiveGuards.NotNull(pages, nameof(pages));
            Pages = pages.ToList();
            return this;
        }

        /// <summary>
        /// Adds a page to the paginator.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns>This builder.</returns>
        public StaticPaginatorBuilder AddPage(IPageBuilder page)
        {
            InteractiveGuards.NotNull(page, nameof(page));
            Pages.Add(page);
            return this;
        }
    }
}