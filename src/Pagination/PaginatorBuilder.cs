using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Discord;

namespace Fergun.Interactive.Pagination
{
    /// <summary>
    /// Represents the properties of a <see cref="PaginatorBuilder{TPaginator, TBuilder}"/>
    /// </summary>
    public abstract class PaginatorBuilderProperties : IInteractiveBuilderProperties<KeyValuePair<IEmote, PaginatorAction>>
    {
        /// <summary>
        /// Gets whether the paginator is restricted to <see cref="Users"/>.
        /// </summary>
        public virtual bool IsUserRestricted => Users.Count > 0;

        /// <summary>
        /// Gets or sets the index of the page the paginator should start.
        /// </summary>
        public virtual int StartPageIndex { get; set; }

        /// <summary>
        /// Gets or sets the footer format in the <see cref="Embed"/> of the paginator.
        /// </summary>
        /// <remarks>Setting this to other than <see cref="PaginatorFooter.None"/> will override any other footer in the pages.</remarks>
        public virtual PaginatorFooter Footer { get; set; } = PaginatorFooter.PageNumber;

        /// <summary>
        /// Gets or sets the users who can interact with the paginator.
        /// </summary>
        public virtual ICollection<IUser> Users { get; set; } = new Collection<IUser>();

        /// <summary>
        /// Gets or sets the emotes and their related actions of the paginator.
        /// </summary>
        public virtual IDictionary<IEmote, PaginatorAction> Options { get; set; } = new Dictionary<IEmote, PaginatorAction>();

        /// <inheritdoc/>
        public virtual IPageBuilder? CanceledPage { get; set; }

        /// <inheritdoc/>
        public virtual IPageBuilder? TimeoutPage { get; set; }

        /// <inheritdoc/>
        /// <remarks>This property is ignored in button-based paginators.</remarks>
        public virtual DeletionOptions Deletion { get; set; } = DeletionOptions.Valid | DeletionOptions.Invalid;

        /// <inheritdoc/>
        public virtual InputType InputType { get; set; } = InputType.Buttons;

        /// <inheritdoc/>
        /// <remarks>The default value is <see cref="ActionOnStop.ModifyMessage"/>.</remarks>
        public virtual ActionOnStop ActionOnCancellation { get; set; } = ActionOnStop.ModifyMessage;

        /// <inheritdoc/>
        /// <remarks>The default value is <see cref="ActionOnStop.ModifyMessage"/>.</remarks>
        public virtual ActionOnStop ActionOnTimeout { get; set; } = ActionOnStop.ModifyMessage;

        /// <inheritdoc/>
        ICollection<KeyValuePair<IEmote, PaginatorAction>> IInteractiveBuilderProperties<KeyValuePair<IEmote, PaginatorAction>>.Options
        {
            get => Options;
            set => Options = value?.ToDictionary(x => x.Key, x => x.Value) ?? throw new ArgumentNullException(nameof(value));
        }
    }

    /// <summary>
    /// Represents an abstract paginator builder.
    /// </summary>
    public abstract class PaginatorBuilder<TPaginator, TBuilder>
        : PaginatorBuilderProperties, IInteractiveBuilder<TPaginator, KeyValuePair<IEmote, PaginatorAction>, TBuilder>
        where TPaginator : Paginator
        where TBuilder : PaginatorBuilder<TPaginator, TBuilder>
    {
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
        /// <remarks>Setting this to other than <see cref="PaginatorFooter.None"/> will override any other footer in the pages.</remarks>
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
            Users = users?.ToList() ?? throw new ArgumentNullException(nameof(users));
            return (TBuilder)this;
        }

        /// <summary>
        /// Sets the users who can interact with the <typeparamref name="TPaginator"/>.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <returns>This builder.</returns>
        public virtual TBuilder WithUsers(IEnumerable<IUser> users)
        {
            Users = users?.ToList() ?? throw new ArgumentNullException(nameof(users));
            return (TBuilder)this;
        }

        /// <summary>
        /// Adds a user who can interact with the <typeparamref name="TPaginator"/>.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>This builder.</returns>
        public virtual TBuilder AddUser(IUser user)
        {
            Users.Add(user ?? throw new ArgumentNullException(nameof(user)));
            return (TBuilder)this;
        }

        /// <summary>
        /// Sets the emotes and their related paginator actions.
        /// </summary>
        /// <param name="emotes">A dictionary of emotes and paginator actions.</param>
        public virtual TBuilder WithOptions(IDictionary<IEmote, PaginatorAction> emotes)
        {
            Options = emotes;
            return (TBuilder)this;
        }

        /// <summary>
        /// Adds an emote related to a paginator action.
        /// </summary>
        /// <param name="option">The pair of emote and action.</param>
        public virtual TBuilder AddOption(KeyValuePair<IEmote, PaginatorAction> option)
            => AddOption(option.Key, option.Value);

        /// <summary>
        /// Adds an emote related to a paginator action.
        /// </summary>
        /// <param name="emote">The emote.</param>
        /// <param name="action">The paginator action.</param>
        public virtual TBuilder AddOption(IEmote emote, PaginatorAction action)
        {
            Options.Add(emote, action);
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
        /// Clears all existing emote-action pairs and adds the default emote-action pairs of the <typeparamref name="TPaginator"/>.
        /// </summary>
        public virtual TBuilder WithDefaultEmotes()
        {
            Options.Clear();

            Options.Add(new Emoji("◀"), PaginatorAction.Backward);
            Options.Add(new Emoji("▶"), PaginatorAction.Forward);
            Options.Add(new Emoji("⏮"), PaginatorAction.SkipToStart);
            Options.Add(new Emoji("⏭"), PaginatorAction.SkipToEnd);
            Options.Add(new Emoji("🛑"), PaginatorAction.Exit);

            return (TBuilder)this;
        }

        /// <summary>
        /// Sets the default canceled page.
        /// </summary>
        public virtual TBuilder WithDefaultCanceledPage()
            => WithCanceledPage(new PageBuilder().WithColor(Color.Orange).WithTitle("Canceled! 👍"));

        /// <summary>
        /// Sets the default timeout page.
        /// </summary>
        public virtual TBuilder WithDefaultTimeoutPage()
            => WithTimeoutPage(new PageBuilder().WithColor(Color.Red).WithTitle("Timed out! ⏰"));

        /// <inheritdoc/>
        TPaginator IInteractiveBuilderMethods<TPaginator, KeyValuePair<IEmote, PaginatorAction>, TBuilder>.Build() => Build();

        /// <inheritdoc/>
        TBuilder IInteractiveBuilderMethods<TPaginator, KeyValuePair<IEmote, PaginatorAction>, TBuilder>.WithOptions(ICollection<KeyValuePair<IEmote, PaginatorAction>> options)
            => WithOptions(options.ToDictionary(x => x.Key, x => x.Value));
    }
}