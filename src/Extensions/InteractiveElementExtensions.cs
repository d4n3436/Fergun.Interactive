using System;
using System.Threading.Tasks;
using Discord;
using Fergun.Interactive.Pagination;
using Fergun.Interactive.Selection;

namespace Fergun.Interactive.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IInteractiveElement{TOption}"/>.
    /// </summary>
    public static class InteractiveElementExtensions
    {
        /// <summary>
        /// Gets whether the specified user can interact with this element.
        /// </summary>
        /// <typeparam name="TOption">The type of the options.</typeparam>
        /// <param name="element">The interactive element.</param>
        /// <param name="user">The user.</param>
        /// <returns>Whether the user can interact with this element.</returns>
        public static bool CanInteract<TOption>(this IInteractiveElement<TOption> element, IUser user)
        {
            InteractiveGuards.NotNull(element, nameof(element));
            InteractiveGuards.NotNull(user, nameof(user));

            return CanInteract(element, user.Id);
        }

        /// <summary>
        /// Gets whether the specified user ID can interact with this element.
        /// </summary>
        /// <typeparam name="TOption">The type of the options.</typeparam>
        /// <param name="element">The interactive element.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>Whether the user ID can interact with this element.</returns>
        public static bool CanInteract<TOption>(this IInteractiveElement<TOption> element, ulong userId)
        {
            InteractiveGuards.NotNull(element, nameof(element));

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
}