using System;
using System.Collections.Generic;

namespace Fergun.Interactive;

/// <summary>
/// Represents the result of an input handler in an interactive element.
/// </summary>
public class InteractiveInputResult : IInteractiveResult<InteractiveInputStatus>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InteractiveInputResult"/> class with the specified status.
    /// </summary>
    /// <param name="status">The status.</param>
    public InteractiveInputResult(InteractiveInputStatus status)
    {
        Status = status;
    }

    /// <inheritdoc/>
    public InteractiveInputStatus Status { get; }

    /// <summary>
    /// Defines an implicit conversion of an <see cref="InteractiveInputStatus"/> to an <see cref="InteractiveInputResult"/>.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <returns>An <see cref="InteractiveInputResult"/>.</returns>
    public static implicit operator InteractiveInputResult(InteractiveInputStatus status) => new(status);
}

/// <summary>
/// Represents the result of an input handler in an interactive element, with a single or multiple selected options.
/// </summary>
/// <typeparam name="TOption">The type of the selected options.</typeparam>
public class InteractiveInputResult<TOption> : InteractiveInputResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InteractiveInputResult{TOption}"/> class with the specified status.
    /// </summary>
    /// <param name="status">The status.</param>
    public InteractiveInputResult(InteractiveInputStatus status) : base(status)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractiveInputResult{TOption}"/> class with the specified status and selected option.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <param name="selectedOption">The selected option.</param>
    public InteractiveInputResult(InteractiveInputStatus status, TOption? selectedOption) : this(status)
    {
        SelectedOption = selectedOption;
        SelectedOptions = selectedOption is null || EqualityComparer<TOption>.Default.Equals(selectedOption, default!) ? Array.Empty<TOption>() : [selectedOption];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractiveInputResult{TOption}"/> class with the specified status and selected options.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <param name="selectedOptions">The selected options.</param>
    public InteractiveInputResult(InteractiveInputStatus status, IReadOnlyList<TOption> selectedOptions) : this(status)
    {
        InteractiveGuards.NotNull(selectedOptions);
        InteractiveGuards.NotEmpty(selectedOptions);

        SelectedOptions = selectedOptions;
        SelectedOption = SelectedOptions[0];
    }

    /// <summary>
    /// Gets the selected option, or <see langword="default"/> if there is none.
    /// </summary>
    /// <remarks>If multiple options were selected, this property will only contain the first option (the complete list of options is exposed on <see cref="SelectedOptions"/>).</remarks>
    public TOption? SelectedOption { get; }

    /// <summary>
    /// Gets a read-only list containing the selected options.
    /// </summary>
    /// <remarks>The list won't be empty if at least one or multiple options were selected (e.g., through a select menu).</remarks>
    public IReadOnlyList<TOption> SelectedOptions { get; } = Array.Empty<TOption>();

    /// <summary>
    /// Defines an implicit conversion of an <see cref="InteractiveInputStatus"/> to an <see cref="InteractiveInputResult{TOption}"/>.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <returns>An <see cref="InteractiveInputResult{TOption}"/>.</returns>
    public static implicit operator InteractiveInputResult<TOption>(InteractiveInputStatus status) => new(status);
}