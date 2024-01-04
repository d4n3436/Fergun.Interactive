using System.Collections.Generic;
using Discord;

namespace Fergun.Interactive.Pagination;

/// <inheritdoc cref="IPaginatorSelectMenu"/>
public class PaginatorSelectMenu : IPaginatorSelectMenu
{
    private readonly SelectMenuBuilder _builder;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatorSelectMenu"/> class.
    /// </summary>
    /// <remarks>Detached paginator select menus are not managed by a paginator and must be manually handled.</remarks>
    /// <param name="builder">The select menu builder.</param>
    /// <param name="isDisabled">A value indicating whether to disable the select menu. If the value is null, the library will decide its status. This value overrides the one in <paramref name="builder"/>.</param>
    public PaginatorSelectMenu(SelectMenuBuilder builder, bool? isDisabled = null)
    {
        InteractiveGuards.NotNull(builder);
        IsDisabled = isDisabled;
        _builder = builder;
    }

    private PaginatorSelectMenu(bool isHidden)
    {
        _builder = new SelectMenuBuilder();
        IsHidden = isHidden;
    }

    /// <summary>
    /// Returns a hidden select menu.
    /// </summary>
    public static PaginatorSelectMenu Hidden { get; } = new(true);

    /// <inheritdoc />
    public string CustomId => _builder.CustomId;

    /// <inheritdoc />
    public ComponentType Type => _builder.Type;

    /// <inheritdoc />
    public string? Placeholder => _builder.Placeholder;

    /// <inheritdoc />
    public int MinValues => _builder.MinValues;

    /// <inheritdoc />
    public int MaxValues => _builder.MaxValues;

    /// <inheritdoc />
    public List<SelectMenuOptionBuilder>? Options => _builder.Options;

    /// <inheritdoc />
    public bool? IsDisabled { get; }

    /// <inheritdoc />
    public List<ChannelType>? ChannelTypes => _builder.ChannelTypes;

    /// <inheritdoc />
    public List<SelectMenuDefaultValue>? DefaultValues  => _builder.DefaultValues;

    /// <inheritdoc />
    public bool IsHidden { get; }
}