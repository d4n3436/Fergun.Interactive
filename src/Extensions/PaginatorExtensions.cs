using Discord;
using Fergun.Interactive.Pagination;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Fergun.Interactive.Extensions;

/// <summary>
/// Provides extension methods for paginators.
/// </summary>
public static class PaginatorExtensions
{
    /// <summary>
    /// Sets the maximum page index of a lazy paginator based on the number of items in a collection.
    /// </summary>
    /// <typeparam name="TPaginator">The type of the lazy paginator.</typeparam>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <typeparam name="T">The type of the elements in <paramref name="collection"/>.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="collection">The collection.</param>
    /// <returns>This builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="collection"/> are <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="collection"/> is empty.</exception>
    public static BaseLazyPaginatorBuilder<TPaginator, TBuilder> WithMaxPageIndex<TPaginator, TBuilder, T>(this BaseLazyPaginatorBuilder<TPaginator, TBuilder> builder, IReadOnlyCollection<T> collection)
        where TPaginator : BaseLazyPaginator
        where TBuilder : BaseLazyPaginatorBuilder<TPaginator, TBuilder>
    {
        InteractiveGuards.NotNull(builder);
        InteractiveGuards.NotNull(collection);
        InteractiveGuards.NotEmpty(collection);

        return builder.WithMaxPageIndex(collection.Count - 1);
    }

    /// <summary>
    /// Increments the <see cref="IComponentPaginator.CurrentPageIndex"/>, so it points to the next page if possible.
    /// </summary>
    /// <returns><see langword="true"/> if <see cref="IComponentPaginator.CurrentPageIndex"/> is lower than <see cref="IComponentPaginator.PageCount"/> minus 1; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="paginator"/> is <see langword="null"/>.</exception>
    public static bool NextPage(this IComponentPaginator paginator)
    {
        InteractiveGuards.NotNull(paginator);

        return paginator.SetPage(paginator.CurrentPageIndex + 1);
    }

    /// <summary>
    /// Decrements the <see cref="IComponentPaginator.CurrentPageIndex"/>, so it points to the previous page if possible.
    /// </summary>
    /// <returns><see langword="true"/> if <see cref="IComponentPaginator.CurrentPageIndex"/> higher than 0; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="paginator"/> is <see langword="null"/>.</exception>
    public static bool PreviousPage(this IComponentPaginator paginator)
    {
        InteractiveGuards.NotNull(paginator);

        return paginator.SetPage(paginator.CurrentPageIndex - 1);
    }

    /// <summary>
    /// Sets the <see cref="IComponentPaginator.CurrentPageIndex"/> to 0, so it points to the first page if possible.
    /// </summary>
    /// <returns><see langword="true"/> if <see cref="IComponentPaginator.CurrentPageIndex"/> is not 0; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="paginator"/> is <see langword="null"/>.</exception>
    public static bool FirstPage(this IComponentPaginator paginator)
    {
        InteractiveGuards.NotNull(paginator);

        return paginator.SetPage(0);
    }

    /// <summary>
    /// Sets the <see cref="IComponentPaginator.CurrentPageIndex"/> to <see cref="IComponentPaginator.PageCount"/> minus 1, so it points to the last page if possible.
    /// </summary>
    /// <returns><see langword="true"/> if <see cref="IComponentPaginator.CurrentPageIndex"/> is not <see cref="IComponentPaginator.PageCount"/> minus 1; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="paginator"/> is <see langword="null"/>.</exception>
    public static bool LastPage(this IComponentPaginator paginator)
    {
        InteractiveGuards.NotNull(paginator);

        return paginator.SetPage(paginator.PageCount - 1);
    }

    /// <summary>
    /// Returns a value indicating whether the specified user can interact with this paginator.
    /// </summary>
    /// <param name="paginator">The paginator.</param>
    /// <param name="user">The user.</param>
    /// <returns><see langword="true"/> if the user can interact with this paginator; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="paginator"/> or <paramref name="user"/> are is <see langword="null"/>.</exception>
    public static bool CanInteract(this IComponentPaginator paginator, IUser user)
    {
        InteractiveGuards.NotNull(paginator);
        InteractiveGuards.NotNull(user);

        return CanInteract(paginator, user.Id);
    }

    /// <summary>
    /// Returns a value indicating whether the specified user ID can interact with this paginator.
    /// </summary>
    /// <param name="paginator">The paginator.</param>
    /// <param name="userId">The user ID.</param>
    /// <returns><see langword="true"/> if the user can interact with this paginator; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="paginator"/> is <see langword="null"/>.</exception>
    public static bool CanInteract(this IComponentPaginator paginator, ulong userId)
    {
        InteractiveGuards.NotNull(paginator);

        if (paginator.Users.Count == 0)
        {
            return true;
        }

        foreach (var user in paginator.Users)
        {
            if (user.Id == userId)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Attempts to get the user state of the paginator as the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the user state.</typeparam>
    /// <param name="paginator">The paginator.</param>
    /// <param name="userState">The user state if it exists and matches the type.</param>
    /// <returns><see langword="true"/> if the user state exists and is of type <typeparamref name="T"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="paginator"/> is <see langword="null"/>.</exception>
    public static bool TryGetUserState<T>(this IComponentPaginator paginator, [MaybeNullWhen(false)] out T userState)
    {
        InteractiveGuards.NotNull(paginator);

        if (paginator.UserState is T state)
        {
            userState = state;
            return true;
        }

        userState = default;
        return false;
    }

    /// <summary>
    /// Gets the user state of the paginator as the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the user state.</typeparam>
    /// <param name="paginator">The paginator.</param>
    /// <returns>The user state if it exists and matches the type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="paginator"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidCastException">Thrown when <see cref="IComponentPaginator.UserState"/> is <see langword="null"/>, or is not of type <typeparamref name="T"/>.</exception>
    public static T GetUserState<T>(this IComponentPaginator paginator)
    {
        InteractiveGuards.NotNull(paginator);

        return paginator.UserState switch
        {
            T state => state,
            null => throw new InvalidCastException("User state is null."),
            _ => throw new InvalidCastException($"User state is of type {paginator.UserState.GetType()}, but type {typeof(T)} was passed.")
        };
    }
}