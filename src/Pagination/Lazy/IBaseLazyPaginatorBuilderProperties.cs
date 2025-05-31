using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents the properties of a <see cref="BaseLazyPaginatorBuilder{TPaginator, TBuilder}"/>.
/// </summary>
[PublicAPI]
public interface IBaseLazyPaginatorBuilderProperties : IBasePaginatorBuilderProperties
{
    /// <summary>
    /// Gets or sets the method used to load the pages of the paginator lazily.
    /// </summary>
    /// <remarks>The first argument of the factory is the current page index.</remarks>
    Func<int, Task<IPageBuilder>> PageFactory { get; set; }

    /// <summary>
    /// Gets or sets the maximum page index of the paginator.
    /// </summary>
    int MaxPageIndex { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to cache loaded pages. The default value is <see langword="true"/>.
    /// </summary>
    bool CacheLoadedPages { get; set; }
}