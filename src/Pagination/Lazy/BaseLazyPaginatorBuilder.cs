using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents a builder class for constructing instances of <typeparamref name="TPaginator"/>.
/// The paginator lazy-loads pages using a factory.
/// </summary>
/// <typeparam name="TPaginator">The type of the paginator.</typeparam>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
[PublicAPI]
public abstract class BaseLazyPaginatorBuilder<TPaginator, TBuilder>
    : PaginatorBuilder<TPaginator, TBuilder>, IBaseLazyPaginatorBuilderProperties
    where TPaginator : BaseLazyPaginator
    where TBuilder : BaseLazyPaginatorBuilder<TPaginator, TBuilder>
{
    /// <inheritdoc/>
    public virtual Func<int, Task<IPageBuilder>> PageFactory { get; set; } = null!;

    /// <inheritdoc/>
    public virtual int MaxPageIndex { get; set; }

    /// <inheritdoc/>
    public virtual bool CacheLoadedPages { get; set; } = true;

    /// <summary>
    /// Sets the method used to load the pages of the paginator lazily.
    /// </summary>
    /// <remarks>The first argument of the factory is the current page index.</remarks>
    /// <param name="pageFactory">The page factory. The first argument is the current page index.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithPageFactory(Func<int, IPageBuilder> pageFactory)
    {
        InteractiveGuards.NotNull(pageFactory);
        return WithPageFactory(index => Task.FromResult(pageFactory(index)));
    }

    /// <summary>
    /// Sets the method used to load the pages of the paginator lazily.
    /// </summary>
    /// <remarks>The first argument of the factory is the current page index.</remarks>
    /// <param name="pageFactory">The page factory. The first argument is the current page index.</param>
    /// <typeparam name="TPageBuilder">A type that is or implements <see cref="IPageBuilder"/>.</typeparam>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithPageFactory<TPageBuilder>(Func<int, Task<TPageBuilder>> pageFactory)
        where TPageBuilder : IPageBuilder
    {
        InteractiveGuards.NotNull(pageFactory);
        PageFactory = pageFactory as Func<int, Task<IPageBuilder>> ?? (async index => await pageFactory(index).ConfigureAwait(false));
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the maximum page index of the paginator.
    /// </summary>
    /// <param name="maxPageIndex">The maximum page index.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithMaxPageIndex(int maxPageIndex)
    {
        MaxPageIndex = maxPageIndex;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets a value indicating whether to cache loaded pages.
    /// </summary>
    /// <param name="cacheLoadedPages">Whether to cache loaded pages.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithCacheLoadedPages(bool cacheLoadedPages)
    {
        CacheLoadedPages = cacheLoadedPages;
        return (TBuilder)this;
    }
}