using System;
using System.Threading.Tasks;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;


namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents an event handler for a paginator.
/// </summary>
internal sealed class PaginatorCallback : IInteractiveCallback
{
    private bool _disposed;

    public PaginatorCallback(Paginator paginator, RestMessage message,
        TimeoutTaskCompletionSource<InteractiveStatus> timeoutTaskSource,
        DateTimeOffset startTime, Interaction? initialInteraction = null)
    {
        Paginator = paginator;
        Message = message;
        TimeoutTaskSource = timeoutTaskSource;
        StartTime = startTime;
        LastInteraction = initialInteraction;
    }

    /// <summary>
    /// Gets the paginator.
    /// </summary>
    public Paginator Paginator { get; }

    /// <summary>
    /// Gets the message that contains the paginator.
    /// </summary>
    public RestMessage Message { get; }

    /// <summary>
    /// Gets the <see cref="TimeoutTaskCompletionSource{TResult}"/> used to set the result of the paginator.
    /// </summary>
    public TimeoutTaskCompletionSource<InteractiveStatus> TimeoutTaskSource { get; }

    /// <inheritdoc/>
    public DateTimeOffset StartTime { get; }

    /// <summary>
    /// Gets the last received interaction that is not <see cref="StopInteraction"/>.
    /// </summary>
    /// <remarks>For paginators, this is either the interaction that was received to update a message to a paginator or the interaction received to change the pages.</remarks>
    public Interaction? LastInteraction { get; private set; }

    /// <summary>
    /// Gets the message that was received to stop the paginator.
    /// </summary>
    public Message? StopMessage { get; private set; }

    /// <summary>
    /// Gets the reaction that was received to stop the paginator.
    /// </summary>
    public MessageReactionAddEventArgs? StopReaction { get; private set; }

    /// <summary>
    /// Gets the interaction that was received to stop the paginator.
    /// </summary>
    public MessageComponentInteraction? StopInteraction { get; private set; }

    /// <inheritdoc/>
    public void Cancel() => TimeoutTaskSource.TryCancel();

    /// <inheritdoc/>
    public async Task ExecuteAsync(Message message)
    {
        var result = await Paginator.HandleMessageAsync(message, Message).ConfigureAwait(false);
        switch (result.Status)
        {
            case InteractiveInputStatus.Success:
                TimeoutTaskSource.TryReset();
                break;

            case InteractiveInputStatus.Canceled:
                StopMessage = message;
                Cancel();
                break;
        }
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(MessageReactionAddEventArgs reaction)
    {
        var result = await Paginator.HandleReactionAsync(reaction, Message).ConfigureAwait(false);
        switch (result.Status)
        {
            case InteractiveInputStatus.Success:
                TimeoutTaskSource.TryReset();
                break;

            case InteractiveInputStatus.Canceled:
                StopReaction = reaction;
                Cancel();
                break;
        }
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(Interaction interaction)
    {
        if (interaction is ModalInteraction modal)
        {
            await Paginator.HandleModalAsync(modal, Message).ConfigureAwait(false);
        }

        if (interaction is not MessageComponentInteraction component)
        {
            return;
        }

        var result = await Paginator.HandleInteractionAsync(component, Message).ConfigureAwait(false);

        switch (result.Status)
        {
            case InteractiveInputStatus.Success:
                LastInteraction = component;
                TimeoutTaskSource.TryReset();
                break;

            case InteractiveInputStatus.Canceled:
                StopInteraction = component;
                Cancel();
                break;
        }
    }

    /// <inheritdoc/>
    public void Dispose() => Dispose(disposing: true);

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            TimeoutTaskSource.Dispose();
        }

        _disposed = true;
    }
}