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
public class ComponentPaginatorBuilder : IComponentPaginatorBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentPaginatorBuilder"/> class.
    /// </summary>
    public ComponentPaginatorBuilder()
    {
    }

    /// <inheritdoc />
    public virtual int PageCount { get; set; }

    /// <inheritdoc />
    public virtual int InitialPageIndex { get; set; }

    /// <inheritdoc />
    public virtual Func<IComponentPaginator, ValueTask<IPage>> PageFactory { get; set; } = null!;

    /// <inheritdoc />
    public virtual object? UserState { get; set; }

    /// <inheritdoc />
    public virtual ICollection<NetCord.User> Users { get; set; } = [];

    /// <inheritdoc />
    /// <remarks>The default value is <see cref="ActionOnStop.None"/>.</remarks>
    public virtual ActionOnStop ActionOnCancellation { get; set; } = ActionOnStop.None;

    /// <inheritdoc />
    /// <remarks>The default value is <see cref="ActionOnStop.None"/>.</remarks>
    public virtual ActionOnStop ActionOnTimeout { get; set; } = ActionOnStop.None;

    /// <inheritdoc />
    public virtual RestrictedInputBehavior RestrictedInputBehavior { get; set; }

    /// <inheritdoc />
    public virtual IPage? CanceledPage { get; set; }

    /// <inheritdoc />
    public virtual IPage? TimeoutPage { get; set; }

    /// <inheritdoc />
    public virtual Func<IComponentPaginator, ModalProperties>? JumpModalFactory { get; set; }

    /// <inheritdoc />
    public virtual Func<IComponentPaginator, IPage>? RestrictedPageFactory { get; set; }

    /// <summary>
    /// Builds this builder into a <see cref="ComponentPaginator"/>.
    /// </summary>
    /// <returns>A <see cref="ComponentPaginator"/>.</returns>
    public virtual ComponentPaginator Build() => new(this);
}