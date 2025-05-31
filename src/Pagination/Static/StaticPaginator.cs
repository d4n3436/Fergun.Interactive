using JetBrains.Annotations;

namespace Fergun.Interactive.Pagination;

/// <inheritdoc/>
[PublicAPI]
public sealed class StaticPaginator : BaseStaticPaginator
{
    internal StaticPaginator(StaticPaginatorBuilder properties)
        : base(properties)
    {
    }
}