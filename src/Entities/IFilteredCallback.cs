namespace Fergun.Interactive;

/// <summary>
/// Provides methods for testing the compatibility of incoming objects.
/// </summary>
internal interface IFilteredCallback : IInteractiveCallback
{
    /// <summary>
    /// Returns a value indicating whether the specified object is compatible with this handler.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object.</param>
    /// <returns><see langword="true"/> if the specified object is compatible with this handler; otherwise, <see langword="false"/>.</returns>
    bool IsCompatible<T>(T obj);

    /// <summary>
    /// Returns a value indicating whether the specified object triggers the filter of this handler.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object.</param>
    /// <returns><see langword="true"/> if the specified object triggers the filter of this handler; otherwise, <see langword="false"/>.</returns>
    bool TriggersFilter<T>(T obj);
}