using JetBrains.Annotations;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace Fergun.Interactive;

/// <summary>
/// Represents a generic result of an interactive action containing a message associated with the action, the user and input that ended the action.
/// </summary>
/// <typeparam name="T">The type of the value or values of this result.</typeparam>
[PublicAPI]
public class InteractiveMessageResult<T> : InteractiveResult<T>, IInteractiveMessageResult
{
    internal InteractiveMessageResult(InteractiveMessageResultBuilder<T> builder)
        : base(builder.Values, builder.Elapsed, builder.Status)
    {
        Message = builder.Message;
        User = builder.User;
        StopMessage = builder.StopMessage;
        StopReaction = builder.StopReaction;
        StopInteraction = builder.StopInteraction;
    }

    /// <inheritdoc/>
    public RestMessage Message { get; }

    /// <inheritdoc/>
    public User? User { get; }

    /// <inheritdoc/>
    public Message? StopMessage { get; }

    /// <inheritdoc/>
    public MessageReactionAddEventArgs? StopReaction { get; }

    /// <inheritdoc/>
    public MessageComponentInteraction? StopInteraction { get; }
}

/// <summary>
/// Represents a result of an interactive action containing a message associated with the action, the user and input that ended the action.
/// </summary>
[PublicAPI]
public class InteractiveMessageResult : InteractiveResult, IInteractiveMessageResult
{
    internal InteractiveMessageResult(InteractiveMessageResultBuilder builder)
        : base(builder.Elapsed, builder.Status)
    {
        Message = builder.Message;
        User = builder.User;
        StopMessage = builder.StopMessage;
        StopReaction = builder.StopReaction;
        StopInteraction = builder.StopInteraction;
    }

    /// <inheritdoc/>
    public RestMessage Message { get; }

    /// <inheritdoc/>
    public User? User { get; }

    /// <inheritdoc/>
    public Message? StopMessage { get; }

    /// <inheritdoc/>
    public MessageReactionAddEventArgs? StopReaction { get; }

    /// <inheritdoc/>
    public MessageComponentInteraction? StopInteraction { get; }
}