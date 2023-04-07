using System;
using System.Collections;
using System.Collections.Generic;
using Discord;

namespace Fergun.Interactive.Pagination;

// Used to add values to both Options and ButtonFactories
internal sealed class OptionsWrapper : IDictionary<IEmote, PaginatorAction>
{
    private readonly IDictionary<IEmote, PaginatorAction> _dictionary = new Dictionary<IEmote, PaginatorAction>();
    private readonly IDictionary<IEmote, Func<IButtonContext, IPaginatorButton>> _factoryDictionary = new Dictionary<IEmote, Func<IButtonContext, IPaginatorButton>>();
    private readonly IList<Func<IButtonContext, IPaginatorButton>> _buttonFactories;

    public OptionsWrapper(IList<Func<IButtonContext, IPaginatorButton>> buttonFactories)
    {
        _buttonFactories = buttonFactories;
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<IEmote, PaginatorAction>> GetEnumerator() => _dictionary.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

    /// <inheritdoc />
    public void Add(KeyValuePair<IEmote, PaginatorAction> item) => Add(item.Key, item.Value);

    /// <inheritdoc />
    public void Clear()
    {
        _dictionary.Clear();
        _factoryDictionary.Clear();
        _buttonFactories.Clear();
    }

    /// <inheritdoc />
    public bool Contains(KeyValuePair<IEmote, PaginatorAction> item) => _dictionary.Contains(item);

    /// <inheritdoc />
    public void CopyTo(KeyValuePair<IEmote, PaginatorAction>[] array, int arrayIndex) => _dictionary.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public bool Remove(KeyValuePair<IEmote, PaginatorAction> item) => _dictionary.Remove(item);

    /// <inheritdoc />
    public int Count => _dictionary.Count;

    /// <inheritdoc />
    public bool IsReadOnly => _dictionary.IsReadOnly;

    /// <inheritdoc />
    public void Add(IEmote key, PaginatorAction value)
    {
        _dictionary.Add(key, value);
        _buttonFactories.Add(Factory);
        _factoryDictionary.Add(key, Factory);

        IPaginatorButton Factory(IButtonContext _) => new PaginatorButton(key, value);
    }

    /// <inheritdoc />
    public bool ContainsKey(IEmote key) => _dictionary.ContainsKey(key);

    /// <inheritdoc />
    public bool Remove(IEmote key)
    {
        if (_factoryDictionary.TryGetValue(key, out var factory) && _factoryDictionary.Remove(key))
        {
            _buttonFactories.Remove(factory);
        }

        return _dictionary.Remove(key);
    }

    /// <inheritdoc />
    public bool TryGetValue(IEmote key, out PaginatorAction value) => _dictionary.TryGetValue(key, out value);

    /// <inheritdoc />
    public PaginatorAction this[IEmote key]
    {
        get => _dictionary[key];
        set => _dictionary[key] = value;
    }

    /// <inheritdoc />
    public ICollection<IEmote> Keys => _dictionary.Keys;

    /// <inheritdoc />
    public ICollection<PaginatorAction> Values => _dictionary.Values;
}