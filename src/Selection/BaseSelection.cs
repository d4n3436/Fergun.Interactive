using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Fergun.Interactive.Extensions;
using System.Collections.ObjectModel;

namespace Fergun.Interactive.Selection;

/// <summary>
/// Represents a selection of options.
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
        InteractiveGuards.SupportedInputType(properties.InputType, ephemeral: false);
        InteractiveGuards.RequiredEmoteConverter(properties.InputType, properties.EmoteConverter);
        InteractiveGuards.NotNull(properties.EqualityComparer);
        InteractiveGuards.NotNull(properties.SelectionPage);
        InteractiveGuards.NotNull(properties.Options);
        InteractiveGuards.NotNull(properties.Users);
        InteractiveGuards.NotEmpty(properties.Options);
        InteractiveGuards.NoDuplicates(properties.Options, properties.EqualityComparer);

        if (properties.RestrictedInputBehavior == RestrictedInputBehavior.SendMessage)
        {
            InteractiveGuards.NotNull(properties.RestrictedPageFactory);
        }

        StringConverter = properties.StringConverter;
        EmoteConverter = properties.EmoteConverter;
        EqualityComparer = properties.EqualityComparer;
        SelectionPage = properties.SelectionPage.Build();
        Options =  new ReadOnlyCollection<TOption>(properties.Options.ToArray());
        AllowCancel = properties.AllowCancel && Options.Count > 1;
        CancelOption = AllowCancel ? Options.Last() : default;
        MinValues = properties.MinValues;
        MaxValues = properties.MaxValues;
        Placeholder = properties.Placeholder;
        Users = new ReadOnlyCollection<IUser>(properties.Users.ToArray());
        CanceledPage = properties.CanceledPage?.Build();
        TimeoutPage = properties.TimeoutPage?.Build();
        RestrictedPage = properties.RestrictedPageFactory?.Invoke(Users);
        SuccessPage = properties.SuccessPage?.Build();
        Deletion = properties.Deletion;
        InputType = properties.InputType;
        ActionOnCancellation = properties.ActionOnCancellation;
        ActionOnTimeout = properties.ActionOnTimeout;
        RestrictedInputBehavior = properties.RestrictedInputBehavior;
        ActionOnSuccess = properties.ActionOnSuccess;

        if (StringConverter is null && (!InputType.HasFlag(InputType.Buttons) || EmoteConverter is null))
        {
            StringConverter = x =>
            {
                if (x is null)
                {
                    throw new ArgumentNullException(nameof(x), $"Value of {nameof(TOption)} cannot be null.");
                }

                return x.ToString()!;
            };
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

    /// <summary>
    /// Gets the minimum number of items a user must select.
    /// </summary>
    /// <remarks>Only applicable to selections using select menus.</remarks>
    public int MinValues { get; }

    /// <summary>
    /// Gets the maximum number of items a user can select.
    /// </summary>
    /// <remarks>Only applicable to selections using select menus.</remarks>
    public int MaxValues { get; }

    /// <summary>
    /// Gets the placeholder text of the selection.
    /// </summary>
    /// <remarks>Only applicable to selections using select menus.</remarks>
    public string? Placeholder { get; }

    /// <inheritdoc/>
    public IReadOnlyCollection<IUser> Users { get; }

    /// <inheritdoc/>
    public IReadOnlyCollection<TOption> Options { get; }

    /// <inheritdoc/>
    public IPage? CanceledPage { get; }

    /// <inheritdoc/>
    public IPage? TimeoutPage { get; }

    /// <summary>
    /// Gets the <see cref="IPage"/> that will be displayed ephemerally to a user when they are not allowed to interact with this selection.
    /// </summary>
    public IPage? RestrictedPage { get; }

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
    /// Gets the behavior the selection should exhibit when a user is not allowed to interact with it.
    /// </summary>
    public RestrictedInputBehavior RestrictedInputBehavior { get; }

    /// <summary>
    /// Gets the action that will be done after valid input is received (except if <see cref="CancelOption"/> is received).
    /// </summary>
    public ActionOnStop ActionOnSuccess { get; }

    /// <inheritdoc/>
    public virtual ComponentBuilder GetOrAddComponents(bool disableAll, ComponentBuilder? builder = null)
    {
        if (!(InputType.HasFlag(InputType.Buttons) || InputType.HasFlag(InputType.SelectMenus)))
        {
            throw new InvalidOperationException($"{nameof(InputType)} must have either {nameof(InputType.Buttons)} or {nameof(InputType.SelectMenus)}.");
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
                .WithDisabled(disableAll)
                .WithMinValues(MinValues)
                .WithMaxValues(MaxValues);

            if (!string.IsNullOrEmpty(Placeholder))
                selectMenu.WithPlaceholder(Placeholder);

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
        foreach (var value in Options)
        {
            string? temp = StringConverter?.Invoke(value);
            if (temp != input.Content) continue;
            selected = value;
            break;
        }

        if (selected is null)
        {
            if (manageMessages && Deletion.HasFlag(DeletionOptions.Invalid))
            {
                await input.DeleteAsync().ConfigureAwait(false);
            }

            return InteractiveInputStatus.Ignored;
        }

        bool isCanceled = AllowCancel && EqualityComparer.Equals(selected, CancelOption);

        if (isCanceled)
        {
            return new InteractiveInputResult<TOption>(InteractiveInputStatus.Canceled, selected);
        }

        if (manageMessages && Deletion.HasFlag(DeletionOptions.Valid))
        {
            await input.DeleteAsync().ConfigureAwait(false);
        }

        return new InteractiveInputResult<TOption>(InteractiveInputStatus.Success, selected);
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
        foreach (var value in Options)
        {
            var temp = EmoteConverter?.Invoke(value);
            if (temp?.Name != input.Emote.Name) continue;
            selected = value;
            break;
        }

        if (selected is null)
        {
            if (manageMessages && Deletion.HasFlag(DeletionOptions.Invalid))
            {
                await message.RemoveReactionAsync(input.Emote, input.UserId).ConfigureAwait(false);
            }

            return InteractiveInputStatus.Ignored;
        }

        bool isCanceled = AllowCancel && EqualityComparer.Equals(selected, CancelOption);

        if (isCanceled)
        {
            return new InteractiveInputResult<TOption>(InteractiveInputStatus.Canceled, selected);
        }

        if (manageMessages && Deletion.HasFlag(DeletionOptions.Valid))
        {
            await message.RemoveReactionAsync(input.Emote, input.UserId).ConfigureAwait(false);
        }

        return new InteractiveInputResult<TOption>(InteractiveInputStatus.Success, selected);
    }

    /// <inheritdoc cref="IInteractiveInputHandler.HandleInteractionAsync"/>
    public virtual async Task<InteractiveInputResult<TOption>> HandleInteractionAsync(SocketMessageComponent input, IUserMessage message)
    {
        InteractiveGuards.NotNull(input);
        InteractiveGuards.NotNull(message);

        if ((!InputType.HasFlag(InputType.Buttons) && !InputType.HasFlag(InputType.SelectMenus)) || input.Message.Id != message.Id)
        {
            return InteractiveInputStatus.Ignored;
        }

        if (!this.CanInteract(input.User))
        {
            return RestrictedInputBehavior switch
            {
                RestrictedInputBehavior.Ignore or RestrictedInputBehavior.Auto when RestrictedPage is null => InteractiveInputStatus.Ignored,
                RestrictedInputBehavior.SendMessage or RestrictedInputBehavior.Auto when RestrictedPage is not null => await SendRestrictedMessageAsync(input).ConfigureAwait(false),
                RestrictedInputBehavior.Defer => await DeferInteractionAsync(input).ConfigureAwait(false),
                _ => InteractiveInputStatus.Ignored
            };
        }

        var selectedValues = input.Data.Type switch
        {
            ComponentType.Button => [input.Data.CustomId],
            ComponentType.SelectMenu => input.Data.Values,
            _ => []
        };

        if (selectedValues.Count == 0)
        {
            return InteractiveInputStatus.Ignored;
        }

        List<TOption> options = [];
        foreach (string value in selectedValues)
        {
            if (Options.Any(option => (EmoteConverter?.Invoke(option)?.ToString() ?? StringConverter?.Invoke(option)) == value))
            {
                var option = Options.First(option => (EmoteConverter?.Invoke(option)?.ToString() ?? StringConverter?.Invoke(option)) == value);
                options.Add(option);
            }
        }

        if (options.Count == 0)
        {
            return InteractiveInputStatus.Ignored;
        }

        var status = AllowCancel && options.Contains(CancelOption, EqualityComparer) ? InteractiveInputStatus.Canceled : InteractiveInputStatus.Success;
        return new InteractiveInputResult<TOption>(status, options.AsReadOnly());
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
    /// <remarks>By default, this method adds the reactions to a message when <see cref="InputType"/> has <see cref="InputType.Reactions"/>.</remarks>
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

    private async Task<InteractiveInputResult<TOption>> SendRestrictedMessageAsync(SocketMessageComponent input)
    {
        var page = RestrictedPage ?? throw new InvalidOperationException($"Expected {nameof(RestrictedPage)} to be non-null.");
        var attachments = page.AttachmentsFactory is null ? null : await page.AttachmentsFactory().ConfigureAwait(false);
        await input.RespondWithFilesAsync(attachments ?? [], page.Text, page.GetEmbedArray(), page.IsTTS,
            ephemeral: true, page.AllowedMentions, flags: page.MessageFlags ?? MessageFlags.None).ConfigureAwait(false);

        return InteractiveInputStatus.Ignored;
    }

    private static async Task<InteractiveInputResult<TOption>> DeferInteractionAsync(SocketMessageComponent input)
    {
        await input.DeferAsync().ConfigureAwait(false);
        return InteractiveInputStatus.Ignored;
    }
}