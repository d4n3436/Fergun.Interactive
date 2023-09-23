namespace Fergun.Interactive.Pagination;

/// <inheritdoc cref="IButtonContext"/>
public readonly struct ButtonContext : IButtonContext
{
    private readonly bool _disableAll;

    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonContext"/> structure.
    /// </summary>
    /// <param name="buttonIndex">The index of the button within the components.</param>
    /// <param name="currentPageIndex">The index of the page that will be displayed.</param>
    /// <param name="maxPageIndex">The maximum page index.</param>
    /// <param name="disableAll">Whether to disable all the buttons.</param>
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
            PaginatorAction.Jump => MaxPageIndex == 0,
            _ => false
        };
    }
}