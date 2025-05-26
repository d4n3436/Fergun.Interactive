using Discord;
using Fergun.Interactive.Pagination;
using System;

namespace Fergun.Interactive.Extensions;

/// <summary>
/// Provides extension methods for component builders to add <see cref="IComponentPaginator"/> buttons.
/// </summary>
public static class ComponentExtensions
{
    /// <summary>
    /// Adds the standard <see cref="PaginatorAction.Backward"/> button to the <see cref="ActionRowBuilder"/>, using the <paramref name="paginator"/> to determine the button's state.
    /// </summary>
    /// <param name="builder">The action row builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="label">The label of the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <param name="style">The style of the button. If one is not provided, the library will decide which one to use.</param>
    /// <param name="emote">The <see cref="IEmote"/> to be used with the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <returns>This <see cref="ActionRowBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ActionRowBuilder AddPreviousButton(this ActionRowBuilder builder, IComponentPaginator paginator, string? label = null, ButtonStyle? style = null, IEmote? emote = null)
        => builder.AddButton(paginator, PaginatorAction.Backward, label, style, emote);

    /// <summary>
    /// Adds the standard <see cref="PaginatorAction.Backward"/> button to the <see cref="ComponentBuilder"/>, using the <paramref name="paginator"/> to determine the button's state.
    /// </summary>
    /// <param name="builder">The component builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="label">The label of the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <param name="style">The style of the button. If one is not provided, the library will decide which one to use.</param>
    /// <param name="emote">The <see cref="IEmote"/> to be used with the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <param name="row">The row the button should be placed on.</param>
    /// <returns>This <see cref="ComponentBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ComponentBuilder AddPreviousButton(this ComponentBuilder builder, IComponentPaginator paginator, string? label = null, ButtonStyle? style = null, IEmote? emote = null, int row = 0)
        => builder.AddButton(paginator, PaginatorAction.Backward, label, style, emote, row);

    /// <summary>
    /// Adds the standard <see cref="PaginatorAction.Forward"/> button to the <see cref="ActionRowBuilder"/>, using the <paramref name="paginator"/> to determine the button's state.
    /// </summary>
    /// <param name="builder">The action row builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="label">The label of the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <param name="style">The style of the button. If one is not provided, the library will decide which one to use.</param>
    /// <param name="emote">The <see cref="IEmote"/> to be used with the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <returns>This <see cref="ActionRowBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ActionRowBuilder AddNextButton(this ActionRowBuilder builder, IComponentPaginator paginator, string? label = null, ButtonStyle? style = null, IEmote? emote = null)
        => builder.AddButton(paginator, PaginatorAction.Forward, label, style, emote);

    /// <summary>
    /// Adds the standard <see cref="PaginatorAction.Forward"/> button to the <see cref="ComponentBuilder"/>, using the <paramref name="paginator"/> to determine the button's state.
    /// </summary>
    /// <param name="builder">The component builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="label">The label of the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <param name="style">The style of the button. If one is not provided, the library will decide which one to use.</param>
    /// <param name="emote">The <see cref="IEmote"/> to be used with the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <param name="row">The row the button should be placed on.</param>
    /// <returns>This <see cref="ComponentBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ComponentBuilder AddNextButton(this ComponentBuilder builder, IComponentPaginator paginator, string? label = null, ButtonStyle? style = null, IEmote? emote = null, int row = 0)
        => builder.AddButton(paginator, PaginatorAction.Forward, label, style, emote, row);

    /// <summary>
    /// Adds the standard <see cref="PaginatorAction.SkipToStart"/> button to the <see cref="ActionRowBuilder"/>, using the <paramref name="paginator"/> to determine the button's state.
    /// </summary>
    /// <param name="builder">The action row builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="label">The label of the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <param name="style">The style of the button. If one is not provided, the library will decide which one to use.</param>
    /// <param name="emote">The <see cref="IEmote"/> to be used with the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <returns>This <see cref="ActionRowBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ActionRowBuilder AddFirstButton(this ActionRowBuilder builder, IComponentPaginator paginator, string? label = null, ButtonStyle? style = null, IEmote? emote = null)
        => builder.AddButton(paginator, PaginatorAction.SkipToStart, label, style, emote);

    /// <summary>
    /// Adds the standard <see cref="PaginatorAction.SkipToStart"/> button to the <see cref="ComponentBuilder"/>, using the <paramref name="paginator"/> to determine the button's state.
    /// </summary>
    /// <param name="builder">The component builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="label">The label of the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <param name="style">The style of the button. If one is not provided, the library will decide which one to use.</param>
    /// <param name="emote">The <see cref="IEmote"/> to be used with the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <param name="row">The row the button should be placed on.</param>
    /// <returns>This <see cref="ComponentBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ComponentBuilder AddFirstButton(this ComponentBuilder builder, IComponentPaginator paginator, string? label = null, ButtonStyle? style = null, IEmote? emote = null, int row = 0)
        => builder.AddButton(paginator, PaginatorAction.SkipToStart, label, style, emote, row);

    /// <summary>
    /// Adds the standard <see cref="PaginatorAction.SkipToEnd"/> button to the <see cref="ActionRowBuilder"/>, using the <paramref name="paginator"/> to determine the button's state.
    /// </summary>
    /// <param name="builder">The action row builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="label">The label of the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <param name="style">The style of the button. If one is not provided, the library will decide which one to use.</param>
    /// <param name="emote">The <see cref="IEmote"/> to be used with the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <returns>This <see cref="ActionRowBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ActionRowBuilder AddLastButton(this ActionRowBuilder builder, IComponentPaginator paginator, string? label = null, ButtonStyle? style = null, IEmote? emote = null)
        => builder.AddButton(paginator, PaginatorAction.SkipToEnd, label, style, emote);

    /// <summary>
    /// Adds the standard <see cref="PaginatorAction.SkipToEnd"/> button to the <see cref="ComponentBuilder"/>, using the <paramref name="paginator"/> to determine the button's state.
    /// </summary>
    /// <param name="builder">The component builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="label">The label of the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <param name="style">The style of the button. If one is not provided, the library will decide which one to use.</param>
    /// <param name="emote">The <see cref="IEmote"/> to be used with the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <param name="row">The row the button should be placed on.</param>
    /// <returns>This <see cref="ComponentBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ComponentBuilder AddLastButton(this ComponentBuilder builder, IComponentPaginator paginator, string? label = null, ButtonStyle? style = null, IEmote? emote = null, int row = 0)
        => builder.AddButton(paginator, PaginatorAction.SkipToEnd, label, style, emote, row);

    /// <summary>
    /// Adds the standard <see cref="PaginatorAction.Exit"/> button to the <see cref="ActionRowBuilder"/>, using the <paramref name="paginator"/> to determine the button's state.
    /// </summary>
    /// <param name="builder">The action row builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="label">The label of the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <param name="style">The style of the button. If one is not provided, the library will decide which one to use.</param>
    /// <param name="emote">The <see cref="IEmote"/> to be used with the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <returns>This <see cref="ActionRowBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ActionRowBuilder AddStopButton(this ActionRowBuilder builder, IComponentPaginator paginator, string? label = null, ButtonStyle? style = null, IEmote? emote = null)
        => builder.AddButton(paginator, PaginatorAction.Exit, label, style, emote);

    /// <summary>
    /// Adds the standard <see cref="PaginatorAction.Exit"/> button to the <see cref="ComponentBuilder"/>, using the <paramref name="paginator"/> to determine the button's state.
    /// </summary>
    /// <param name="builder">The component builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="label">The label of the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <param name="style">The style of the button. If one is not provided, the library will decide which one to use.</param>
    /// <param name="emote">The <see cref="IEmote"/> to be used with the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <param name="row">The row the button should be placed on.</param>
    /// <returns>This <see cref="ComponentBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ComponentBuilder AddStopButton(this ComponentBuilder builder, IComponentPaginator paginator, string? label = null, ButtonStyle? style = null, IEmote? emote = null, int row = 0)
        => builder.AddButton(paginator, PaginatorAction.Exit, label, style, emote, row);

    /// <summary>
    /// Adds the standard <see cref="PaginatorAction.Jump"/> button to the <see cref="ActionRowBuilder"/>, using the <paramref name="paginator"/> to determine the button's state.
    /// </summary>
    /// <param name="builder">The action row builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="label">The label of the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <param name="style">The style of the button. If one is not provided, the library will decide which one to use.</param>
    /// <param name="emote">The <see cref="IEmote"/> to be used with the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <returns>This <see cref="ActionRowBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ActionRowBuilder AddJumpButton(this ActionRowBuilder builder, IComponentPaginator paginator, string? label = null, ButtonStyle? style = null, IEmote? emote = null)
        => builder.AddButton(paginator, PaginatorAction.Jump, label, style, emote);

    /// <summary>
    /// Adds the standard <see cref="PaginatorAction.Jump"/> button to the <see cref="ComponentBuilder"/>, using the <paramref name="paginator"/> to determine the button's state.
    /// </summary>
    /// <param name="builder">The component builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="label">The label of the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <param name="style">The style of the button. If one is not provided, the library will decide which one to use.</param>
    /// <param name="emote">The <see cref="IEmote"/> to be used with the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <param name="row">The row the button should be placed on.</param>
    /// <returns>This <see cref="ComponentBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ComponentBuilder AddJumpButton(this ComponentBuilder builder, IComponentPaginator paginator, string? label = null, ButtonStyle? style = null, IEmote? emote = null, int row = 0)
        => builder.AddButton(paginator, PaginatorAction.Jump, label, style, emote, row);

    /// <summary>
    /// Adds a standard paginator button to the <see cref="ActionRowBuilder"/>, using the <paramref name="paginator"/> to determine the button's state.
    /// </summary>
    /// <param name="builder">The component builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="action">The paginator action the button represents.</param>
    /// <param name="label">The label of the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <param name="style">The style of the button. If one is not provided, the library will decide which one to use.</param>
    /// <param name="emote">The <see cref="IEmote"/> to be used with the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <returns>This <see cref="ActionRowBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ActionRowBuilder AddButton(this ActionRowBuilder builder, IComponentPaginator paginator, PaginatorAction action, string? label = null, ButtonStyle? style = null, IEmote? emote = null)
    {
        InteractiveGuards.NotNull(builder);
        InteractiveGuards.NotNull(paginator);

        return builder.WithButton(CreateButton(paginator, action, label, style, emote));
    }

    /// <summary>
    /// Adds a standard paginator button to the <see cref="ComponentBuilder"/>, using the <paramref name="paginator"/> to determine the button's state.
    /// </summary>
    /// <param name="builder">The component builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="action">The paginator action the button represents.</param>
    /// <param name="label">The label of the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <param name="style">The style of the button. If one is not provided, the library will decide which one to use.</param>
    /// <param name="emote">The <see cref="IEmote"/> to be used with the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="IEmote"/> will be set.</param>
    /// <param name="row">The row the button should be placed on.</param>
    /// <returns>This <see cref="ComponentBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ComponentBuilder AddButton(this ComponentBuilder builder, IComponentPaginator paginator, PaginatorAction action, string? label = null, ButtonStyle? style = null, IEmote? emote = null, int row = 0)
    {
        InteractiveGuards.NotNull(builder);
        InteractiveGuards.NotNull(paginator);

        return builder.WithButton(CreateButton(paginator, action, label, style, emote), row);
    }

    /// <summary>
    /// Adds the standard set of paginator buttons (previous, next, first, last and stop) to the <see cref="ActionRowBuilder"/>, using the <paramref name="paginator"/> to determine the buttons' states.
    /// </summary>
    /// <param name="builder">The action row builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <returns>This <see cref="ActionRowBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ActionRowBuilder AddPaginatorButtons(this ActionRowBuilder builder, IComponentPaginator paginator)
    {
        InteractiveGuards.NotNull(builder);
        InteractiveGuards.NotNull(paginator);

        return builder.AddPreviousButton(paginator)
            .AddNextButton(paginator)
            .AddFirstButton(paginator)
            .AddLastButton(paginator)
            .AddStopButton(paginator);
    }

    /// <summary>
    /// Adds the standard set of paginator buttons (previous, next, first, last and stop) to the <see cref="ComponentBuilder"/>, using the <paramref name="paginator"/> to determine the buttons' states.
    /// </summary>
    /// <param name="builder">The component builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <returns>This <see cref="ComponentBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ComponentBuilder AddPaginatorButtons(this ComponentBuilder builder, IComponentPaginator paginator)
    {
        InteractiveGuards.NotNull(builder);
        InteractiveGuards.NotNull(paginator);

        return builder.AddPreviousButton(paginator)
            .AddNextButton(paginator)
            .AddFirstButton(paginator)
            .AddLastButton(paginator)
            .AddStopButton(paginator);
    }

    /// <summary>
    /// Modifies the <see cref="ButtonBuilder"/> to be a paginator button by setting its custom ID and disabling it if necessary.
    /// </summary>
    /// <param name="builder">The button builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="action">The paginator action the button will represent.</param>
    /// <returns>This <see cref="ButtonBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="action"/> is invalid.</exception>
    public static ButtonBuilder AsPaginatorButton(this ButtonBuilder builder, IComponentPaginator paginator, PaginatorAction action)
    {
        InteractiveGuards.NotNull(builder);
        InteractiveGuards.NotNull(paginator);

        return builder.WithCustomId(paginator.GetCustomId(action))
            .WithDisabled(paginator, action);
    }

    /// <summary>
    /// Configures the <see cref="ButtonBuilder"/> to be disabled based on the state of the <paramref name="paginator"/>.
    /// </summary>
    /// <param name="builder">The button builder.</param>
    /// <param name="paginator">The paginator used to determine whether the button should be disabled.</param>
    /// <param name="action">An optional action the button represents that may influence the disabled status.</param>
    /// <returns>This <see cref="ButtonBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ButtonBuilder WithDisabled(this ButtonBuilder builder, IComponentPaginator paginator, PaginatorAction? action = null)
    {
        InteractiveGuards.NotNull(builder);
        InteractiveGuards.NotNull(paginator);

        return builder.WithDisabled(paginator.ShouldDisable(action));
    }

    /// <summary>
    /// Configures the <see cref="SelectMenuBuilder"/> to be disabled based on the state of the <paramref name="paginator"/>.
    /// </summary>
    /// <param name="builder">The select menu builder.</param>
    /// <param name="paginator">The paginator used to determine whether the select menu should be disabled.</param>
    /// <param name="action">An optional action the select menu represents that may influence the disabled status.</param>
    /// <returns>This <see cref="SelectMenuBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static SelectMenuBuilder WithDisabled(this SelectMenuBuilder builder, IComponentPaginator paginator, PaginatorAction? action = null)
    {
        InteractiveGuards.NotNull(builder);
        InteractiveGuards.NotNull(paginator);

        return builder.WithDisabled(paginator.ShouldDisable(action));
    }

    private static ButtonBuilder CreateButton(IComponentPaginator paginator, PaginatorAction action, string? label = null, ButtonStyle? style = null, IEmote? emote = null)
    {
        InteractiveGuards.NotNull(paginator);

        if (string.IsNullOrEmpty(label) && emote is null)
        {
            emote = action switch
            {
                PaginatorAction.Backward => new Emoji("◀"),
                PaginatorAction.Forward => new Emoji("▶"),
                PaginatorAction.SkipToStart => new Emoji("⏮"),
                PaginatorAction.SkipToEnd => new Emoji("⏭"),
                PaginatorAction.Exit => new Emoji("🛑"),
                PaginatorAction.Jump => new Emoji("🔢"),
                _ => throw new ArgumentOutOfRangeException(nameof(action))
            };
        }

        style ??= action == PaginatorAction.Exit ? ButtonStyle.Danger : ButtonStyle.Primary;    

        return new ButtonBuilder()
            .WithLabel(label)
            .WithStyle(style.Value)
            .WithEmote(emote)
            .AsPaginatorButton(paginator, action);
    }
}