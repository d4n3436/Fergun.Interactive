using System;
using JetBrains.Annotations;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents the context of a paginator select menu.
/// </summary>
[PublicAPI]
[Obsolete("Paginator select menus are obsolete and its functionality has been replaced by component paginators, which offer better control of select menus.")]
public interface ISelectMenuContext : IComponentContext
{
    /// <summary>
    /// Gets the index of the select menu in relation to other select menus.
    /// </summary>
    int SelectMenuIndex { get; }

    /// <summary>
    /// Returns a value indicating whether the select menu should be disabled. This value is <see langword="true"/> if the paginator is stopping and the action on stop is <see cref="ActionOnStop.DisableInput"/>.
    /// </summary>
    /// <returns><see langword="true"/> the select menu should be disabled; otherwise, <see langword="false"/>.</returns>
    bool ShouldDisable();
}