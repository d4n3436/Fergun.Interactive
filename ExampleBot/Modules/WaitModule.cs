using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Fergun.Interactive;

namespace ExampleBot.Modules;

[Group("next")]
public class WaitModule : ModuleBase
{
    private readonly InteractiveService _interactive;

    public WaitModule(InteractiveService interactive)
    {
        _interactive = interactive;
    }

    [Command("message", RunMode = RunMode.Async)]
    public async Task NextMessageAsync()
    {
        var msg = await ReplyAsync("Waiting for a message...");

        // Wait for a message in the same channel the command was executed.
        var result = await _interactive.NextMessageAsync(x => x.Channel.Id == Context.Channel.Id, timeout: TimeSpan.FromSeconds(30));

        await msg.ModifyAsync(x => x.Content = result.IsSuccess ? $"{result.Value!.Author} said: {result.Value.Content}" : $"Failed to get message. Status: {result.Status}");
    }

    [Command("reaction", RunMode = RunMode.Async)]
    public async Task NextReactionAsync()
    {
        var msg = await ReplyAsync("Add a reaction to this message.");

        // Wait for a reaction in the message.
        var result = await _interactive.NextReactionAsync(x => x.MessageId == msg.Id, timeout: TimeSpan.FromSeconds(30));

        await msg.ModifyAsync(x =>
        {
            x.Content = result.IsSuccess ? $"{MentionUtils.MentionUser(result.Value!.UserId)} reacted: {result.Value.Emote}" : $"Failed to get reaction. Status: {result.Status}";
            x.AllowedMentions = AllowedMentions.None;
            x.Embeds = Array.Empty<Embed>(); // workaround for d.net bug
        });
    }

    [Command("interaction", RunMode = RunMode.Async)]
    public async Task NextInteractionAsync()
    {
        var builder = new ComponentBuilder()
            .WithButton("Hey", "id");

        var msg = await ReplyAsync("Press this button!", components: builder.Build());

        // Wait for a user to press the button
        var result = await _interactive.NextMessageComponentAsync(x => x.Message.Id == msg.Id, timeout: TimeSpan.FromSeconds(30));

        if (result.IsSuccess)
        {
            // Acknowledge the interaction
            await result.Value!.DeferAsync();
        }

        await msg.ModifyAsync(x =>
        {
            x.Content = result.IsSuccess ? $"{MentionUtils.MentionUser(result.Value!.User.Id)} pressed the button!" : $"Failed to get interaction. Status: {result.Status}";
            x.Components = new ComponentBuilder().Build(); // No components
            x.AllowedMentions = AllowedMentions.None;
            x.Embeds = Array.Empty<Embed>(); // workaround for d.net bug
        });
    }
}