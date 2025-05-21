namespace Fergun.Interactive;

/// <summary>
/// Specifies the possible status of the input handler of an interactive entity.
/// </summary>
/// <remarks>This is used as the status of the result of input handlers on component paginators and entities that implement <see cref="IInteractiveInputHandler"/>.</remarks>
public enum InteractiveInputStatus
{
    /// <summary>
    /// The handling of the input was successful.
    /// </summary>
    Success,

    /// <summary>
    /// The input was ignored.
    /// </summary>
    Ignored,

    /// <summary>
    /// The handling of the input was successful and the interactive entity should be canceled.
    /// </summary>
    Canceled
}