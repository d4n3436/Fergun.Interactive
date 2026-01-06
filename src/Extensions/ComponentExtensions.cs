using System;
using JetBrains.Annotations;
using NetCord;
using NetCord.Rest;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Provides extension methods for component builders to add <see cref="IComponentPaginator"/> buttons.
/// </summary>
[PublicAPI]
public static class ComponentExtensions
{
    /// <summary>
    /// Adds the standard <see cref="PaginatorAction.Backward"/> button to the <see cref="ActionRowProperties"/>, using the <paramref name="paginator"/> to determine the button's state.
    /// </summary>
    /// <param name="builder">The action row builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="label">The label of the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="EmojiProperties"/> will be set.</param>
    /// <param name="style">The style of the button. If one is not provided, the library will decide which one to use.</param>
    /// <param name="emote">The <see cref="EmojiProperties"/> to be used with the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="EmojiProperties"/> will be set.</param>
    /// <returns>This <see cref="ActionRowProperties"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ActionRowProperties AddPreviousButton(this ActionRowProperties builder, IComponentPaginator paginator, string? label = null, ButtonStyle? style = null, EmojiProperties? emote = null)
        => builder.AddButton(paginator, PaginatorAction.Backward, label, style, emote);

    /// <summary>
    /// Adds the standard <see cref="PaginatorAction.Forward"/> button to the <see cref="ActionRowProperties"/>, using the <paramref name="paginator"/> to determine the button's state.
    /// </summary>
    /// <param name="builder">The action row builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="label">The label of the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="EmojiProperties"/> will be set.</param>
    /// <param name="style">The style of the button. If one is not provided, the library will decide which one to use.</param>
    /// <param name="emote">The <see cref="EmojiProperties"/> to be used with the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="EmojiProperties"/> will be set.</param>
    /// <returns>This <see cref="ActionRowProperties"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ActionRowProperties AddNextButton(this ActionRowProperties builder, IComponentPaginator paginator, string? label = null, ButtonStyle? style = null, EmojiProperties? emote = null)
        => builder.AddButton(paginator, PaginatorAction.Forward, label, style, emote);

    /// <summary>
    /// Adds the standard <see cref="PaginatorAction.SkipToStart"/> button to the <see cref="ActionRowProperties"/>, using the <paramref name="paginator"/> to determine the button's state.
    /// </summary>
    /// <param name="builder">The action row builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="label">The label of the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="EmojiProperties"/> will be set.</param>
    /// <param name="style">The style of the button. If one is not provided, the library will decide which one to use.</param>
    /// <param name="emote">The <see cref="EmojiProperties"/> to be used with the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="EmojiProperties"/> will be set.</param>
    /// <returns>This <see cref="ActionRowProperties"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ActionRowProperties AddFirstButton(this ActionRowProperties builder, IComponentPaginator paginator, string? label = null, ButtonStyle? style = null, EmojiProperties? emote = null)
        => builder.AddButton(paginator, PaginatorAction.SkipToStart, label, style, emote);

    /// <summary>
    /// Adds the standard <see cref="PaginatorAction.SkipToEnd"/> button to the <see cref="ActionRowProperties"/>, using the <paramref name="paginator"/> to determine the button's state.
    /// </summary>
    /// <param name="builder">The action row builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="label">The label of the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="EmojiProperties"/> will be set.</param>
    /// <param name="style">The style of the button. If one is not provided, the library will decide which one to use.</param>
    /// <param name="emote">The <see cref="EmojiProperties"/> to be used with the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="EmojiProperties"/> will be set.</param>
    /// <returns>This <see cref="ActionRowProperties"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ActionRowProperties AddLastButton(this ActionRowProperties builder, IComponentPaginator paginator, string? label = null, ButtonStyle? style = null, EmojiProperties? emote = null)
        => builder.AddButton(paginator, PaginatorAction.SkipToEnd, label, style, emote);

    /// <summary>
    /// Adds the standard <see cref="PaginatorAction.Exit"/> button to the <see cref="ActionRowProperties"/>, using the <paramref name="paginator"/> to determine the button's state.
    /// </summary>
    /// <param name="builder">The action row builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="label">The label of the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="EmojiProperties"/> will be set.</param>
    /// <param name="style">The style of the button. If one is not provided, the library will decide which one to use.</param>
    /// <param name="emote">The <see cref="EmojiProperties"/> to be used with the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="EmojiProperties"/> will be set.</param>
    /// <returns>This <see cref="ActionRowProperties"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ActionRowProperties AddStopButton(this ActionRowProperties builder, IComponentPaginator paginator, string? label = null, ButtonStyle? style = null, EmojiProperties? emote = null)
        => builder.AddButton(paginator, PaginatorAction.Exit, label, style, emote);

    /// <summary>
    /// Adds the standard <see cref="PaginatorAction.Jump"/> button to the <see cref="ActionRowProperties"/>, using the <paramref name="paginator"/> to determine the button's state.
    /// </summary>
    /// <param name="builder">The action row builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="label">The label of the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="EmojiProperties"/> will be set.</param>
    /// <param name="style">The style of the button. If one is not provided, the library will decide which one to use.</param>
    /// <param name="emote">The <see cref="EmojiProperties"/> to be used with the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="EmojiProperties"/> will be set.</param>
    /// <returns>This <see cref="ActionRowProperties"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ActionRowProperties AddJumpButton(this ActionRowProperties builder, IComponentPaginator paginator, string? label = null, ButtonStyle? style = null, EmojiProperties? emote = null)
        => builder.AddButton(paginator, PaginatorAction.Jump, label, style, emote);

    /// <summary>
    /// Adds a standard paginator button to the <see cref="ActionRowProperties"/>, using the <paramref name="paginator"/> to determine the button's state.
    /// </summary>
    /// <param name="builder">The component builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="action">The paginator action the button represents.</param>
    /// <param name="label">The label of the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="EmojiProperties"/> will be set.</param>
    /// <param name="style">The style of the button. If one is not provided, the library will decide which one to use.</param>
    /// <param name="emote">The <see cref="EmojiProperties"/> to be used with the button. If both <paramref name="label"/> and <paramref name="emote"/> are empty, a default <see cref="EmojiProperties"/> will be set.</param>
    /// <returns>This <see cref="ActionRowProperties"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ActionRowProperties AddButton(this ActionRowProperties builder, IComponentPaginator paginator, PaginatorAction action, string? label = null, ButtonStyle? style = null, EmojiProperties? emote = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(paginator);

        builder.Add(CreateButton(paginator, action, label, style, emote));
        return builder;
    }

    /// <summary>
    /// Adds the standard set of paginator buttons (previous, next, first, last and stop) to the <see cref="ActionRowProperties"/>, using the <paramref name="paginator"/> to determine the buttons' states.
    /// </summary>
    /// <param name="builder">The action row builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <returns>This <see cref="ActionRowProperties"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ActionRowProperties AddPaginatorButtons(this ActionRowProperties builder, IComponentPaginator paginator)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(paginator);

        return builder.AddPreviousButton(paginator)
            .AddNextButton(paginator)
            .AddFirstButton(paginator)
            .AddLastButton(paginator)
            .AddStopButton(paginator);
    }

    /// <summary>
    /// Modifies the <see cref="ButtonProperties"/> to be a paginator button by setting its custom ID and disabling it if necessary.
    /// </summary>
    /// <param name="builder">The button builder.</param>
    /// <param name="paginator">The paginator.</param>
    /// <param name="action">The paginator action the button will represent.</param>
    /// <returns>This <see cref="ButtonProperties"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="action"/> is invalid.</exception>
    public static ButtonProperties AsPaginatorButton(this ButtonProperties builder, IComponentPaginator paginator, PaginatorAction action)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(paginator);

        return builder.WithCustomId(paginator.GetCustomId(action))
            .WithDisabled(paginator, action);
    }

    /// <summary>
    /// Configures the <see cref="ButtonProperties"/> to be disabled based on the state of the <paramref name="paginator"/>.
    /// </summary>
    /// <param name="builder">The button builder.</param>
    /// <param name="paginator">The paginator used to determine whether the button should be disabled.</param>
    /// <param name="action">An optional action the button represents that may influence the disabled status.</param>
    /// <returns>This <see cref="ButtonProperties"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static ButtonProperties WithDisabled(this ButtonProperties builder, IComponentPaginator paginator, PaginatorAction? action = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(paginator);

        return builder.WithDisabled(paginator.ShouldDisable(action));
    }

    /// <summary>
    /// Configures the <typeparamref name="TMenuProperties"/> to be disabled based on the state of the <paramref name="paginator"/>.
    /// </summary>
    /// <param name="builder">The select menu builder.</param>
    /// <param name="paginator">The paginator used to determine whether the select menu should be disabled.</param>
    /// <param name="action">An optional action the select menu represents that may influence the disabled status.</param>
    /// <returns>This <typeparamref name="TMenuProperties"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="paginator"/> are <see langword="null"/>.</exception>
    public static TMenuProperties WithDisabled<TMenuProperties>(this TMenuProperties builder, IComponentPaginator paginator, PaginatorAction? action = null) where TMenuProperties : MenuProperties
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(paginator);

        return (TMenuProperties)builder.WithDisabled(paginator.ShouldDisable(action));
    }

    private static ButtonProperties CreateButton(IComponentPaginator paginator, PaginatorAction action, string? label = null, ButtonStyle? style = null, EmojiProperties? emote = null)
    {
        ArgumentNullException.ThrowIfNull(paginator);

        if (string.IsNullOrEmpty(label) && emote is null)
        {
            emote = action switch
            {
                PaginatorAction.Backward => EmojiProperties.Standard("◀"),
                PaginatorAction.Forward => EmojiProperties.Standard("▶"),
                PaginatorAction.SkipToStart => EmojiProperties.Standard("⏮"),
                PaginatorAction.SkipToEnd => EmojiProperties.Standard("⏭"),
                PaginatorAction.Exit => EmojiProperties.Standard("🛑"),
                PaginatorAction.Jump => EmojiProperties.Standard("🔢"),
                _ => throw new ArgumentOutOfRangeException(nameof(action))
            };
        }

        style ??= action == PaginatorAction.Exit ? ButtonStyle.Danger : ButtonStyle.Primary;

        return new ButtonProperties(null!, label!, emote!, style.Value)
            .AsPaginatorButton(paginator, action);
    }
}