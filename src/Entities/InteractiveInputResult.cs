namespace Fergun.Interactive
{
    /// <summary>
    /// Represents the result of an input handler in an interactive element.
    /// </summary>
    public class InteractiveInputResult : IInteractiveResult<InteractiveInputStatus>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveInputResult"/> structure with the specified action.
        /// </summary>
        /// <param name="status">The action.</param>
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
    /// Represents the result of an input handler in an interactive element, with a selected option.
    /// </summary>
    /// <typeparam name="TOption">The type of the selected option.</typeparam>
    public class InteractiveInputResult<TOption> : InteractiveInputResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveInputResult{TOption}"/> structure with the specified action.
        /// </summary>
        /// <param name="status">The action.</param>
        public InteractiveInputResult(InteractiveInputStatus status) : base(status)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveInputResult{TOption}"/> structure with the specified action and selected option.
        /// </summary>
        /// <param name="status">The action.</param>
        /// <param name="selectedOption">The selected option.</param>
        public InteractiveInputResult(InteractiveInputStatus status, TOption? selectedOption) : this(status)
        {
            SelectedOption = selectedOption;
        }

        /// <summary>
        /// Gets the selected option, or <see langword="default" /> if there is none.
        /// </summary>
        public TOption? SelectedOption { get; }

        /// <summary>
        /// Defines an implicit conversion of an <see cref="InteractiveInputStatus"/> to an <see cref="InteractiveInputResult{TOption}"/>.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>An <see cref="InteractiveInputResult{TOption}"/>.</returns>
        public static implicit operator InteractiveInputResult<TOption>(InteractiveInputStatus status) => new(status);
    }
}