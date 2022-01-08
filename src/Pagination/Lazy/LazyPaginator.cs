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
        private readonly Dictionary<int, IPage>? _cachedPages;

        /// <summary>
        /// Gets the function used to load the pages of this paginator lazily.
        /// </summary>
        public Func<int, Task<IPage>> PageFactory { get; }

        /// <inheritdoc/>
        public override int MaxPageIndex { get; }

        /// <summary>
        /// Gets whether to cache loaded pages.
        /// </summary>
        [MemberNotNullWhen(true, nameof(_cachedPages))]
        public bool CacheLoadedPages { get; }

        internal LazyPaginator(LazyPaginatorBuilder builder)
            : base(builder)
        {
            InteractiveGuards.SupportedPaginatorInputType(builder.InputType);
            InteractiveGuards.NotNull(builder.PageFactory, nameof(builder.PageFactory));

            PageFactory = AddPaginatorFooterAsync;
            MaxPageIndex = builder.MaxPageIndex;
            CacheLoadedPages = builder.CacheLoadedPages;

            if (CacheLoadedPages)
            {
                _cachedPages = new Dictionary<int, IPage>();
            }

            async Task<IPage> AddPaginatorFooterAsync(int page)
            {
                var pageBuilder = await builder.PageFactory(page).ConfigureAwait(false);
                (pageBuilder as PageBuilder)?.WithPaginatorFooter(builder.Footer, page, MaxPageIndex, builder.Users);
                return pageBuilder.Build();
            }
        }

        /// <inheritdoc/>
        public override async Task<IPage> GetOrLoadPageAsync(int pageIndex)
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