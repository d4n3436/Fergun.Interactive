using System.Collections.Generic;
using Fergun.Interactive.Extensions;
using Fergun.Interactive.Pagination;
using Fergun.Interactive.Selection;

namespace Fergun.Interactive;

internal sealed class InteractiveMessageResultBuilder<T> : BaseInteractiveMessageResultBuilder<T, InteractiveMessageResultBuilder<T>, InteractiveMessageResult<T>>
{
    public static InteractiveMessageResultBuilder<T> FromCallback<TOption>(SelectionCallback<TOption> callback, IReadOnlyList<T> options, InteractiveStatus status)
    {
        var user = callback.StopMessage?.Author ?? callback.StopReaction?.User ?? callback.StopInteraction?.User;

        return new InteractiveMessageResultBuilder<T>()
            .WithValues(options)
            .WithElapsed(callback.GetElapsedTime(status))
            .WithStatus(status)
            .WithMessage(callback.Message)
            .WithUser(user)
            .WithStopMessage(callback.StopMessage)
            .WithStopReaction(callback.StopReaction)
            .WithStopInteraction(callback.StopInteraction);
    }

    public override InteractiveMessageResult<T> Build() => new(this);
}

internal sealed class InteractiveMessageResultBuilder : BaseInteractiveMessageResultBuilder<InteractiveMessageResultBuilder, InteractiveMessageResult>
{
    public static InteractiveMessageResultBuilder FromCallback(PaginatorCallback callback, InteractiveStatus status)
    {
        var user = callback.StopMessage?.Author ?? callback.StopReaction?.User ?? callback.StopInteraction?.User;

        return new InteractiveMessageResultBuilder()
            .WithElapsed(callback.GetElapsedTime(status))
            .WithStatus(status)
            .WithMessage(callback.Message)
            .WithUser(user)
            .WithStopMessage(callback.StopMessage)
            .WithStopReaction(callback.StopReaction)
            .WithStopInteraction(callback.StopInteraction);
    }

    public static InteractiveMessageResultBuilder FromCallback(ComponentPaginatorCallback callback, InteractiveStatus status)
    {
        var user = callback.StopInteraction?.User;

        return new InteractiveMessageResultBuilder()
            .WithElapsed(callback.GetElapsedTime(status))
            .WithStatus(status)
            .WithMessage(callback.Message)
            .WithUser(user)
            .WithStopInteraction(callback.StopInteraction);
    }

    public override InteractiveMessageResult Build() => new(this);
}