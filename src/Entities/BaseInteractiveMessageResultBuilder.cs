using System;
using Discord;
using Discord.WebSocket;

namespace Fergun.Interactive;

internal abstract class BaseInteractiveMessageResultBuilder<TBuilder, TResult>
    where TBuilder : BaseInteractiveMessageResultBuilder<TBuilder, TResult>
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

    public TBuilder WithElapsed(TimeSpan elapsed)
    {
        Elapsed = elapsed;
        return (TBuilder)this;
    }

    public TBuilder WithStatus(InteractiveStatus status)
    {
        Status = status;
        return (TBuilder)this;
    }

    public TBuilder WithMessage(IUserMessage message)
    {
        Message = message;
        return (TBuilder)this;
    }

    public TBuilder WithUser(IUser? user)
    {
        User = user;
        return (TBuilder)this;
    }

    public TBuilder WithStopMessage(IMessage? stopMessage)
    {
        StopMessage = stopMessage;
        return (TBuilder)this;
    }

    public TBuilder WithStopReaction(SocketReaction? stopReaction)
    {
        StopReaction = stopReaction;
        return (TBuilder)this;
    }

    public TBuilder WithStopInteraction(IComponentInteraction? stopInteraction)
    {
        StopInteraction = stopInteraction;
        return (TBuilder)this;
    }
}

internal abstract class BaseInteractiveMessageResultBuilder<TValue, TBuilder, TResult> : BaseInteractiveMessageResultBuilder<TBuilder, TResult>
    where TBuilder : BaseInteractiveMessageResultBuilder<TValue, TBuilder, TResult>
    where TResult : IInteractiveMessageResult
{
    public TValue? Value { get; set; }

    public TBuilder WithValue(TValue? value)
    {
        Value = value;
        return (TBuilder)this;
    }
}