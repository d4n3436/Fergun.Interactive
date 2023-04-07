namespace Fergun.Interactive.Pagination;

/// <inheritdoc cref="IButtonContext"/>
internal class ButtonContext : IButtonContext
{
    private readonly bool _disableAll;

    public ButtonContext(int buttonIndex, int currentPageIndex, int maxPageIndex, bool disableAll)
    {
        ButtonIndex = buttonIndex;
        CurrentPageIndex = currentPageIndex;
        MaxPageIndex = maxPageIndex;
        _disableAll = disableAll;
    }

    /// <inheritdoc />
    public int ButtonIndex { get; }

    /// <inheritdoc />
    public int CurrentPageIndex { get; }

    /// <inheritdoc />
    public int MaxPageIndex { get; }

    /// <inheritdoc />
    public bool ShouldDisable(PaginatorAction action)
    {
        return _disableAll || action switch
        {
            PaginatorAction.SkipToStart => CurrentPageIndex == 0,
            PaginatorAction.Backward => CurrentPageIndex == 0,
            PaginatorAction.Forward => CurrentPageIndex == MaxPageIndex,
            PaginatorAction.SkipToEnd => CurrentPageIndex == MaxPageIndex,
            _ => false
        };
    }
}