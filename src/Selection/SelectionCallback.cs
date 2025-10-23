using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace Fergun.Interactive.Selection;

/// <summary>
/// Represents an event handler for a selection.
/// </summary>
/// <typeparam name="TOption">The type of the options of the selection.</typeparam>
internal sealed class SelectionCallback<TOption> : IInteractiveCallback
{
    private bool _disposed;

    public SelectionCallback(BaseSelection<TOption> selection, RestMessage message,
        TimeoutTaskCompletionSource<(IReadOnlyList<TOption>, InteractiveStatus)> timeoutTaskSource,
        DateTimeOffset startTime, Interaction? initialInteraction = null)
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
    public RestMessage Message { get; }

    /// <summary>
    /// Gets the <see cref="TimeoutTaskCompletionSource{TResult}"/> used to set the result of the selection.
    /// </summary>
    public TimeoutTaskCompletionSource<(IReadOnlyList<TOption>, InteractiveStatus)> TimeoutTaskSource { get; }

    /// <inheritdoc/>
    public DateTimeOffset StartTime { get; }

    /// <summary>
    /// Gets the last received interaction that is not <see cref="StopInteraction"/>.
    /// </summary>
    /// <remarks>For selections, this is the interaction that was received to update a message to a selection.</remarks>
    public Interaction? LastInteraction { get; }

    /// <summary>
    /// Gets the messages that was received to stop the selection.
    /// </summary>
    public Message? StopMessage { get; private set; }

    /// <summary>
    /// Gets the reaction that was received to stop the selection.
    /// </summary>
    public MessageReactionAddEventArgs? StopReaction { get; private set; }

    /// <summary>
    /// Gets the interaction that was received to stop the selection.
    /// </summary>
    public MessageComponentInteraction? StopInteraction { get; private set; }

    /// <inheritdoc/>
    public void Cancel() => TimeoutTaskSource.TryCancel();

    /// <inheritdoc/>
    public async Task ExecuteAsync(Message message)
    {
        var result = await Selection.HandleMessageAsync(message, Message).ConfigureAwait(false);

        switch (result.Status)
        {
            case InteractiveInputStatus.Success:
                StopMessage = message;
                TimeoutTaskSource.TrySetResult((result.SelectedOptions, InteractiveStatus.Success));
                break;

            case InteractiveInputStatus.Canceled:
                StopMessage = message;
                TimeoutTaskSource.TrySetResult((result.SelectedOptions, InteractiveStatus.Canceled));
                break;
        }
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(MessageReactionAddEventArgs reaction)
    {
        var result = await Selection.HandleReactionAsync(reaction, Message).ConfigureAwait(false);

        switch (result.Status)
        {
            case InteractiveInputStatus.Success:
                StopReaction = reaction;
                TimeoutTaskSource.TrySetResult((result.SelectedOptions, InteractiveStatus.Success));
                break;

            case InteractiveInputStatus.Canceled:
                StopReaction = reaction;
                TimeoutTaskSource.TrySetResult((result.SelectedOptions, InteractiveStatus.Canceled));
                break;
        }
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(Interaction interaction)
    {
        if (interaction is not MessageComponentInteraction component)
            return;

        var result = await Selection.HandleInteractionAsync(component, Message).ConfigureAwait(false);

        switch (result.Status)
        {
            case InteractiveInputStatus.Success:
                StopInteraction = component;
                TimeoutTaskSource.TrySetResult((result.SelectedOptions, InteractiveStatus.Success));
                break;

            case InteractiveInputStatus.Canceled:
                StopInteraction = component;
                TimeoutTaskSource.TrySetResult((result.SelectedOptions, InteractiveStatus.Canceled));
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