using Discord;

namespace Fergun.Interactive
{
    /// <summary>
    /// Represents a result from an interactive action containing a message associated with the action.
    /// </summary>
    public interface IInteractiveMessageResult : IInteractiveResult<InteractiveStatus>, IElapsed
    {
        /// <summary>
        /// Gets the message this interactive result comes from.
        /// </summary>
        public IUserMessage Message { get; }
    }
}