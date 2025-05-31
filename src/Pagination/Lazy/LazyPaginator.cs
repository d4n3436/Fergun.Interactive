using JetBrains.Annotations;

namespace Fergun.Interactive.Pagination;

/// <inheritdoc/>
[PublicAPI]
public sealed class LazyPaginator : BaseLazyPaginator
{
    internal LazyPaginator(LazyPaginatorBuilder properties)
        : base(properties)
    {
    }
}