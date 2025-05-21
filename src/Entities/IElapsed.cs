using System;
using System.ComponentModel;

namespace Fergun.Interactive;

/// <summary>
/// Provides an elapsed time.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IElapsed
{
    /// <summary>
    /// Gets the elapsed time.
    /// </summary>
    TimeSpan Elapsed { get; }
}