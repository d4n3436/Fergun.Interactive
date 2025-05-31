using System.Collections.Generic;
using JetBrains.Annotations;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents the properties of a <see cref="BaseStaticPaginatorBuilder{TPaginator, TBuilder}"/>.
/// </summary>
[PublicAPI]
public interface IBaseStaticPaginatorBuilderProperties : IBasePaginatorBuilderProperties
{
    /// <summary>
    /// Gets or sets the pages of the paginator.
    /// </summary>
    IList<IPageBuilder> Pages { get; set; }
}