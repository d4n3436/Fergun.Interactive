using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ExampleBot.Extensions;
using Fergun.Interactive;
using Fergun.Interactive.Selection;

namespace ExampleBot.Modules;

[Group("custom")]
public partial class CustomModule : ModuleBase
{
    public InteractiveService Interactive { get; set; }

    // Sends a selection of buttons, where each option has its own button style/color.
    [Command("button", RunMode = RunMode.Async)]
    public async Task CustomButtonSelectionAsync()
    {
        // To be able to create buttons with custom colors, we need to create a custom selection and a builder for that new selection.
        // See ButtonSelectionBuilder<T> and ButtonSelection<T> (below) for more information.

        // A ButtonSelection uses ButtonOption<T>s, specifically created for this custom selection.
        var options = new ButtonOption<string>[]
        {
            new("Primary", ButtonStyle.Primary),
            new("Secondary", ButtonStyle.Secondary),
            new("Success", ButtonStyle.Success),
            new("Danger", ButtonStyle.Danger)
        };

        var pageBuilder = new PageBuilder()
            .WithDescription("Button selection")
            .WithRandomColor();

        var buttonSelection = new ButtonSelectionBuilder<string>()
            .WithOptions(options)
            .WithStringConverter(x => x.Option)
            .WithSelectionPage(pageBuilder)
            .AddUser(Context.User)
            .Build();

        await Interactive.SendSelectionAsync(buttonSelection, Context.Channel);
    }

    // Custom selection builder for ButtonSelections
    public class ButtonSelectionBuilder<T> : BaseSelectionBuilder<ButtonSelection<T>, ButtonOption<T>, ButtonSelectionBuilder<T>>
    {
        // Since this selection specifically created for buttons, it makes sense to make this option the default.
        public override InputType InputType => InputType.Buttons;

        // We must override the Build method
        public override ButtonSelection<T> Build() => new(this);
    }

    // Custom selection where you can override the default button style/color
    public class ButtonSelection<T> : BaseSelection<ButtonOption<T>>
    {
        public ButtonSelection(ButtonSelectionBuilder<T> builder)
            : base(builder)
        {
        }

        // This method needs to be overriden to build our own component the way we want.
        public override ComponentBuilder GetOrAddComponents(bool disableAll, ComponentBuilder builder = null)
        {
            builder ??= new ComponentBuilder();
            foreach (var option in Options)
            {
                var emote = EmoteConverter?.Invoke(option);
                string label = StringConverter?.Invoke(option);
                if (emote is null && label is null)
                {
                    throw new InvalidOperationException($"Neither {nameof(EmoteConverter)} nor {nameof(StringConverter)} returned a valid emote or string.");
                }

                var button = new ButtonBuilder()
                    .WithCustomId(emote?.ToString() ?? label)
                    .WithStyle(option.Style) // Use the style of the option
                    .WithEmote(emote)
                    .WithDisabled(disableAll);

                if (label is not null)
                    button.Label = label;

                builder.WithButton(button);
            }

            return builder;
        }
    }

    public record ButtonOption<T>(T Option, ButtonStyle Style); // An option with an style
}