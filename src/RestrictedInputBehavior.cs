using Fergun.Interactive.Pagination;
using JetBrains.Annotations;

namespace Fergun.Interactive;

/// <summary>
/// Specifies the behavior an interactive element should exhibit when a user is not allowed to interact with it.
/// </summary>
[PublicAPI]
public enum RestrictedInputBehavior
{
    /// <summary>
    /// <see cref="SendMessage"/> if a restricted page is present; otherwise <see cref="Ignore"/>.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// Ignore the input. The interaction is not deferred.
    /// </summary>
    Ignore = 1,

    /// <summary>
    /// Defer the interaction.
    /// </summary>
    Defer = 2,

    /// <summary>
    /// Send a message (from <see cref="IInteractiveElement{TOption}.RestrictedPage"/> or <see cref="IComponentPaginator.RestrictedPageFactory"/>) to the user.
    /// </summary>
    /// <remarks>This requires the factory for the restricted page to be set.</remarks>
    SendMessage = 3
}