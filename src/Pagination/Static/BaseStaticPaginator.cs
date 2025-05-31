using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents a paginator that holds a fixed, read-only collection of pages.
/// </summary>
[PublicAPI]
public abstract class BaseStaticPaginator : Paginator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseStaticPaginator"/> class.
    /// </summary>
    /// <param name="properties">The builder properties to copy from.</param>
    protected BaseStaticPaginator(IBaseStaticPaginatorBuilderProperties properties)
        : base(properties)
    {
        InteractiveGuards.SupportedPaginatorInputType(properties.InputType);
        InteractiveGuards.NotNull(properties.Pages);
        InteractiveGuards.NotEmpty(properties.Pages);
        InteractiveGuards.IndexInRange(properties.Pages, properties.StartPageIndex);

        Pages = properties.Pages.Select((x, i) =>
        {
            (x as PageBuilder)?.WithPaginatorFooter(properties.Footer, i, properties.Pages.Count - 1, properties.Users);
            return x.Build();
        }).ToArray();
    }

    /// <summary>
    /// Gets the pages of this paginator.
    /// </summary>
    public virtual IReadOnlyCollection<IPage> Pages { get; }

    /// <summary>
    /// Gets the maximum page index of this paginator.
    /// </summary>
    public override int MaxPageIndex => Pages.Count - 1;

    /// <inheritdoc/>
    public override Task<IPage> GetOrLoadPageAsync(int pageIndex)
        => Task.FromResult(Pages.ElementAt(pageIndex));
}