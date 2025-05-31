using System;
using System.ComponentModel;
using JetBrains.Annotations;

namespace Fergun.Interactive;

/// <summary>
/// Provides an elapsed time.
/// </summary>
[PublicAPI]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IElapsed
{
    /// <summary>
    /// Gets the elapsed time.
    /// </summary>
    TimeSpan Elapsed { get; }
}