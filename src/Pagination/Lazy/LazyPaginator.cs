using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Discord;

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

        internal LazyPaginator(IReadOnlyCollection<IUser> users, IReadOnlyDictionary<IEmote, PaginatorAction> emotes,
            IPage? canceledPage, IPage? timeoutPage, DeletionOptions deletion, InputType inputType,
            ActionOnStop actionOnCancellation, ActionOnStop actionOnTimeout, Func<int, Task<IPage>> pageFactory,
            int startPage, int maxPageIndex, bool cacheLoadedPages)
            : base(users, emotes, canceledPage, timeoutPage, deletion, inputType, actionOnCancellation, actionOnTimeout, startPage)
        {
            PageFactory = pageFactory ?? throw new ArgumentNullException(nameof(pageFactory));
            MaxPageIndex = maxPageIndex;
            CacheLoadedPages = cacheLoadedPages;

            if (CacheLoadedPages)
            {
                _cachedPages = new Dictionary<int, IPage>();
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