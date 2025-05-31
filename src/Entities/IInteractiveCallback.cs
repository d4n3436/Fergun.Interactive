using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using JetBrains.Annotations;

namespace Fergun.Interactive;

/// <summary>
/// Represents a generic event handler for incoming inputs.
/// </summary>
/// <typeparam name="TInput">The type of the input.</typeparam>
[PublicAPI]
public interface IInteractiveCallback<in TInput> : IInteractiveCallback
{
    /// <summary>
    /// Executes this event.
    /// </summary>
    /// <param name="input">The incoming input.</param>
    /// <returns>A task that represents the operation.</returns>
    Task ExecuteAsync(TInput input);
}

/// <summary>
/// Represents an event handler for incoming socket events.
/// </summary>
[PublicAPI]
public interface IInteractiveCallback : IDisposable
{
    /// <summary>
    /// Gets the start time.
    /// </summary>
    DateTimeOffset StartTime { get; }

    /// <summary>
    /// Cancels the element associated to this callback.
    /// </summary>
    void Cancel();

    /// <summary>
    /// Executes this event.
    /// </summary>
    /// <param name="message">The incoming message.</param>
    /// <returns>A task that represents the operation.</returns>
    Task ExecuteAsync(SocketMessage message);

    /// <summary>
    /// Executes this event.
    /// </summary>
    /// <param name="reaction">The incoming reaction.</param>
    /// <returns>A task that represents the operation.</returns>
    Task ExecuteAsync(SocketReaction reaction);

    /// <summary>
    /// Executes this event.
    /// </summary>
    /// <param name="interaction">The incoming interaction.</param>
    /// <returns>A task that represents the operation.</returns>
    Task ExecuteAsync(SocketInteraction interaction);
}