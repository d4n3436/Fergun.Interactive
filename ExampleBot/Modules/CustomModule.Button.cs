using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExampleBot.Extensions;
using Fergun.Interactive;
using Fergun.Interactive.Selection;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace ExampleBot.Modules;

public partial class CustomModule : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly InteractiveService _interactive;

    public CustomModule(InteractiveService interactive)
    {
        _interactive = interactive;
    }

    [SlashCommand("custom-button", "Sends a selection of buttons, where each option has its own button style/color.")]
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

        await _interactive.SendSelectionAsync(buttonSelection, Context.Interaction);
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
    public class ButtonSelection<T>(ButtonSelectionBuilder<T> builder) : BaseSelection<ButtonOption<T>>(builder)
    {
        // This method needs to be overriden to build our own component the way we want.
        public override List<IMessageComponentProperties> GetOrAddComponents(bool disableAll, List<IMessageComponentProperties>? builder = null)
        {
            builder ??= [];

            var buttons = new List<ButtonProperties>();
            foreach (var option in Options)
            {
                var emote = EmoteConverter?.Invoke(option);
                string? label = StringConverter?.Invoke(option);
                if (emote is null && label is null)
                {
                    throw new InvalidOperationException($"Neither {nameof(EmoteConverter)} nor {nameof(StringConverter)} returned a valid emote or string.");
                }

                var button = new ButtonProperties(emote?.GetValue() ?? label!, label!, emote!, option.Style)
                    .WithDisabled(disableAll);

                buttons.Add(button);
            }

            builder.AddRange(buttons.Chunk(5).Select(x => new ActionRowProperties(x)));

            return builder;
        }
    }

    public record ButtonOption<T>(T Option, ButtonStyle Style); // An option with a style
}