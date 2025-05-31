using System.Threading;
using Discord;
using Discord.WebSocket;
using JetBrains.Annotations;

namespace Fergun.Interactive;

/// <summary>
/// Represents a result of an interactive action containing a message associated with the action, the user and input that ended the action.
/// </summary>
[PublicAPI]
public interface IInteractiveMessageResult : IInteractiveResult<InteractiveStatus>, IElapsed
{
    /// <summary>
    /// Gets the message this interactive result comes from.
    /// </summary>
    IUserMessage Message { get; }

    /// <summary>
    /// Gets the user that caused the interactive action to end, if the result was successful.
    /// </summary>
    /// <remarks>This is not present if there was no input that ended this action, e.g. a timeout or a cancellation with a <see cref="CancellationToken"/>.</remarks>
    IUser? User { get; }

    /// <summary>
    /// Gets the message that caused the interactive action to end.
    /// </summary>
    /// <remarks>
    /// This is not present if:<br/>
    /// - There was no input that ended this action, e.g. a timeout or a cancellation with a <see cref="CancellationToken"/>.<br/>
    /// - The action was ended with other input type.
    /// </remarks>
    IMessage? StopMessage { get; }

    /// <summary>
    /// Gets the reaction that caused the interactive action to end.
    /// </summary>
    /// <remarks>
    /// This is not present if:<br/>
    /// - There was no input that ended this action, e.g. a timeout or a cancellation with a <see cref="CancellationToken"/>.<br/>
    /// - The action was ended with other input type.
    /// </remarks>
    SocketReaction? StopReaction { get; }

    /// <summary>
    /// Gets the component interaction that caused the interactive action to end.
    /// </summary>
    /// <remarks>
    /// This is not present if:<br/>
    /// - There was no input that ended this action, e.g. a timeout or a cancellation with a <see cref="CancellationToken"/>.<br/>
    /// - The action was ended with other input type.
    /// </remarks>
    IComponentInteraction? StopInteraction { get; }
}