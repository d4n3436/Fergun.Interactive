using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fergun.Interactive.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> to add <see cref="InteractiveService"/>.
/// </summary>
[PublicAPI]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the <see cref="InteractiveService"/> to the specified service collection using the default configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddInteractiveService(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddInteractiveService(_ => { });
    }

    /// <summary>
    /// Adds the <see cref="InteractiveService"/> to the specified service collection using the default configuration.
    /// </summary>
    /// <param name="configureOptions">A delegate to configure the <see cref="InteractiveService"/>.</param>
    /// <param name="services">The service collection.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddInteractiveService(this IServiceCollection services, Action<InteractiveServiceOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<InteractiveService>();
        services.Configure(configureOptions);

        return services;
    }
}