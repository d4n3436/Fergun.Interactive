namespace Fergun.Interactive;

/// <summary>
/// Represents a builder for pages.
/// </summary>
/// <typeparam name="TPage">The type of the built page.</typeparam>
public interface IPageBuilder<out TPage> where TPage : IPage
{
    /// <summary>
    /// Builds this builder into a <typeparamref name="TPage"/>.
    /// </summary>
    /// <returns>A <typeparamref name="TPage"/>.</returns>
    TPage Build();
}

/// <inheritdoc/>
public interface IPageBuilder : IPageBuilder<IPage>;