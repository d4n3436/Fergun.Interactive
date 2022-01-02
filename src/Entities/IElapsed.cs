using System;

namespace Fergun.Interactive
{
    /// <summary>
    /// Provides an elapsed time.
    /// </summary>
    public interface IElapsed
    {
        /// <summary>
        /// Gets the elapsed time.
        /// </summary>
        TimeSpan Elapsed { get; }
    }
}