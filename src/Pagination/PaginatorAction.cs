using JetBrains.Annotations;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Defines the possible actions a paginator can perform.
/// </summary>
[PublicAPI]
public enum PaginatorAction
{
    /// <summary>
    /// Go to the next page.
    /// </summary>
    Forward = 0,

    /// <summary>
    /// Go to the previous page.
    /// </summary>
    Backward = 1,

    /// <summary>
    /// Skip to the end (last page).
    /// </summary>
    SkipToEnd = 2,

    /// <summary>
    /// Skip to the start (first page).
    /// </summary>
    SkipToStart = 3,

    /// <summary>
    /// Exit (stop) the paginator.
    /// </summary>
    Exit = 4,

    /// <summary>
    /// Jump to a specific page.
    /// </summary>
    Jump = 5
}