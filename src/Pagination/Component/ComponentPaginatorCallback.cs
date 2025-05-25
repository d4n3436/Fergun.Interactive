using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Provides an event handler for a component paginator.
/// </summary>
internal sealed class ComponentPaginatorCallback : IInteractiveCallback
{
    private bool _disposed;

    public ComponentPaginatorCallback(IComponentPaginator paginator, IUserMessage message,
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
    public IComponentPaginator Paginator { get; }

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
    /// <remarks>This is either the interaction that was received to update a message to a paginator or the interaction received to change the pages.</remarks>
    public IDiscordInteraction? LastInteraction { get; private set; }

    /// <summary>
    /// Gets the interaction that was received to stop the paginator.
    /// </summary>
    public SocketMessageComponent? StopInteraction { get; private set; }

    /// <inheritdoc/>
    public void Cancel() => TimeoutTaskSource.TryCancel();

    /// <inheritdoc />
    Task IInteractiveCallback.ExecuteAsync(SocketMessage message) => Task.CompletedTask;

    /// <inheritdoc />
    Task IInteractiveCallback.ExecuteAsync(SocketReaction reaction) => Task.CompletedTask;

    /// <inheritdoc />
    public async Task ExecuteAsync(SocketInteraction interaction)
    {
        if (interaction is IModalInteraction modalInteraction)
        {
            await Paginator.HandleModalInteractionAsync(modalInteraction).ConfigureAwait(false);
            return;
        }

        if (interaction is not IComponentInteraction component)
        {
            return;
        }

        var status = await Paginator.HandleInteractionAsync(component).ConfigureAwait(false);

        switch (status)
        {
            case InteractiveInputStatus.Success:
                LastInteraction = component;
                TimeoutTaskSource.TryReset();
                break;

            case InteractiveInputStatus.Canceled:
                StopInteraction = component as SocketMessageComponent;
                Cancel();
                break;

            case InteractiveInputStatus.Ignored:
            default:
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