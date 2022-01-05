using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Fergun.Interactive.Selection
{
    /// <summary>
    /// Represents the base of selections.
    /// </summary>
    /// <typeparam name="TOption">The type of the options.</typeparam>
    public abstract class BaseSelection<TOption> : IInteractiveElement<TOption>
    {
        /// <summary>
        /// Gets whether the selection is restricted to <see cref="Users"/>.
        /// </summary>
        public bool IsUserRestricted => Users.Count > 0;

        /// <summary>
        /// Gets a function that returns an <see cref="IEmote"/> representation of a <typeparamref name="TOption"/>.
        /// </summary>
        public Func<TOption, IEmote>? EmoteConverter { get; }

        /// <summary>
        /// Gets a function that returns a <see cref="string"/> representation of a <typeparamref name="TOption"/>.
        /// </summary>
        public Func<TOption, string>? StringConverter { get; }

        /// <summary>
        /// Gets the equality comparer of <typeparamref name="TOption"/>s.
        /// </summary>
        public IEqualityComparer<TOption> EqualityComparer { get; }

        /// <summary>
        /// Gets whether this selection allows for cancellation.
        /// </summary>
        [MemberNotNullWhen(true, nameof(CancelOption))]
        public bool AllowCancel { get; }

        /// <summary>
        /// Gets the option used for cancellation.
        /// </summary>
        /// <remarks>This option is ignored (and <see langword="default"/>) if <see cref="AllowCancel"/> is <see langword="false"/> or <see cref="Options"/> contains only one element.</remarks>
        public TOption? CancelOption { get; }

        /// <summary>
        /// Gets the <see cref="IPage"/> which is sent into the channel.
        /// </summary>
        public IPage SelectionPage { get; }

        /// <inheritdoc/>
        public IReadOnlyCollection<IUser> Users { get; }

        /// <inheritdoc/>
        public IReadOnlyCollection<TOption> Options { get; }

        /// <inheritdoc/>
        public IPage? CanceledPage { get; }

        /// <inheritdoc/>
        public IPage? TimeoutPage { get; }

        /// <summary>
        /// Gets the <see cref="IPage"/> which this selection gets modified to after a valid input is received
        /// (except if <see cref="CancelOption"/> is received).
        /// </summary>
        public IPage? SuccessPage { get; }

        /// <inheritdoc/>
        public DeletionOptions Deletion { get; }

        /// <inheritdoc/>
        public InputType InputType { get; }

        /// <inheritdoc/>
        public ActionOnStop ActionOnCancellation { get; }

        /// <inheritdoc/>
        public ActionOnStop ActionOnTimeout { get; }

        /// <summary>
        /// Gets the action that will be done after valid input is received (except if <see cref="CancelOption"/> is received).
        /// </summary>
        public ActionOnStop ActionOnSuccess { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSelection{TOption}"/> class using the specified builder properties.
        /// </summary>
        /// <param name="builder">The builder to copy the properties from.</param>
        protected BaseSelection(BaseSelectionBuilderProperties<TOption> builder)
        {
            InteractiveGuards.SupportedInputType(builder.InputType, false);
            InteractiveGuards.RequiredEmoteConverter(builder.InputType, builder.EmoteConverter);
            InteractiveGuards.NotNull(builder.EqualityComparer, nameof(builder.EqualityComparer));
            InteractiveGuards.NotNull(builder.SelectionPage, nameof(builder.SelectionPage));
            InteractiveGuards.NotNull(builder.Options, nameof(builder.Options));
            InteractiveGuards.NotNull(builder.Users, nameof(builder.Users));
            InteractiveGuards.NotEmpty(builder.Options, nameof(builder.Options));

            StringConverter = builder.StringConverter;
            EmoteConverter = builder.EmoteConverter;
            EqualityComparer = builder.EqualityComparer;
            SelectionPage = builder.SelectionPage.Build();
            AllowCancel = builder.AllowCancel && builder.Options.Count > 1;
            CancelOption = AllowCancel ? builder.Options.Last() : default;
            Users = builder.Users.ToArray();
            Options = builder.Options.ToArray();
            CanceledPage = builder.CanceledPage?.Build();
            TimeoutPage = builder.TimeoutPage?.Build();
            SuccessPage = builder.SuccessPage?.Build();
            Deletion = builder.Deletion;
            InputType = builder.InputType;
            ActionOnCancellation = builder.ActionOnCancellation;
            ActionOnTimeout = builder.ActionOnTimeout;
            ActionOnSuccess = builder.ActionOnSuccess;

            if (StringConverter is null && (!InputType.HasFlag(InputType.Buttons) || EmoteConverter is null))
            {
                StringConverter = x => x?.ToString()!;
            }
        }

        /// <summary>
        /// Initializes a message based on this selection.
        /// </summary>
        /// <remarks>By default this method adds the reactions to a message when <see cref="InputType"/> has <see cref="InputType.Reactions"/>.</remarks>
        /// <param name="message">The message to initialize.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel this request.</param>
        internal virtual async Task InitializeMessageAsync(IUserMessage message, CancellationToken cancellationToken = default)
        {
            if (!InputType.HasFlag(InputType.Reactions)) return;
            if (EmoteConverter is null)
            {
                throw new InvalidOperationException($"Reaction-based selections must have a valid {nameof(EmoteConverter)}.");
            }

            foreach (var selection in Options)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var emote = EmoteConverter(selection);

                // Only add missing reactions
                if (!message.Reactions.ContainsKey(emote))
                {
                    await message.AddReactionAsync(emote).ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc/>
        public virtual MessageComponent BuildComponents(bool disableAll)
        {
            if (!(InputType.HasFlag(InputType.Buttons) || InputType.HasFlag(InputType.SelectMenus)))
            {
                throw new InvalidOperationException($"{nameof(InputType)} must have either {InputType.Buttons} or {InputType.SelectMenus}.");
            }

            var builder = new ComponentBuilder();
            if (InputType.HasFlag(InputType.SelectMenus))
            {
                var options = new List<SelectMenuOptionBuilder>();

                foreach (var selection in Options)
                {
                    var emote = EmoteConverter?.Invoke(selection);
                    string? label = StringConverter?.Invoke(selection);
                    if (emote is null && label is null)
                    {
                        throw new InvalidOperationException($"Neither {nameof(EmoteConverter)} nor {nameof(StringConverter)} returned a valid emote or string.");
                    }

                    var option = new SelectMenuOptionBuilder()
                        .WithLabel(label)
                        .WithEmote(emote)
                        .WithValue(emote?.ToString() ?? label);

                    options.Add(option);
                }

                var selectMenu = new SelectMenuBuilder()
                    .WithCustomId("foobar")
                    .WithOptions(options)
                    .WithDisabled(disableAll);

                builder.WithSelectMenu(selectMenu);
            }
            if (InputType.HasFlag(InputType.Buttons))
            {
                foreach (var selection in Options)
                {
                    var emote = EmoteConverter?.Invoke(selection);
                    string? label = StringConverter?.Invoke(selection);
                    if (emote is null && label is null)
                    {
                        throw new InvalidOperationException($"Neither {nameof(EmoteConverter)} nor {nameof(StringConverter)} returned a valid emote or string.");
                    }

                    var button = new ButtonBuilder()
                        .WithCustomId(emote?.ToString() ?? label)
                        .WithStyle(ButtonStyle.Primary)
                        .WithEmote(emote)
                        .WithDisabled(disableAll);

                    if (label is not null)
                        button.Label = label;

                    builder.WithButton(button);
                }
            }

            return builder.Build();
        }

        /// <inheritdoc cref="IInteractiveInputHandler.HandleMessageAsync"/>
        public virtual async Task<InteractiveInputResult<TOption>> HandleMessageAsync(IMessage input, IUserMessage message)
        {
            InteractiveGuards.NotNull(input, nameof(input));
            InteractiveGuards.NotNull(message, nameof(message));

            if (!InputType.HasFlag(InputType.Messages) || !this.CanInteract(input.Author))
            {
                return InteractiveInputStatus.Ignored;
            }

            bool manageMessages = await message.Channel.CurrentUserHasManageMessagesAsync().ConfigureAwait(false);

            TOption? selected = default;
            string? selectedString = null;
            foreach (var value in Options)
            {
                string? temp = StringConverter?.Invoke(value);
                if (temp != input.Content) continue;
                selectedString = temp;
                selected = value;
                break;
            }

            if (selectedString is null)
            {
                if (manageMessages && Deletion.HasFlag(DeletionOptions.Invalid))
                {
                    await input.DeleteAsync().ConfigureAwait(false);
                }
                return InteractiveInputStatus.Ignored;
            }

            bool isCanceled = AllowCancel && StringConverter?.Invoke(CancelOption) == selectedString;

            if (isCanceled)
            {
                return new(InteractiveInputStatus.Canceled, selected!);
            }

            if (manageMessages && Deletion.HasFlag(DeletionOptions.Valid))
            {
                await input.DeleteAsync().ConfigureAwait(false);
            }

            return new(InteractiveInputStatus.Success, selected!);
        }

        /// <inheritdoc cref="IInteractiveInputHandler.HandleReactionAsync"/>
        public virtual async Task<InteractiveInputResult<TOption>> HandleReactionAsync(SocketReaction input, IUserMessage message)
        {
            InteractiveGuards.NotNull(input, nameof(input));
            InteractiveGuards.NotNull(message, nameof(message));

            if (!InputType.HasFlag(InputType.Reactions) || !this.CanInteract(input.UserId))
            {
                return InteractiveInputStatus.Ignored;
            }

            bool manageMessages = await message.Channel.CurrentUserHasManageMessagesAsync().ConfigureAwait(false);

            TOption? selected = default;
            IEmote? selectedEmote = null;
            foreach (var value in Options)
            {
                var temp = EmoteConverter?.Invoke(value);
                if (temp?.Name != input.Emote.Name) continue;
                selectedEmote = temp;
                selected = value;
                break;
            }

            if (selectedEmote is null)
            {
                if (manageMessages && Deletion.HasFlag(DeletionOptions.Invalid))
                {
                    await message.RemoveReactionAsync(input.Emote, input.UserId).ConfigureAwait(false);
                }
                return InteractiveInputStatus.Ignored;
            }

            bool isCanceled = AllowCancel && EmoteConverter?.Invoke(CancelOption).Name == selectedEmote.Name;

            if (isCanceled)
            {
                return new(InteractiveInputStatus.Canceled, selected);
            }

            if (manageMessages && Deletion.HasFlag(DeletionOptions.Valid))
            {
                await message.RemoveReactionAsync(input.Emote, input.UserId).ConfigureAwait(false);
            }

            return new(InteractiveInputStatus.Success, selected);
        }

        /// <inheritdoc cref="IInteractiveInputHandler.HandleInteractionAsync"/>
        public virtual Task<InteractiveInputResult<TOption>> HandleInteractionAsync(SocketInteraction input, IUserMessage message)
            => Task.FromResult(HandleInteraction(input, message));

        private InteractiveInputResult<TOption> HandleInteraction(SocketInteraction input, IUserMessage message)
        {
            InteractiveGuards.NotNull(input, nameof(input));
            InteractiveGuards.NotNull(message, nameof(message));

            if ((!InputType.HasFlag(InputType.Buttons) && !InputType.HasFlag(InputType.SelectMenus)) || input is not SocketMessageComponent interaction)
            {
                return InteractiveInputStatus.Ignored;
            }

            if (interaction.Message.Id != message.Id || !this.CanInteract(interaction.User))
            {
                return InteractiveInputStatus.Ignored;
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
                return InteractiveInputStatus.Ignored;
            }

            foreach (var value in Options)
            {
                string? stringValue = EmoteConverter?.Invoke(value)?.ToString() ?? StringConverter?.Invoke(value);
                if (customId != stringValue) continue;
                selected = value;
                selectedString = stringValue;
                break;
            }

            if (selectedString is null)
            {
                return InteractiveInputStatus.Ignored;
            }

            bool isCanceled = AllowCancel && (EmoteConverter?.Invoke(CancelOption)?.ToString() ?? StringConverter?.Invoke(CancelOption)) == selectedString;

            return new(isCanceled ? InteractiveInputStatus.Canceled : InteractiveInputStatus.Success, selected);
        }

        /// <inheritdoc/>
        async Task<IInteractiveResult<InteractiveInputStatus>> IInteractiveInputHandler.HandleMessageAsync(IMessage input, IUserMessage message)
            => await HandleMessageAsync(input, message).ConfigureAwait(false);

        /// <inheritdoc/>
        async Task<IInteractiveResult<InteractiveInputStatus>> IInteractiveInputHandler.HandleReactionAsync(IReaction input, IUserMessage message)
        {
            InteractiveGuards.ExpectedType<IReaction, SocketReaction>(input, nameof(input), out var socketReaction);
            return await HandleReactionAsync(socketReaction, message).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<IInteractiveResult<InteractiveInputStatus>> IInteractiveInputHandler.HandleInteractionAsync(IDiscordInteraction input, IUserMessage message)
        {
            InteractiveGuards.ExpectedType<IDiscordInteraction, SocketInteraction>(input, nameof(input), out var socketInteraction);
            return await HandleInteractionAsync(socketInteraction, message).ConfigureAwait(false);
        }
    }
}