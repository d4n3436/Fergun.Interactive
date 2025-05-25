using Discord;
using System;
using System.Collections.Generic;

namespace Fergun.Interactive;

/// <summary>
/// Contains the properties of an <see cref="IInteractiveBuilder{TElement, TOption, TBuilder}"/>.
/// </summary>
/// <typeparam name="TOption">The type of the options.</typeparam>
public interface IInteractiveBuilderProperties<TOption>
{
    /// <summary>
    /// Gets or sets the users who can interact with the element.
    /// </summary>
    ICollection<IUser> Users { get; set; }

    /// <summary>
    /// Gets or sets a collection of options.
    /// </summary>
    ICollection<TOption> Options { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="IPage"/> which the element gets modified to after cancellation.
    /// </summary>
    IPageBuilder? CanceledPage { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="IPage"/> which the element gets modified to after a timeout.
    /// </summary>
    IPageBuilder? TimeoutPage { get; set; }

    /// <summary>
    /// Gets or sets what type of inputs the element should delete.
    /// </summary>
    DeletionOptions Deletion { get; set; }

    /// <summary>
    /// Gets or sets the input type, that is, what is used to interact with the element.
    /// </summary>
    InputType InputType { get; set; }

    /// <summary>
    /// Gets or sets the action that will be done after a cancellation.
    /// </summary>
    ActionOnStop ActionOnCancellation { get; set; }

    /// <summary>
    /// Gets or sets the action that will be done after a timeout.
    /// </summary>
    ActionOnStop ActionOnTimeout { get; set; }

    /// <summary>
    /// Gets or sets the behavior the element should exhibit when a user is not allowed to interact with it.
    /// </summary>
    /// <remarks>The default value is <see cref="RestrictedInputBehavior.Auto"/>.</remarks>
    RestrictedInputBehavior RestrictedInputBehavior { get; set; }

    /// <summary>
    /// Gets or sets the factory of the <see cref="IPage"/> that will be displayed ephemerally to a user when they are not allowed to interact with the element.
    /// </summary>
    /// <remarks>The first argument of the factory is a read-only collection of users who are allowed to interact with the paginator.</remarks>
    Func<IReadOnlyCollection<IUser>, IPage>? RestrictedPageFactory { get; set; }
}