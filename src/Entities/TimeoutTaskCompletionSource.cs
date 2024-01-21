using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fergun.Interactive;

/// <summary>
/// Represents a <see cref="TaskCompletionSource{TResult}"/> with a timeout timer which can be reset.
/// </summary>
/// <typeparam name="TResult">The type of the result value associated with this <see cref="TimeoutTaskCompletionSource{TResult}"/>.</typeparam>
public class TimeoutTaskCompletionSource<TResult> : IDisposable
{
    private readonly Timer _timer;
    private readonly bool _canReset;
    private TaskCompletionSource<TResult> _taskSource;
    private bool _disposed;
    private CancellationTokenRegistration _tokenRegistration; // Do not make readonly

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutTaskCompletionSource{TResult}"/> class.
    /// </summary>
    /// <param name="delay">The delay before the timeout.</param>
    /// <param name="canReset">Whether the internal <see cref="Timer"/> can be reset.</param>
    /// <param name="timeoutResult">The timeout result.</param>
    /// <param name="cancelResult">The cancel result.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public TimeoutTaskCompletionSource(TimeSpan delay, bool canReset = true, TResult? timeoutResult = default,
        TResult? cancelResult = default, CancellationToken cancellationToken = default)
    {
        Delay = delay;
        TimeoutResult = timeoutResult;
        CancelResult = cancelResult;
        _canReset = canReset;
        _taskSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        _timer = new Timer(OnTimerFired, null, delay, Timeout.InfiniteTimeSpan);
        _tokenRegistration = cancellationToken.Register(() => TryCancel());
    }

    /// <summary>
    /// Gets the delay before the timeout.
    /// </summary>
    public TimeSpan Delay { get; }

    /// <summary>
    /// Gets a value indicating whether the internal <see cref="Timer"/> can be reset.
    /// </summary>
    public bool CanReset => !_disposed && _canReset;

    /// <summary>
    /// Gets the timeout result.
    /// </summary>
    public TResult? TimeoutResult { get; }

    /// <summary>
    /// Gets the cancel result.
    /// </summary>
    public TResult? CancelResult { get; }

    /// <summary>
    /// Gets the <see cref="Task{TResult}"/> created by this <see cref="TimeoutTaskCompletionSource{TResult}"/>.
    /// </summary>
    public Task<TResult> Task => _taskSource.Task;

    /// <summary>
    /// Resets the underlying <see cref="TaskCompletionSource{TResult}"/>.
    /// </summary>
    public void ResetTaskSource() => _taskSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Attempts to reset the internal <see cref="Timer"/>.
    /// </summary>
    /// <returns><see langword="true"/> if the operation was successful; otherwise, <see langword="false"/>.</returns>
    public bool TryReset() => !_disposed && CanReset && _timer.Change(Delay, Timeout.InfiniteTimeSpan);

    /// <summary>
    /// Attempts to cancel the underlying <see cref="TaskCompletionSource{TResult}"/> using <see cref="CancelResult"/>.
    /// </summary>
    /// <returns><see langword="true"/> if the operation was successful; otherwise, <see langword="false"/>.</returns>
    public bool TryCancel() => !_disposed && TrySetResult(CancelResult);

    /// <summary>
    /// Attempts to set the result of the underlying <see cref="TaskCompletionSource{TResult}"/>.
    /// </summary>
    /// <param name="result">The result to set.</param>
    /// <returns><see langword="true"/> if the operation was successful; otherwise, <see langword="false"/>.</returns>
    public bool TrySetResult(TResult? result) => _taskSource.TrySetResult(result!);

    /// <summary>
    /// Disposes the internal <see cref="Timer"/> and cancels the underlying <see cref="TaskCompletionSource{TResult}"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="Dispose()"/>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _timer.Dispose();
            TryCancel();
            _tokenRegistration.Dispose();
            _disposed = true;
        }
    }

    private void OnTimerFired(object state)
    {
        TrySetResult(TimeoutResult);
    }
}