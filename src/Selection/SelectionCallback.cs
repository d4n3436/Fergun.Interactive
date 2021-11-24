using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Fergun.Interactive.Selection
{
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
        /// Gets or sets the interaction that was received to stop the selection.
        /// </summary>
        public SocketMessageComponent? StopInteraction { get; private set; }

        /// <inheritdoc/>
        public void Cancel() => TimeoutTaskSource.TryCancel();

        /// <inheritdoc/>
        public async Task ExecuteAsync(SocketMessage message)
        {
            if (!Selection.InputType.HasFlag(InputType.Messages) || !Selection.CanInteract(message.Author))
            {
                return;
            }

            bool manageMessages = message.Channel is SocketGuildChannel guildChannel
                                  && guildChannel.Guild.CurrentUser.GetPermissions(guildChannel).ManageMessages;

            TOption? selected = default;
            string? selectedString = null;
            foreach (var value in Selection.Options)
            {
                string? temp = Selection.StringConverter?.Invoke(value);
                if (temp != message.Content) continue;
                selectedString = temp;
                selected = value;
                break;
            }

            if (selectedString is null)
            {
                if (manageMessages && Selection.Deletion.HasFlag(DeletionOptions.Invalid))
                {
                    await message.DeleteAsync().ConfigureAwait(false);
                }
                return;
            }

            bool isCanceled = Selection.AllowCancel && Selection.StringConverter?.Invoke(Selection.CancelOption) == selectedString;

            if (isCanceled)
            {
                TimeoutTaskSource.TrySetResult((selected, InteractiveStatus.Canceled));
                return;
            }

            if (manageMessages && Selection.Deletion.HasFlag(DeletionOptions.Valid))
            {
                await message.DeleteAsync().ConfigureAwait(false);
            }

            TimeoutTaskSource.TrySetResult((selected, InteractiveStatus.Success));
        }

        /// <inheritdoc/>
        public async Task ExecuteAsync(SocketReaction reaction)
        {
            if (!Selection.InputType.HasFlag(InputType.Reactions) || !Selection.CanInteract(reaction.UserId))
            {
                return;
            }

            bool manageMessages = Message.Channel is SocketGuildChannel guildChannel
                                  && guildChannel.Guild.CurrentUser.GetPermissions(guildChannel).ManageMessages;

            TOption? selected = default;
            IEmote? selectedEmote = null;
            foreach (var value in Selection.Options)
            {
                var temp = Selection.EmoteConverter?.Invoke(value);
                if (temp?.Name != reaction.Emote.Name) continue;
                selectedEmote = temp;
                selected = value;
                break;
            }

            if (selectedEmote is null)
            {
                if (manageMessages && Selection.Deletion.HasFlag(DeletionOptions.Invalid))
                {
                    await Message.RemoveReactionAsync(reaction.Emote, reaction.UserId).ConfigureAwait(false);
                }
                return;
            }

            bool isCanceled = Selection.AllowCancel && Selection.EmoteConverter?.Invoke(Selection.CancelOption).Name == selectedEmote.Name;

            if (isCanceled)
            {
                TimeoutTaskSource.TrySetResult((selected, InteractiveStatus.Canceled));
                return;
            }

            if (manageMessages && Selection.Deletion.HasFlag(DeletionOptions.Valid))
            {
                await Message.RemoveReactionAsync(reaction.Emote, reaction.UserId).ConfigureAwait(false);
            }

            TimeoutTaskSource.TrySetResult((selected, InteractiveStatus.Success));
        }

        /// <inheritdoc/>
        public Task ExecuteAsync(SocketInteraction interaction)
        {
            if ((Selection.InputType.HasFlag(InputType.Buttons) || Selection.InputType.HasFlag(InputType.SelectMenus))
                && interaction is SocketMessageComponent componentInteraction)
            {
                Execute(componentInteraction);
            }

            return Task.CompletedTask;
        }

        public void Execute(SocketMessageComponent interaction)
        {
            if (interaction.Message.Id != Message.Id || !Selection.CanInteract(interaction.User))
            {
                return;
            }

            TOption? selected = default;
            string? selectedString = null;
            string? customId = interaction.Data.Type switch
            {
                ComponentType.Button => interaction.Data.CustomId,
                ComponentType.SelectMenu => (interaction
                    .Message
                    .Components
                    .FirstOrDefault(x => x.Components.Any(y => y.Type == ComponentType.SelectMenu && y.CustomId == interaction.Data.CustomId))?
                    .Components
                    .FirstOrDefault() as SelectMenuComponent)?
                    .Options
                    .FirstOrDefault(x => x.Value == interaction.Data.Values.FirstOrDefault())?
                    .Value,
                _ => null
            };

            if (customId is null)
            {
                return;
            }

            foreach (var value in Selection.Options)
            {
                string? stringValue = Selection.EmoteConverter?.Invoke(value)?.ToString() ?? Selection.StringConverter?.Invoke(value);
                if (customId != stringValue) continue;
                selected = value;
                selectedString = stringValue;
                break;
            }

            if (selectedString is null)
            {
                return;
            }

            StopInteraction = interaction;

            bool isCanceled = Selection.AllowCancel
                && (Selection.EmoteConverter?.Invoke(Selection.CancelOption)?.ToString()
                ?? Selection.StringConverter?.Invoke(Selection.CancelOption)) == selectedString;

            TimeoutTaskSource.TrySetResult((selected, isCanceled ? InteractiveStatus.Canceled : InteractiveStatus.Success));
            Dispose();
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