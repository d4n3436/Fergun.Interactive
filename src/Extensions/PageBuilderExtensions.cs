using Fergun.Interactive.Pagination;
using System;

namespace Fergun.Interactive.Extensions;

/// <summary>
/// Provides extension methods for <see cref="PageBuilder"/>.
/// </summary>
public static class PageBuilderExtensions
{
    /// <summary>
    /// Applies the standard paginator footer to this page builder (implies using an embed to display the footer).
    /// </summary>
    /// <param name="builder">The page builder.</param>
    /// <param name="paginator">The component paginator, used to get the required information.</param>
    /// <param name="style">The footer style.</param>
    /// <returns>This <see cref="PageBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static PageBuilder WithPaginatorFooter(this PageBuilder builder, IComponentPaginator paginator, PaginatorFooter style = PaginatorFooter.PageNumber)
    {
        InteractiveGuards.NotNull(builder);
        InteractiveGuards.NotNull(paginator);

        builder.GetEmbedBuilder().WithPaginatorFooter(paginator, style);

        return builder;
    }
}