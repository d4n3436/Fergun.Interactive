using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Discord;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents an abstract paginator builder.
/// </summary>
/// <typeparam name="TPaginator">The type of the paginator.</typeparam>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public abstract class PaginatorBuilder<TPaginator, TBuilder>
    : IInteractiveBuilder<TPaginator, KeyValuePair<IEmote, PaginatorAction>, TBuilder>, IBasePaginatorBuilderProperties
    where TPaginator : Paginator
    where TBuilder : PaginatorBuilder<TPaginator, TBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatorBuilder{TPaginator, TBuilder}"/> class.
    /// </summary>
    protected PaginatorBuilder()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        Options = new OptionsWrapper(ButtonFactories);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    /// <inheritdoc/>
    public virtual bool IsUserRestricted => Users.Count > 0;

    /// <inheritdoc/>
    public virtual int StartPageIndex { get; set; }

    /// <inheritdoc/>
    public virtual PaginatorFooter Footer { get; set; } = PaginatorFooter.PageNumber;

    /// <summary>
    /// Gets or sets the users who can interact with the paginator.
    /// </summary>
    public virtual ICollection<IUser> Users { get; set; } = new Collection<IUser>();

    /// <inheritdoc/>
    public virtual IDictionary<IEmote, PaginatorAction> Options
    {
        get;
        [Obsolete($"The library no longer uses this property for button-based paginators and it will add any values added here into {nameof(ButtonFactories)}, unless this value is changed.")]
        set;
    }

    /// <inheritdoc/>
    public virtual IList<Func<IButtonContext, IPaginatorButton>> ButtonFactories { get; protected set; } = [];

    /// <inheritdoc/>
    public virtual IList<Func<ISelectMenuContext, IPaginatorSelectMenu>> SelectMenuFactories { get; protected set; } = [];

    /// <inheritdoc/>
    public virtual IPageBuilder? CanceledPage { get; set; }

    /// <inheritdoc/>
    public virtual IPageBuilder? TimeoutPage { get; set; }

    /// <inheritdoc/>
    public virtual DeletionOptions Deletion { get; set; } = DeletionOptions.Valid | DeletionOptions.Invalid;

    /// <inheritdoc/>
    /// <remarks>The default value is <see cref="InputType.Buttons"/>.</remarks>
    public virtual InputType InputType { get; set; } = InputType.Buttons;

    /// <inheritdoc/>
    /// <remarks>The default value is <see cref="ActionOnStop.ModifyMessage"/>.</remarks>
    public virtual ActionOnStop ActionOnCancellation { get; set; } = ActionOnStop.ModifyMessage;

    /// <inheritdoc/>
    /// <remarks>The default value is <see cref="ActionOnStop.ModifyMessage"/>.</remarks>
    public virtual ActionOnStop ActionOnTimeout { get; set; } = ActionOnStop.ModifyMessage;

    /// <inheritdoc/>
    /// <remarks>The default value is 30 seconds.</remarks>
    public TimeSpan JumpInputTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <inheritdoc/>
    public string? JumpInputPrompt { get; set; }

    /// <inheritdoc/>
    public string? JumpInputTextLabel { get; set; }

    /// <inheritdoc/>
    public string? InvalidJumpInputMessage { get; set; }

    /// <inheritdoc/>
    public string? JumpInputInUseMessage { get; set; }

    /// <inheritdoc/>
    public string? ExpiredJumpInputMessage { get; set; }

    /// <inheritdoc/>
    ICollection<KeyValuePair<IEmote, PaginatorAction>> IInteractiveBuilderProperties<KeyValuePair<IEmote, PaginatorAction>>.Options
    {
        get => Options;
        set => WithOptions(value.ToDictionary(x => x.Key, x => x.Value));
    }

    /// <summary>
    /// Builds this <typeparamref name="TBuilder"/> into an immutable <typeparamref name="TPaginator"/>.
    /// </summary>
    /// <returns>A <typeparamref name="TPaginator"/>.</returns>
    public abstract TPaginator Build();

    /// <summary>
    /// Sets the index of the page the <typeparamref name="TPaginator"/> should start.
    /// </summary>
    /// <param name="startPageIndex">The index of the page the <typeparamref name="TPaginator"/> should start.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithStartPageIndex(int startPageIndex)
    {
        StartPageIndex = startPageIndex;
        return (TBuilder)this;
    }

    /// <summary>
    /// Gets the footer format in the <see cref="Embed"/> of the <typeparamref name="TPaginator"/>.
    /// </summary>
    /// <param name="footer">The footer.</param>
    /// <remarks>Setting this to other than <see cref="PaginatorFooter.None"/> will override any other footer in the pages.</remarks>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithFooter(PaginatorFooter footer)
    {
        Footer = footer;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the users who can interact with the <typeparamref name="TPaginator"/>.
    /// </summary>
    /// <param name="users">The users.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithUsers(params IUser[] users)
    {
        InteractiveGuards.NotNull(users);
        Users = users.ToList();
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the users who can interact with the <typeparamref name="TPaginator"/>.
    /// </summary>
    /// <param name="users">The users.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithUsers(IEnumerable<IUser> users)
    {
        InteractiveGuards.NotNull(users);
        Users = users.ToList();
        return (TBuilder)this;
    }

    /// <summary>
    /// Adds a user who can interact with the <typeparamref name="TPaginator"/>.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder AddUser(IUser user)
    {
        InteractiveGuards.NotNull(user);
        Users.Add(user);
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the emotes and their related paginator actions.
    /// </summary>
    /// <param name="emotes">A dictionary of emotes and paginator actions.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithOptions(IDictionary<IEmote, PaginatorAction> emotes)
    {
        InteractiveGuards.NotNull(emotes);

        Options.Clear();
        ButtonFactories.Clear();

        foreach (var pair in emotes)
        {
            AddOption(pair);
        }

        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the paginator buttons.
    /// </summary>
    /// <param name="buttons">The paginator buttons.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithOptions(IEnumerable<IPaginatorButton> buttons)
    {
        InteractiveGuards.NotNull(buttons);

        return WithOptions(buttons.Select(x => new Func<IButtonContext, IPaginatorButton>(_ => x)));
    }

    /// <summary>
    /// Sets the paginator buttons.
    /// </summary>
    /// <param name="buttonFactories">The factories that create the paginator buttons.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithOptions(IEnumerable<Func<IButtonContext, IPaginatorButton>> buttonFactories)
    {
        InteractiveGuards.NotNull(buttonFactories);

        // Clear ButtonFactories instead of setting a new value, otherwise OptionsWrapper would break
        Options.Clear();
        ButtonFactories.Clear();

        foreach (var factory in buttonFactories)
        {
            AddOption(factory);
        }

        return (TBuilder)this;
    }

    /// <summary>
    /// Adds an emote related to a paginator action.
    /// </summary>
    /// <remarks>If you want to customize your buttons, use the other overloads instead.</remarks>
    /// <param name="option">The pair of emote and action.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder AddOption(KeyValuePair<IEmote, PaginatorAction> option)
        => AddOption(option.Key, option.Value);

    /// <summary>
    /// Adds an emote related to a paginator action.
    /// </summary>
    /// <remarks>If you want to customize your buttons, use the other overloads instead.</remarks>
    /// <param name="emote">The emote.</param>
    /// <param name="action">The paginator action.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder AddOption(IEmote emote, PaginatorAction action)
    {
        InteractiveGuards.NotNull(emote);

        // This will add the option to both Options and ButtonFactories
        Options.Add(emote, action);

        // If the value of Options is changed, add the option into ButtonFactories through the new overloads
        if (Options is not OptionsWrapper)
            return AddOption(action, emote, null);

        return (TBuilder)this;
    }

    /// <summary>
    /// Adds a paginator button with the specified text, action and style.
    /// </summary>
    /// <param name="text">The text (label) that will be displayed in the button.</param>
    /// <param name="action">The paginator action.</param>
    /// <param name="style">The button style. If the value is null, the library will decide the style of the button.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder AddOption(string text, PaginatorAction action, ButtonStyle? style)
    {
        return AddOption(action, null, text, style);
    }

    /// <summary>
    /// Adds a paginator button with the specified emote, action and style.
    /// </summary>
    /// <param name="emote">The emote.</param>
    /// <param name="action">The paginator action.</param>
    /// <param name="style">The button style. If the value is null, the library will decide the style of the button.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder AddOption(IEmote emote, PaginatorAction action, ButtonStyle? style)
    {
        return AddOption(action, emote, null, style);
    }

    /// <summary>
    /// Adds a link-style paginator button with the specified properties.
    /// </summary>
    /// <param name="url">The url of the button.</param>
    /// <param name="emote">The emote.</param>
    /// <param name="text">The text (label) that will be displayed in the button.</param>
    /// <param name="isDisabled">A value indicating whether to disable the button. If the value is null, the library will decide its status.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder AddOption(string url, IEmote? emote, string? text, bool? isDisabled = null)
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("Url cannot be null or empty.", nameof(url));

        if (emote is null && string.IsNullOrEmpty(text))
            throw new ArgumentException($"Either {nameof(emote)} or {nameof(text)} must have a valid value.");

        return AddOption(new PaginatorButton(url, emote, text, isDisabled));
    }

    /// <summary>
    /// Adds a detached paginator button with the specified properties.
    /// </summary>
    /// <remarks>Detached paginator buttons are not managed by a paginator and must be manually handled.</remarks>
    /// <param name="customId">The custom ID.</param>
    /// <param name="emote">The emote.</param>
    /// <param name="text">The text (label) that will be displayed in the button.</param>
    /// <param name="style">The button style to use in the button. If the value is null, the library will decide the style of the button.</param>
    /// <param name="isDisabled">A value indicating whether to disable the button. If the value is null, the library will decide its status.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder AddOption(string customId, IEmote? emote, string? text, ButtonStyle? style, bool? isDisabled = null)
    {
        if (string.IsNullOrEmpty(customId))
            throw new ArgumentException("CustomId cannot be null or empty.", nameof(customId));

        if (emote is null && string.IsNullOrEmpty(text))
            throw new ArgumentException($"Either {nameof(emote)} or {nameof(text)} must have a valid value.");

        return AddOption(new PaginatorButton(customId, emote, text, style, isDisabled));
    }

    /// <summary>
    /// Adds a paginator button with the specified properties.
    /// </summary>
    /// <param name="action">The paginator action.</param>
    /// <param name="emote">The emote.</param>
    /// <param name="text">The text (label) that will be displayed in the button.</param>
    /// <param name="style">The button style to use in the button. If the value is null, the library will decide the style of the button.</param>
    /// <param name="isDisabled">A value indicating whether to disable the button. If the value is null, the library will decide its status.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder AddOption(PaginatorAction action, IEmote? emote, string? text, ButtonStyle? style = null, bool? isDisabled = null)
    {
        if (emote is null && string.IsNullOrEmpty(text))
            throw new ArgumentException($"Either {nameof(emote)} or {nameof(text)} must have a valid value.");

        return AddOption(new PaginatorButton(action, emote, text, style, isDisabled));
    }

    /// <summary>
    /// Adds a paginator button.
    /// </summary>
    /// <param name="button">The paginator button.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder AddOption(IPaginatorButton button)
    {
        InteractiveGuards.NotNull(button);

        return AddOption(_ => button);
    }

    /// <summary>
    /// Adds a factory method that creates a paginator button.
    /// </summary>
    /// <param name="buttonFactory">The factory of a paginator button.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder AddOption(Func<IButtonContext, IPaginatorButton> buttonFactory)
    {
        InteractiveGuards.NotNull(buttonFactory);
        ButtonFactories.Add(buttonFactory);
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the paginator select menus.
    /// </summary>
    /// <remarks>
    /// When using this overload, the select menus will have their enabled status managed by the library.<br/>
    /// Paginator select menus are detached from the paginator and their interactions must be manually handled.
    /// </remarks>
    /// <param name="builders">The select menu builders.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithSelectMenus(IEnumerable<SelectMenuBuilder> builders)
    {
        InteractiveGuards.NotNull(builders);
        return WithSelectMenus(builders.Select(x => new PaginatorSelectMenu(x)));
    }

    /// <summary>
    /// Sets the paginator select menus.
    /// </summary>
    /// <remarks>Paginator select menus are detached from the paginator and their interactions must be manually handled.</remarks>
    /// <param name="selectMenus">The paginator select menus.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithSelectMenus(IEnumerable<IPaginatorSelectMenu> selectMenus)
    {
        InteractiveGuards.NotNull(selectMenus);
        return WithSelectMenus(selectMenus.Select(x => new Func<ISelectMenuContext, IPaginatorSelectMenu>(_ => x)));
    }

    /// <summary>
    /// Sets the paginator select menus.
    /// </summary>
    /// <remarks>Paginator select menus are detached from the paginator and their interactions must be manually handled.</remarks>
    /// <param name="selectMenuFactories">The paginator select menu factories.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithSelectMenus(IEnumerable<Func<ISelectMenuContext, IPaginatorSelectMenu>> selectMenuFactories)
    {
        InteractiveGuards.NotNull(selectMenuFactories);
        SelectMenuFactories = selectMenuFactories.ToList();
        return (TBuilder)this;
    }

    /// <summary>
    /// Adds a paginator select menu with the specified properties.
    /// </summary>
    /// <remarks>Paginator select menus are detached from the paginator and their interactions must be manually handled.</remarks>
    /// <param name="customId">The custom ID.</param>
    /// <param name="options">The options.</param>
    /// <param name="placeholder">The placeholder of this select menu.</param>
    /// <param name="maxValues">The max values of this select menu.</param>
    /// <param name="minValues">The min values of this select menu.</param>
    /// <param name="isDisabled">A value indicating whether to disable the select menu. If the value is null, the library will decide its status.</param>
    /// <param name="type">The <see cref="ComponentType"/> of this select menu.</param>
    /// <param name="channelTypes">The types of channels this menu can select (only valid on select menus of type <see cref="ComponentType.ChannelSelect"/>).</param>
    /// <param name="defaultValues">The default values of the select menu.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder AddSelectMenu(string customId, List<SelectMenuOptionBuilder>? options = null, string? placeholder = null, int maxValues = 1, int minValues = 1,
        bool? isDisabled = null, ComponentType type = ComponentType.SelectMenu, List<ChannelType>? channelTypes = null, List<SelectMenuDefaultValue>? defaultValues = null)
    {
        return AddSelectMenu(new SelectMenuBuilder(customId, options, placeholder, maxValues, minValues, isDisabled ?? false, type, channelTypes, defaultValues), isDisabled);
    }

    /// <summary>
    /// Adds a paginator select menu from a select menu builder.
    /// </summary>
    /// <remarks>Paginator select menus are detached from the paginator and their interactions must be manually handled.</remarks>
    /// <param name="builder">The select menu builder.</param>
    /// <param name="isDisabled">A value indicating whether to disable the select menu. If the value is null, the library will decide its status. This value overrides the one in <paramref name="builder"/>.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder AddSelectMenu(SelectMenuBuilder builder, bool? isDisabled = null)
    {
        InteractiveGuards.NotNull(builder);
        return AddSelectMenu(new PaginatorSelectMenu(builder, isDisabled));
    }

    /// <summary>
    /// Adds a paginator select menu.
    /// </summary>
    /// <remarks>Paginator select menus are detached from the paginator and their interactions must be manually handled.</remarks>
    /// <param name="selectMenu">The select menu.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder AddSelectMenu(IPaginatorSelectMenu selectMenu)
    {
        InteractiveGuards.NotNull(selectMenu);
        return AddSelectMenu(_ => selectMenu);
    }

    /// <summary>
    /// Adds a factory method that creates a paginator select menu.
    /// </summary>
    /// <remarks>Paginator select menus are detached from the paginator and their interactions must be manually handled.</remarks>
    /// <param name="selectMenuFactory">The select menu factory.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder AddSelectMenu(Func<ISelectMenuContext, IPaginatorSelectMenu> selectMenuFactory)
    {
        InteractiveGuards.NotNull(selectMenuFactory);
        SelectMenuFactories.Add(selectMenuFactory);
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the <see cref="IPage"/> which the <typeparamref name="TPaginator"/> gets modified to after a cancellation.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithCanceledPage(IPageBuilder? page)
    {
        CanceledPage = page;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the <see cref="IPage"/> which the <typeparamref name="TPaginator"/> gets modified to after a timeout.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithTimeoutPage(IPageBuilder? page)
    {
        TimeoutPage = page;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets what type of inputs the <typeparamref name="TPaginator"/> should delete.
    /// </summary>
    /// <param name="deletion">The deletion options.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithDeletion(DeletionOptions deletion)
    {
        Deletion = deletion;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the input type, that is, what is used to interact with the <typeparamref name="TPaginator"/>.
    /// </summary>
    /// <remarks>The default value is <see cref="InputType.Buttons"/>.</remarks>
    /// <param name="type">The input type.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithInputType(InputType type)
    {
        InputType = type;
        return (TBuilder)this;
    }

    /// <inheritdoc/>
    /// <remarks>The default value is <see cref="ActionOnStop.ModifyMessage"/>.</remarks>
    public virtual TBuilder WithActionOnCancellation(ActionOnStop action)
    {
        ActionOnCancellation = action;
        return (TBuilder)this;
    }

    /// <inheritdoc/>
    /// <remarks>The default value is <see cref="ActionOnStop.ModifyMessage"/>.</remarks>
    public virtual TBuilder WithActionOnTimeout(ActionOnStop action)
    {
        ActionOnTimeout = action;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the maximum time to wait for a "jump to page" input.
    /// </summary>
    /// <remarks>The default value is 30 seconds.</remarks>
    /// <param name="jumpInputTimeout">The time.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithJumpInputTimeout(TimeSpan jumpInputTimeout)
    {
        JumpInputTimeout = jumpInputTimeout;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the "jump to page" prompt that is displayed to the user.
    /// </summary>
    /// <remarks>
    /// In button inputs, this is the title of the modal that is displayed.<br/>
    /// In reaction inputs, this is the content of the temporary message that is sent.
    /// </remarks>
    /// <param name="jumpInputPrompt">The prompt.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithJumpInputPrompt(string jumpInputPrompt)
    {
        JumpInputPrompt = jumpInputPrompt;
        return (TBuilder)this;
    }

    /// <summary>
    /// Gets or sets the "jump to page" text label that is displayed in the modal.
    /// </summary>
    /// <param name="jumpInputTextLabel">The text label.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithJumpInputTextLabel(string jumpInputTextLabel)
    {
        JumpInputTextLabel = jumpInputTextLabel;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the message to display when receiving an invalid "jump to page" input.
    /// </summary>
    /// <remarks>
    /// An invalid input may be one that isn't a number, or a number that is outside the valid range.<br/>
    /// To avoid sending a warning message about this, set the value to an empty string.
    /// </remarks>
    /// <param name="invalidJumpInputMessage">The message.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithInvalidJumpInputMessage(string invalidJumpInputMessage)
    {
        InvalidJumpInputMessage = invalidJumpInputMessage;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the message to display when a user attempts to use the "jump to page" action while other user is using it.
    /// </summary>
    /// <remarks>To avoid sending a warning message about this, set the value to an empty string.</remarks>
    /// <param name="jumpInputInUseMessage">The message.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithJumpInputInUseMessage(string jumpInputInUseMessage)
    {
        JumpInputInUseMessage = jumpInputInUseMessage;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the message to display when receiving an expired "jump to page" input.
    /// </summary>
    /// <remarks>To avoid sending a warning message about this, set the value to an empty string.</remarks>
    /// <param name="expiredJumpInputMessage">The message.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithExpiredJumpInputMessage(string expiredJumpInputMessage)
    {
        ExpiredJumpInputMessage = expiredJumpInputMessage;
        return (TBuilder)this;
    }

    /// <summary>
    /// Clears all existing emote-action pairs and adds the default emote-action pairs of the <typeparamref name="TPaginator"/>.
    /// </summary>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithDefaultEmotes()
    {
        Options.Clear();
        ButtonFactories.Clear();

        AddOption(new Emoji("◀"), PaginatorAction.Backward);
        AddOption(new Emoji("▶"), PaginatorAction.Forward);
        AddOption(new Emoji("⏮"), PaginatorAction.SkipToStart);
        AddOption(new Emoji("⏭"), PaginatorAction.SkipToEnd);
        AddOption(new Emoji("🛑"), PaginatorAction.Exit);

        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the default canceled page.
    /// </summary>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithDefaultCanceledPage()
        => WithCanceledPage(new PageBuilder().WithColor(Color.Orange).WithTitle("Canceled! 👍"));

    /// <summary>
    /// Sets the default timeout page.
    /// </summary>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithDefaultTimeoutPage()
        => WithTimeoutPage(new PageBuilder().WithColor(Color.Red).WithTitle("Timed out! ⏰"));

    /// <inheritdoc/>
    TPaginator IInteractiveBuilderMethods<TPaginator, KeyValuePair<IEmote, PaginatorAction>, TBuilder>.Build() => Build();

    /// <inheritdoc/>
    TBuilder IInteractiveBuilderMethods<TPaginator, KeyValuePair<IEmote, PaginatorAction>, TBuilder>.WithOptions(ICollection<KeyValuePair<IEmote, PaginatorAction>> options)
        => WithOptions(options.ToDictionary(x => x.Key, x => x.Value));
}