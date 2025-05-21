using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents a builder for a component-based paginator.
/// </summary>
public class ComponentPaginatorBuilder : IComponentPaginatorBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentPaginatorBuilder"/> class.
    /// </summary>
    public ComponentPaginatorBuilder()
    {
    }

    /// <inheritdoc />
    public int PageCount { get; set; }

    /// <inheritdoc />
    public int InitialPageIndex { get; set; }

    /// <inheritdoc />
    public Func<IComponentPaginator, ValueTask<IPage>> PageFactory { get; set; } = null!;

    /// <inheritdoc />
    public object? UserState { get; set; }

    /// <inheritdoc />
    public ICollection<IUser> Users { get; set; } = [];

    /// <inheritdoc />
    /// <remarks>The default value is <see cref="ActionOnStop.ModifyMessage"/>.</remarks>
    public ActionOnStop ActionOnCancellation { get; set; } = ActionOnStop.ModifyMessage;

    /// <inheritdoc />
    /// <remarks>The default value is <see cref="ActionOnStop.ModifyMessage"/>.</remarks>
    public ActionOnStop ActionOnTimeout { get; set; } = ActionOnStop.ModifyMessage;

    /// <inheritdoc />
    public RestrictedInputBehavior RestrictedInputBehavior { get; set; }

    /// <inheritdoc />
    public IPage? CanceledPage { get; set; }

    /// <inheritdoc />
    public IPage? TimeoutPage { get; set; }

    /// <inheritdoc />
    public Func<IComponentPaginator, ModalBuilder>? JumpModalFactory { get; set; }

    /// <inheritdoc />
    public Func<IReadOnlyCollection<IUser>, IPage>? RestrictedPageFactory { get; set; }

    /// <summary>
    /// Builds this builder into a <see cref="ComponentPaginator"/>.
    /// </summary>
    /// <returns>A <see cref="ComponentPaginator"/>.</returns>
    public ComponentPaginator Build() => new(this);
}