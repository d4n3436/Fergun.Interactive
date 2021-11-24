using System;
using Discord;
using Discord.WebSocket;
using Fergun.Interactive.Pagination;

namespace Fergun.Interactive
{
    internal static class InteractiveGuards
    {
        public static void NotNull<T>(T? obj, string parameterName) where T : class
        {
            if (obj is null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        public static void MessageFromCurrentUser(BaseSocketClient client, IUserMessage? message, string parameterName)
        {
            if (message is not null && message.Author.Id != client.CurrentUser.Id)
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

        public static void SupportedInputType<TOption>(IInteractiveElement<TOption> element, bool ephemeral)
        {
            if (element.InputType == 0)
            {
                throw new ArgumentException("At least one input type must be set.");
            }

            if (ephemeral && element.InputType.HasFlag(InputType.Reactions))
            {
                throw new NotSupportedException("Ephemeral messages cannot use reactions as input.");
            }

            if (element is Paginator paginator)
            {
                if (paginator.InputType.HasFlag(InputType.Messages))
                {
                    throw new NotSupportedException("Paginators using messages as input are not supported (yet).");
                }

                if (paginator.InputType.HasFlag(InputType.SelectMenus))
                {
                    throw new NotSupportedException("Paginators using select menus as input are not supported (yet).");
                }
            }
        }

        public static void ValidResponseType(InteractionResponseType responseType, string parameterName)
        {
            int value = (int)responseType;
            if (value is >= 1 and <= 3)
            {
                throw new ArgumentException("Invalid response type.", parameterName);
            }
        }

        public static void ValidResponseType(InteractionResponseType responseType, SocketInteraction interaction, string parameterName)
        {
            if (interaction is not SocketMessageComponent &&
                responseType is InteractionResponseType.DeferredUpdateMessage or InteractionResponseType.UpdateMessage)
            {
                throw new ArgumentException($"Interaction response type {responseType} can only be used on component interactions.", parameterName);
            }
        }
    }
}