using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Fergun.Interactive.Extensions;

namespace Fergun.Interactive.Selection;

/// <summary>
/// Represents the base of selections.
/// </summary>
/// <typeparam name="TOption">The type of the options.</typeparam>
public abstract class BaseSelection<TOption> : IInteractiveElement<TOption>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseSelection{TOption}"/> class using the specified builder properties.
    /// </summary>
    /// <param name="properties">The builder properties to copy from.</param>
    protected BaseSelection(IBaseSelectionBuilderProperties<TOption> properties)
    {
        InteractiveGuards.NotNull(properties);
        InteractiveGuards.SupportedInputType(properties.InputType, false);
        InteractiveGuards.RequiredEmoteConverter(properties.InputType, properties.EmoteConverter);
        InteractiveGuards.NotNull(properties.EqualityComparer);
        InteractiveGuards.NotNull(properties.SelectionPage);
        InteractiveGuards.NotNull(properties.Options);
        InteractiveGuards.NotNull(properties.Users);
        InteractiveGuards.NotEmpty(properties.Options);
        InteractiveGuards.NoDuplicates(properties.Options, properties.EqualityComparer);

        StringConverter = properties.StringConverter;
        EmoteConverter = properties.EmoteConverter;
        EqualityComparer = properties.EqualityComparer;
        SelectionPage = properties.SelectionPage.Build();
        AllowCancel = properties.AllowCancel && properties.Options.Count > 1;
        CancelOption = AllowCancel ? properties.Options.Last() : default;
        Users = properties.Users.ToArray();
        Options = properties.Options.ToArray();
        CanceledPage = properties.CanceledPage?.Build();
        TimeoutPage = properties.TimeoutPage?.Build();
        SuccessPage = properties.SuccessPage?.Build();
        Deletion = properties.Deletion;
        InputType = properties.InputType;
        ActionOnCancellation = properties.ActionOnCancellation;
        ActionOnTimeout = properties.ActionOnTimeout;
        ActionOnSuccess = properties.ActionOnSuccess;

        if (StringConverter is null && (!InputType.HasFlag(InputType.Buttons) || EmoteConverter is null))
        {
            StringConverter = x => x?.ToString()!;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the selection is restricted to <see cref="Users"/>.
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
    /// Gets a value indicating whether this selection allows for cancellation.
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

    /// <inheritdoc/>
    public virtual ComponentBuilder GetOrAddComponents(bool disableAll, ComponentBuilder? builder = null)
    {
        if (!(InputType.HasFlag(InputType.Buttons) || InputType.HasFlag(InputType.SelectMenus)))
        {
            throw new InvalidOperationException($"{nameof(InputType)} must have either {InputType.Buttons} or {InputType.SelectMenus}.");
        }

        builder ??= new ComponentBuilder();
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

        return builder;
    }

    /// <inheritdoc cref="IInteractiveInputHandler.HandleMessageAsync"/>
    public virtual async Task<InteractiveInputResult<TOption>> HandleMessageAsync(IMessage input, IUserMessage message)
    {
        InteractiveGuards.NotNull(input);
        InteractiveGuards.NotNull(message);

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
        InteractiveGuards.NotNull(input);
        InteractiveGuards.NotNull(message);

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
    public virtual Task<InteractiveInputResult<TOption>> HandleInteractionAsync(SocketMessageComponent input, IUserMessage message)
    {
        InteractiveGuards.NotNull(input);
        InteractiveGuards.NotNull(message);

        if (!InputType.HasFlag(InputType.Buttons) && !InputType.HasFlag(InputType.SelectMenus))
        {
            return Task.FromResult<InteractiveInputResult<TOption>>(InteractiveInputStatus.Ignored);
        }

        if (input.Message.Id != message.Id || !this.CanInteract(input.User))
        {
            return Task.FromResult<InteractiveInputResult<TOption>>(InteractiveInputStatus.Ignored);
        }

        TOption? selected = default;
        string? selectedString = null;
        string? customId = input.Data.Type switch
        {
            ComponentType.Button => input.Data.CustomId,
            ComponentType.SelectMenu => (input
                    .Message
                    .Components
                    .FirstOrDefault(x => x.Components.Any(y => y.Type == ComponentType.SelectMenu && y.CustomId == input.Data.CustomId))?
                    .Components
                    .FirstOrDefault() as SelectMenuComponent)?
                .Options
                .FirstOrDefault(x => x.Value == input.Data.Values.FirstOrDefault())?
                .Value,
            _ => null
        };

        if (customId is null)
        {
            return Task.FromResult<InteractiveInputResult<TOption>>(InteractiveInputStatus.Ignored);
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
            return Task.FromResult<InteractiveInputResult<TOption>>(InteractiveInputStatus.Ignored);
        }

        bool isCanceled = AllowCancel && (EmoteConverter?.Invoke(CancelOption)?.ToString() ?? StringConverter?.Invoke(CancelOption)) == selectedString;

        return Task.FromResult<InteractiveInputResult<TOption>>(new(isCanceled ? InteractiveInputStatus.Canceled : InteractiveInputStatus.Success, selected));
    }

    /// <inheritdoc/>
    async Task<IInteractiveResult<InteractiveInputStatus>> IInteractiveInputHandler.HandleMessageAsync(IMessage input, IUserMessage message)
        => await HandleMessageAsync(input, message).ConfigureAwait(false);

    /// <inheritdoc/>
    async Task<IInteractiveResult<InteractiveInputStatus>> IInteractiveInputHandler.HandleReactionAsync(IReaction input, IUserMessage message)
    {
        InteractiveGuards.ExpectedType<IReaction, SocketReaction>(input, out var socketReaction);
        return await HandleReactionAsync(socketReaction, message).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    async Task<IInteractiveResult<InteractiveInputStatus>> IInteractiveInputHandler.HandleInteractionAsync(IComponentInteraction input, IUserMessage message)
    {
        InteractiveGuards.ExpectedType<IComponentInteraction, SocketMessageComponent>(input, out var socketMessageComponent);
        return await HandleInteractionAsync(socketMessageComponent, message).ConfigureAwait(false);
    }

    /// <summary>
    /// Initializes a message based on this selection.
    /// </summary>
    /// <remarks>By default this method adds the reactions to a message when <see cref="InputType"/> has <see cref="InputType.Reactions"/>.</remarks>
    /// <param name="message">The message to initialize.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel this request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
}