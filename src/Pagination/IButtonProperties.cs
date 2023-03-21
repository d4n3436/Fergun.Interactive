using Discord;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents the properties of a paginator button.
/// </summary>
public interface IButtonProperties
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
    /// Gets a value indicating whether to disable the button.
    /// </summary>
    bool IsDisabled { get; }

    /// <summary>
    /// Gets a value indicating whether to hide the button.
    /// </summary>
    bool IsHidden { get; }
}