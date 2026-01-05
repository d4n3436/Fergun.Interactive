using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Fergun.Interactive;

/// <summary>
/// Represents a generic result of an interactive action.
/// </summary>
/// <typeparam name="T">The type of the value or values of this result.</typeparam>
[PublicAPI]
public class InteractiveResult<T> : InteractiveResult
{
    internal InteractiveResult(T? value, TimeSpan elapsed, InteractiveStatus status = InteractiveStatus.Success)
        : base(elapsed, status)
    {
        Value = value;
        Values = value is null ? [] : [value];
    }

    internal InteractiveResult(IReadOnlyList<T> values, TimeSpan elapsed, InteractiveStatus status = InteractiveStatus.Success)
        : base(elapsed, status)
    {
        ArgumentNullException.ThrowIfNull(values);

        Values = values;
        Value = values.Count > 0 ? values[0] : default;
    }

    /// <summary>
    /// Gets the value representing the result returned by the interactive action.
    /// </summary>
    /// <remarks>
    /// This value is not <see langword="null"/> if <see cref="IsSuccess"/> is <see langword="true"/>.<br/>
    /// If multiple values were returned because the interactive action allowed for it, this property will only contain the first value (the complete list of values is exposed on <see cref="Values"/>).
    /// </remarks>
    public T? Value { get; }

    /// <summary>
    /// Gets a read-only list containing the values returned by the interactive action.
    /// </summary>
    /// <remarks>The list won't be empty if at least one or multiple options were selected (e.g., through a selection using a select menu).</remarks>
    public IReadOnlyList<T> Values { get; }

    /// <inheritdoc/>
    [MemberNotNullWhen(true, nameof(Value))]
    public override bool IsSuccess => Status == InteractiveStatus.Success;
}

/// <summary>
/// Represents a result of an interactive action.
/// </summary>
[PublicAPI]
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