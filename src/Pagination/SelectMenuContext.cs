namespace Fergun.Interactive.Pagination;

/// <inheritdoc cref="ISelectMenuContext"/>
public readonly struct SelectMenuContext : ISelectMenuContext
{
    private readonly bool _disableAll;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectMenuContext"/> structure.
    /// </summary>
    /// <param name="selectMenuIndex">The index of the select menu in relation to other select menus.</param>
    /// <param name="currentPageIndex">The index of the page that will be displayed.</param>
    /// <param name="maxPageIndex">The maximum page index.</param>
    /// <param name="disableAll">Whether to disable all the buttons.</param>
    public SelectMenuContext(int selectMenuIndex, int currentPageIndex, int maxPageIndex, bool disableAll)
    {
        SelectMenuIndex = selectMenuIndex;
        CurrentPageIndex = currentPageIndex;
        MaxPageIndex = maxPageIndex;
        _disableAll = disableAll;
    }

    /// <inheritdoc />
    public int SelectMenuIndex { get; }

    /// <inheritdoc />
    public int CurrentPageIndex { get; }

    /// <inheritdoc />
    public int MaxPageIndex { get; }

    /// <inheritdoc />
    public bool ShouldDisable() => _disableAll;
}