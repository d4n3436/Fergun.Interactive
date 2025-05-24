using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents a component-based paginator that can be used to paginate messages with buttons or select menus.
/// </summary>
public interface IComponentPaginator
{
    /// <summary>
    /// Gets the index of the current page of this paginator.
    /// </summary>
    int CurrentPageIndex { get; }

    /// <summary>
    /// Gets or sets the total number of pages. This can be manually updated.
    /// </summary>
    int PageCount { get; set; }

    /// <summary>
    /// Gets or sets the status of the paginator.
    /// </summary>
    /// <remarks>This is used to determine the state of components owned by the paginator and to determine what action to apply when the paginator is stopped.</remarks>
    PaginatorStatus Status { get; set; }

    /// <summary>
    /// Gets the factory of the <see cref="IPage"/> that will be displayed when the paginator is updated.
    /// </summary>
    /// <remarks>The first argument is the current paginator. <see cref="CurrentPageIndex"/> can be used to get the current page index.</remarks>
    Func<IComponentPaginator, ValueTask<IPage>> PageFactory { get; }

    /// <summary>
    /// Gets or sets the user state of the paginator. This can be used to store any user-defined data that needs to be retrieved elsewhere (like component interaction commands).
    /// </summary>
    object? UserState { get; set; }

    /// <summary>
    /// Gets a read-only collection of users who can interact with this paginator.
    /// </summary>
    IReadOnlyCollection<IUser> Users { get; }

    /// <summary>
    /// Gets the action that will be done after a cancellation.
    /// </summary>
    ActionOnStop ActionOnCancellation { get; }

    /// <summary>
    /// Gets the action that will be done after a timeout.
    /// </summary>
    ActionOnStop ActionOnTimeout { get; }

    /// <summary>
    /// Gets the behavior the paginator should exhibit when a user is not allowed to interact with it.
    /// </summary>
    RestrictedInputBehavior RestrictedInputBehavior { get; }

    /// <summary>
    /// Gets the <see cref="IPage"/> that will be displayed after cancellation.
    /// </summary>
    IPage? CanceledPage { get; }

    /// <summary>
    /// Gets the <see cref="IPage"/> that will be displayed after a timeout.
    /// </summary>
    IPage? TimeoutPage { get; }

    /// <summary>
    /// Gets the factory of the modal that will be displayed when the user clicks the jump button.
    /// </summary>
    Func<IComponentPaginator, ModalBuilder>? JumpModalFactory { get; }

    /// <summary>
    /// Gets the factory of the <see cref="IPage"/> that will be displayed ephemerally to a user when they are not allowed to interact with the paginator.
    /// </summary>
    Func<IComponentPaginator, IPage>? RestrictedPageFactory { get; }

    /// <summary>
    /// Sets the <see cref="CurrentPageIndex"/> to the specified page index, if it is valid.
    /// </summary>
    /// <param name="pageIndex">The page index.</param>
    /// <returns>
    /// <see langword="true"/> if <see cref="CurrentPageIndex"/> was updated; otherwise, <see langword="false"/>.
    /// It returns <see langword="false"/> if <paramref name="pageIndex"/> is already <see cref="CurrentPageIndex"/>, it's less than 0, or if the index is equal or higher than <see cref="PageCount"/>.
    /// </returns>
    bool SetPage(int pageIndex);

    /// <summary>
    /// Gets a value that indicates whether a button or select menu should be disabled based on the status of the paginator, and optionally an <paramref name="action"/>.
    /// </summary>
    /// <param name="action">The paginator action that the button or select menu represents.</param>
    /// <returns><see langword="true"/> if the component should be disabled; otherwise, <see langword="false"/>.</returns>
    bool ShouldDisable(PaginatorAction? action = null);

    /// <summary>
    /// Handles an incoming interaction and applies an action on the message of the interaction.
    /// </summary>
    /// <param name="interaction">The interaction.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the operation. The result contains the status of the input.</returns>
    ValueTask<InteractiveInputStatus> HandleInteractionAsync(IComponentInteraction interaction);

    /// <summary>
    /// Renders the specified <paramref name="page"/> or the current page of the paginator by responding to the interaction based on the given <paramref name="responseType"/>.
    /// </summary>
    /// <param name="interaction">The interaction to respond to.</param>
    /// <param name="responseType">
    /// The response type to use. This is used to determine how the interaction should be responded. Here's a list explaining the available response types:
    /// <list type="bullet">
    ///     <item>
    ///         <term><see cref="InteractionResponseType.ChannelMessageWithSource"/></term>
    ///         <description>Sends a new message using <c>IDiscordInteraction.RespondWithFilesAsync</c> (requires a non-deferred interaction).</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="InteractionResponseType.DeferredChannelMessageWithSource"/></term>
    ///         <description>Sends a new message using <c>IDiscordInteraction.FollowupWithFilesAsync</c> (requires a deferred interaction).</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="InteractionResponseType.UpdateMessage"/></term>
    ///         <description>Updates the message where the interaction comes from using <c>IComponentInteraction.UpdateAsync</c> (requires a non-deferred <see cref="IComponentInteraction"/>).</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="InteractionResponseType.DeferredUpdateMessage"/></term>
    ///         <description>Updates the message where the interaction comes from using <c>IComponentInteraction.ModifyOriginalResponseAsync</c> (requires a deferred interaction).</description>
    ///     </item>
    /// </list>
    /// </param>
    /// <param name="isEphemeral">Whether the response message should be ephemeral. Ignored if responding to a non-ephemeral interaction.</param>
    /// <param name="page">A specific page to render.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the operation. The result contains an <see cref="IUserMessage"/> object with the current page.</returns>
    Task<IUserMessage> RenderPageAsync(IDiscordInteraction interaction, InteractionResponseType responseType, bool isEphemeral, IPage? page = null);

    /// <summary>
    /// Renders the specified <paramref name="page"/> or the current page of the paginator by updating the message where the interaction comes from, using <see cref="IComponentInteraction.UpdateAsync(Action{MessageProperties}, RequestOptions)"/>.
    /// </summary>
    /// <param name="interaction">The interaction whose message will be updated.</param>
    /// <param name="page">A specific page to render.</param>
    /// <returns>A <see cref="Task"/> representing the operation.</returns>
    Task RenderPageAsync(IComponentInteraction interaction, IPage? page = null);

    /// <summary>
    /// Renders the specified <paramref name="page"/> or the current page of the paginator by sending a new message.
    /// </summary>
    /// <param name="channel">The channel where the new message will be sent.</param>
    /// <param name="page">A specific page to render.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the operation. The result contains an <see cref="IUserMessage"/> object with the current page.</returns>
    Task<IUserMessage> RenderPageAsync(IMessageChannel channel, IPage? page = null);

    /// <summary>
    /// Renders the specified <paramref name="page"/> or the current page of the paginator by modifying an existing message.
    /// </summary>
    /// <param name="message">The message to modify.</param>
    /// <param name="page">A specific page to render.</param>
    /// <returns>A <see cref="Task"/> representing the operation.</returns>
    Task RenderPageAsync(IUserMessage message, IPage? page = null);

    /// <summary>
    /// Responds to the interaction with a modal that allows the user to jump to a specific page of the paginator.
    /// </summary>
    /// <param name="interaction">The interaction that triggered the prompt.</param>
    /// <returns>A <see cref="Task"/> representing the operation.</returns>
    Task SendJumpPromptAsync(IComponentInteraction interaction);

    /// <summary>
    /// Handles an incoming modal interaction and applies an action on the message of the interaction.
    /// </summary>
    /// <remarks>This is mainly used to receive jump-to-page requests.</remarks>
    /// <param name="interaction">The interaction.to respond to.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the operation. The result contains the status of the input.</returns>
    ValueTask<InteractiveInputStatus> HandleModalInteractionAsync(IModalInteraction interaction);

    /// <summary>
    /// Applies an <see cref="ActionOnStop"/> on the message of the paginator based on the <see cref="Status"/>.
    /// </summary>
    /// <param name="message">The message containing the paginator.</param>
    /// <param name="stopInteraction">The interaction that was used to stop the paginator, if available.</param>
    /// <param name="deferInteraction">Whether to defer the interaction if the message isn't getting modified.</param>
    /// <returns>A <see cref="ValueTask"/> representing the operation.</returns>
    ValueTask ApplyActionOnStopAsync(IUserMessage message, IComponentInteraction? stopInteraction, bool deferInteraction);
}