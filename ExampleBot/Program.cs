using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ExampleBot.Services;
using Fergun.Interactive;
using Microsoft.Extensions.DependencyInjection;

namespace ExampleBot;

internal static class Program
{
    private static Task Main() => StartAsync();

    private static async Task StartAsync()
    {
        var services = ConfigureServices();
        var client = services.GetRequiredService<DiscordSocketClient>();

        services.GetRequiredService<CommandService>().Log += LogAsync;
        services.GetRequiredService<InteractiveService>().Log += LogAsync;
        client.Log += LogAsync;

        // Put your token here
        string token = "Token";
        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

        await Task.Delay(Timeout.Infinite);
    }

    private static Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());

        return Task.CompletedTask;
    }

    private static IServiceProvider ConfigureServices()
        => new ServiceCollection()
            .AddSingleton(new DiscordSocketConfig { LogLevel = LogSeverity.Verbose })
            .AddSingleton(new CommandServiceConfig { LogLevel = LogSeverity.Verbose })
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandlingService>()
            .AddSingleton<InteractiveService>()
            .BuildServiceProvider();
}