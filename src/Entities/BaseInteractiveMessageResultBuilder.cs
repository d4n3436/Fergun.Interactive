using System;
using System.Collections.Generic;


using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace Fergun.Interactive;

internal abstract class BaseInteractiveMessageResultBuilder<TSelf, TResult>
    where TSelf : BaseInteractiveMessageResultBuilder<TSelf, TResult>
    where TResult : IInteractiveMessageResult
{
    public TimeSpan Elapsed { get; set; }

    public InteractiveStatus Status { get; set; } = InteractiveStatus.Success;

    public RestMessage Message { get; set; } = null!;

    public NetCord.User? User { get; set; }

    public Message? StopMessage { get; set; }

    public MessageReactionAddEventArgs? StopReaction { get; set; }

    public MessageComponentInteraction? StopInteraction { get; set; }

    public abstract TResult Build();

    public TSelf WithElapsed(TimeSpan elapsed)
    {
        Elapsed = elapsed;
        return (TSelf)this;
    }

    public TSelf WithStatus(InteractiveStatus status)
    {
        Status = status;
        return (TSelf)this;
    }

    public TSelf WithMessage(RestMessage message)
    {
        Message = message;
        return (TSelf)this;
    }

    public TSelf WithUser(User? user)
    {
        User = user;
        return (TSelf)this;
    }

    public TSelf WithStopMessage(Message? stopMessage)
    {
        StopMessage = stopMessage;
        return (TSelf)this;
    }

    public TSelf WithStopReaction(MessageReactionAddEventArgs? stopReaction)
    {
        StopReaction = stopReaction;
        return (TSelf)this;
    }

    public TSelf WithStopInteraction(MessageComponentInteraction? stopInteraction)
    {
        StopInteraction = stopInteraction;
        return (TSelf)this;
    }
}

internal abstract class BaseInteractiveMessageResultBuilder<TValue, TSelf, TResult> : BaseInteractiveMessageResultBuilder<TSelf, TResult>
    where TSelf : BaseInteractiveMessageResultBuilder<TValue, TSelf, TResult>
    where TResult : IInteractiveMessageResult
{
    public IReadOnlyList<TValue> Values { get; set; } = [];

    public TSelf WithValues(IReadOnlyList<TValue> values)
    {
        Values = values;
        return (TSelf)this;
    }
}