using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Fergun.Interactive.Pagination
{
    /// <summary>
    /// Represents a lazy paginator.
    /// </summary>
    public sealed class LazyPaginator : Paginator
    {
        private readonly Dictionary<int, Page>? _cachedPages;

        /// <summary>
        /// Gets the function used to load the pages of this paginator lazily.
        /// </summary>
        public Func<int, Task<Page>> PageFactory { get; }

        /// <inheritdoc/>
        public override int MaxPageIndex { get; }

        /// <summary>
        /// Gets whether to cache loaded pages.
        /// </summary>
        [MemberNotNullWhen(true, nameof(_cachedPages))]
        public bool CacheLoadedPages { get; }

        internal LazyPaginator(LazyPaginatorBuilder builder, int startPageIndex)
            : base(builder, startPageIndex)
        {
            InteractiveGuards.NotNull(builder.PageFactory, nameof(builder.PageFactory));

            PageFactory = AddPaginatorFooterAsync;
            MaxPageIndex = builder.MaxPageIndex;
            CacheLoadedPages = builder.CacheLoadedPages;

            if (CacheLoadedPages)
            {
                _cachedPages = new Dictionary<int, Page>();
            }

            async Task<Page> AddPaginatorFooterAsync(int page)
            {
                var pageBuilder = await builder.PageFactory(page).ConfigureAwait(false);

                return pageBuilder
                    .WithPaginatorFooter(builder.Footer, page, MaxPageIndex, builder.Users)
                    .Build();
            }
        }

        /// <inheritdoc/>
        public override async Task<Page> GetOrLoadPageAsync(int pageIndex)
        {
            if (CacheLoadedPages && _cachedPages.TryGetValue(pageIndex, out var page))
            {
                return page;
            }

            page = await PageFactory(pageIndex).ConfigureAwait(false);

            if (_cachedPages?.ContainsKey(pageIndex) == false)
            {
                _cachedPages.Add(pageIndex, page);
            }
            return page;
        }
    }
}