using System;

namespace Fergun.Interactive
{
    /// <summary>
    /// Represents a result of an interactive action.
    /// </summary>
    /// <typeparam name="TStatus">The type of the status.</typeparam>
    public interface IInteractiveResult<out TStatus> where TStatus : Enum
    {
        /// <summary>
        /// Gets the status of this result.
        /// </summary>
        TStatus Status { get; }
    }
}