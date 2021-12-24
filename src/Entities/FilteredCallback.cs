using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Fergun.Interactive
{
    /// <summary>
    /// Represents an event handler with a filter.
    /// </summary>
    /// <typeparam name="TInput">The type of the incoming inputs.</typeparam>
    internal sealed class FilteredCallback<TInput> : IInteractiveCallback<TInput>
    {
        private bool _disposed;

        public FilteredCallback(Func<TInput, bool> filter, Func<TInput, bool, Task> action,
            TimeoutTaskCompletionSource<(TInput?, InteractiveStatus)> timeoutTaskSource, DateTimeOffset startTime)
        {
            Filter = filter;
            Action = action;
            TimeoutTaskSource = timeoutTaskSource;
            StartTime = startTime;
        }

        /// <summary>
        /// Gets the filter.
        /// </summary>
        public Func<TInput, bool> Filter { get; }

        /// <summary>
        /// Gets the action which gets executed to incoming inputs.
        /// </summary>
        public Func<TInput, bool, Task> Action { get; }

        /// <summary>
        /// Gets the <see cref="TimeoutTaskCompletionSource{TResult}"/> used to set the result of the callback.
        /// </summary>
        public TimeoutTaskCompletionSource<(TInput?, InteractiveStatus)> TimeoutTaskSource { get; }

        /// <inheritdoc/>
        public DateTimeOffset StartTime { get; }

        /// <inheritdoc/>
        public void Cancel() => TimeoutTaskSource.TryCancel();

        /// <inheritdoc/>
        public async Task ExecuteAsync(TInput input)
        {
            bool success = Filter(input);
            await Action(input, success).ConfigureAwait(false);

            if (success)
            {
                TimeoutTaskSource.TrySetResult((input, InteractiveStatus.Success));
                Dispose();
            }
        }

        /// <inheritdoc/>
        public Task ExecuteAsync(SocketMessage message)
        {
            if (message is TInput input)
            {
                return ExecuteAsync(input);
            }

            throw new ArgumentException("Cannot execute this callback using a message.", nameof(message));
        }

        /// <inheritdoc/>
        public Task ExecuteAsync(SocketReaction reaction)
        {
            if (reaction is TInput input)
            {
                return ExecuteAsync(input);
            }

            throw new ArgumentException("Cannot execute this callback using a reaction.", nameof(reaction));
        }

        /// <inheritdoc/>
        public Task ExecuteAsync(IDiscordInteraction interaction)
        {
            if (interaction is TInput input)
            {
                return ExecuteAsync(input);
            }

            throw new ArgumentException("Cannot execute this callback using an interaction.", nameof(interaction));
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                TimeoutTaskSource.TryDispose();
            }

            _disposed = true;
        }

        /// <inheritdoc/>
        public void Dispose() => Dispose(true);
    }
}