using System;
using System.Linq;
using JetBrains.Annotations;
using NetCord;

namespace Fergun.Interactive.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IInteractiveBuilder{TElement, TOption, TBuilder}"/>.
/// </summary>
[PublicAPI]
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
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(action);

        action((TBuilder)builder);
        return (TBuilder)builder;
    }

    /// <summary>
    /// Configures the builder with a default restricted page. The page contains an embed with the description "🚫 Only (<c>allowed users</c>) can respond to this interaction.".
    /// </summary>
    /// <typeparam name="TElement">The type of the built element.</typeparam>
    /// <typeparam name="TOption">The type of the options.</typeparam>
    /// <typeparam name="TBuilder">The type of this builder.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns>This builder.</returns>
    public static TBuilder WithDefaultRestrictedPage<TElement, TOption, TBuilder>(this IInteractiveBuilder<TElement, TOption, TBuilder> builder)
        where TElement : IInteractiveElement<TOption>
        where TBuilder : IInteractiveBuilder<TElement, TOption, TBuilder>
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithRestrictedPageFactory(users =>
        {
            return new PageBuilder()
                .WithColor(Color.Orange)
                .WithDescription($"🚫 Only {string.Join(", ", users.Select(x => x.Mention))} can respond to this interaction.")
                .Build();
        });
    }
}