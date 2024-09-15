namespace Fergun.Interactive.Pagination;

///<inheritdoc/>
public sealed class StaticPaginatorBuilder : BaseStaticPaginatorBuilder<StaticPaginator, StaticPaginatorBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StaticPaginatorBuilder"/> class.
    /// </summary>
    public StaticPaginatorBuilder()
    {
    }

    /// <inheritdoc/>
    public override StaticPaginator Build()
    {
        if (ButtonFactories.Count == 0)
        {
            WithDefaultEmotes();
        }

        return new StaticPaginator(this);
    }
}