using System.Collections.Generic;
using Discord;

namespace Fergun.Interactive.Selection;

internal sealed class EmoteComparer<TValue> : IEqualityComparer<KeyValuePair<IEmote, TValue>>
{
    public bool Equals(KeyValuePair<IEmote, TValue> x, KeyValuePair<IEmote, TValue> y)
        => x.Key.ToString() == y.Key.ToString() && Equals(x.Value, y.Value);

    // ReSharper disable once UsageOfDefaultStructEquality
    public int GetHashCode(KeyValuePair<IEmote, TValue> pair) => pair.GetHashCode();
}

internal sealed class EmoteComparer : IEqualityComparer<IEmote>
{
    public bool Equals(IEmote? x, IEmote? y) => x?.ToString() == y?.ToString();

    public int GetHashCode(IEmote obj) => obj.ToString()?.GetHashCode() ?? 0;
}