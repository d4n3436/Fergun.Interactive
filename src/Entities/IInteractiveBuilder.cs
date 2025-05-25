namespace Fergun.Interactive;

/// <summary>
/// Represents a builder of interactive elements.
/// </summary>
/// <typeparam name="TElement">The type of the built element.</typeparam>
/// <typeparam name="TOption">The type of the options.</typeparam>
/// <typeparam name="TBuilder">The type of this builder.</typeparam>
public interface IInteractiveBuilder<out TElement, TOption, out TBuilder>
    : IInteractiveBuilderProperties<TOption>, IInteractiveBuilderMethods<TElement, TOption, TBuilder>
    where TElement : IInteractiveElement<TOption>
    where TBuilder : IInteractiveBuilder<TElement, TOption, TBuilder>;