using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NetCord;
using NetCord.Rest;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents a component-based paginator. This is a new type of paginator that offers more flexibility than <see cref="Paginator"/> and supports components V2.
/// </summary>
[PublicAPI]
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
    IReadOnlyCollection<User> Users { get; }

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
    Func<IComponentPaginator, ModalProperties>? JumpModalFactory { get; }

    /// <summary>
    /// Gets the factory of the <see cref="IPage"/> that will be displayed ephemerally to a user when they are not allowed to interact with the paginator.
    /// </summary>
    Func<IComponentPaginator, IPage>? RestrictedPageFactory { get; }

    /// <summary>
    /// Sets the <see cref="CurrentPageIndex"/> to the specified page index, if it is valid.
    /// </summary>
    /// <param name="pageIndex">The page index.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="pageIndex"/> is not <see cref="CurrentPageIndex"/>, it's equal or higher than 0, or if it's lower than <see cref="PageCount"/>; otherwise, <see langword="false"/>.
    /// </returns>
    bool SetPage(int pageIndex);

    /// <summary>
    /// Returns a value indicating whether a button or select menu should be disabled based on the status of the paginator, and optionally an <paramref name="action"/>.
    /// </summary>
    /// <param name="action">The paginator action that the button or select menu represents.</param>
    /// <returns><see langword="true"/> if the component should be disabled; otherwise, <see langword="false"/>.</returns>
    bool ShouldDisable(PaginatorAction? action = null);

    /// <summary>
    /// Returns a value indicating whether this paginator owns the component (button, select menu or modal) with the specified custom ID.
    /// </summary>
    /// <param name="customId">The custom ID of the component.</param>
    /// <returns><see langword="true"/> if this paginator owns the component with the specified <paramref name="customId"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="customId"/> is <see langword="null"/>.</exception>
    bool OwnsComponent(string customId);

    /// <summary>
    /// Gets the custom ID for the specified <see cref="PaginatorAction"/>.
    /// </summary>
    /// <remarks>The custom ID is used to mark components as owned by this paginator.</remarks>
    /// <param name="action">The paginator action.</param>
    /// <returns>The custom ID.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="action"/> is invalid.</exception>
    string GetCustomId(PaginatorAction action);

    /// <summary>
    /// Handles an incoming interaction and applies an action on the message of the interaction.
    /// </summary>
    /// <param name="interaction">The interaction.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the operation. The result contains the status of the input.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="interaction"/> is <see langword="null"/>.</exception>
    ValueTask<InteractiveInputStatus> HandleInteractionAsync(MessageComponentInteraction interaction);

    /// <summary>
    /// Renders the specified <paramref name="page"/> or the current page of the paginator by responding to the interaction based on the given <paramref name="responseType"/>.
    /// </summary>
    /// <param name="interaction">The interaction to respond to.</param>
    /// <param name="responseType">
    /// The response type to use. This is used to determine how the interaction should be responded. Here's a list explaining the available response types:
    /// <list type="bullet">
    ///     <item>
    ///         <term><see cref="InteractionCallbackType.Message"/></term>
    ///         <description>Sends a new message using <c>Interaction.RespondWithFilesAsync</c> (requires a non-deferred interaction).</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="InteractionCallbackType.DeferredMessage"/></term>
    ///         <description>Sends a new message using <c>Interaction.FollowupWithFilesAsync</c> (requires a deferred interaction).</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="InteractionCallbackType.ModifyMessage"/></term>
    ///         <description>Updates the message where the interaction comes from using <c>MessageComponentInteraction.UpdateAsync</c> (requires a non-deferred <see cref="MessageComponentInteraction"/>).</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="InteractionCallbackType.DeferredModifyMessage"/></term>
    ///         <description>Updates the message where the interaction comes from using <c>MessageComponentInteraction.ModifyOriginalResponseAsync</c> (requires a deferred interaction).</description>
    ///     </item>
    /// </list>
    /// </param>
    /// <param name="isEphemeral">Whether the response message should be ephemeral. Ignored if responding to a non-ephemeral interaction.</param>
    /// <param name="page">A specific page to render.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the operation. The result contains an <see cref="RestMessage"/> object with the current page.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="interaction"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the value of <paramref name="responseType"/> is not supported.</exception>
    Task<RestMessage> RenderPageAsync(Interaction interaction, InteractionCallbackType responseType, bool isEphemeral, IPage? page = null);

    /// <summary>
    /// Renders the specified <paramref name="page"/> or the current page of the paginator by sending a new message.
    /// </summary>
    /// <param name="channel">The channel where the new message will be sent.</param>
    /// <param name="page">A specific page to render.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the operation. The result contains an <see cref="RestMessage"/> object with the current page.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="channel"/> is <see langword="null"/>.</exception>
    Task<RestMessage> RenderPageAsync(TextChannel channel, IPage? page = null);

    /// <summary>
    /// Renders the specified <paramref name="page"/> or the current page of the paginator by modifying an existing message.
    /// </summary>
    /// <param name="message">The message to modify.</param>
    /// <param name="page">A specific page to render.</param>
    /// <returns>A <see cref="Task"/> representing the operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is <see langword="null"/>.</exception>
    Task RenderPageAsync(RestMessage message, IPage? page = null);

    /// <summary>
    /// Responds to the interaction with a modal that allows the user to jump to a specific page of the paginator.
    /// </summary>
    /// <param name="interaction">The interaction that triggered the prompt.</param>
    /// <returns>A <see cref="Task"/> representing the operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="interaction"/> is <see langword="null"/>.</exception>
    Task SendJumpPromptAsync(MessageComponentInteraction interaction);

    /// <summary>
    /// Handles an incoming modal interaction and applies an action on the message of the interaction.
    /// </summary>
    /// <remarks>This is mainly used to receive jump-to-page requests.</remarks>
    /// <param name="interaction">The interaction.to respond to.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the operation. The result contains the status of the input.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="interaction"/> is <see langword="null"/>.</exception>
    ValueTask<InteractiveInputStatus> HandleModalInteractionAsync(ModalInteraction interaction);

    /// <summary>
    /// Applies an <see cref="ActionOnStop"/> on the message of the paginator based on the <see cref="Status"/>.
    /// </summary>
    /// <param name="message">The message containing the paginator.</param>
    /// <param name="stopInteraction">The interaction that was used to stop the paginator, if available.</param>
    /// <param name="deferInteraction">Whether to defer the interaction if the message isn't getting modified.</param>
    /// <returns>A <see cref="ValueTask"/> representing the operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is <see langword="null"/>.</exception>
    ValueTask ApplyActionOnStopAsync(RestMessage message, MessageComponentInteraction? stopInteraction, bool deferInteraction);
}