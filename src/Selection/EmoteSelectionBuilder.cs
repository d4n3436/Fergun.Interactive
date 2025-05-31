using System;
using System.Collections.Generic;
using Discord;
using JetBrains.Annotations;

namespace Fergun.Interactive.Selection;

/// <summary>
/// Represents a variant of <see cref="SelectionBuilder{TValue}"/> that allows using emotes as input.
/// It provides overriden properties with default values that makes it ready to use with options using reactions or buttons as input.
/// </summary>
/// <typeparam name="TValue">The type of the value that represents a specific emote.</typeparam>
[PublicAPI]
public sealed class EmoteSelectionBuilder<TValue>
    : BaseSelectionBuilder<Selection<KeyValuePair<IEmote, TValue>>, KeyValuePair<IEmote, TValue>, EmoteSelectionBuilder<TValue>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmoteSelectionBuilder{TValue}"/> class.
    /// </summary>
    public EmoteSelectionBuilder()
    {
    }

    /// <inheritdoc/>
    public override Func<KeyValuePair<IEmote, TValue>, IEmote>? EmoteConverter { get; set; } = pair => pair.Key;

    /// <inheritdoc/>
    public override IEqualityComparer<KeyValuePair<IEmote, TValue>> EqualityComparer { get; set; } = new EmoteComparer<TValue>();

    /// <summary>
    /// Builds this <see cref="EmoteSelectionBuilder{TValue}"/> into an immutable <see cref="Selection{TOption}"/>.
    /// </summary>
    /// <returns>A <see cref="Selection{TOption}"/>.</returns>
    public override Selection<KeyValuePair<IEmote, TValue>> Build() => new(this);

    /// <summary>
    /// Sets the options.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <returns>This builder.</returns>
    public EmoteSelectionBuilder<TValue> WithOptions(IDictionary<IEmote, TValue> options)
    {
        InteractiveGuards.NotNull(options);
        Options = options;
        return this;
    }

    /// <summary>
    /// Adds an option.
    /// </summary>
    /// <param name="emote">The emote.</param>
    /// <param name="value">The value.</param>
    /// <returns>This builder.</returns>
    public EmoteSelectionBuilder<TValue> AddOption(IEmote emote, TValue value)
    {
        Options.Add(new KeyValuePair<IEmote, TValue>(emote, value));
        return this;
    }
}

/// <summary>
/// Represents a variant of <see cref="SelectionBuilder{TValue}"/> that allows using emotes as input.
/// It provides overriden properties with default values that makes it ready to use with options using reactions or buttons as input.
/// </summary>
[PublicAPI]
public sealed class EmoteSelectionBuilder : BaseSelectionBuilder<Selection<IEmote>, IEmote, EmoteSelectionBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmoteSelectionBuilder"/> class.
    /// </summary>
    public EmoteSelectionBuilder()
    {
    }

    /// <inheritdoc/>
    public override Func<IEmote, IEmote>? EmoteConverter { get; set; } = emote => emote;

    /// <inheritdoc/>
    public override IEqualityComparer<IEmote> EqualityComparer { get; set; } = new EmoteComparer();

    /// <summary>
    /// Builds this <see cref="EmoteSelectionBuilder"/> into an immutable <see cref="Selection{TOption}"/>.
    /// </summary>
    /// <returns>A <see cref="Selection{TOption}"/>.</returns>
    public override Selection<IEmote> Build() => new(this);
}