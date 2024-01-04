namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents the context of a paginator component.
/// </summary>
public interface IComponentContext
{
    /// <summary>
    /// Gets the index of the page that will be displayed.
    /// </summary>
    int CurrentPageIndex { get; }

    /// <summary>
    /// Gets the maximum page index.
    /// </summary>
    int MaxPageIndex { get; }
}