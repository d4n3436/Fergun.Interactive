using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents an event handler for a paginator.
/// </summary>
internal sealed class PaginatorCallback : IInteractiveCallback
{
    private bool _disposed;

    public PaginatorCallback(Paginator paginator, IUserMessage message,
        TimeoutTaskCompletionSource<InteractiveStatus> timeoutTaskSource,
        DateTimeOffset startTime, IDiscordInteraction? initialInteraction = null)
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
    public IUserMessage Message { get; }

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
    public IDiscordInteraction? LastInteraction { get; private set; }

    /// <summary>
    /// Gets the messages that was received to stop the paginator.
    /// </summary>
    public IMessage? StopMessage { get; private set; }

    /// <summary>
    /// Gets the reaction that was received to stop the paginator.
    /// </summary>
    public SocketReaction? StopReaction { get; private set; }

    /// <summary>
    /// Gets the interaction that was received to stop the paginator.
    /// </summary>
    public SocketMessageComponent? StopInteraction { get; private set; }

    /// <inheritdoc/>
    public void Cancel() => TimeoutTaskSource.TryCancel();

    /// <inheritdoc/>
    public async Task ExecuteAsync(SocketMessage message)
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

            case InteractiveInputStatus.Ignored:
            default:
                break;
        }
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(SocketReaction reaction)
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

            case InteractiveInputStatus.Ignored:
            default:
                break;
        }
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(SocketInteraction interaction)
    {
        if (interaction is SocketModal modal)
        {
            await Paginator.HandleModalAsync(modal, Message).ConfigureAwait(false);
        }

        if (interaction is not SocketMessageComponent component)
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

            case InteractiveInputStatus.Ignored:
            default:
                break;
        }
    }

    /// <inheritdoc/>
    public void Dispose() => Dispose(true);

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