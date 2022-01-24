using System.Collections.Generic;

namespace Fergun.Interactive.Pagination
{
    /// <summary>
    /// Represents the properties of a <see cref="BaseStaticPaginatorBuilder{TPaginator, TBuilder}"/>.
    /// </summary>
    public interface IBaseStaticPaginatorBuilderProperties : IBasePaginatorBuilderProperties
    {
        /// <summary>
        /// Gets or sets the pages of the paginator.
        /// </summary>
        IList<IPageBuilder> Pages { get; set; }
    }
}