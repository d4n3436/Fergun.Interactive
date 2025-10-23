using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Fergun.Interactive;

/// <summary>
/// Provides a fluent interface for <see cref="IInteractiveBuilder{TElement, TOption, TBuilder}"/>.
/// </summary>
/// <typeparam name="TElement">The type of the built element.</typeparam>
/// <typeparam name="TOption">The type of the options.</typeparam>
/// <typeparam name="TBuilder">The type of this builder.</typeparam>
[PublicAPI]
public interface IInteractiveBuilderMethods<out TElement, TOption, out TBuilder>
{
    /// <summary>
    /// Builds this interactive builder to an immutable <typeparamref name="TElement"/>.
    /// </summary>
    /// <returns>An immutable <typeparamref name="TElement"/>.</returns>
    TElement Build();

    /// <summary>
    /// Sets the users who can interact with the <typeparamref name="TElement"/>.
    /// </summary>
    /// <param name="users">The users.</param>
    /// <returns>This builder.</returns>
    TBuilder WithUsers(params NetCord.User[] users);

    /// <summary>
    /// Sets the users who can interact with the <typeparamref name="TElement"/>.
    /// </summary>
    /// <param name="users">The users.</param>
    /// <returns>This builder.</returns>
    TBuilder WithUsers(IEnumerable<NetCord.User> users);

    /// <summary>
    /// Adds a user who can interact with the <typeparamref name="TElement"/>.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>This builder.</returns>
    TBuilder AddUser(NetCord.User user);

    /// <summary>
    /// Sets the options.
    /// </summary>
    /// <param name="options">A collection of options.</param>
    /// <returns>This builder.</returns>
    TBuilder WithOptions(ICollection<TOption> options);

    /// <summary>
    /// Adds an option.
    /// </summary>
    /// <param name="option">The option.</param>
    /// <returns>This builder.</returns>
    TBuilder AddOption(TOption option);

    /// <summary>
    /// Sets the <see cref="IPage"/> which the <typeparamref name="TElement"/> gets modified to after a cancellation.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>This builder.</returns>
    TBuilder WithCanceledPage(IPageBuilder page);

    /// <summary>
    /// Sets the <see cref="IPage"/> which the <typeparamref name="TElement"/> gets modified to after a timeout.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>This builder.</returns>
    TBuilder WithTimeoutPage(IPageBuilder page);

    /// <summary>
    /// Sets what type of inputs the <typeparamref name="TElement"/> should delete.
    /// </summary>
    /// <param name="deletion">The deletion options.</param>
    /// <returns>This builder.</returns>
    TBuilder WithDeletion(DeletionOptions deletion);

    /// <summary>
    /// Sets input type, that is, what is used to interact with the <typeparamref name="TElement"/>.
    /// </summary>
    /// <param name="type">The input type.</param>
    /// <returns>This builder.</returns>
    TBuilder WithInputType(InputType type);

    /// <summary>
    /// Sets the action that will be done after a cancellation.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <returns>This builder.</returns>
    TBuilder WithActionOnCancellation(ActionOnStop action);

    /// <summary>
    /// Sets the action that will be done after a timeout.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <returns>This builder.</returns>
    TBuilder WithActionOnTimeout(ActionOnStop action);

    /// <summary>
    /// Sets the behavior the <typeparamref name="TElement"/> should exhibit when a user is not allowed to interact with it.
    /// </summary>
    /// <param name="behavior">The behavior.</param>
    /// <returns>This builder.</returns>
    TBuilder WithRestrictedInputBehavior(RestrictedInputBehavior behavior);

    /// <summary>
    /// Sets the <see cref="IPage"/> that will be displayed ephemerally to a user when they are not allowed to interact with the <typeparamref name="TElement"/>.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>This builder.</returns>
    TBuilder WithRestrictedPage(IPage page);

    /// <summary>
    /// Sets the factory of the <see cref="IPage"/> that will be displayed ephemerally to a user when they are not allowed to interact with the <typeparamref name="TElement"/>.
    /// </summary>
    /// <remarks>The first argument of the factory is a read-only collection of users who are allowed to interact with the paginator.</remarks>
    /// <param name="pageFactory">The restricted page factory.</param>
    /// <returns>This builder.</returns>
    TBuilder WithRestrictedPageFactory(Func<IReadOnlyCollection<NetCord.User>, IPage> pageFactory);
}