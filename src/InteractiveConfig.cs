using System;
using Discord;

namespace Fergun.Interactive;

/// <summary>
/// Represents a configuration for <see cref="InteractiveService"/>.
/// </summary>
public class InteractiveConfig
{
    private TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the default timeout for the interactive actions.
    /// </summary>
    public TimeSpan DefaultTimeout
    {
        get => _defaultTimeout;
        set
        {
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Timespan cannot be negative or zero.");
            }

            _defaultTimeout = value;
        }
    }

    /// <summary>
    /// Gets or sets the minimum log level severity that will be sent to the <see cref="InteractiveService.Log"/> event.
    /// </summary>
    public LogSeverity LogLevel { get; set; } = LogSeverity.Info;

    /// <summary>
    /// Gets or sets a value indicating if the SendPaginatorAsync() methods should immediately return after sending the paginated message instead of waiting for a timeout or a cancellation. The library will still handle its inputs in the background.
    /// </summary>
    /// <remarks>
    /// The default value is <see langword="false"/>. If set to <see langword="true"/>, the SendPaginatorAsync() methods will return an <see cref="InteractiveMessageResult"/> with status <see cref="InteractiveStatus.Success"/>.<br/>
    /// Note that any exceptions that would otherwise be thrown will be swallowed instead.
    /// </remarks>
    public bool ReturnAfterSendingPaginator { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if the interactions that stop paginators should be deferred.
    /// </summary>
    /// <remarks>
    /// These interactions get deferred depending of the <see cref="ActionOnStop"/> and some specific circumstances:<br/>
    /// - <see cref="ActionOnStop.None"/>: Always deferred.<br/>
    /// - <see cref="ActionOnStop.ModifyMessage"/>: If there's no page to modify to.<br/>
    /// - <see cref="ActionOnStop.DeleteInput"/> and <see cref="ActionOnStop.DisableInput"/>: If the input type is neither <see cref="InputType.Buttons"/> nor <see cref="InputType.SelectMenus"/>.
    /// </remarks>
    public bool DeferStopPaginatorInteractions { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating if the interactions that stop selections should be deferred.
    /// </summary>
    /// <remarks>
    /// These interactions get deferred depending of the <see cref="ActionOnStop"/> and some specific circumstances:<br/>
    /// - <see cref="ActionOnStop.None"/>: Always deferred.<br/>
    /// - <see cref="ActionOnStop.ModifyMessage"/>: If there's no page to modify to.<br/>
    /// - <see cref="ActionOnStop.DeleteInput"/> and <see cref="ActionOnStop.DisableInput"/>: If the input type is neither <see cref="InputType.Buttons"/> nor <see cref="InputType.SelectMenus"/>.
    /// </remarks>
    public bool DeferStopSelectionInteractions { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating if the SendPaginatorAsync() methods should completely process single-page paginators, that is, display their components, handle their inputs and wait for a result.
    /// </summary>
    /// <remarks>Note that regular buttons will be disabled by default as this option is intended for custom (detached) buttons.</remarks>
    public bool ProcessSinglePagePaginators { get; set; }
}