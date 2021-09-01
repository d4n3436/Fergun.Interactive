using System;
using Discord;

namespace Fergun.Interactive
{
    /// <summary>
    /// Represents a generic result from an interactive action containing a message associated with the action.
    /// </summary>
    public class InteractiveMessageResult<T> : InteractiveResult<T>, IInteractiveMessageResult
    {
        internal InteractiveMessageResult(T? value, TimeSpan elapsed, IUserMessage message,
            InteractiveStatus status = InteractiveStatus.Success)
            : base (value, elapsed, status)
        {
            Message = message;
        }

        /// <inheritdoc/>
        public IUserMessage Message { get; }
    }

    /// <summary>
    /// Represents a non-generic result from an interactive action containing a message associated with the action.
    /// </summary>
    public class InteractiveMessageResult : InteractiveResult, IInteractiveMessageResult
    {
        internal InteractiveMessageResult(TimeSpan elapsed, IUserMessage message,
            InteractiveStatus status = InteractiveStatus.Success)
            : base(elapsed, status)
        {
            Message = message;
        }

        /// <inheritdoc/>
        public IUserMessage Message { get; }
    }
}