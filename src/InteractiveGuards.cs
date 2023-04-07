using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Discord;
using Discord.WebSocket;

namespace Fergun.Interactive;

internal static class InteractiveGuards
{
    public static void NotNull<T>(T? obj, [CallerArgumentExpression(nameof(obj))] string? parameterName = null) where T : class
    {
        if (obj is null)
        {
            throw new ArgumentNullException(parameterName);
        }
    }

    public static void NotEmpty<T>(ICollection<T> collection, [CallerArgumentExpression(nameof(collection))] string? parameterName = null)
    {
        if (collection.Count == 0)
        {
            throw new ArgumentException("Collection must not be empty.", parameterName);
        }
    }

    public static void NotCanceled(CancellationToken cancellationToken, [CallerArgumentExpression(nameof(cancellationToken))] string? parameterName = null)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new ArgumentException("Cancellation token must not be canceled.", parameterName);
        }
    }

    public static void NoDuplicates<TOption>(ICollection<TOption> collection, IEqualityComparer<TOption> equalityComparer,
        [CallerArgumentExpression(nameof(collection))] string? parameterName = null)
    {
        if (collection.Distinct(equalityComparer).Count() != collection.Count)
        {
            throw new ArgumentException("Collection must not contain duplicate elements.", parameterName);
        }
    }

    public static void IndexInRange<T>(ICollection<T> collection, int index, [CallerArgumentExpression(nameof(index))] string? parameterName = null)
    {
        if (index < 0 || index >= collection.Count)
        {
            throw new ArgumentOutOfRangeException(parameterName, index, $"Index must be greater than or equal to 0 and lower than {collection.Count}.");
        }
    }

    public static void ExpectedType<TInput, TExpected>(TInput obj, out TExpected expected, [CallerArgumentExpression(nameof(obj))] string? parameterName = null)
    {
        if (obj is not TExpected temp)
        {
            throw new ArgumentException($"Parameter must be of type {typeof(TExpected)}.", parameterName);
        }

        expected = temp;
    }

    public static void EmbedCountInRange(ICollection<EmbedBuilder> builders, bool ensureMaxCapacity = false, [CallerArgumentExpression(nameof(builders))] string? parameterName = null)
    {
        if (builders.Count > 10 || ensureMaxCapacity && builders.Count + 1 > 10)
        {
            throw new ArgumentException("A page cannot have more than 10 embeds.", parameterName);
        }
    }

    public static void MessageFromCurrentUser(BaseSocketClient client, IUserMessage? message, [CallerArgumentExpression(nameof(message))] string? parameterName = null)
    {
        if (message is not null && message.Author.Id != client.CurrentUser.Id)
        {
            throw new ArgumentException("Message author must be the current user.", parameterName);
        }
    }

    public static void DeleteAndDisableInputNotSet(ActionOnStop action, [CallerArgumentExpression(nameof(action))] string? parameterName = null)
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
        => SupportedInputType(element.InputType, ephemeral);

    public static void SupportedInputType(InputType inputType, bool ephemeral, [CallerArgumentExpression(nameof(inputType))] string? parameterName = null)
    {
        if (inputType == 0)
        {
            throw new ArgumentException("At least one input type must be set.", parameterName);
        }

        if (ephemeral && inputType.HasFlag(InputType.Reactions))
        {
            throw new NotSupportedException("Ephemeral messages cannot use reactions as input.");
        }
    }

    // The paginators included in the library don't support messages or select menus as input
    public static void SupportedPaginatorInputType(InputType inputType)
    {
        if (inputType.HasFlag(InputType.Messages))
        {
            throw new NotSupportedException("This paginator doesn't support using messages as inputs.");
        }

        if (inputType.HasFlag(InputType.SelectMenus))
        {
            throw new NotSupportedException("This paginator doesn't support using select menus as input.");
        }
    }

    public static void RequiredEmoteConverter<TOption>(InputType inputType, Func<TOption, IEmote>? emoteConverter, [CallerArgumentExpression(nameof(emoteConverter))] string? parameterName = null)
    {
        if (inputType.HasFlag(InputType.Reactions) && emoteConverter is null)
        {
            throw new ArgumentNullException(parameterName, $"{parameterName} is required when {nameof(inputType)} has InputType.Reactions.");
        }
    }

    public static void ValidResponseType(InteractionResponseType responseType, [CallerArgumentExpression(nameof(responseType))] string? parameterName = null)
    {
        int value = (int)responseType;
        if (value is >= 1 and <= 3)
        {
            throw new ArgumentException("Invalid response type.", parameterName);
        }
    }

    public static void ValidResponseType(InteractionResponseType responseType, IDiscordInteraction interaction, [CallerArgumentExpression(nameof(responseType))] string? parameterName = null)
    {
        if (interaction is not IComponentInteraction &&
            responseType is InteractionResponseType.DeferredUpdateMessage or InteractionResponseType.UpdateMessage)
        {
            throw new ArgumentException($"Interaction response type {responseType} can only be used on component interactions.", parameterName);
        }
    }
}