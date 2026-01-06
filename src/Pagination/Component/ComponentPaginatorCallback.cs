using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Provides an event handler for a component paginator.
/// </summary>
internal sealed class ComponentPaginatorCallback : IInteractiveCallback
{
    private bool _disposed;

    public ComponentPaginatorCallback(IComponentPaginator paginator, RestMessage message,
        TimeoutTaskCompletionSource<InteractiveStatus> timeoutTaskSource, DateTimeOffset startTime)
    {
        Paginator = paginator;
        Message = message;
        TimeoutTaskSource = timeoutTaskSource;
        StartTime = startTime;
    }

    /// <summary>
    /// Gets the paginator.
    /// </summary>
    public IComponentPaginator Paginator { get; }

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
    /// Gets the interaction that was received to stop the paginator.
    /// </summary>
    public MessageComponentInteraction? StopInteraction { get; private set; }

    /// <inheritdoc/>
    public void Cancel() => TimeoutTaskSource.TryCancel();

    /// <inheritdoc />
    Task IInteractiveCallback.ExecuteAsync(Message message) => Task.CompletedTask;

    /// <inheritdoc />
    Task IInteractiveCallback.ExecuteAsync(MessageReactionAddEventArgs reaction) => Task.CompletedTask;

    /// <inheritdoc />
    public async Task ExecuteAsync(Interaction interaction)
    {
        if (interaction is ModalInteraction modalInteraction)
        {
            await Paginator.HandleModalInteractionAsync(modalInteraction).ConfigureAwait(false);
            return;
        }

        if (interaction is not MessageComponentInteraction component)
        {
            return;
        }

        var status = await Paginator.HandleInteractionAsync(component).ConfigureAwait(false);

        switch (status)
        {
            case InteractiveInputStatus.Success:
                TimeoutTaskSource.TryReset();
                break;

            case InteractiveInputStatus.Canceled:
                StopInteraction = component;
                Cancel();
                break;

            case InteractiveInputStatus.Ignored:
                break;
            default:
                throw new UnreachableException();
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