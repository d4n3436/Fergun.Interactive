using System;
using Discord;
using Discord.WebSocket;
using Fergun.Interactive.Pagination;

namespace Fergun.Interactive
{
    internal static class InteractiveGuards
    {
        public static void NotNull<T>(T obj, string parameterName) where T : class
        {
            if (obj is null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        public static void MessageFromCurrentUser(IDiscordClient client, IUserMessage message, string parameterName)
        {
            if (message is null) return;

            if (message.Author.Id != client.CurrentUser.Id)
            {
                throw new ArgumentException("Message author must be the current user.", parameterName);
            }
        }

        public static void DeleteAndDisableInputNotSet(ActionOnStop action, string parameterName)
        {
            if (action.HasFlag(ActionOnStop.DeleteMessage))
            {
                return;
            }

            if (action.HasFlag(ActionOnStop.DeleteInput | ActionOnStop.DisableInput))
            {
                throw new ArgumentException($"{ActionOnStop.DeleteInput} and {ActionOnStop.DisableInput} are mutually exclusive.", parameterName);
            }
        }

        public static void SupportedInputType(Paginator paginator)
        {
            if (paginator.InputType == InputType.Messages)
            {
                throw new NotSupportedException("Paginators using messages as input are not supported (yet).");
            }

            if (paginator.InputType == InputType.SelectMenus)
            {
                throw new NotSupportedException("Paginators using select menus as input are not supported (yet).");
            }
        }

#if DNETLABS
        public static void ValidResponseType(InteractionResponseType responseType, string parameterName)
        {
            int value = (int)responseType;
            if (value >= 1 && value <= 3)
            {
                throw new ArgumentException("Invalid response type.", parameterName);
            }
        }

        public static void ValidResponseType(InteractionResponseType responseType, SocketInteraction interaction, string parameterName)
        {
            if (!(interaction is SocketMessageComponent) &&
                (responseType == InteractionResponseType.DeferredUpdateMessage || responseType == InteractionResponseType.UpdateMessage))
            {
                throw new ArgumentException($"Interaction response type {responseType} can only be used on component interactions.", parameterName);
            }
        }
#else
        public static void CanUseComponents<TOption>(IInteractiveElement<TOption> element)
        {
            if (element.InputType == InputType.Buttons || element.InputType == InputType.SelectMenus)
            {
                throw new NotSupportedException("Discord.Net does not support components (yet). Use Discord.Net.Labs and Fergun.Interactive.Labs.");
            }
        }
#endif
    }
}