using Discord;

namespace Fergun.Interactive.Pagination;

/// <inheritdoc cref="IButtonContext"/>
internal class ButtonContext : IButtonContext
{
    public ButtonContext(int currentPageIndex, int maxPageIndex, IEmote emote, PaginatorAction action, bool shouldDisable)
    {
        CurrentPageIndex = currentPageIndex;
        MaxPageIndex = maxPageIndex;
        Emote = emote;
        Action = action;
        ShouldDisable = shouldDisable;
    }

    /// <inheritdoc />
    public int CurrentPageIndex { get; }

    /// <inheritdoc />
    public int MaxPageIndex { get; }

    /// <inheritdoc />
    public PaginatorAction Action { get; }

    /// <inheritdoc />
    public IEmote Emote { get; }

    /// <inheritdoc />
    public bool ShouldDisable { get; }
}