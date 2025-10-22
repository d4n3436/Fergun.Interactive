using System.Threading.Tasks;

using JetBrains.Annotations;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace Fergun.Interactive;

/// <summary>
/// Provides methods for handling inputs in interactive entities.
/// </summary>
[PublicAPI]
public interface IInteractiveInputHandler
{
    /// <summary>
    /// Handles a message.
    /// </summary>
    /// <param name="input">The message to handle.</param>
    /// <param name="message">The message containing the interactive element.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing the result.</returns>
    Task<IInteractiveResult<InteractiveInputStatus>> HandleMessageAsync(Message input, RestMessage message);

    /// <summary>
    /// Handles a reaction.
    /// </summary>
    /// <param name="input">The reaction to handle.</param>
    /// <param name="message">The message containing the interactive element.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing the result.</returns>
    Task<IInteractiveResult<InteractiveInputStatus>> HandleReactionAsync(MessageReactionAddEventArgs input, RestMessage message);

    /// <summary>
    /// Handles a component interaction.
    /// </summary>
    /// <param name="input">The component interaction to handle.</param>
    /// <param name="message">The message containing the interactive element.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing the result.</returns>
    Task<IInteractiveResult<InteractiveInputStatus>> HandleInteractionAsync(MessageComponentInteraction input, RestMessage message);
}