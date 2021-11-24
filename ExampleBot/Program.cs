using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ExampleBot.Services;
using Fergun.Interactive;
using Microsoft.Extensions.DependencyInjection;

namespace ExampleBot
{
    internal class Program
    {
        private static Task Main() => new Program().StartAsync();

        private async Task StartAsync()
        {
            var services = ConfigureServices();
            var client = services.GetRequiredService<DiscordSocketClient>();

            services.GetRequiredService<CommandService>().Log += LogAsync;
            client.Log += LogAsync;

            // Put your token here
            string token = "Token";
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

            await Task.Delay(Timeout.Infinite);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }

        private IServiceProvider ConfigureServices()
            => new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<InteractiveService>()
                .AddSingleton<Random>()
                .BuildServiceProvider();
    }
}