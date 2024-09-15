namespace Fergun.Interactive;

/// <summary>
/// Specifies the behavior an interactive element should exhibit when a user is not allowed to interact with it.
/// </summary>
public enum RestrictedInputBehavior
{
    /// <summary>
    /// <see cref="SendMessage"/> if a restricted page is present; otherwise <see cref="Ignore"/>.
    /// </summary>
    /// <remarks>This is the default option.</remarks>
    Auto,

    /// <summary>
    /// Ignore the input. The interaction is not deferred.
    /// </summary>
    Ignore,

    /// <summary>
    /// Defer the interaction.
    /// </summary>
    Defer,

    /// <summary>
    /// Send a message (from <see cref="IInteractiveElement{TOption}.RestrictedPage"/>) to the user.
    /// </summary>
    /// <remarks>This requires the factory for the restricted page (<see cref="IInteractiveBuilderProperties{TOption}.RestrictedPageFactory"/>) to be set.</remarks>
    SendMessage
}