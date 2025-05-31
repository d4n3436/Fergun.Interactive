using JetBrains.Annotations;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Specifies the status of a <see cref="IComponentPaginator"/>.
/// </summary>
[PublicAPI]
public enum PaginatorStatus
{
    /// <summary>
    /// The paginator is active and is currently accepting pagination requests.
    /// </summary>
    Active = 0,

    /// <summary>
    /// The paginator is canceled and the action on <see cref="IComponentPaginator.ActionOnCancellation"/> will be performed.
    /// </summary>
    Canceled = 1,

    /// <summary>
    /// The paginator is timed out and the action on <see cref="IComponentPaginator.ActionOnTimeout"/> will be performed.
    /// </summary>
    TimedOut = 2
}