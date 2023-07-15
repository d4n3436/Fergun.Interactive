using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Fergun.Interactive.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IHostBuilder"/>.
/// </summary>
public static class HostBuilderExtensions
{
    /// <summary>
    /// Adds <see cref="InteractiveService"/> and optionally configures a <see cref="InteractiveConfig"/> along with the required services.
    /// </summary>
    /// <param name="builder">The host builder to configure.</param>
    /// <param name="config">The delegate for the <see cref="InteractiveConfig" /> that will be used to configure the host.</param>
    /// <returns>The generic host builder.</returns>
    public static IHostBuilder UseInteractiveService(this IHostBuilder builder, Action<HostBuilderContext, InteractiveConfig>? config = null)
    {
        return builder.ConfigureServices((context, collection) =>
        {
            if (config != null)
                collection.AddSingleton(config);

            collection.AddSingleton<InteractiveService>();
        });
    }
}
