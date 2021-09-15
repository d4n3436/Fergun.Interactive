using System;
using System.Threading.Tasks;
using Discord;
using Fergun.Interactive.Pagination;
using Fergun.Interactive.Selection;

namespace Fergun.Interactive
{
    internal static class InteractiveExtensions
    {
        public static bool CanInteract<TOption>(this IInteractiveElement<TOption> element, IUser user)
            => CanInteract(element, user.Id);

        public static bool CanInteract<TOption>(this IInteractiveElement<TOption> element, ulong userId)
        {
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

        public static async Task<Page> GetCurrentPageAsync<TOption>(this IInteractiveElement<TOption> element)
            => element switch
            {
                Paginator paginator => await paginator.GetOrLoadCurrentPageAsync().ConfigureAwait(false),
                BaseSelection<TOption> selection => selection.SelectionPage,
                _ => throw new ArgumentException("Unknown interactive element.", nameof(element))
            };
    }
}