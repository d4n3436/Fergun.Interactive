using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Fergun.Interactive;

namespace ExampleBot.Modules
{
    [Group("next")]
    public class WaitModule : ModuleBase
    {
        public InteractiveService Interactive { get; set; }

        [Command("message", RunMode = RunMode.Async)]
        public async Task NextMessageAsync()
        {
            var msg = await ReplyAsync("Waiting for a message...");

            // Wait for a message in the same channel the command was executed.
            var result = await Interactive.NextMessageAsync(x => x.Channel.Id == Context.Channel.Id, timeout: TimeSpan.FromSeconds(30));

            await msg.ModifyAsync(x => x.Content = result.IsTimeout ? "Timeout!" : $"{result.Value.Author} said: {result.Value.Content}");
        }

        [Command("reaction", RunMode = RunMode.Async)]
        public async Task NextReactionAsync()
        {
            var msg = await ReplyAsync("Add a reaction to this message.");

            // Wait for a reaction in the message.
            var result = await Interactive.NextReactionAsync(x => x.MessageId == msg.Id, timeout: TimeSpan.FromSeconds(30));

            await msg.ModifyAsync(x =>
            {
                x.Content = result.IsTimeout ? "Timeout!" : $"{MentionUtils.MentionUser(result.Value.UserId)} reacted: {result.Value.Emote}";
                x.AllowedMentions = AllowedMentions.None;
                x.Embeds = Array.Empty<Embed>(); // workaround for d.net bug
            });
        }

        [Command("interaction", RunMode = RunMode.Async)]
        public async Task NextInteractionAsync()
        {
            var builder = new ComponentBuilder()
                .WithButton("Hey", "id");

            var msg = await ReplyAsync("Press this button!", component: builder.Build());

            // Wait for a user to press the button
            var result = await Interactive.NextMessageComponentAsync(x => x.Message.Id == msg.Id,
                timeout: TimeSpan.FromSeconds(30));

            if (result.IsSuccess)
            {
                // Acknowledge the interaction
                await result.Value.DeferAsync();
            }

            await msg.ModifyAsync(x =>
            {
                x.Content = result.IsTimeout ? "Timeout!" : $"{MentionUtils.MentionUser(result.Value.User?.Id ?? 0)} pressed the button!";
                x.Components = new ComponentBuilder().Build(); // No components
                x.AllowedMentions = AllowedMentions.None;
                x.Embeds = Array.Empty<Embed>(); // workaround for d.net bug
            });
        }
    }
}