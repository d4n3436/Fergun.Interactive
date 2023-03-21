using Discord;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents the context of a paginator button.
/// </summary>
public interface IButtonContext
{
    /// <summary>
    /// Gets the index of the page that will be displayed.
    /// </summary>
    int CurrentPageIndex { get; }

    /// <summary>
    /// Gets the maximum page index.
    /// </summary>
    int MaxPageIndex { get; }

    /// <summary>
    /// Gets the emote being displayed in the button.
    /// </summary>
    IEmote Emote { get; }

    /// <summary>
    /// Gets the action associated with the button.
    /// </summary>
    PaginatorAction Action { get; }

    /// <summary>
    /// Gets a value indicating whether the button should be disabled. This value is <see langword="true"/> if:<br/>
    /// - The paginator is stopping and the action on stop is <see cref="ActionOnStop.DisableInput"/>.<br/>
    /// - It's unnecessary to have the button enabled (e.g., a button with action <see cref="PaginatorAction.SkipToStart"/> and the current page index is 0).
    /// </summary>
    bool ShouldDisable { get; }
}