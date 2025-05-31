using System;
using System.Collections.Generic;
using Discord;
using JetBrains.Annotations;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents the properties of a <see cref="PaginatorBuilder{TPaginator, TBuilder}"/>.
/// </summary>
[PublicAPI]
public interface IBasePaginatorBuilderProperties : IInteractiveBuilderProperties<KeyValuePair<IEmote, PaginatorAction>>
{
    /// <summary>
    /// Gets a value indicating whether the paginator is restricted to <see cref="IInteractiveBuilderProperties{TOption}.Users"/>.
    /// </summary>
    bool IsUserRestricted { get; }

    /// <summary>
    /// Gets or sets the index of the page the paginator should start.
    /// </summary>
    int StartPageIndex { get; set; }

    /// <summary>
    /// Gets or sets the footer format in the <see cref="Embed"/> of the paginator.
    /// </summary>
    /// <remarks>Setting this to other than <see cref="PaginatorFooter.None"/> will override any other footer in the pages.</remarks>
    PaginatorFooter Footer { get; set; }

    /// <summary>
    /// Gets or sets the emotes and their related actions of the paginator.
    /// </summary>
    /// <remarks>This property has been replaced by <see cref="ButtonFactories"/> and it shouldn't be used on button-based paginators.</remarks>
    new IDictionary<IEmote, PaginatorAction> Options
    {
        get;
        [Obsolete($"The library no longer uses this property for button-based paginators and it will add any values added here into {nameof(ButtonFactories)}, unless this value is changed.")]
        set;
    }

    /// <summary>
    /// Gets the button factories.
    /// </summary>
    IList<Func<IButtonContext, IPaginatorButton>> ButtonFactories { get; }

    /// <summary>
    /// Gets the select menu factories.
    /// </summary>
    /// <remarks>Paginator select menus are detached from the paginator and their interactions must be manually handled.</remarks>
    IList<Func<ISelectMenuContext, IPaginatorSelectMenu>> SelectMenuFactories { get; }

    /// <summary>
    /// Gets or sets the maximum time to wait for a "jump to page" input.
    /// </summary>
    TimeSpan JumpInputTimeout { get; set; }

    /// <summary>
    /// Gets or sets the "jump to page" prompt that is displayed to the user.
    /// </summary>
    /// <remarks>
    /// In button inputs, this is the title of the modal that is displayed.<br/>
    /// In reaction inputs, this is the content of the temporary message that is sent.
    /// </remarks>
    string? JumpInputPrompt { get; set; }

    /// <summary>
    /// Gets or sets the "jump to page" text label that is displayed in the modal.
    /// </summary>
    string? JumpInputTextLabel { get; set; }

    /// <summary>
    /// Gets or sets the message to display when receiving an invalid "jump to page" input.
    /// </summary>
    /// <remarks>An invalid input may be one that isn't a number, or a number that is outside the valid range.</remarks>
    string? InvalidJumpInputMessage { get; set; }

    /// <summary>
    /// Gets or sets the message to display when a user attempts to use the "jump to page" action while other user is using it.
    /// </summary>
    string? JumpInputInUseMessage { get; set; }

    /// <summary>
    /// Gets or sets the message to display when receiving an expired "jump to page" input.
    /// </summary>
    string? ExpiredJumpInputMessage { get; set; }
}