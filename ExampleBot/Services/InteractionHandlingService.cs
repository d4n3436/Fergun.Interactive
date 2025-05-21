using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;

namespace ExampleBot.Services;

// Minimal interaction handler
public class InteractionHandlingService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly InteractiveService _interactive;
    private readonly IServiceProvider _services;

    public InteractionHandlingService(IServiceProvider services, DiscordSocketClient client, InteractionService commands, InteractiveService interactive)
    {
        _services = services;
        _client = client;
        _commands = commands;
        _interactive = interactive;

        _client.Ready += ReadyAsync;
        _client.InteractionCreated += HandleInteractionAsync;
    }

    private async Task ReadyAsync()
    {
        _client.Ready -= ReadyAsync;

        await _commands.RegisterCommandsGloballyAsync();
    }

    public async Task InitializeAsync()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        if (_interactive.IsManaged(interaction))
            return;

        var context = new SocketInteractionContext(_client, interaction);
        await _commands.ExecuteCommandAsync(context, _services);
    }
}