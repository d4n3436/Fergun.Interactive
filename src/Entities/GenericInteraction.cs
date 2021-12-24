using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Fergun.Interactive
{
    /// <summary>
    /// Represents a wrapper for REST and WebSocket interactions.
    /// </summary>
    internal class GenericInteraction : IDiscordInteraction
    {
        protected readonly SocketInteraction? _socketInteraction;
        protected readonly RestInteraction? _restInteraction;
        protected readonly IDiscordInteraction _interaction;
        protected readonly Func<string, Task>? _restCallback;

        [MemberNotNullWhen(true, nameof(_restInteraction), nameof(_restCallback))]
        [MemberNotNullWhen(false, nameof(_socketInteraction))]
        public virtual bool IsRestInteraction => _restInteraction != null;

        public GenericInteraction(IDiscordInteraction interaction, Func<string, Task> restCallback)
        {
            switch (interaction)
            {
                case SocketInteraction socketInteraction:
                    _socketInteraction = socketInteraction;
                    _interaction = socketInteraction;
                    break;

                case RestInteraction restInteraction:
                    InteractiveGuards.NotNull(restCallback, nameof(restCallback));
                    _restInteraction = restInteraction;
                    _restCallback = restCallback;
                    _interaction = restInteraction;
                    break;

                default:
                    throw new ArgumentException("Interaction must be either SocketInteraction or RestInteraction", nameof(interaction));
            }
        }

        /// <inheritdoc />
        public async Task RespondAsync(string? text = null, Embed[]? embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions? allowedMentions = null, MessageComponent? components = null, Embed? embed = null, RequestOptions? options = null)
        {
            if (IsRestInteraction)
                await _restCallback(_restInteraction.Respond(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options)).ConfigureAwait(false);
            else
                await _socketInteraction.RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DeferAsync(bool ephemeral = false, RequestOptions? options = null)
        {
            if (IsRestInteraction)
                await _restCallback(_restInteraction.Defer(ephemeral, options)).ConfigureAwait(false);
            else
                await _socketInteraction.DeferAsync(ephemeral, options).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task RespondWithFileAsync(Stream fileStream, string fileName, string? text = null, Embed[]? embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions? allowedMentions = null, MessageComponent? components = null, Embed? embed = null, RequestOptions? options = null)
            => await _interaction.RespondWithFileAsync(fileStream, fileName, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task RespondWithFileAsync(string filePath, string? fileName = null, string? text = null, Embed[]? embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions? allowedMentions = null, MessageComponent? components = null, Embed? embed = null, RequestOptions? options = null)
            => await _interaction.RespondWithFileAsync(filePath, fileName, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task RespondWithFileAsync(FileAttachment attachment, string? text = null, Embed[]? embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions? allowedMentions = null, MessageComponent? components = null, Embed? embed = null, RequestOptions? options = null)
            => await _interaction.RespondWithFileAsync(attachment, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task RespondWithFilesAsync(IEnumerable<FileAttachment> attachments, string? text = null, Embed[]? embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions? allowedMentions = null, MessageComponent? components = null, Embed? embed = null, RequestOptions? options = null)
            => await _interaction.RespondWithFilesAsync(attachments, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<IUserMessage> FollowupAsync(string? text = null, Embed[]? embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions? allowedMentions = null, MessageComponent? components = null, Embed? embed = null, RequestOptions? options = null)
            => await _interaction.FollowupAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<IUserMessage> FollowupWithFileAsync(Stream fileStream, string fileName, string? text = null, Embed[]? embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions? allowedMentions = null, MessageComponent? components = null, Embed? embed = null, RequestOptions? options = null)
            => await _interaction.FollowupWithFileAsync(fileStream, fileName, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<IUserMessage> FollowupWithFileAsync(string filePath, string? fileName = null, string? text = null, Embed[]? embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions? allowedMentions = null, MessageComponent? components = null, Embed? embed = null, RequestOptions? options = null)
            => await _interaction.FollowupWithFileAsync(filePath, fileName, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<IUserMessage> FollowupWithFileAsync(FileAttachment attachment, string? text = null, Embed[]? embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions? allowedMentions = null, MessageComponent? components = null, Embed? embed = null, RequestOptions? options = null)
            => await _interaction.FollowupWithFileAsync(attachment, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<IUserMessage> FollowupWithFilesAsync(IEnumerable<FileAttachment> attachments, string? text = null, Embed[]? embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions? allowedMentions = null, MessageComponent? components = null, Embed? embed = null, RequestOptions? options = null)
            => await _interaction.FollowupWithFilesAsync(attachments, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<IUserMessage> GetOriginalResponseAsync(RequestOptions? options = null)
            => await _interaction.GetOriginalResponseAsync(options).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<IUserMessage> ModifyOriginalResponseAsync(Action<MessageProperties> func, RequestOptions? options = null)
            => await _interaction.ModifyOriginalResponseAsync(func, options).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task DeleteOriginalResponseAsync(RequestOptions? options = null)
            => await _interaction.DeleteOriginalResponseAsync(options).ConfigureAwait(false);

        /// <inheritdoc />
        ulong IDiscordInteraction.Id => _interaction.Id;

        /// <inheritdoc />
        public InteractionType Type => _interaction.Type;

        /// <inheritdoc />
        IDiscordInteractionData IDiscordInteraction.Data => _interaction.Data;

        /// <inheritdoc />
        string IDiscordInteraction.Token => _interaction.Token;

        /// <inheritdoc />
        int IDiscordInteraction.Version => _interaction.Version;

        /// <inheritdoc />
        public IUser User => _interaction.User;

        /// <inheritdoc />
        ulong IEntity<ulong>.Id => _interaction.Id;

        /// <inheritdoc />
        DateTimeOffset ISnowflakeEntity.CreatedAt => _interaction.CreatedAt;
    }
}
