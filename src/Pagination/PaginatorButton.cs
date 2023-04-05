using Discord;
using System;

namespace Fergun.Interactive.Pagination;

/// <inheritdoc cref="IPaginatorButton"/>
public class PaginatorButton : IPaginatorButton
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatorButton"/> class.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="emote">The emote.</param>
    /// <param name="style">The button style.</param>
    public PaginatorButton(IEmote emote, PaginatorAction action, ButtonStyle? style = null)
        : this(action, emote, null, style, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatorButton"/> class.
    /// </summary>
    /// <param name="text">The button text.</param>
    /// <param name="action">The action.</param>
    /// <param name="style">The button style.</param>
    public PaginatorButton(string text, PaginatorAction action, ButtonStyle? style = null)
        : this(action, null, text, style, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatorButton"/> class.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="emote">The emote.</param>
    /// <param name="text">The button text.</param>
    /// <param name="style">The button style.</param>
    /// <param name="isDisabled">A value indicating whether to disable the button.</param>
    public PaginatorButton(PaginatorAction action, IEmote? emote, string? text, ButtonStyle? style, bool? isDisabled)
    {
        if (emote is null && string.IsNullOrEmpty(text))
        {
            throw new ArgumentException($"Either {nameof(emote)} or {nameof(text)} must have a valid value.");
        }

        Style = style;
        Text = text;
        Emote = emote;
        Action = action;
        IsDisabled = isDisabled;
    }

    private PaginatorButton(bool isHidden)
    {
        IsHidden = isHidden;
    }

    /// <summary>
    /// Returns a hidden button.
    /// </summary>
    public static PaginatorButton Hidden { get; } = new(true); 

    /// <inheritdoc/>
    public ButtonStyle? Style { get; }

    /// <inheritdoc/>
    public string? Text { get; }

    /// <inheritdoc/>
    public IEmote? Emote { get; }

    /// <inheritdoc/>
    public PaginatorAction Action { get; }

    /// <inheritdoc/>
    public bool? IsDisabled { get; }

    /// <inheritdoc/>
    public bool IsHidden { get; }
}