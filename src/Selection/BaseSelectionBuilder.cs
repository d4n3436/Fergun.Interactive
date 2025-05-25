using System;
using System.Collections.Generic;
using System.Linq;
using Discord;

namespace Fergun.Interactive.Selection;

/// <summary>
/// Represents a builder class for constructing instances of <typeparamref name="TSelection"/>.
/// </summary>
/// <typeparam name="TSelection">The type of the built selection.</typeparam>
/// <typeparam name="TOption">The type of the options the selection will have.</typeparam>
/// <typeparam name="TBuilder">The type of this builder.</typeparam>
public abstract class BaseSelectionBuilder<TSelection, TOption, TBuilder>
    : IInteractiveBuilder<TSelection, TOption, TBuilder>, IBaseSelectionBuilderProperties<TOption>
    where TSelection : BaseSelection<TOption>
    where TBuilder : BaseSelectionBuilder<TSelection, TOption, TBuilder>
{
    /// <inheritdoc/>
    public virtual bool IsUserRestricted => Users.Count > 0;

    /// <inheritdoc/>
    public virtual Func<TOption, IEmote>? EmoteConverter { get; set; }

    /// <inheritdoc/>
    public virtual Func<TOption, string>? StringConverter { get; set; }

    /// <inheritdoc/>
    public virtual IEqualityComparer<TOption> EqualityComparer { get; set; } = EqualityComparer<TOption>.Default;

    /// <inheritdoc/>
    public virtual bool AllowCancel { get; set; }

    /// <inheritdoc/>
    public virtual IPageBuilder SelectionPage { get; set; } = null!;

    /// <inheritdoc/>
    public virtual ICollection<IUser> Users { get; set; } = [];

    /// <summary>
    /// Gets or sets the options to select from.
    /// </summary>
    public virtual ICollection<TOption> Options { get; set; } = [];

    /// <inheritdoc />
    public virtual IPageBuilder? CanceledPage { get; set; }

    /// <inheritdoc />
    public virtual IPageBuilder? TimeoutPage { get; set; }

    /// <inheritdoc/>
    public virtual IPageBuilder? SuccessPage { get; set; }

    /// <inheritdoc />
    /// <remarks>This property is ignored on interaction-based selections.</remarks>
    public virtual DeletionOptions Deletion { get; set; } = DeletionOptions.Valid;

    /// <inheritdoc />
    /// <remarks>The default value is <see cref="InputType.Buttons"/>.</remarks>
    public virtual InputType InputType { get; set; } = InputType.Buttons;

    /// <inheritdoc />
    public virtual ActionOnStop ActionOnCancellation { get; set; }

    /// <inheritdoc />
    public virtual ActionOnStop ActionOnTimeout { get; set; }

    /// <summary>
    /// Gets or sets the behavior the <typeparamref name="TSelection"/> should exhibit when a user is not allowed to interact with it.
    /// </summary>
    /// <remarks>The default value is <see cref="RestrictedInputBehavior.Auto"/>.</remarks>
    public virtual RestrictedInputBehavior RestrictedInputBehavior { get; set; }

    /// <summary>
    /// Gets or sets the factory of the <see cref="IPage"/> that will be displayed ephemerally to a user when they are not allowed to interact with the <typeparamref name="TSelection"/>.
    /// </summary>
    /// <remarks>The first argument of the factory is a read-only collection of users who are allowed to interact with the paginator.</remarks>
    public virtual Func<IReadOnlyCollection<IUser>, IPage>? RestrictedPageFactory { get; set; }

    /// <inheritdoc/>
    public virtual ActionOnStop ActionOnSuccess { get; set; }

    /// <inheritdoc/>
    public virtual int MinValues { get; set; } = 1;

    /// <inheritdoc/>
    public virtual int MaxValues { get; set; } = 1;

    /// <inheritdoc/>
    public virtual string? Placeholder { get; set; }

    /// <summary>
    /// Builds this <typeparamref name="TBuilder"/> into an immutable <typeparamref name="TSelection"/>.
    /// </summary>
    /// <returns>A <typeparamref name="TSelection"/>.</returns>
    public abstract TSelection Build();

    /// <summary>
    /// Sets a function that returns an <see cref="IEmote"/> representation of a <typeparamref name="TOption"/>.
    /// </summary>
    /// <param name="emoteConverter">The emote converter.</param>
    /// <remarks>
    /// Requirements for each input type:<br/><br/>
    /// Reactions: Required.<br/>
    /// Messages: Unused.<br/>
    /// Buttons: Required (for emotes) unless a <see cref="StringConverter"/> is provided (for labels).<br/>
    /// Select menus: Optional.
    /// </remarks>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithEmoteConverter(Func<TOption, IEmote> emoteConverter)
    {
        EmoteConverter = emoteConverter;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets a function that returns a <see cref="string"/> representation of a <typeparamref name="TOption"/>.
    /// </summary>
    /// <param name="stringConverter">The string converter.</param>
    /// <remarks>
    /// Requirements for each input type:<br/><br/>
    /// Reactions: Unused.<br/>
    /// Messages: Required. If not set, defaults to <see cref="object.ToString()"/>.<br/>
    /// Buttons: Required (for labels) unless a <see cref="EmoteConverter"/> is provided (for emotes). Defaults to <see cref="object.ToString()"/> if neither are set.<br/>
    /// Select menus: Required. If not set, defaults to <see cref="object.ToString()"/>.
    /// </remarks>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithStringConverter(Func<TOption, string>? stringConverter)
    {
        StringConverter = stringConverter;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the equality comparer of <typeparamref name="TOption"/>s.
    /// </summary>
    /// <param name="equalityComparer">The equality comparer.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithEqualityComparer(IEqualityComparer<TOption> equalityComparer)
    {
        InteractiveGuards.NotNull(equalityComparer);
        EqualityComparer = equalityComparer;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets a value indicating whether the <typeparamref name="TSelection"/> allows for cancellation.
    /// </summary>
    /// <param name="allowCancel">Whether this selection allows for cancellation.</param>
    /// <remarks>When this value is <see langword="true"/>, the last element in <see cref="Options"/>
    /// will be used to cancel the <typeparamref name="TSelection"/>.</remarks>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithAllowCancel(bool allowCancel)
    {
        AllowCancel = allowCancel;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the <see cref="IPage"/> which is sent into the channel.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithSelectionPage(IPageBuilder page)
    {
        InteractiveGuards.NotNull(page);
        SelectionPage = page;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the users who can interact with the <typeparamref name="TSelection"/>.
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
    /// Sets the users who can interact with the <typeparamref name="TSelection"/>.
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
    /// Adds a user who can interact with the <typeparamref name="TSelection"/>.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder AddUser(IUser user)
    {
        InteractiveGuards.NotNull(user);
        Users.Add(user);
        return (TBuilder)this;
    }

    /// <inheritdoc/>
    public virtual TBuilder WithOptions(ICollection<TOption> options)
    {
        InteractiveGuards.NotNull(options);
        Options = options;
        return (TBuilder)this;
    }

    /// <inheritdoc/>
    public virtual TBuilder AddOption(TOption option)
    {
        Options.Add(option ?? throw new ArgumentNullException(nameof(option)));
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the <see cref="IPage"/> which the <typeparamref name="TSelection"/> gets modified to after a cancellation.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithCanceledPage(IPageBuilder? page)
    {
        CanceledPage = page;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the <see cref="IPage"/> which the <typeparamref name="TSelection"/> gets modified to after a timeout.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithTimeoutPage(IPageBuilder? page)
    {
        TimeoutPage = page;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the <see cref="IPage"/> which the <typeparamref name="TSelection"/> gets modified to after a valid input is received (except cancellation inputs).
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithSuccessPage(IPageBuilder? page)
    {
        SuccessPage = page;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets what type of inputs the <typeparamref name="TSelection"/> should delete.
    /// </summary>
    /// <param name="deletion">The deletion options.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithDeletion(DeletionOptions deletion)
    {
        Deletion = deletion;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the input type, that is, what is used to interact with the <typeparamref name="TSelection"/>.
    /// </summary>
    /// <param name="type">The input type.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithInputType(InputType type)
    {
        InputType = type;
        return (TBuilder)this;
    }

    /// <inheritdoc/>
    public virtual TBuilder WithActionOnCancellation(ActionOnStop action)
    {
        ActionOnCancellation = action;
        return (TBuilder)this;
    }

    /// <inheritdoc/>
    public virtual TBuilder WithActionOnTimeout(ActionOnStop action)
    {
        ActionOnTimeout = action;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the behavior the <typeparamref name="TSelection"/> should exhibit when a user is not allowed to interact with it.
    /// </summary>
    /// <param name="behavior">The behavior.</param>
    /// <returns>This builder.</returns>
    public TBuilder WithRestrictedInputBehavior(RestrictedInputBehavior behavior)
    {
        RestrictedInputBehavior = behavior;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the <see cref="IPage"/> that will be displayed ephemerally to a user when they are not allowed to interact with the <typeparamref name="TSelection"/>.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithRestrictedPage(IPage page)
    {
        InteractiveGuards.NotNull(page);
        return WithRestrictedPageFactory(_ => page);
    }

    /// <summary>
    /// Sets the factory of the <see cref="IPage"/> that will be displayed ephemerally to a user when they are not allowed to interact with the <typeparamref name="TSelection"/>.
    /// </summary>
    /// <remarks>The first argument of the factory is a read-only collection of users who are allowed to interact with the selection.</remarks>
    /// <param name="pageFactory">The restricted page factory. The first argument is a read-only collection of users who are allowed to interact with the selection.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithRestrictedPageFactory(Func<IReadOnlyCollection<IUser>, IPage> pageFactory)
    {
        InteractiveGuards.NotNull(pageFactory);
        RestrictedPageFactory = pageFactory;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the action that will be done after valid input is received (except cancellation inputs).
    /// </summary>
    /// <param name="action">The action.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithActionOnSuccess(ActionOnStop action)
    {
        ActionOnSuccess = action;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the minimum number of items a user must select.
    /// </summary>
    /// <remarks>Only applicable to selections using select menus.</remarks>
    /// <param name="minValues">The minimum number of items a user must select.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithMinValues(int minValues)
    {
        InteractiveGuards.ValueInRange(0, SelectMenuBuilder.MaxValuesCount, minValues);
        MinValues = minValues;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the maximum number of items a user can select.
    /// </summary>
    /// <remarks>Only applicable to selections using select menus.</remarks>
    /// <param name="maxValues">The maximum number of items a user can select.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithMaxValues(int maxValues)
    {
        InteractiveGuards.ValueInRange(1, SelectMenuBuilder.MaxValuesCount, maxValues);
        MaxValues = maxValues;
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets the placeholder text of the selection.
    /// </summary>
    /// <remarks>Only applicable to selections using select menus.</remarks>
    /// <param name="placeholder">The placeholder text.</param>
    /// <returns>This builder.</returns>
    public virtual TBuilder WithPlaceholder(string placeholder)
    {
        InteractiveGuards.NotNull(placeholder);
        InteractiveGuards.StringLengthInRange(1, SelectMenuBuilder.MaxPlaceholderLength, placeholder);
        Placeholder = placeholder;
        return (TBuilder)this;
    }
}