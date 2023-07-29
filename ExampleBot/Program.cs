using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ExampleBot.Services;
using Fergun.Interactive;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExampleBot;

internal static class Program
{
    private static async Task Main()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        await using var services = ConfigureServices();
        var client = services.GetRequiredService<DiscordSocketClient>();

        services.GetRequiredService<CommandService>().Log += LogAsync;
        services.GetRequiredService<InteractiveService>().Log += LogAsync;
        client.Log += LogAsync;

        string token = config["Token"];

        try
        {
            TokenUtils.ValidateToken(TokenType.Bot, token);
        }
        catch (ArgumentException ex)
        {
            await LogAsync(new LogMessage(LogSeverity.Critical, "Bot", "Token is invalid, cannot continue. Make sure to put your bot token in appsettings.json", ex));
            throw;
        }

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

    private static ServiceProvider ConfigureServices()
        => new ServiceCollection()
            .AddSingleton(new DiscordSocketConfig { LogLevel = LogSeverity.Verbose, GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent })
            .AddSingleton(new CommandServiceConfig { LogLevel = LogSeverity.Verbose })
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandlingService>()
            .AddSingleton<InteractiveService>()
            .BuildServiceProvider();
}