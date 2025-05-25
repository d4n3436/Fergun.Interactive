using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using ExampleBot.Extensions;
using Fergun.Interactive;
using Fergun.Interactive.Extensions;
using Fergun.Interactive.Selection;

namespace ExampleBot.Modules;

[Group("selection", "Selection commands.")]
public class SelectionModule : InteractionModuleBase
{
    private readonly InteractiveService _interactive;

    public SelectionModule(InteractiveService interactive)
    {
        _interactive = interactive;
    }

    [SlashCommand("simple", "Sends a message that contains selection of options which the user can select one.")]
    public async Task SelectAsync()
    {
        string[] options = ["C", "C++", "C#", "Java", "Python", "JavaScript", "PHP"];

        var pageBuilder = new PageBuilder() // A PageBuilder is just an EmbedBuilder with a Text property (content).
            .WithTitle("Selection Example")
            .WithDescription($"Select a programming language:\n{string.Join('\n', options.Select(x => $"- **{x}**"))}")
            .WithRandomColor(); // Random embed color.

        var selection = new SelectionBuilder<string>() // Create a new SelectionBuilder that uses strings as input.
            .AddUser(Context.User) // Only allow the user that executed the command to interact with the selection.
            .WithOptions(options) // Set the options.
            .WithInputType(InputType.Messages) // Use messages to receive the selection.
            .WithSelectionPage(pageBuilder) // Set the selection page, this is the page that will be displayed in the message.
            .WithDeletion(DeletionOptions.None) // By default, the selections delete valid inputs (messages, reactions) but you can change this behaviour.
            .Build(); // Build the SelectionBuilder.

        // Respond to the interaction with the selection and wait until the user selects an option, or until it times out
        var result = await _interactive.SendSelectionAsync(selection, Context.Interaction, TimeSpan.FromMinutes(1));

        // Get the selected option. This may be null/default if the selection times out or gets cancelled.
        string selected = result.Value!;

        // You can check whether a selection failed or ended successfully with the Status property.
        // You can also use the IsSuccess, IsCanceled and IsTimeout properties.
        bool isSuccess = result.IsSuccess;

        var builder = new EmbedBuilder()
            .WithDescription(isSuccess ? $"You selected: {selected}" : "Timeout!")
            .WithRandomColor();

        await ReplyAsync(embed: builder.Build());
    }

    // This variant of SelectionBuilder just exists for convenience,
    // but makes creating selections using reactions or buttons much easier.
    [SlashCommand("emote", "Sends an emote selection. An emote selection is built using an EmoteSelectionBuilder.")]
    public async Task EmoteAsync()
    {
        var emotes = new[]
        {
            new Emoji("📱"),
            new Emoji("🖥"),
            new Emoji("💻")
        };

        var pageBuilder = new PageBuilder()
            .WithDescription("Select the device you're using")
            .WithRandomColor();

        var selection = new EmoteSelectionBuilder()
            .AddUser(Context.User)
            .WithDefaultRestrictedPage() // Set a page that a user will see if they are not allowed to use the paginator. This is an extension method that provides a default page for convenience.
            .WithOptions(emotes)
            .WithSelectionPage(pageBuilder)
            // Normally you would need to specify an EmoteConverter, but EmoteSelectionBuilder has already implemented a default converter for emotes.
            //.WithEmoteConverter(x => x)
            // There's no need to specify an input type since the builder now uses the most appropriate one (reactions/buttons) by default
            //.WithInputType(InputType.Reactions)
            .Build();

        var result = await _interactive.SendSelectionAsync(selection, Context.Interaction, TimeSpan.FromMinutes(1));

        var builder = new EmbedBuilder()
            .WithDescription(result.IsSuccess ? $"You selected: {result.Value}" : "Timeout!")
            .WithRandomColor();

        await ReplyAsync(embed: builder.Build());
    }

    [SlashCommand("emote2", "Sends an emote selection, where each emote represents a specific value.")]
    public async Task Emote2Async()
    {
        var emotes = new Dictionary<IEmote, string>
        {
            [new Emoji("\u0031\u20E3")] = "one",
            [new Emoji("\u0032\u20E3")] = "two",
            [new Emoji("\u0033\u20E3")] = "three",
            [new Emoji("\u0034\u20E3")] = "four",
            [new Emoji("\u0035\u20E3")] = "five"
        };

        var pageBuilder = new PageBuilder()
            .WithDescription("Select a number")
            .WithRandomColor();

        var selection = new EmoteSelectionBuilder<string>()
            .AddUser(Context.User)
            .WithOptions(emotes)
            .WithSelectionPage(pageBuilder)
            .Build();

        var result = await _interactive.SendSelectionAsync(selection, Context.Interaction, TimeSpan.FromMinutes(1));

        // In this case the result's value will be a KeyValuePair<IEmote, int>
        var emote = result.Value.Key; // Selected emote
        string selected = result.Value.Value; // Selected option

        var builder = new EmbedBuilder()
            .WithDescription(result.IsSuccess ? $"You selected: {emote} ({selected})" : "Timeout!")
            .WithRandomColor();

        await ReplyAsync(embed: builder.Build());
    }

    [SlashCommand("multi", "Sends a selection that uses a select menu and allows selecting multiple options.")]
    public async Task MultiAsync()
    {
        var colors = new Item[]
        {
            new("Red", new Emoji("\ud83d\udd34")),
            new("Green", new Emoji("\ud83d\udfe2")),
            new("Blue", new Emoji("\ud83d\udd35")),
            new("Yellow", new Emoji("\ud83d\udfe1")),
            new("Purple", new Emoji("\ud83d\udfe3"))
        };

        var color = Utils.GetRandomColor();

        var pageBuilder = new PageBuilder()
            .WithDescription("Select a color\nYou can select multiple options.")
            .WithColor(color);

        var selection = new SelectionBuilder<Item>()
            .AddUser(Context.User)
            .WithOptions(colors)
            .WithSelectionPage(pageBuilder)
            .WithStringConverter(x => x.Name)
            .WithEmoteConverter(x => x.Emote)
            .WithActionOnSuccess(ActionOnStop.DeleteInput) // Delete select menu when the user selects any option
            .WithInputType(InputType.SelectMenus) // Set the input type to select menus. This is required to receive multiple options.
            .WithMaxValues(3) // Change the max. values so the user can select up to 3 options
            .WithPlaceholder("Select a color") // Set the placeholder text for the select menu
            .Build();

        var result = await _interactive.SendSelectionAsync(selection, Context.Interaction, TimeSpan.FromMinutes(1));

        if (result.IsSuccess)
        {
            var selected = result.Values;

            var embed = new EmbedBuilder()
                .WithDescription($"You selected the color(s): {string.Join(", ", selected.Select(x => $"{x.Emote} {x.Name}"))}")
                .WithColor(color)
                .Build();

            await result.StopInteraction!.ModifyOriginalResponseAsync(x => x.Embed = embed);
        }
    }

    // Sends a selection that can be canceled, disables/deletes its input (reactions/buttons) after selecting an option,
    // modifies itself to a specific page after ending, and deletes itself after cancelling it.
    [SlashCommand("extra", "Sends a selection that displays the customization options.")]
    public async Task ExtraAsync()
    {
        var items = new Item[]
        {
            new("Fruit", new Emoji("🍎")),
            new("Vegetable", new Emoji("🥦")),
            new("Fast food", new Emoji("🍔")),
            new("Dessert", new Emoji("🍰")),
            new("Dairy", new Emoji("🧀")),
            // Important: When using an option for cancellation (with WithAllowCancel(), shown below), the last option will be used to cancel the selection.
            new("Cancel", new Emoji("❌"))
        };

        var color = Utils.GetRandomColor();

        var pageBuilder = new PageBuilder()
            .WithDescription("Select a food type")
            .WithColor(color);

        // This page will be displayed when a valid input is received (except cancellations).
        var successPage = new PageBuilder()
            .WithDescription("Thank you for participating.")
            .WithColor(color);

        // This page will be displayed when a selection times out.
        var timeoutPage = new PageBuilder()
            .WithDescription("Timeout!")
            .WithColor(color);

        var selection = new SelectionBuilder<Item>()
            .AddUser(Context.User)
            .WithOptions(items)
            .WithSelectionPage(pageBuilder)
            .WithSuccessPage(successPage)
            .WithTimeoutPage(timeoutPage)
            .WithActionOnSuccess(ActionOnStop.ModifyMessage | ActionOnStop.DeleteInput) // Modify the message to successPage and delete the input (reactions, buttons, select menu) if a valid input is received.
            .WithActionOnTimeout(ActionOnStop.ModifyMessage | ActionOnStop.DeleteInput) // Modify the message to timeoutPage and delete the input if the selection times out.
            .WithAllowCancel(true) // We need to tell the selection there's a cancel option.
            .WithActionOnCancellation(ActionOnStop.DeleteMessage) // Delete the message if the selection gets canceled.
            .WithStringConverter(item => item.Name) // The Name property is used to get a string representation of an item. This is required in selections using messages and select menus as input.
            .WithEmoteConverter(item => item.Emote) // The Emote property is used to get an emote representation of an item. This is required in selections using reactions as input.
            .WithInputType(InputType.Reactions | InputType.Messages | InputType.Buttons | InputType.SelectMenus) // Since we have set both string and emote converters, we can use all 4 input types.
            .Build();

        await _interactive.SendSelectionAsync(selection, Context.Interaction, TimeSpan.FromMinutes(1));
    }

    [SlashCommand("menu", "Sends a menu of options that reuses a selection message.")]
    public async Task MenuAsync()
    {
        string[] options =
        [
            "Cache messages",
            "Cache users",
            "Allow using mentions as prefix",
            "Ignore command errors"
        ];

        bool[] values =
        [
            true,
            false,
            true,
            false
        ];

        // Dynamically create the number emotes
        var emotes = Enumerable.Range(1, options.Length)
            .ToDictionary(x => new Emoji($"{x}\ufe0f\u20e3"), y => y);

        // Add the cancel emote at the end of the dictionary
        emotes.Add(new Emoji("❌"), -1);

        var selection = new MenuSelectionBuilder<KeyValuePair<Emoji, int>>()
            .AddUser(Context.User)
            .WithSelectionPage(GeneratePage())
            .WithInputHandler(HandleResult) // We use a method that handles the result and returns a page.
            .WithOptions(emotes)
            .WithAllowCancel(true)
            .WithEmoteConverter(pair => pair.Key)
            .WithActionOnCancellation(ActionOnStop.DisableInput) // Prefer disabling the input (buttons, select menus) instead of removing them from the message.
            .WithActionOnTimeout(ActionOnStop.DisableInput)
            .Build();

        await _interactive.SendSelectionAsync(selection, Context.Interaction, TimeSpan.FromMinutes(10));

        PageBuilder GeneratePage()
            => new PageBuilder()
                .WithTitle("Bot Control Panel")
                .WithDescription("Use the reactions/buttons to enable or disable an option.")
                .AddField("Option", string.Join('\n', options.Select((x, i) => $"**{i + 1}**. {x}")), inline: true)
                .AddField("Value", string.Join('\n', values), inline: true)
                .WithColor(Color.Blue);

        Page HandleResult(KeyValuePair<Emoji, int> input)
        {
            int selected = input.Value;
            // Invert the value of the selected option
            values[selected - 1] = !values[selected - 1];

            // Return a new page, this page will be applied to the message.
            return GeneratePage().Build();
        }
    }

    private sealed record Item(string Name, Emoji Emote);
}