using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using JetBrains.Annotations;
using NetCord.Rest;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents a builder for a component-based paginator.
/// </summary>
[PublicAPI]
public interface IComponentPaginatorBuilder
{
    /// <summary>
    /// Gets or sets the total number of pages.
    /// </summary>
    int PageCount { get; set; }

    /// <summary>
    /// Gets or sets the index of the page from which the paginator should start.
    /// </summary>
    int InitialPageIndex { get; set; }

    /// <summary>
    /// Gets or sets the factory of the <see cref="IPage"/> that will be displayed when the paginator is updated.
    /// </summary>
    /// <remarks>The first argument is the current paginator. <see cref="IComponentPaginator.CurrentPageIndex"/> can be used to get the current page index.</remarks>
    Func<IComponentPaginator, ValueTask<IPage>> PageFactory { get; set; }

    /// <summary>
    /// Gets or sets the user state of the paginator. This can be used to store any user-defined data that needs to be retrieved elsewhere (like component interaction commands).
    /// </summary>
    object? UserState { get; set; }

    /// <summary>
    /// Gets or sets a collection of users who can interact with this paginator.
    /// </summary>
    ICollection<NetCord.User> Users { get; set; }

    /// <summary>
    /// Gets or sets the action that will be done after a cancellation.
    /// </summary>
    ActionOnStop ActionOnCancellation { get; set; }

    /// <summary>
    /// Gets or sets the action that will be done after a timeout.
    /// </summary>
    ActionOnStop ActionOnTimeout { get; set; }

    /// <summary>
    /// Gets or sets the behavior the paginator should exhibit when a user is not allowed to interact with it.
    /// </summary>
    RestrictedInputBehavior RestrictedInputBehavior { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="IPage"/> which the message gets modified to after cancellation.
    /// </summary>
    IPage? CanceledPage { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="IPage"/> which the message gets modified to after a timeout.
    /// </summary>
    IPage? TimeoutPage { get; set; }

    /// <summary>
    /// Gets or sets the factory of the modal that will be displayed when the user clicks the jump button. The paginator will use a default/standard jump modal if one is not provided.
    /// </summary>
    /// <remarks>The first argument is the current paginator. The paginator will automatically set the custom ID of the modal to identify it and will take the page number from the first text input.</remarks>
    Func<IComponentPaginator, ModalProperties>? JumpModalFactory { get; set; }

    /// <summary>
    /// Gets or sets the factory of the <see cref="IPage"/> that will be displayed ephemerally to a user when they are not allowed to interact with the paginator.
    /// </summary>
    /// <remarks>The first argument is the current paginator.</remarks>
    Func<IComponentPaginator, IPage>? RestrictedPageFactory { get; set; }
}