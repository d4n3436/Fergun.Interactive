﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Fergun.Interactive;
using Fergun.Interactive.Selection;

namespace ExampleBot.Modules;

public partial class CustomModule
{
    private readonly InteractionService _commandService;
    private readonly InteractiveService _interactive;

    public CustomModule(InteractionService commandService, InteractiveService interactive)
    {
        _commandService = commandService;
        _interactive = interactive;
    }

    [SlashCommand("select", "Sends a multi selection (a message with multiple select menus with options).")]
    public async Task MultiSelectionAsync()
    {
        // Create CancellationTokenSource that will be canceled after 10 minutes.
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));

        var modules = _commandService.Modules.ToArray();
        var color = Utils.GetRandomColor();

        // Used to track the selected module
        string? selectedModule = null;

        var interaction = Context.Interaction;
        InteractiveMessageResult<MultiSelectionOption<string>>? result = null;

        // This timeout page is used for both cancellation (from the cancellation token) and timeout (specified in the SendSelectionAsync method).
        var timeoutPage = new PageBuilder()
            .WithDescription("Timeout!")
            .WithColor(color);

        // Here we will use a "menu" message, that is, it can be reused multiple times until a timeout is received.
        // In this case, we will reuse the message with the multi selection until we get a result (multi selection option) with a row value of 1.
        // This can only happen if the result is successful and an option in the second select menu (the one that contains the commands) in selected.
        while (result is null || (result.IsSuccess && result.Value?.Row != 1))
        {
            // A multi selection uses the Row property of MultiSelectionOption to determine the position (the select menu) the options will appear.
            // The modules will appear in the first select menu.
            // There's also an IsDefault property used to determine whether an option is the default for a specific row.
            string? module = selectedModule;
            var options = modules.Select(x => new MultiSelectionOption<string>(option: x.Name, row: 0, isDefault: x.Name == module));

            string description = "Select a module";

            // result will be null until a module is selected once.
            // we know result won't be a command here because of the condition in the while loop.
            if (result != null)
            {
                description = "Select a command\nNote: You can also update your selected module.";
                var commands = modules
                    .First(x => x.Name == result.Value!.Option)
                    .SlashCommands
                    .Select(x => new MultiSelectionOption<string>(x.Name, 1));

                options = options.Concat(commands);
            }

            var pageBuilder = new PageBuilder()
                .WithDescription(description)
                .WithColor(color);

            var multiSelection = new MultiSelectionBuilder<string>()
                .WithSelectionPage(pageBuilder)
                .WithTimeoutPage(timeoutPage)
                .WithCanceledPage(timeoutPage)
                .WithActionOnTimeout(ActionOnStop.ModifyMessage | ActionOnStop.DeleteInput)
                .WithActionOnCancellation(ActionOnStop.ModifyMessage | ActionOnStop.DeleteInput)
                .WithOptions(options.ToArray())
                .WithStringConverter(x => x.Option)
                .AddUser(Context.User)
                .Build();

            var responseType = interaction is IComponentInteraction ? InteractionResponseType.DeferredUpdateMessage : InteractionResponseType.ChannelMessageWithSource;
            result = await _interactive.SendSelectionAsync(multiSelection, interaction, TimeSpan.FromMinutes(2), responseType, false, null, cts.Token);

            interaction = result.StopInteraction!;

            if (result.IsSuccess && result.Value.Row == 0)
            {
                // We need to track the selected module so we can set it as the default option.
                selectedModule = result.Value.Option;
            }
        }

        if (!result.IsSuccess)
            return;

        var embed = new EmbedBuilder()
            .WithDescription($"You selected:\n**Module**: {selectedModule}\n**Command**: {result.Value.Option}")
            .WithColor(color)
            .Build();

        await interaction.ModifyOriginalResponseAsync(x =>
        {
            x.Embed = embed;
            x.Components = new ComponentBuilder().Build(); // Remove components
        });
    }
}

public class MultiSelectionBuilder<T> : BaseSelectionBuilder<MultiSelection<T>, MultiSelectionOption<T>, MultiSelectionBuilder<T>>
    where T : notnull
{
    public override InputType InputType => InputType.SelectMenus;

    public override MultiSelection<T> Build() => new(this);
}

public class MultiSelection<T>(MultiSelectionBuilder<T> builder) : BaseSelection<MultiSelectionOption<T>>(builder)
    where T : notnull
{
    public override ComponentBuilder GetOrAddComponents(bool disableAll, ComponentBuilder? builder = null)
    {
        builder ??= new ComponentBuilder();
        var selectMenus = new Dictionary<int, SelectMenuBuilder>();

        foreach (var option in Options)
        {
            if (!selectMenus.TryGetValue(option.Row, out var value))
            {
                value = new SelectMenuBuilder()
                    .WithCustomId($"selectmenu{option.Row}")
                    .WithDisabled(disableAll);
                selectMenus[option.Row] = value;
            }

            var emote = EmoteConverter?.Invoke(option);
            string? label = StringConverter?.Invoke(option);
            if (emote is null && label is null)
            {
                throw new InvalidOperationException($"Neither {nameof(EmoteConverter)} nor {nameof(StringConverter)} returned a valid emote or string.");
            }

            var optionBuilder = new SelectMenuOptionBuilder()
                .WithLabel(label)
                .WithEmote(emote)
                .WithValue(emote?.ToString() ?? label)
                .WithDefault(option.IsDefault);

            value.AddOption(optionBuilder);
        }

        foreach ((int row, var selectMenu) in selectMenus)
        {
            builder.WithSelectMenu(selectMenu, row);
        }

        return builder;
    }
}

public class MultiSelectionOption<T>(T option, int row, bool isDefault = false)
    where T : notnull
{
    public T Option { get; } = option;

    public int Row { get; } = row;

    public bool IsDefault { get; set; } = isDefault;

    public override string? ToString() => Option.ToString();

    public override int GetHashCode() => Option.GetHashCode();

    public override bool Equals(object? obj) => Equals(Option, obj);
}