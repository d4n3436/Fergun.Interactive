using Discord;
using Discord.WebSocket;

namespace Fergun.Interactive
{
    /// <summary>
    /// Represents a generic result of an interactive action containing a message associated with the action, the user and input that ended the action.
    /// </summary>
    public class InteractiveMessageResult<T> : InteractiveResult<T>, IInteractiveMessageResult
    {
        internal InteractiveMessageResult(InteractiveMessageResultBuilder<T> builder)
            : base(builder.Value, builder.Elapsed, builder.Status)
        {
            Message = builder.Message;
            User = builder.User;
            StopMessage = builder.StopMessage;
            StopReaction = builder.StopReaction;
            StopInteraction = builder.StopInteraction;
        }

        /// <inheritdoc/>
        public IUserMessage Message { get; }

        /// <inheritdoc/>
        public IUser? User { get; }

        /// <inheritdoc/>
        public IMessage? StopMessage { get; }

        /// <inheritdoc/>
        public SocketReaction? StopReaction { get; }

        /// <inheritdoc/>
        public IComponentInteraction? StopInteraction { get; }
    }

    /// <summary>
    /// Represents a non-generic result of an interactive action containing a message associated with the action.
    /// </summary>
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
        public IUserMessage Message { get; }

        /// <inheritdoc/>
        public IUser? User { get; }

        /// <inheritdoc/>
        public IMessage? StopMessage { get; }

        /// <inheritdoc/>
        public SocketReaction? StopReaction { get; }

        /// <inheritdoc/>
        public IComponentInteraction? StopInteraction { get; }
    }
}