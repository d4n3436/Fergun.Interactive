using System;
using System.Collections;
using System.Collections.Generic;
using NetCord;


namespace Fergun.Interactive.Pagination;

/// <summary>
/// Used to add values to both <see cref="PaginatorBuilder{TPaginator, TBuilder}.Options"/> and <see cref="PaginatorBuilder{TPaginator, TBuilder}.ButtonFactories"/>.
/// </summary>
internal sealed class OptionsWrapper : IDictionary<EmojiProperties, PaginatorAction>
{
    private readonly IDictionary<EmojiProperties, PaginatorAction> _dictionary = new Dictionary<EmojiProperties, PaginatorAction>();
    private readonly Dictionary<EmojiProperties, Func<IButtonContext, IPaginatorButton>> _factoryDictionary = [];
    private readonly IList<Func<IButtonContext, IPaginatorButton>> _buttonFactories;

    public OptionsWrapper(IList<Func<IButtonContext, IPaginatorButton>> buttonFactories)
    {
        _buttonFactories = buttonFactories;
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<EmojiProperties, PaginatorAction>> GetEnumerator() => _dictionary.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

    /// <inheritdoc />
    public void Add(KeyValuePair<EmojiProperties, PaginatorAction> item) => Add(item.Key, item.Value);

    /// <inheritdoc />
    public void Clear()
    {
        _dictionary.Clear();
        _factoryDictionary.Clear();
        _buttonFactories.Clear();
    }

    /// <inheritdoc />
    public bool Contains(KeyValuePair<EmojiProperties, PaginatorAction> item) => _dictionary.Contains(item);

    /// <inheritdoc />
    public void CopyTo(KeyValuePair<EmojiProperties, PaginatorAction>[] array, int arrayIndex) => _dictionary.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public bool Remove(KeyValuePair<EmojiProperties, PaginatorAction> item) => _dictionary.Remove(item);

    /// <inheritdoc />
    public int Count => _dictionary.Count;

    /// <inheritdoc />
    public bool IsReadOnly => _dictionary.IsReadOnly;

    /// <inheritdoc />
    public void Add(EmojiProperties key, PaginatorAction value)
    {
        _dictionary.Add(key, value);
        _buttonFactories.Add(Factory);
        _factoryDictionary.Add(key, Factory);
        return;

        IPaginatorButton Factory(IButtonContext _) => new PaginatorButton(key, value);
    }

    /// <inheritdoc />
    public bool ContainsKey(EmojiProperties key) => _dictionary.ContainsKey(key);

    /// <inheritdoc />
    public bool Remove(EmojiProperties key)
    {
        if (_factoryDictionary.TryGetValue(key, out var factory) && _factoryDictionary.Remove(key))
        {
            _buttonFactories.Remove(factory);
        }

        return _dictionary.Remove(key);
    }

    /// <inheritdoc />
    public bool TryGetValue(EmojiProperties key, out PaginatorAction value) => _dictionary.TryGetValue(key, out value);

    /// <inheritdoc />
    public PaginatorAction this[EmojiProperties key]
    {
        get => _dictionary[key];
        set => _dictionary[key] = value;
    }

    /// <inheritdoc />
    public ICollection<EmojiProperties> Keys => _dictionary.Keys;

    /// <inheritdoc />
    public ICollection<PaginatorAction> Values => _dictionary.Values;
}