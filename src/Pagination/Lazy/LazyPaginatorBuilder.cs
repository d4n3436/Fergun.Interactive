namespace Fergun.Interactive.Pagination;

/// <inheritdoc/>
public sealed class LazyPaginatorBuilder : BaseLazyPaginatorBuilder<LazyPaginator, LazyPaginatorBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LazyPaginatorBuilder"/> class.
    /// </summary>
    public LazyPaginatorBuilder()
    {
    }

    /// <inheritdoc/>
    public override LazyPaginator Build()
    {
        if (ButtonFactories.Count == 0)
        {
            WithDefaultEmotes();
        }

        return new LazyPaginator(this);
    }
}