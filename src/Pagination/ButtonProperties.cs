using Discord;

namespace Fergun.Interactive.Pagination;

/// <inheritdoc cref="IButtonProperties"/>
public class ButtonProperties : IButtonProperties
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonProperties"/> class.
    /// </summary>
    /// <param name="style">The button style.</param>
    /// <param name="text">The button text.</param>
    /// <param name="isDisabled">A value indicating whether to disable the button.</param>
    public ButtonProperties(ButtonStyle? style, string? text, bool isDisabled)
    {
        Style = style;
        Text = text;
        IsDisabled = isDisabled;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonProperties"/> class.
    /// </summary>
    /// <param name="isHidden">A value indicating whether to hide the button.</param>
    public ButtonProperties(bool isHidden)
    {
        IsHidden = isHidden;
    }

    /// <inheritdoc/>
    public ButtonStyle? Style { get; }

    /// <inheritdoc/>
    public string? Text { get; }

    /// <inheritdoc/>
    public bool IsDisabled { get; }

    /// <inheritdoc/>
    public bool IsHidden { get; }
}