using Discord;
using System.Collections.Generic;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents a paginator select menu.
/// </summary>
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
    int MinValues { get; }

    /// <summary>
    /// Gets the maximum values of the select menu.
    /// </summary>
    int MaxValues { get; }

    /// <summary>
    /// Gets a collection of <see cref="SelectMenuOptionBuilder"/> for the select menu.
    /// </summary>
    List<SelectMenuOptionBuilder>? Options { get; }

    /// <summary>
    /// Gets whether the select menu is disabled.
    /// </summary>
    /// <remarks>If the value is left as null, the library will use the result from <see cref="ISelectMenuContext.ShouldDisable"/>.</remarks>
    bool? IsDisabled { get; }

    /// <summary>
    /// Gets the menu's channel types (only valid on select menus of type <see cref="ComponentType.ChannelSelect"/>).
    /// </summary>
    List<ChannelType>? ChannelTypes { get; }

    /// <summary>
    /// Gets the default values of the select menu.
    /// </summary>
    List<SelectMenuDefaultValue>? DefaultValues { get; }

    /// <summary>
    /// Gets a value indicating whether to hide the select menu.
    /// </summary>
    bool IsHidden { get; }
}