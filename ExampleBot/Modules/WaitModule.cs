using System;
using System.Threading.Tasks;
using ExampleBot.Extensions;
using Fergun.Interactive;
using JetBrains.Annotations;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace ExampleBot.Modules;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class WaitModule : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly InteractiveService _interactive;

    public WaitModule(InteractiveService interactive)
    {
        _interactive = interactive;
    }

    [SlashCommand("next-message", "Waits for an incoming message.")]
    public async Task NextMessageAsync()
    {
        await RespondAsync(InteractionCallback.Message("Waiting for a message..."));
        var message = await GetResponseAsync();

        // Wait for a message in the same channel the command was executed.
        var result = await _interactive.NextMessageAsync(x => x.Channel?.Id == Context.Channel.Id, timeout: TimeSpan.FromSeconds(30));

        string content = result.IsSuccess
            ? $"{result.Value!.Author} said: {result.Value.Content}"
            : $"Failed to get message. Status: {result.Status}";

        await message.ModifyAsync(x => x.Content = content);
    }

    [SlashCommand("next-reaction", "Waits for a reaction on a message.")]
    public async Task NextReactionAsync()
    {
        await RespondAsync(InteractionCallback.Message("Add a reaction to this message."));
        var message = await GetResponseAsync();

        // Wait for a reaction in the message.
        var result = await _interactive.NextReactionAsync(x => x.MessageId == message.Id, timeout: TimeSpan.FromSeconds(30));

        string content = result.IsSuccess
            ? $"<@{result.Value!.UserId}> reacted: {result.Value.Emoji.GetValue()}"
            : $"Failed to get reaction. Status: {result.Status}";

        await message.ModifyAsync(x => x.Content = content);
    }

    [SlashCommand("next-interaction", "Waits for an incoming interaction.")]
    public async Task NextInteractionAsync()
    {
        var builder = new ActionRowProperties()
            .AddComponents([new ButtonProperties("id", "Hey", ButtonStyle.Primary)]);
        
        var props = new InteractionMessageProperties
        {
            Content = "Press this button!",
            Components = [builder]
        };

        await RespondAsync(InteractionCallback.Message(props));
        var message = await GetResponseAsync();

        // Wait for a user to press the button
        var result = await _interactive.NextComponentInteractionAsync(x => x.Message.Id == message.Id, timeout: TimeSpan.FromSeconds(30));

        if (result.IsSuccess)
        {
            await result.Value.SendResponseAsync(InteractionCallback.ModifyMessage(x =>
            {
                x.Content = $"<@{result.Value!.User.Id}> pressed the button!";
                x.Components = []; // No components
            }));
        }
        else
        {
            await ModifyResponseAsync(x =>
            {
                x.Content = $"Failed to get interaction. Status: {result.Status}";
                x.Components = []; // No components
            });
        }
    }
}