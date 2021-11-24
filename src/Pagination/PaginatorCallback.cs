using System;
using System.Linq;
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
        public Task ExecuteAsync(SocketMessage message)
            => throw new NotSupportedException("Cannot execute this callback using a message.");

        /// <inheritdoc/>
        public async Task ExecuteAsync(SocketReaction reaction)
        {
            if (!Paginator.InputType.HasFlag(InputType.Reactions) || reaction.MessageId != Message.Id)
            {
                return;
            }

            bool valid = Paginator.Emotes.TryGetValue(reaction.Emote, out var action)
                         && Paginator.CanInteract(reaction.UserId);

            bool manageMessages = Message.Channel is SocketGuildChannel guildChannel
                                  && guildChannel.Guild.CurrentUser.GetPermissions(guildChannel).ManageMessages;

            if (manageMessages)
            {
                switch (valid)
                {
                    case false when Paginator.Deletion.HasFlag(DeletionOptions.Invalid):
                    case true when Paginator.Deletion.HasFlag(DeletionOptions.Valid):
                        await Message.RemoveReactionAsync(reaction.Emote, reaction.UserId).ConfigureAwait(false);
                        break;
                }
            }

            if (!valid)
            {
                return;
            }

            if (action == PaginatorAction.Exit)
            {
                Cancel();
                return;
            }

            TimeoutTaskSource.TryReset();
            bool refreshPage = await Paginator.ApplyActionAsync(action).ConfigureAwait(false);
            if (refreshPage)
            {
                var currentPage = await Paginator.GetOrLoadCurrentPageAsync().ConfigureAwait(false);
                await Message.ModifyAsync(x =>
                {
                    x.Embed = currentPage.Embed;
                    x.Content = currentPage.Text;
                }).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public Task ExecuteAsync(SocketInteraction interaction)
        {
            if (Paginator.InputType.HasFlag(InputType.Buttons) && interaction is SocketMessageComponent componentInteraction)
            {
                return ExecuteAsync(componentInteraction);
            }

            return Task.CompletedTask;
        }

        public async Task ExecuteAsync(SocketMessageComponent interaction)
        {
            if (interaction.Message.Id != Message.Id || !Paginator.CanInteract(interaction.User))
            {
                return;
            }

            var emote = (interaction
                .Message
                .Components
                .FirstOrDefault()?
                .Components?
                .FirstOrDefault(x => x is ButtonComponent button && button.CustomId == interaction.Data.CustomId) as ButtonComponent)?
                .Emote;

            if (emote is null || !Paginator.Emotes.TryGetValue(emote, out var action))
            {
                return;
            }

            if (action == PaginatorAction.Exit)
            {
                StopInteraction = interaction;
                Cancel();
                return;
            }

            LastInteraction = interaction;

            TimeoutTaskSource.TryReset();
            bool refreshPage = await Paginator.ApplyActionAsync(action).ConfigureAwait(false);
            if (refreshPage)
            {
                var currentPage = await Paginator.GetOrLoadCurrentPageAsync().ConfigureAwait(false);
                var buttons = Paginator.BuildComponents(false);

                await interaction.UpdateAsync(x =>
                {
                    x.Content = currentPage.Text;
                    x.Embed = currentPage.Embed;
                    x.Components = buttons;
                }).ConfigureAwait(false);
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