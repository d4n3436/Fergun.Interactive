using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Fergun.Interactive.Selection;

/// <summary>
/// Represents a builder of <see cref="MenuSelection{TOption}"/>. The menu selection uses <see cref="InputHandler"/> to dynamically change the page it's currently displaying.
/// </summary>
/// <typeparam name="TOption">The type of the options.</typeparam>
[PublicAPI]
public sealed class MenuSelectionBuilder<TOption> : BaseSelectionBuilder<MenuSelection<TOption>, TOption, MenuSelectionBuilder<TOption>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuSelectionBuilder{TOption}"/> class.
    /// </summary>
    public MenuSelectionBuilder()
    {
    }

    /// <summary>
    /// Gets or sets a value indicating whether to set the default values on select menus. The values will be the last selected options.
    /// </summary>
    public bool SetDefaultValues { get; set; }

    /// <summary>
    /// Gets or sets the delegate that will be executed when a valid input is received (except cancel options).
    /// </summary>
    /// <remarks>
    /// The first argument of the delegate is a read-only list containing at least one selected option and the return value is a <see cref="ValueTask{TResult}"/> containing the page the selection should display.
    /// Return a <see cref="ValueTask{TResult}"/> containing a <see langword="null"/> page to leave the message unmodified.
    /// </remarks>
    public Func<IReadOnlyList<TOption>, ValueTask<IPage?>> InputHandler { get; set; } = null!;

    /// <summary>
    /// Builds this <see cref="MenuSelectionBuilder{TOption}"/> into an immutable <see cref="MenuSelection{TOption}"/>.
    /// </summary>
    /// <returns>A <see cref="MenuSelection{TOption}"/>.</returns>
    public override MenuSelection<TOption> Build() => new(this);

    /// <summary>
    /// Sets a value indicating whether to set the default values on select menus. The values will be the last selected options.
    /// </summary>
    /// <param name="setDefaultValues">Whether to set the default values.</param>
    /// <returns>This builder.</returns>
    public MenuSelectionBuilder<TOption> WithSetDefaultValues(bool setDefaultValues)
    {
        SetDefaultValues = setDefaultValues;
        return this;
    }

    /// <summary>
    /// Sets the delegate that will be executed when a valid input is received (except cancel options).
    /// </summary>
    /// <remarks>
    /// The first argument of the delegate is the result of a valid input and the return value is the page the selection should display.
    /// Return <see langword="null"/> to leave the message unmodified.
    /// </remarks>
    /// <param name="inputHandler">
    /// The delegate. The first argument is the result of a valid input and the return value is the page the selection should display.
    /// Return <see langword="null"/> to leave the message unmodified.
    /// </param>
    /// <returns>This builder.</returns>
    public MenuSelectionBuilder<TOption> WithInputHandler(Func<TOption, IPage?> inputHandler)
    {
        InteractiveGuards.NotNull(inputHandler);
        return WithInputHandler(input => new ValueTask<IPage?>(inputHandler(input[0])));
    }

    /// <summary>
    /// Sets the delegate that will be executed when a valid input is received (except cancel options).
    /// </summary>
    /// <remarks>
    /// The first argument of the delegate is a read-only list containing the selected options of a multi-choice select menu
    /// and the return value is the page the selection should display. Return <see langword="null"/> to leave the message unmodified.
    /// </remarks>
    /// <param name="inputHandler">
    /// The delegate. The first argument is a read-only list containing the selected options of a multi-choice select menu
    /// and the return value is the page the selection should display. Return <see langword="null"/> to leave the message unmodified.
    /// </param>
    /// <returns>This builder.</returns>
    public MenuSelectionBuilder<TOption> WithInputHandler(Func<IReadOnlyList<TOption>, IPage?> inputHandler)
    {
        InteractiveGuards.NotNull(inputHandler);
        return WithInputHandler(input => new ValueTask<IPage?>(inputHandler(input)));
    }

    /// <summary>
    /// Sets the delegate that will be executed when a valid input is received (except cancel options).
    /// </summary>
    /// <typeparam name="TPage">A type that implements <see cref="IPage"/>.</typeparam>
    /// <remarks>
    /// The first argument of the delegate is the result of a valid input and the return value is a <see cref="ValueTask{TResult}"/> containing the page the selection should display.
    /// Return a <see cref="ValueTask{TResult}"/> containing a <see langword="null"/> page to leave the message unmodified.
    /// </remarks>
    /// <param name="inputHandler">
    /// The delegate. The first argument is the result of a valid input and return value is a <see cref="ValueTask{TResult}"/> containing the page the selection should display.
    /// Return a <see cref="ValueTask{TResult}"/> containing a <see langword="null"/> page to leave the message unmodified.
    /// </param>
    /// <returns>This builder.</returns>
    public MenuSelectionBuilder<TOption> WithInputHandler<TPage>(Func<TOption, ValueTask<TPage?>> inputHandler)
        where TPage : IPage
    {
        InteractiveGuards.NotNull(inputHandler);
        return WithInputHandler(async input => await inputHandler(input[0]).ConfigureAwait(false));
    }

    /// <summary>
    /// Sets the delegate that will be executed when a valid input is received (except cancel options).
    /// </summary>
    /// <typeparam name="TPage">A type that implements <see cref="IPage"/>.</typeparam>
    /// <remarks>
    /// The first argument of the delegate is a read-only list containing the selected options of a multi-choice select menu and the return value is a <see cref="ValueTask{TResult}"/> containing the page the selection should display.
    /// Return a <see cref="ValueTask{TResult}"/> containing a <see langword="null"/> page to leave the message unmodified.
    /// </remarks>
    /// <param name="inputHandler">
    /// The delegate. The first argument is a read-only list containing the selected options of a multi-choice select menu and the return value is a <see cref="ValueTask{TResult}"/> containing the page the selection should display.
    /// Return a <see cref="ValueTask{TResult}"/> containing a <see langword="null"/> page to leave the message unmodified.
    /// </param>
    /// <returns>This builder.</returns>
    public MenuSelectionBuilder<TOption> WithInputHandler<TPage>(Func<IReadOnlyList<TOption>, ValueTask<TPage?>> inputHandler)
        where TPage : IPage
    {
        InteractiveGuards.NotNull(inputHandler);
        InputHandler = inputHandler as Func<IReadOnlyList<TOption>, ValueTask<IPage?>> ?? (async input => await inputHandler(input).ConfigureAwait(false));
        return this;
    }
}