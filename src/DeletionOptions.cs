using System;
using JetBrains.Annotations;

namespace Fergun.Interactive;

/// <summary>
/// Specifies the type of inputs an interactive element should delete.
/// </summary>
[Flags]
[PublicAPI]
public enum DeletionOptions
{
    /// <summary>
    /// Don't delete anything.
    /// </summary>
    None = 0,

    /// <summary>
    /// Delete valid responses.
    /// </summary>
    Valid = 1 << 0,

    /// <summary>
    /// Delete invalid responses.
    /// </summary>
    Invalid = 1 << 1
}