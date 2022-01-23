using System;
using System.Diagnostics.CodeAnalysis;

namespace Fergun.Interactive
{
    /// <summary>
    /// Represents a generic result of an interactive action.
    /// </summary>
    /// <typeparam name="T">The type of the value of this result.</typeparam>
    public class InteractiveResult<T> : InteractiveResult
    {
        internal InteractiveResult(T? value, TimeSpan elapsed, InteractiveStatus status = InteractiveStatus.Success)
        : base(elapsed, status)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the value representing the result returned by the interactive action.
        /// </summary>
        /// <remarks>
        /// This value is not <see langword="null"/> if <see cref="IsSuccess"/> is <see langword="true"/>.
        /// </remarks>
        public T? Value { get; }

        /// <inheritdoc/>
        [MemberNotNullWhen(true, nameof(Value))]
        public override bool IsSuccess => Status == InteractiveStatus.Success;
    }

    /// <summary>
    /// Represents a non-generic result of an interactive action.
    /// </summary>
    public class InteractiveResult : IInteractiveResult<InteractiveStatus>, IElapsed
    {
        internal InteractiveResult(TimeSpan elapsed, InteractiveStatus status = InteractiveStatus.Success)
        {
            Elapsed = elapsed;
            Status = status;
        }

        /// <summary>
        /// Gets the time passed between starting the interactive action and getting its result.
        /// </summary>
        public virtual TimeSpan Elapsed { get; }

        /// <summary>
        /// Gets the status of this result.
        /// </summary>
        public virtual InteractiveStatus Status { get; }

        /// <summary>
        /// Gets a value indicating whether the interactive action timed out.
        /// </summary>
        public virtual bool IsTimeout => Status == InteractiveStatus.Timeout;

        /// <summary>
        /// Gets a value indicating whether the interactive action was canceled.
        /// </summary>
        public virtual bool IsCanceled => Status == InteractiveStatus.Canceled;

        /// <summary>
        /// Gets a value indicating whether the interactive action was successful.
        /// </summary>
        public virtual bool IsSuccess => Status == InteractiveStatus.Success;
    }
}