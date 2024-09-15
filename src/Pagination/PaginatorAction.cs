namespace Fergun.Interactive.Pagination;

/// <summary>
/// Defines the possible actions a paginator can perform.
/// </summary>
public enum PaginatorAction
{
    /// <summary>
    /// Go to the next page.
    /// </summary>
    Forward,

    /// <summary>
    /// Go to the previous page.
    /// </summary>
    Backward,

    /// <summary>
    /// Skip to the end (last page).
    /// </summary>
    SkipToEnd,

    /// <summary>
    /// Skip to the start (first page).
    /// </summary>
    SkipToStart,

    /// <summary>
    /// Exit (stop) the paginator.
    /// </summary>
    Exit,

    /// <summary>
    /// Jump to a specific page.
    /// </summary>
    Jump
}