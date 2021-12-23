namespace Fergun.Interactive
{
    /// <summary>
    /// Represents a builder for pages.
    /// </summary>
    /// <typeparam name="TPage">The type of built page.</typeparam>
    public interface IPageBuilder<out TPage> where TPage : IPage
    {
        /// <summary>
        /// Builds this builder into an <typeparamref name="TPage"/>.
        /// </summary>
        /// <returns>An <typeparamref name="TPage"/>.</returns>
        TPage Build();
    }
}