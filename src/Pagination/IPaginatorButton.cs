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
    /// <remarks>If the value is null, the library will decide the style of the button.</remarks>
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
    /// Gets the url of the button.
    /// </summary>
    string? Url { get; }

    /// <summary>
    /// Gets a value indicating whether to disable the button.
    /// </summary>
    /// <remarks>If the value is left as null, the library will use the result from <see cref="IButtonContext.ShouldDisable(PaginatorAction)"/>.</remarks>
    bool? IsDisabled { get; }

    /// <summary>
    /// Gets the custom ID.
    /// </summary>
    public string? CustomId { get; }

    /// <summary>
    /// Gets a value indicating whether to hide the button.
    /// </summary>
    bool IsHidden { get; }
}