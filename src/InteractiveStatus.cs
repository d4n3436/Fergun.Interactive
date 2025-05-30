using JetBrains.Annotations;

namespace Fergun.Interactive;

/// <summary>
/// Specifies the possible status of an <see cref="IInteractiveResult{TStatus}"/> whose status is <see cref="InteractiveStatus"/>.
/// </summary>
[PublicAPI]
public enum InteractiveStatus
{
    /// <summary>
    /// The interactive action status is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The interactive action was successful.
    /// </summary>
    Success = 1,

    /// <summary>
    /// The interactive action timed out.
    /// </summary>
    Timeout = 2,

    /// <summary>
    /// The interactive action was canceled.
    /// </summary>
    Canceled = 3
}