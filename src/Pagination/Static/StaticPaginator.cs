using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fergun.Interactive.Pagination
{
    /// <summary>
    /// Represents a static paginator.
    /// </summary>
    public sealed class StaticPaginator : Paginator
    {
        /// <summary>
        /// Gets the pages of this paginator.
        /// </summary>
        public IReadOnlyCollection<IPage> Pages { get; }

        /// <summary>
        /// Gets the maximum page index of this paginator.
        /// </summary>
        public override int MaxPageIndex => Pages.Count - 1;

        internal StaticPaginator(StaticPaginatorBuilder builder)
            : base(builder)
        {
            InteractiveGuards.SupportedPaginatorInputType(builder.InputType);
            InteractiveGuards.NotNull(builder.Pages, nameof(builder.Pages));
            InteractiveGuards.NotEmpty(builder.Pages, nameof(builder.Pages));
            InteractiveGuards.IndexInRange(builder.Pages, builder.StartPageIndex, nameof(builder.StartPageIndex));

            Pages = builder.Pages.Select((x, i) =>
            {
                (x as PageBuilder)?.WithPaginatorFooter(builder.Footer, i, builder.Pages.Count - 1, builder.Users);
                return x.Build();
            }).ToArray();
        }

        /// <inheritdoc/>
        public override Task<IPage> GetOrLoadPageAsync(int pageIndex)
            => Task.FromResult(Pages.ElementAt(pageIndex));
    }
}