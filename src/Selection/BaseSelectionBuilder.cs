using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Discord;

namespace Fergun.Interactive.Selection
{
    /// <summary>
    /// Represents the base of the selection builders.
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
        public virtual ICollection<IUser> Users { get; set; } = new Collection<IUser>();

        /// <summary>
        /// Gets or sets the options to select from.
        /// </summary>
        public virtual ICollection<TOption> Options { get; set; } = new Collection<TOption>();

        /// <inheritdoc />
        public virtual IPageBuilder? CanceledPage { get; set; }

        /// <inheritdoc />
        public virtual IPageBuilder? TimeoutPage { get; set; }

        /// <inheritdoc/>
        public virtual IPageBuilder? SuccessPage { get; set; }

        /// <inheritdoc />
        public virtual DeletionOptions Deletion { get; set; } = DeletionOptions.Valid;

        /// <inheritdoc />
        public virtual InputType InputType { get; set; } = InputType.Buttons;

        /// <inheritdoc />
        public virtual ActionOnStop ActionOnCancellation { get; set; }

        /// <inheritdoc />
        public virtual ActionOnStop ActionOnTimeout { get; set; }

        /// <inheritdoc/>
        public virtual ActionOnStop ActionOnSuccess { get; set; }

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
            InteractiveGuards.NotNull(equalityComparer, nameof(equalityComparer));
            EqualityComparer = equalityComparer;
            return (TBuilder)this;
        }

        /// <summary>
        /// Sets whether the <see cref="BaseSelection{TOption}"/> allows for cancellation.
        /// </summary>
        /// <param name="allowCancel">Whether this selection allows for cancellation.</param>
        /// <remarks>When this value is <see langword="true"/>, the last element in <see cref="Options"/>
        /// will be used to cancel the <see cref="BaseSelection{TOption}"/>.</remarks>
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
            InteractiveGuards.NotNull(page, nameof(page));
            SelectionPage = page;
            return (TBuilder)this;
        }

        /// <summary>
        /// Sets the users who can interact with the <see cref="BaseSelection{TOption}"/>.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <returns>This builder.</returns>
        public virtual TBuilder WithUsers(params IUser[] users)
        {
            InteractiveGuards.NotNull(users, nameof(users));
            Users = users.ToList();
            return (TBuilder)this;
        }

        /// <summary>
        /// Sets the users who can interact with the <see cref="BaseSelection{TOption}"/>.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <returns>This builder.</returns>
        public virtual TBuilder WithUsers(IEnumerable<IUser> users)
        {
            InteractiveGuards.NotNull(users, nameof(users));
            Users = users.ToList();
            return (TBuilder)this;
        }

        /// <summary>
        /// Adds a user who can interact with the <see cref="BaseSelection{TOption}"/>.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>This builder.</returns>
        public virtual TBuilder AddUser(IUser user)
        {
            InteractiveGuards.NotNull(user, nameof(user));
            Users.Add(user);
            return (TBuilder)this;
        }

        /// <inheritdoc/>
        public virtual TBuilder WithOptions(ICollection<TOption> options)
        {
            InteractiveGuards.NotNull(options, nameof(options));
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
        /// Sets the <see cref="IPage"/> which the <see cref="BaseSelection{TOption}"/> gets modified to after a cancellation.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns>This builder.</returns>
        public virtual TBuilder WithCanceledPage(IPageBuilder? page)
        {
            CanceledPage = page;
            return (TBuilder)this;
        }

        /// <summary>
        /// Sets the <see cref="IPage"/> which the <see cref="BaseSelection{TOption}"/> gets modified to after a timeout.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns>This builder.</returns>
        public virtual TBuilder WithTimeoutPage(IPageBuilder? page)
        {
            TimeoutPage = page;
            return (TBuilder)this;
        }

        /// <summary>
        /// Sets the <see cref="IPage"/> which the <see cref="BaseSelection{TOption}"/> gets modified to after a valid input is received (except cancellation inputs).
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns>This builder.</returns>
        public virtual TBuilder WithSuccessPage(IPageBuilder? page)
        {
            SuccessPage = page;
            return (TBuilder)this;
        }

        /// <summary>
        /// Sets what type of inputs the <see cref="BaseSelection{TOption}"/> should delete.
        /// </summary>
        /// <param name="deletion">The deletion options.</param>
        /// <returns>This builder.</returns>
        public virtual TBuilder WithDeletion(DeletionOptions deletion)
        {
            Deletion = deletion;
            return (TBuilder)this;
        }

        /// <summary>
        /// Sets the input type, that is, what is used to interact with the <see cref="BaseSelection{TOption}"/>.
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
        /// Sets the action that will be done after valid input is received (except cancellation inputs).
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>This builder.</returns>
        public virtual TBuilder WithActionOnSuccess(ActionOnStop action)
        {
            ActionOnSuccess = action;
            return (TBuilder)this;
        }
    }
}