using System;

namespace Fergun.Interactive.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IInteractiveBuilder{TElement, TOption, TBuilder}"/>.
    /// </summary>
    public static class InteractiveBuilderExtensions
    {
        /// <summary>
        /// Applies an action to this builder.
        /// </summary>
        /// <typeparam name="TElement">The type of the built element.</typeparam>
        /// <typeparam name="TOption">The type of the options.</typeparam>
        /// <typeparam name="TBuilder">The type of this builder.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="action">The action.</param>
        /// <returns>This builder.</returns>
        public static TBuilder With<TElement, TOption, TBuilder>(this IInteractiveBuilder<TElement, TOption, TBuilder> builder, Action<TBuilder> action)
            where TElement : IInteractiveElement<TOption>
            where TBuilder : IInteractiveBuilder<TElement, TOption, TBuilder>
        {
            InteractiveGuards.NotNull(builder, nameof(builder));
            InteractiveGuards.NotNull(action, nameof(action));

            action((TBuilder)builder);
            return (TBuilder)builder;
        }
    }
}