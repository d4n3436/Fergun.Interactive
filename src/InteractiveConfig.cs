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
}