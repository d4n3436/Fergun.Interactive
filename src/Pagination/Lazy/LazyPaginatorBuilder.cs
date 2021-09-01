using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace Fergun.Interactive.Pagination
{
    /// <summary>
    /// Represents a builder class for making a <see cref="LazyPaginator"/>.
    /// </summary>
    public sealed class LazyPaginatorBuilder : PaginatorBuilder<LazyPaginator, LazyPaginatorBuilder>
    {
        /// <summary>
        /// Gets or sets the method used to load the pages of the paginator lazily.
        /// </summary>
        public Func<int, Task<PageBuilder>> PageFactory { get; set; } = null!;

        /// <summary>
        /// Gets or sets the maximum page index of the paginator.
        /// </summary>
        public int MaxPageIndex { get; set; }

        /// <summary>
        /// Gets or sets whether to cache loaded pages. The default value is <see langword="true"/>.
        /// </summary>
        public bool CacheLoadedPages { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyPaginatorBuilder"/> class.
        /// </summary>
        public LazyPaginatorBuilder()
        {
        }

        /// <inheritdoc/>
        public override LazyPaginator Build(int startPageIndex = 0)
        {
            if (Options.Count == 0)
            {
                WithDefaultEmotes();
            }

            return new LazyPaginator(
                Users.ToArray(),
                new ReadOnlyDictionary<IEmote, PaginatorAction>(Options), // TODO: Find a way to create an ImmutableDictionary without getting the contents reordered.
                CanceledPage?.Build(),
                TimeoutPage?.Build(),
                Deletion,
                InputType,
                ActionOnCancellation,
                ActionOnTimeout,
                PageFactory != null! ? AddPaginatorFooterAsync : null!,
                startPageIndex,
                MaxPageIndex,
                CacheLoadedPages);

            async Task<Page> AddPaginatorFooterAsync(int page)
            {
                var builder = await PageFactory(page).ConfigureAwait(false);

                return builder
                    .WithPaginatorFooter(Footer, page, MaxPageIndex, Users)
                    .Build();
            }
        }

        /// <summary>
        /// Sets the <see cref="PageFactory"/> of the paginator.
        /// </summary>
        /// <param name="pageFactory">The page factory.</param>
        /// <returns>This builder.</returns>
        public LazyPaginatorBuilder WithPageFactory(Func<int, Task<PageBuilder>> pageFactory)
        {
            PageFactory = pageFactory ?? throw new ArgumentNullException(nameof(pageFactory));
            return this;
        }

        /// <summary>
        /// Sets the maximum page index of the paginator.
        /// </summary>
        /// <param name="maxPageIndex">The maximum page index.</param>
        /// <returns>This builder.</returns>
        public LazyPaginatorBuilder WithMaxPageIndex(int maxPageIndex)
        {
            MaxPageIndex = maxPageIndex;
            return this;
        }

        /// <summary>
        /// Sets whether to cache loaded pages.
        /// </summary>
        /// <param name="cacheLoadedPages">Whether to cache loaded pages.</param>
        /// <returns>This builder.</returns>
        public LazyPaginatorBuilder WithCacheLoadedPages(bool cacheLoadedPages)
        {
            CacheLoadedPages = cacheLoadedPages;
            return this;
        }
    }
}