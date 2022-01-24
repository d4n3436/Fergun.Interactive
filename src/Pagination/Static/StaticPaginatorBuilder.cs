namespace Fergun.Interactive.Pagination
{
    /// <summary>
    /// Represents a builder class for making a <see cref="StaticPaginator"/>.
    /// </summary>
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
            if (Options.Count == 0)
            {
                WithDefaultEmotes();
            }

            return new StaticPaginator(this);
        }
    }
}