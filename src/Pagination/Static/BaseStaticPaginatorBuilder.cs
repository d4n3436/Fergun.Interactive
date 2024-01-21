using System.Collections.Generic;
using System.Linq;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents the base of static paginator builders.
/// </summary>
/// <typeparam name="TPaginator">The type of the paginator.</typeparam>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public abstract class BaseStaticPaginatorBuilder<TPaginator, TBuilder>
    : PaginatorBuilder<TPaginator, TBuilder>, IBaseStaticPaginatorBuilderProperties
    where TPaginator : BaseStaticPaginator
    where TBuilder : BaseStaticPaginatorBuilder<TPaginator, TBuilder>
{
    /// <inheritdoc/>
    public virtual IList<IPageBuilder> Pages { get; set; } = [];

    /// <summary>
    /// Sets the pages of the paginator.
    /// </summary>
    /// <param name="pages">The pages.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithPages(params IPageBuilder[] pages)
    {
        InteractiveGuards.NotNull(pages);
        Pages = pages.ToList();
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the pages of the paginator.
    /// </summary>
    /// <param name="pages">The pages.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithPages(IEnumerable<IPageBuilder> pages)
    {
        InteractiveGuards.NotNull(pages);
        Pages = pages.ToList();
        return (TBuilder)this;
    }

    /// <summary>
    /// Adds a page to the paginator.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder AddPage(IPageBuilder page)
    {
        InteractiveGuards.NotNull(page);
        Pages.Add(page);
        return (TBuilder)this;
    }
}