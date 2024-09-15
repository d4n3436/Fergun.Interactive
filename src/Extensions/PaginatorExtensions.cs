using System.Collections.Generic;
using Fergun.Interactive.Pagination;

namespace Fergun.Interactive.Extensions;

/// <summary>
/// Provides extension methods for paginators.
/// </summary>
public static class PaginatorExtensions
{
    /// <summary>
    /// Sets the maximum page index of a lazy paginator based on the number of items in a collection.
    /// </summary>
    /// <typeparam name="TPaginator">The type of the lazy paginator.</typeparam>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <typeparam name="T">The type of the elements in <paramref name="collection"/>.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="collection">The collection.</param>
    /// <returns>This builder.</returns>
    public static BaseLazyPaginatorBuilder<TPaginator, TBuilder> WithMaxPageIndex<TPaginator, TBuilder, T>(this BaseLazyPaginatorBuilder<TPaginator, TBuilder> builder, IReadOnlyCollection<T> collection)
        where TPaginator : BaseLazyPaginator
        where TBuilder : BaseLazyPaginatorBuilder<TPaginator, TBuilder>
    {
        InteractiveGuards.NotNull(builder);
        InteractiveGuards.NotNull(collection);
        InteractiveGuards.NotEmpty(collection);

        return builder.WithMaxPageIndex(collection.Count - 1);
    }
}