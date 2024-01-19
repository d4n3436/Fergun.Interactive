using System;
using Discord;
using Discord.WebSocket;

namespace Fergun.Interactive;

internal abstract class BaseInteractiveMessageResultBuilder<TSelf, TResult>
    where TSelf : BaseInteractiveMessageResultBuilder<TSelf, TResult>
    where TResult : IInteractiveMessageResult
{
    public TimeSpan Elapsed { get; set; }

    public InteractiveStatus Status { get; set; } = InteractiveStatus.Success;

    public IUserMessage Message { get; set; } = null!;

    public IUser? User { get; set; }

    public IMessage? StopMessage { get; set; }

    public SocketReaction? StopReaction { get; set; }

    public IComponentInteraction? StopInteraction { get; set; }

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

    public TSelf WithMessage(IUserMessage message)
    {
        Message = message;
        return (TSelf)this;
    }

    public TSelf WithUser(IUser? user)
    {
        User = user;
        return (TSelf)this;
    }

    public TSelf WithStopMessage(IMessage? stopMessage)
    {
        StopMessage = stopMessage;
        return (TSelf)this;
    }

    public TSelf WithStopReaction(SocketReaction? stopReaction)
    {
        StopReaction = stopReaction;
        return (TSelf)this;
    }

    public TSelf WithStopInteraction(IComponentInteraction? stopInteraction)
    {
        StopInteraction = stopInteraction;
        return (TSelf)this;
    }
}

internal abstract class BaseInteractiveMessageResultBuilder<TValue, TSelf, TResult> : BaseInteractiveMessageResultBuilder<TSelf, TResult>
    where TSelf : BaseInteractiveMessageResultBuilder<TValue, TSelf, TResult>
    where TResult : IInteractiveMessageResult
{
    public TValue? Value { get; set; }

    public TSelf WithValue(TValue? value)
    {
        Value = value;
        return (TSelf)this;
    }
}