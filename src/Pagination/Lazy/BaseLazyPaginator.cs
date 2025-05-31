using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents a paginator that lazily loads pages using a factory.
/// </summary>
[PublicAPI]
public abstract class BaseLazyPaginator : Paginator
{
    private readonly Dictionary<int, IPage>? _cachedPages;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseLazyPaginator"/> class.
    /// </summary>
    /// <param name="properties">The builder properties to copy from.</param>
    protected BaseLazyPaginator(IBaseLazyPaginatorBuilderProperties properties)
        : base(properties)
    {
        InteractiveGuards.SupportedPaginatorInputType(properties.InputType);
        InteractiveGuards.NotNull(properties.PageFactory);

        PageFactory = AddPaginatorFooterAsync;
        MaxPageIndex = properties.MaxPageIndex;
        CacheLoadedPages = properties.CacheLoadedPages;

        if (CacheLoadedPages)
        {
            _cachedPages = [];
        }

        return;

        async Task<IPage> AddPaginatorFooterAsync(int page)
        {
            var pageBuilder = await properties.PageFactory(page).ConfigureAwait(false);
            (pageBuilder as PageBuilder)?.WithPaginatorFooter(properties.Footer, page, MaxPageIndex, properties.Users);
            return pageBuilder.Build();
        }
    }

    /// <summary>
    /// Gets the function used to load the pages of this paginator lazily.
    /// </summary>
    public virtual Func<int, Task<IPage>> PageFactory { get; }

    /// <inheritdoc/>
    public override int MaxPageIndex { get; }

    /// <summary>
    /// Gets a value indicating whether to cache loaded pages.
    /// </summary>
    [MemberNotNullWhen(true, nameof(_cachedPages))]
    public bool CacheLoadedPages { get; }

    /// <inheritdoc/>
    public override async Task<IPage> GetOrLoadPageAsync(int pageIndex)
    {
        if (CacheLoadedPages && _cachedPages.TryGetValue(pageIndex, out var page))
        {
            return page;
        }

        page = await PageFactory(pageIndex).ConfigureAwait(false);

        if (CacheLoadedPages && !_cachedPages.ContainsKey(pageIndex))
        {
            _cachedPages.Add(pageIndex, page);
        }

        return page;
    }
}