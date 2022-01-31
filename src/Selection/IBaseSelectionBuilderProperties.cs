using System;
using System.Collections.Generic;
using Discord;

namespace Fergun.Interactive.Selection;

/// <summary>
/// Represents the properties in a <see cref="BaseSelectionBuilder{TSelection, TOption, TBuilder}"/>.
/// </summary>
/// <typeparam name="TOption">The type of the options the selection will have.</typeparam>
public interface IBaseSelectionBuilderProperties<TOption> : IInteractiveBuilderProperties<TOption>
{
    /// <summary>
    /// Gets a value indicating whether the selection is restricted to <see cref="IInteractiveBuilderProperties{TOption}.Users"/>.
    /// </summary>
    bool IsUserRestricted { get; }

    /// <summary>
    /// Gets or sets a function that returns an <see cref="IEmote"/> representation of a <typeparamref name="TOption"/>.
    /// </summary>
    /// <remarks>
    /// Requirements for each input type:<br/><br/>
    /// Reactions: Required.<br/>
    /// Messages: Unused.<br/>
    /// Buttons: Required (for emotes) unless a <see cref="StringConverter"/> is provided (for labels).<br/>
    /// Select menus: Optional.
    /// </remarks>
    Func<TOption, IEmote>? EmoteConverter { get; set; }

    /// <summary>
    /// Gets or sets a function that returns a <see cref="string"/> representation of a <typeparamref name="TOption"/>.
    /// </summary>
    /// <remarks>
    /// Requirements for each input type:<br/><br/>
    /// Reactions: Unused.<br/>
    /// Messages: Required. If not set, defaults to <see cref="object.ToString()"/>.<br/>
    /// Buttons: Required (for labels) unless a <see cref="EmoteConverter"/> is provided (for emotes). Defaults to <see cref="object.ToString()"/> if neither are set.<br/>
    /// Select menus: Required. If not set, defaults to <see cref="object.ToString()"/>.
    /// </remarks>
    Func<TOption, string>? StringConverter { get; set; }

    /// <summary>
    /// Gets or sets the equality comparer of <typeparamref name="TOption"/>s.
    /// </summary>
    IEqualityComparer<TOption> EqualityComparer { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="BaseSelection{TOption}"/> allows for cancellation.
    /// </summary>
    /// <remarks>When this value is <see langword="true"/>, the last element in <see cref="IInteractiveBuilderProperties{TOption}.Options"/>
    /// will be used to cancel the <see cref="BaseSelection{TOption}"/>.</remarks>
    bool AllowCancel { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="IPage"/> which is sent into the channel.
    /// </summary>
    IPageBuilder SelectionPage { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="IPage"/> which the <see cref="BaseSelection{TOption}"/>
    /// gets modified to after a valid input is received (except cancellation inputs).
    /// </summary>
    IPageBuilder? SuccessPage { get; set; }

    /// <summary>
    /// Gets or sets the action that will be done after valid input is received (except cancellation inputs).
    /// </summary>
    ActionOnStop ActionOnSuccess { get; set; }
}