using System;
using System.Threading.Tasks;
using Discord;
using Fergun.Interactive.Pagination;
using Fergun.Interactive.Selection;
using JetBrains.Annotations;

namespace Fergun.Interactive.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IInteractiveElement{TOption}"/>.
/// </summary>
[PublicAPI]
public static class InteractiveElementExtensions
{
    /// <summary>
    /// Returns a value indicating whether the specified user can interact with this element.
    /// </summary>
    /// <typeparam name="TOption">The type of the options.</typeparam>
    /// <param name="element">The interactive element.</param>
    /// <param name="user">The user.</param>
    /// <returns><see langword="true"/> the user can interact with this element; otherwise, <see langword="false"/>.</returns>
    public static bool CanInteract<TOption>(this IInteractiveElement<TOption> element, IUser user)
    {
        ArgumentNullException.ThrowIfNull(element);
        ArgumentNullException.ThrowIfNull(user);

        return element.CanInteract(user.Id);
    }

    /// <summary>
    /// Returns a value indicating whether the specified user ID can interact with this element.
    /// </summary>
    /// <typeparam name="TOption">The type of the options.</typeparam>
    /// <param name="element">The interactive element.</param>
    /// <param name="userId">The user ID.</param>
    /// <returns><see langword="true"/> the user ID can interact with this element; otherwise, <see langword="false"/>.</returns>
    public static bool CanInteract<TOption>(this IInteractiveElement<TOption> element, ulong userId)
    {
        ArgumentNullException.ThrowIfNull(element);

        if (element.Users.Count == 0)
        {
            return true;
        }

        foreach (var user in element.Users)
        {
            if (user.Id == userId)
            {
                return true;
            }
        }

        return false;
    }

    internal static async Task<IPage> GetCurrentPageAsync<TOption>(this IInteractiveElement<TOption> element)
        => element switch
        {
            Paginator paginator => await paginator.GetOrLoadCurrentPageAsync().ConfigureAwait(false),
            BaseSelection<TOption> selection => selection.SelectionPage,
            _ => throw new ArgumentException("Unknown interactive element.", nameof(element))
        };
}