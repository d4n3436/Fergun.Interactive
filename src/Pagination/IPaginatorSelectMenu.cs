using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NetCord;
using NetCord.Rest;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents a paginator select menu.
/// </summary>
[PublicAPI]
[Obsolete("Paginator select menus are obsolete and its functionality has been replaced by component paginators, which offer better control of select menus.")]
public interface IPaginatorSelectMenu
{
    /// <summary>
    /// Gets the custom ID of the select menu.
    /// </summary>
    string CustomId { get; }

    /// <summary>
    /// Gets the type of the select menu.
    /// </summary>
    ComponentType Type { get; }

    /// <summary>
    /// Gets the placeholder text of the select menu.
    /// </summary>
    string? Placeholder { get; }

    /// <summary>
    /// Gets the minimum values of the select menu.
    /// </summary>
    int? MinValues { get; }

    /// <summary>
    /// Gets the maximum values of the select menu.
    /// </summary>
    int? MaxValues { get; }

    /// <summary>
    /// Gets a collection of <see cref="StringMenuSelectOptionProperties"/> for the select menu.
    /// </summary>
    IEnumerable<StringMenuSelectOptionProperties> Options { get; }

    /// <summary>
    /// Gets a value indicating whether the select menu is disabled.
    /// </summary>
    /// <remarks>If the value is left as null, the library will use the result from <see cref="ISelectMenuContext.ShouldDisable"/>.</remarks>
    bool? IsDisabled { get; }

    /// <summary>
    /// Gets a value indicating whether to hide the select menu.
    /// </summary>
    bool IsHidden { get; }
}