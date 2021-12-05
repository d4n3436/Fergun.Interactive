using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ExampleBot.Services;

// Minimal command handler
public class CommandHandlingService
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;

    public CommandHandlingService(IServiceProvider services)
    {
        _client = services.GetRequiredService<DiscordSocketClient>();
        _commands = services.GetRequiredService<CommandService>();
        _services = services;

        _client.MessageReceived += HandleMessageAsync;
    }

    public async Task InitializeAsync() => await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

    private async Task HandleMessageAsync(SocketMessage socketMessage)
    {
        if (socketMessage is not SocketUserMessage { Source: MessageSource.User } message)
            return;

        int argPos = 0;
        if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
            return;

        var context = new SocketCommandContext(_client, message);
        await _commands.ExecuteAsync(context, argPos, _services);
    }
}