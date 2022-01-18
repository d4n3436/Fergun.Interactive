using System.Threading.Tasks;
using Discord;

namespace Fergun.Interactive
{
    /// <summary>
    /// Provides methods for handling inputs in interactive entities.
    /// </summary>
    public interface IInteractiveInputHandler
    {
        /// <summary>
        /// Handles a message.
        /// </summary>
        /// <param name="input">The message to handle.</param>
        /// <param name="message">The message containing the interactive element.</param>
        /// <returns>A <see cref="Task{TResult}"/> containing the result.</returns>
        Task<IInteractiveResult<InteractiveInputStatus>> HandleMessageAsync(IMessage input, IUserMessage message);

        /// <summary>
        /// Handles a reaction.
        /// </summary>
        /// <param name="input">The reaction to handle.</param>
        /// <param name="message">The message containing the interactive element.</param>
        /// <returns>A <see cref="Task{TResult}"/> containing the result.</returns>
        Task<IInteractiveResult<InteractiveInputStatus>> HandleReactionAsync(IReaction input, IUserMessage message);

        /// <summary>
        /// Handles a component interaction.
        /// </summary>
        /// <param name="input">The component interaction to handle.</param>
        /// <param name="message">The message containing the interactive element.</param>
        /// <returns>A <see cref="Task{TResult}"/> containing the result.</returns>
        Task<IInteractiveResult<InteractiveInputStatus>> HandleInteractionAsync(IComponentInteraction input, IUserMessage message);
    }
}