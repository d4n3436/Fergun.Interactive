using Discord;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents a paginator button.
/// </summary>
public interface IPaginatorButton
{
    /// <summary>
    /// Gets the style to use in the button.
    /// </summary>
    ButtonStyle? Style { get; }

    /// <summary>
    /// Gets the text (label) that will be displayed in the button.
    /// </summary>
    string? Text { get; }

    /// <summary>
    /// Gets the emote that will be displayed in the button.
    /// </summary>
    IEmote? Emote { get; }

    /// <summary>
    /// Gets the action that will be applied when the button is pressed.
    /// </summary>
    PaginatorAction Action { get; }

    /// <summary>
    /// Gets a value indicating whether to disable the button.
    /// </summary>
    /// <remarks>If the value is left as null, the library will use the result from <see cref="IButtonContext.ShouldDisable(PaginatorAction)"/>.</remarks>
    bool? IsDisabled { get; }

    /// <summary>
    /// Gets a value indicating whether to hide the button.
    /// </summary>
    bool IsHidden { get; }
}