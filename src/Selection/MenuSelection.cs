using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using JetBrains.Annotations;

namespace Fergun.Interactive.Selection;

/// <summary>
/// Represents a menu selection. A menu selection is a selection where its options can be selected multiple times and uses <see cref="InputHandler"/> to dynamically change the page it's currently displaying.
/// </summary>
/// <typeparam name="TOption">The type of the options.</typeparam>
[PublicAPI]
public sealed class MenuSelection<TOption> : BaseSelection<TOption>
{
    private IReadOnlyList<TOption> _lastSelectedOptions = [];

    internal MenuSelection(MenuSelectionBuilder<TOption> builder)
        : base(builder)
    {
        InteractiveGuards.NotNull(builder.InputHandler);
        SetDefaultValues = builder.SetDefaultValues;
        InputHandler = builder.InputHandler;
    }

    /// <summary>
    /// Gets a value indicating whether to set the default values on select menus. The values are the last selected options.
    /// </summary>
    public bool SetDefaultValues { get; }

    /// <summary>
    /// Gets the delegate that will be executed when a valid input is received (except cancel options).
    /// </summary>
    public Func<IReadOnlyList<TOption>, ValueTask<IPage?>> InputHandler { get; }

    /// <inheritdoc/>
    public override ComponentBuilder GetOrAddComponents(bool disableAll, ComponentBuilder? builder = null)
    {
        if (!(InputType.HasFlag(InputType.Buttons) || InputType.HasFlag(InputType.SelectMenus)))
        {
            throw new InvalidOperationException($"{nameof(InputType)} must have either {nameof(InputType.Buttons)} or {nameof(InputType.SelectMenus)}.");
        }

        builder ??= new ComponentBuilder();
        if (InputType.HasFlag(InputType.SelectMenus))
        {
            var options = new List<SelectMenuOptionBuilder>();

            foreach (var selection in Options)
            {
                var emote = EmoteConverter?.Invoke(selection);
                string? label = StringConverter?.Invoke(selection);
                if (emote is null && label is null)
                {
                    throw new InvalidOperationException($"Neither {nameof(EmoteConverter)} nor {nameof(StringConverter)} returned a valid emote or string.");
                }

                var option = new SelectMenuOptionBuilder()
                    .WithLabel(label)
                    .WithEmote(emote)
                    .WithDefault(SetDefaultValues && _lastSelectedOptions.Contains(selection, EqualityComparer))
                    .WithValue(emote?.ToString() ?? label);

                options.Add(option);
            }

            var selectMenu = new SelectMenuBuilder()
                .WithCustomId("foobar")
                .WithOptions(options)
                .WithDisabled(disableAll)
                .WithMinValues(MinValues)
                .WithMaxValues(MaxValues);

            if (!string.IsNullOrEmpty(Placeholder))
                selectMenu.WithPlaceholder(Placeholder);

            builder.WithSelectMenu(selectMenu);
        }

        if (!InputType.HasFlag(InputType.Buttons))
            return builder;

        foreach (var selection in Options)
        {
            var emote = EmoteConverter?.Invoke(selection);
            string? label = StringConverter?.Invoke(selection);
            if (emote is null && label is null)
            {
                throw new InvalidOperationException($"Neither {nameof(EmoteConverter)} nor {nameof(StringConverter)} returned a valid emote or string.");
            }

            var button = new ButtonBuilder()
                .WithCustomId(emote?.ToString() ?? label)
                .WithStyle(ButtonStyle.Primary)
                .WithEmote(emote)
                .WithDisabled(disableAll);

            if (label is not null)
                button.Label = label;

            builder.WithButton(button);
        }

        return builder;
    }

    /// <inheritdoc/>
    public override async Task<InteractiveInputResult<TOption>> HandleMessageAsync(IMessage input, IUserMessage message)
    {
        var result = await base.HandleMessageAsync(input, message).ConfigureAwait(false);
        if (result.Status is not InteractiveInputStatus.Success)
            return result;

        var page = await InputHandler(result.SelectedOptions).ConfigureAwait(false);

        if (page is not null)
        {
            await message.ModifyAsync(x =>
            {
                x.Content = page.Text ?? new Optional<string>();
                x.Embeds = page.GetEmbedArray();
            }).ConfigureAwait(false);
        }

        return new InteractiveInputResult<TOption>(InteractiveInputStatus.Ignored, result.SelectedOptions);
    }

    /// <inheritdoc/>
    public override async Task<InteractiveInputResult<TOption>> HandleReactionAsync(SocketReaction input, IUserMessage message)
    {
        var result = await base.HandleReactionAsync(input, message).ConfigureAwait(false);
        if (result.Status is not InteractiveInputStatus.Success)
            return result;

        var page = await InputHandler(result.SelectedOptions).ConfigureAwait(false);

        if (page is not null)
        {
            await message.ModifyAsync(x =>
            {
                x.Content = page.Text ?? new Optional<string>();
                x.Embeds = page.GetEmbedArray();
            }).ConfigureAwait(false);
        }

        return new InteractiveInputResult<TOption>(InteractiveInputStatus.Ignored, result.SelectedOptions);
    }

    /// <inheritdoc/>
    public override async Task<InteractiveInputResult<TOption>> HandleInteractionAsync(SocketMessageComponent input, IUserMessage message)
    {
        var result = await base.HandleInteractionAsync(input, message).ConfigureAwait(false);
        if (result.Status is not InteractiveInputStatus.Success)
            return result;

        var page = await InputHandler(result.SelectedOptions).ConfigureAwait(false);

        if (SetDefaultValues)
            _lastSelectedOptions = result.SelectedOptions;

        if (page is not null)
        {
            await input.UpdateAsync(x =>
            {
                x.Content = page.Text ?? new Optional<string>();
                x.Embeds = page.GetEmbedArray();
                x.Components = GetOrAddComponents(disableAll: false).Build();
            }).ConfigureAwait(false);
        }
        else
        {
            await input.DeferAsync().ConfigureAwait(false);
        }

        return new InteractiveInputResult<TOption>(InteractiveInputStatus.Ignored, result.SelectedOptions);
    }
}