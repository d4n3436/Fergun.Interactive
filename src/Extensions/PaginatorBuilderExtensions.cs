using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fergun.Interactive.Extensions;
using JetBrains.Annotations;
using NetCord;
using NetCord.Rest;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Provides extension methods for <see cref="IComponentPaginatorBuilder"/>.
/// </summary>
[PublicAPI]
public static class PaginatorBuilderExtensions
{
    /// <summary>
    /// Sets the number of pages the paginator initially has.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the component paginator builder.</typeparam>
    /// <param name="builder">A paginator builder that implements <see cref="IComponentPaginatorBuilder"/>.</param>
    /// <param name="pageCount">The page count.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public static TBuilder WithPageCount<TBuilder>(this TBuilder builder, int pageCount)
        where TBuilder : class, IComponentPaginatorBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.PageCount = pageCount;
        return builder;
    }

    /// <summary>
    /// Sets the index of the page from where the paginator should start.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the component paginator builder.</typeparam>
    /// <param name="builder">A paginator builder that implements <see cref="IComponentPaginatorBuilder"/>.</param>
    /// <param name="initialPageIndex">The initial page index.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public static TBuilder WithInitialPageIndex<TBuilder>(this TBuilder builder, int initialPageIndex)
        where TBuilder : class, IComponentPaginatorBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.InitialPageIndex = initialPageIndex;
        return builder;
    }

    /// <summary>
    /// Sets the method used to load the pages of the paginator lazily.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the component paginator builder.</typeparam>
    /// <remarks>The first argument of the factory is the paginator. <see cref="IComponentPaginator.CurrentPageIndex"/> can be used to get the current page index.</remarks>
    /// <param name="builder">A paginator builder that implements <see cref="IComponentPaginatorBuilder"/>.</param>
    /// <param name="pageFactory">The page factory. The first argument is the paginator. <see cref="IComponentPaginator.CurrentPageIndex"/> can be used to get the current page index.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="pageFactory"/> are <see langword="null"/>.</exception>
    public static TBuilder WithPageFactory<TBuilder>(this TBuilder builder, Func<IComponentPaginator, IPage> pageFactory)
        where TBuilder : class, IComponentPaginatorBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(pageFactory);

        return builder.WithPageFactory(paginator => new ValueTask<IPage>(pageFactory(paginator)));
    }

    /// <summary>
    /// Sets the method used to load the pages of the paginator lazily.
    /// </summary>
    /// <remarks>The first argument of the factory is the paginator. <see cref="IComponentPaginator.CurrentPageIndex"/> can be used to get the current page index.</remarks>
    /// <typeparam name="TBuilder">The type of the component paginator builder.</typeparam>
    /// <typeparam name="TPage">A type that implements <see cref="IPage"/>.</typeparam>
    /// <param name="builder">A paginator builder that implements <see cref="IComponentPaginatorBuilder"/>.</param>
    /// <param name="pageFactory">The page factory. The first argument is the paginator. <see cref="IComponentPaginator.CurrentPageIndex"/> can be used to get the current page index.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="pageFactory"/> are <see langword="null"/>.</exception>
    public static TBuilder WithPageFactory<TBuilder, TPage>(this TBuilder builder, Func<IComponentPaginator, ValueTask<TPage>> pageFactory)
        where TBuilder : class, IComponentPaginatorBuilder
        where TPage : IPage
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(pageFactory);

        builder.PageFactory = pageFactory as Func<IComponentPaginator, ValueTask<IPage>> ?? (async paginator => await pageFactory(paginator).ConfigureAwait(false));
        return builder;
    }

    /// <summary>
    /// Sets the user state of the paginator. This can be used to store any user-defined data that needs to be retrieved elsewhere (like component interaction commands).
    /// </summary>
    /// <typeparam name="TBuilder">The type of the component paginator builder.</typeparam>
    /// <param name="builder">A paginator builder that implements <see cref="IComponentPaginatorBuilder"/>.</param>
    /// <param name="state">The user state.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public static TBuilder WithUserState<TBuilder>(this TBuilder builder, object state)
        where TBuilder : IComponentPaginatorBuilder
    {
        builder.UserState = state;
        return builder;
    }

    /// <summary>
    /// Sets the users who can interact with the paginator.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the component paginator builder.</typeparam>
    /// <param name="builder">A paginator builder that implements <see cref="IComponentPaginatorBuilder"/>.</param>
    /// <param name="users">The users.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="users"/> are <see langword="null"/>.</exception>
    public static TBuilder WithUsers<TBuilder>(this TBuilder builder, params User[] users)
        where TBuilder : class, IComponentPaginatorBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(users);

        builder.Users = users.ToList();
        return builder;
    }

    /// <summary>
    /// Sets the users who can interact with the paginator.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the component paginator builder.</typeparam>
    /// <param name="builder">A paginator builder that implements <see cref="IComponentPaginatorBuilder"/>.</param>
    /// <param name="users">The users.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="users"/> are <see langword="null"/>.</exception>
    public static TBuilder WithUsers<TBuilder>(this TBuilder builder, IEnumerable<User> users)
        where TBuilder : class, IComponentPaginatorBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(users);

        builder.Users = users.ToList();
        return builder;
    }

    /// <summary>
    /// Adds a user who can interact with the paginator.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the component paginator builder.</typeparam>
    /// <param name="builder">A paginator builder that implements <see cref="IComponentPaginatorBuilder"/>.</param>
    /// <param name="user">The user.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="user"/> are <see langword="null"/>.</exception>
    public static TBuilder AddUser<TBuilder>(this TBuilder builder, User user)
        where TBuilder : class, IComponentPaginatorBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(user);

        builder.Users.Add(user);
        return builder;
    }

    /// <summary>
    /// Sets the action that will be done after a cancellation.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the component paginator builder.</typeparam>
    /// <param name="builder">A paginator builder that implements <see cref="IComponentPaginatorBuilder"/>.</param>
    /// <param name="action">The action.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public static TBuilder WithActionOnCancellation<TBuilder>(this TBuilder builder, ActionOnStop action)
        where TBuilder : class, IComponentPaginatorBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ActionOnCancellation = action;
        return builder;
    }

    /// <summary>
    /// Sets the action that will be done after a timeout.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the component paginator builder.</typeparam>
    /// <param name="builder">A paginator builder that implements <see cref="IComponentPaginatorBuilder"/>.</param>
    /// <param name="action">The action.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public static TBuilder WithActionOnTimeout<TBuilder>(this TBuilder builder, ActionOnStop action)
        where TBuilder : class, IComponentPaginatorBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ActionOnTimeout = action;
        return builder;
    }

    /// <summary>
    /// Sets the <see cref="IPage"/> which the paginator gets modified to after a cancellation.
    /// </summary>
    /// <remarks>This will set <see cref="IComponentPaginatorBuilder.ActionOnCancellation"/> to <see cref="ActionOnStop.ModifyMessage"/>.</remarks>
    /// <typeparam name="TBuilder">The type of the component paginator builder.</typeparam>
    /// <param name="builder">A paginator builder that implements <see cref="IComponentPaginatorBuilder"/>.</param>
    /// <param name="page">The page.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="page"/> are <see langword="null"/>.</exception>
    public static TBuilder WithCanceledPage<TBuilder>(this TBuilder builder, IPage page)
        where TBuilder : class, IComponentPaginatorBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(page);

        builder.CanceledPage = page;
        return builder.WithActionOnCancellation(ActionOnStop.ModifyMessage);
    }

    /// <summary>
    /// Sets the <see cref="IPage"/> which the paginator gets modified to after a timeout.
    /// </summary>
    /// <remarks>This will set <see cref="IComponentPaginatorBuilder.ActionOnCancellation"/> to <see cref="ActionOnStop.ModifyMessage"/>.</remarks>
    /// <typeparam name="TBuilder">The type of the component paginator builder.</typeparam>
    /// <param name="builder">A paginator builder that implements <see cref="IComponentPaginatorBuilder"/>.</param>
    /// <param name="page">The page.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="page"/> are <see langword="null"/>.</exception>
    public static TBuilder WithTimeoutPage<TBuilder>(this TBuilder builder, IPage page)
        where TBuilder : class, IComponentPaginatorBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(page);

        builder.TimeoutPage = page;
        return builder.WithActionOnTimeout(ActionOnStop.ModifyMessage);
    }

    /// <summary>
    /// Sets the behavior the paginator should exhibit when a user is not allowed to interact with it.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the component paginator builder.</typeparam>
    /// <param name="builder">A paginator builder that implements <see cref="IComponentPaginatorBuilder"/>.</param>
    /// <param name="behavior">The behavior.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public static TBuilder WithRestrictedInputBehavior<TBuilder>(this TBuilder builder, RestrictedInputBehavior behavior)
        where TBuilder : class, IComponentPaginatorBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.RestrictedInputBehavior = behavior;
        return builder;
    }

    /// <summary>
    /// Sets the factory of the modal that will be displayed when the user clicks the jump button. The paginator will use a default/standard jump modal if one is not provided.
    /// </summary>
    /// <remarks>The first argument of the factory is the current paginator. The paginator will automatically set the custom ID of the modal to identify it and will take the page number from the first text input.</remarks>
    /// <typeparam name="TBuilder">The type of the component paginator builder.</typeparam>
    /// <param name="builder">A paginator builder that implements <see cref="IComponentPaginatorBuilder"/>.</param>
    /// <param name="modalFactory">The jump modal factory. The first argument is the current paginator.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="modalFactory"/> are <see langword="null"/>.</exception>
    public static TBuilder WithJumpModalFactory<TBuilder>(this TBuilder builder, Func<IComponentPaginator, ModalProperties> modalFactory)
        where TBuilder : class, IComponentPaginatorBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(modalFactory);

        builder.JumpModalFactory = modalFactory;
        return builder;
    }

    /// <summary>
    /// Sets the <see cref="IPage"/> that will be displayed ephemerally to a user when they are not allowed to interact with the paginator.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the component paginator builder.</typeparam>
    /// <param name="builder">A paginator builder that implements <see cref="IComponentPaginatorBuilder"/>.</param>
    /// <param name="page">The page.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="page"/> are <see langword="null"/>.</exception>
    public static TBuilder WithRestrictedPage<TBuilder>(this TBuilder builder, IPage page)
        where TBuilder : class, IComponentPaginatorBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(page);

        return builder.WithRestrictedPageFactory(_ => page);
    }

    /// <summary>
    /// Sets the factory of the <see cref="IPage"/> that will be displayed ephemerally to a user when they are not allowed to interact with the paginator.
    /// </summary>
    /// <remarks>The first argument of the factory is the current paginator. <see cref="IComponentPaginator.Users"/> can be used to display what users are allowed to interact with the paginator.</remarks>
    /// <typeparam name="TBuilder">The type of the component paginator builder.</typeparam>
    /// <param name="builder">A paginator builder that implements <see cref="IComponentPaginatorBuilder"/>.</param>
    /// <param name="pageFactory">The restricted page factory. The first argument is the current paginator. <see cref="IComponentPaginator.Users"/> can be used to display what users are allowed to interact with the paginator.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="pageFactory"/> are <see langword="null"/>.</exception>
    public static TBuilder WithRestrictedPageFactory<TBuilder>(this TBuilder builder, Func<IComponentPaginator, IPage> pageFactory)
        where TBuilder : class, IComponentPaginatorBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(pageFactory);

        builder.RestrictedPageFactory = pageFactory;
        return builder;
    }

    /// <summary>
    /// Configures the paginator to use a standard restricted page. The page contains an embed with the description "🚫 Only (<c>allowed users</c>) can respond to this interaction.".
    /// </summary>
    /// <typeparam name="TBuilder">The type of the component paginator builder.</typeparam>
    /// <param name="builder">A paginator builder that implements <see cref="IComponentPaginatorBuilder"/>.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public static TBuilder WithStandardRestrictedPage<TBuilder>(this TBuilder builder)
        where TBuilder : class, IComponentPaginatorBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithRestrictedPageFactory(paginator =>
        {
            return new PageBuilder()
                .WithColor(Color.Orange)
                .WithDescription($"🚫 Only {string.Join(", ", paginator.Users.Select(x => x.Mention))} can respond to this interaction.")
                .Build();
        });
    }
}