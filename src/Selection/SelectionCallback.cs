using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Fergun.Interactive.Selection;

/// <summary>
/// Represents an event handler for a selection.
/// </summary>
/// <typeparam name="TOption">The type of the options of the selection.</typeparam>
internal sealed class SelectionCallback<TOption> : IInteractiveCallback
{
    private bool _disposed;

    public SelectionCallback(BaseSelection<TOption> selection, IUserMessage message,
        TimeoutTaskCompletionSource<(TOption?, InteractiveStatus)> timeoutTaskSource,
        DateTimeOffset startTime, SocketInteraction? initialInteraction = null)
    {
        Selection = selection;
        Message = message;
        TimeoutTaskSource = timeoutTaskSource;
        StartTime = startTime;
        LastInteraction = initialInteraction;
    }

    /// <summary>
    /// Gets the selection.
    /// </summary>
    public BaseSelection<TOption> Selection { get; }

    /// <summary>
    /// Gets the message that contains the selection.
    /// </summary>
    public IUserMessage Message { get; }

    /// <summary>
    /// Gets the <see cref="TimeoutTaskCompletionSource{TResult}"/> used to set the result of the selection.
    /// </summary>
    public TimeoutTaskCompletionSource<(TOption?, InteractiveStatus)> TimeoutTaskSource { get; }

    /// <inheritdoc/>
    public DateTimeOffset StartTime { get; }

    /// <summary>
    /// Gets the last received interaction that is not <see cref="StopInteraction"/>.
    /// </summary>
    /// <remarks>For selections, this is the interaction that was received to update a message to a selection.</remarks>
    public SocketInteraction? LastInteraction { get; }

    /// <summary>
    /// Gets the messages that was received to stop the selection.
    /// </summary>
    public IMessage? StopMessage { get; private set; }

    /// <summary>
    /// Gets the reaction that was received to stop the selection.
    /// </summary>
    public SocketReaction? StopReaction { get; private set; }

    /// <summary>
    /// Gets the interaction that was received to stop the selection.
    /// </summary>
    public SocketMessageComponent? StopInteraction { get; private set; }

    /// <inheritdoc/>
    public void Cancel() => TimeoutTaskSource.TryCancel();

    /// <inheritdoc/>
    public async Task ExecuteAsync(SocketMessage message)
    {
        var result = await Selection.HandleMessageAsync(message, Message).ConfigureAwait(false);

        switch (result.Status)
        {
            case InteractiveInputStatus.Success:
                StopMessage = message;
                TimeoutTaskSource.TrySetResult((result.SelectedOption, InteractiveStatus.Success));
                break;

            case InteractiveInputStatus.Canceled:
                StopMessage = message;
                TimeoutTaskSource.TrySetResult((result.SelectedOption, InteractiveStatus.Canceled));
                break;

            case InteractiveInputStatus.Ignored:
            default:
                break;
        }
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(SocketReaction reaction)
    {
        var result = await Selection.HandleReactionAsync(reaction, Message).ConfigureAwait(false);

        switch (result.Status)
        {
            case InteractiveInputStatus.Success:
                StopReaction = reaction;
                TimeoutTaskSource.TrySetResult((result.SelectedOption, InteractiveStatus.Success));
                break;

            case InteractiveInputStatus.Canceled:
                StopReaction = reaction;
                TimeoutTaskSource.TrySetResult((result.SelectedOption, InteractiveStatus.Canceled));
                break;

            case InteractiveInputStatus.Ignored:
            default:
                break;
        }
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(SocketInteraction interaction)
    {
        if (interaction is not SocketMessageComponent component)
            return;

        var result = await Selection.HandleInteractionAsync(component, Message).ConfigureAwait(false);

        switch (result.Status)
        {
            case InteractiveInputStatus.Success:
                StopInteraction = component;
                TimeoutTaskSource.TrySetResult((result.SelectedOption, InteractiveStatus.Success));
                break;

            case InteractiveInputStatus.Canceled:
                StopInteraction = component;
                TimeoutTaskSource.TrySetResult((result.SelectedOption, InteractiveStatus.Canceled));
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