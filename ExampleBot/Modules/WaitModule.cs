using System;
using System.Threading.Tasks;


using Fergun.Interactive;
using JetBrains.Annotations;

namespace ExampleBot.Modules;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Group("next", "Commands that demonstrate waiting for inputs (messages, reactions, interactions).")]
public class WaitModule : InteractionModuleBase
{
    private readonly InteractiveService _interactive;

    public WaitModule(InteractiveService interactive)
    {
        _interactive = interactive;
    }

    [SlashCommand("message", "Waits for an incoming message.")]
    public async Task NextMessageAsync()
    {
        await RespondAsync("Waiting for a message...");
        var message = await GetOriginalResponseAsync();

        // Wait for a message in the same channel the command was executed.
        var result = await _interactive.NextMessageAsync(x => x.Channel.Id == Context.Channel.Id, timeout: TimeSpan.FromSeconds(30));

        string content = result.IsSuccess
            ? $"{result.Value!.Author} said: {result.Value.Content}"
            : $"Failed to get message. Status: {result.Status}";

        await message.ModifyAsync(x => x.Content = content);
    }

    [SlashCommand("reaction", "Waits for a reaction on a message.")]
    public async Task NextReactionAsync()
    {
        await RespondAsync("Add a reaction to this message.");
        var message = await GetOriginalResponseAsync();

        // Wait for a reaction in the message.
        var result = await _interactive.NextReactionAsync(x => x.MessageId == message.Id, timeout: TimeSpan.FromSeconds(30));

        string content = result.IsSuccess
            ? $"{MentionUtils.MentionUser(result.Value!.UserId)} reacted: {result.Value.Emote}"
            : $"Failed to get reaction. Status: {result.Status}";

        await message.ModifyAsync(x => x.Content = content);
    }

    [SlashCommand("interaction", "Waits for an incoming interaction.")]
    public async Task NextInteractionAsync()
    {
        var builder = new ComponentBuilder()
            .WithButton("Hey", "id");
        
        await RespondAsync("Press this button!", components: builder.Build());
        var message = await GetOriginalResponseAsync();

        // Wait for a user to press the button
        var result = await _interactive.NextMessageComponentAsync(x => x.Message.Id == message.Id, timeout: TimeSpan.FromSeconds(30));

        if (result.IsSuccess)
        {
            await result.Value.UpdateAsync(x =>
            {
                x.Content = $"{MentionUtils.MentionUser(result.Value!.User.Id)} pressed the button!";
                x.Components = new ComponentBuilder().Build(); // No components
            });
        }
        else
        {
            await ModifyOriginalResponseAsync(x =>
            {
                x.Content = $"Failed to get interaction. Status: {result.Status}";
                x.Components = new ComponentBuilder().Build(); // No components
            });
        }
    }
}