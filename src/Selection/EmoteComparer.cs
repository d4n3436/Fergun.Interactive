using System;
using System.Collections.Generic;
using NetCord;

namespace Fergun.Interactive.Selection;

internal sealed class EmoteComparer<TValue> : IEqualityComparer<KeyValuePair<EmojiProperties, TValue>>
{
    public bool Equals(KeyValuePair<EmojiProperties, TValue> x, KeyValuePair<EmojiProperties, TValue> y)
        => x.Key.Id == y.Key.Id && x.Key.Name == y.Key.Name && Equals(x.Value, y.Value);

    // ReSharper disable once UsageOfDefaultStructEquality
    public int GetHashCode(KeyValuePair<EmojiProperties, TValue> obj) => obj.GetHashCode();
}

internal sealed class EmoteComparer : IEqualityComparer<EmojiProperties>
{
    public bool Equals(EmojiProperties? x, EmojiProperties? y) => x?.Id == y?.Id && x?.Name == y?.Name;

    public int GetHashCode(EmojiProperties obj) => HashCode.Combine(obj.Id, obj.Name);
}