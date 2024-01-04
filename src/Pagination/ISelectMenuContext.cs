namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents the context of a paginator select menu.
/// </summary>
public interface ISelectMenuContext : IComponentContext
{
    /// <summary>
    /// Gets the index of the select menu in relation to other select menus.
    /// </summary>
    int SelectMenuIndex { get; }

    /// <summary>
    /// Returns a value indicating whether the select menu should be disabled. This value is <see langword="true"/> if the paginator is stopping and the action on stop is <see cref="ActionOnStop.DisableInput"/>.
    /// </summary>
    /// <returns>A value indicating whether the select menu should be disabled.</returns>
    bool ShouldDisable();
}