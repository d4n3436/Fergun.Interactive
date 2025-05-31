using System;
using System.Linq;
using Discord;
using JetBrains.Annotations;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Provides extension methods for <see cref="EmbedBuilder"/>.
/// </summary>
[PublicAPI]
public static class EmbedBuilderExtensions
{
    /// <summary>
    /// Applies the standard paginator footer to this embed builder.
    /// </summary>
    /// <param name="builder">The embed builder.</param>
    /// <param name="paginator">The component paginator, used to get the required information.</param>
    /// <param name="style">The footer style.</param>
    /// <returns>This <see cref="EmbedBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static EmbedBuilder WithPaginatorFooter(this EmbedBuilder builder, IComponentPaginator paginator, PaginatorFooter style = PaginatorFooter.PageNumber)
    {
        InteractiveGuards.NotNull(builder);
        InteractiveGuards.NotNull(paginator);

        if (style == PaginatorFooter.None)
            return builder;

        builder.Footer = new EmbedFooterBuilder();

        if (style.HasFlag(PaginatorFooter.Users))
        {
            if (paginator.Users.Count == 0)
            {
                builder.Footer.Text += "Interactors: Everyone";
            }
            else if (paginator.Users.Count == 1)
            {
                var user = paginator.Users.Single();

                builder.Footer.Text += $"Interactor: {user}";
                builder.Footer.IconUrl = user.GetDisplayAvatarUrl();
            }
            else
            {
                builder.Footer.Text += $"Interactors: {string.Join(", ", paginator.Users)}";
            }

            builder.Footer.Text += '\n';
        }

        if (style.HasFlag(PaginatorFooter.PageNumber))
        {
            builder.Footer.Text += $"Page {paginator.CurrentPageIndex + 1}/{paginator.PageCount}";
        }

        return builder;
    }
}