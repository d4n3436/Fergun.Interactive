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
        public static bool CanUseInteractions;

        public static readonly DiscordSocketConfig ClientConfig = new();

        private static Task Main() => new Program().StartAsync();

        private async Task StartAsync()
        {
#if DNETLABS
            CanUseInteractions = true;
            // Currently we need to disable this option otherwise the selection/paginator won't respond
            ClientConfig.AlwaysAcknowledgeInteractions = false;
#endif

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
                .AddSingleton(ClientConfig)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<InteractiveService>()
                .AddSingleton<Random>()
                .BuildServiceProvider();
    }
}