using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Fergun.Interactive.Pagination
{
    /// <summary>
    /// Represents an event handler for a paginator.
    /// </summary>
    internal sealed class PaginatorCallback : IInteractiveCallback
    {
        private bool _disposed;

        public PaginatorCallback(Paginator paginator, IUserMessage message,
            TimeoutTaskCompletionSource<InteractiveStatus> timeoutTaskSource,
            DateTimeOffset startTime, SocketInteraction? initialInteraction = null)
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
        /// Gets or sets the last received interaction that is not <see cref="StopInteraction"/>.
        /// </summary>
        /// <remarks>For paginators, this is either the interaction that was received to update a message to a paginator or the interaction received to change the pages.</remarks>
        public SocketInteraction? LastInteraction { get; private set; }

        /// <summary>
        /// Gets or sets the interaction that was received to stop the paginator.
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
            var result = await Paginator.HandleInteractionAsync(interaction, Message).ConfigureAwait(false);
            switch (result.Status)
            {
                case InteractiveInputStatus.Success:
                    LastInteraction = interaction;
                    TimeoutTaskSource.TryReset();
                    break;

                case InteractiveInputStatus.Canceled:
                    StopInteraction = interaction as SocketMessageComponent ?? StopInteraction;
                    Cancel();
                    break;

                case InteractiveInputStatus.Ignored:
                default:
                    break;
            }
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