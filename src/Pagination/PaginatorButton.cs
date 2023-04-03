using Discord;
using System;

namespace Fergun.Interactive.Pagination;

/// <inheritdoc cref="IPaginatorButton"/>
public class PaginatorButton : IPaginatorButton
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatorButton"/> class.
    /// </summary>
    /// <param name="style">The button style.</param>
    /// <param name="text">The button text.</param>
    /// <param name="emote">The emote.</param>
    /// <param name="action">The action.</param>
    /// <param name="isDisabled">A value indicating whether to disable the button.</param>
    public PaginatorButton(ButtonStyle? style, string? text, IEmote? emote, PaginatorAction action, bool isDisabled)
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

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatorButton"/> class.
    /// </summary>
    /// <param name="isHidden">A value indicating whether to hide the button.</param>
    public PaginatorButton(bool isHidden)
    {
        IsHidden = isHidden;
    }

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