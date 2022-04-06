using System;
using System.Collections.Generic;
using System.Linq;
using Discord;

namespace Fergun.Interactive;

/// <summary>
/// Represents a page with multiple embeds.
/// </summary>
public class MultiEmbedPage : IPage
{
    private readonly Embed[] _embedArray;

    internal MultiEmbedPage(MultiEmbedPageBuilder builder)
    {
        InteractiveGuards.NotNull(builder);
        InteractiveGuards.NotNull(builder.Stickers);
        InteractiveGuards.NotNull(builder.Builders);
        InteractiveGuards.EmbedCountInRange(builder.Builders);
        if (string.IsNullOrEmpty(builder.Text) && builder.Builders.Count == 0)
        {
            throw new ArgumentException("At least 1 EmbedBuilder is required when Text is null or empty.", nameof(builder));
        }

        Text = builder.Text;
        IsTTS = builder.IsTTS;
        AllowedMentions = builder.AllowedMentions;
        MessageReference = builder.MessageReference;
        Stickers = builder.Stickers;
        _embedArray = builder.Builders.Select(x => x.Build()).ToArray();
    }

    /// <inheritdoc/>
    public string? Text { get; }

    /// <inheritdoc/>
    public bool IsTTS { get; }

    /// <inheritdoc/>
    public AllowedMentions? AllowedMentions { get; }

    /// <inheritdoc/>
    public MessageReference? MessageReference { get; }

    /// <inheritdoc/>
    public IReadOnlyCollection<ISticker> Stickers { get; }

    /// <inheritdoc/>
    public IReadOnlyCollection<Embed> Embeds => _embedArray;

    /// <summary>
    /// Converts this <see cref="MultiEmbedPage"/> into a <see cref="MultiEmbedPageBuilder"/>.
    /// </summary>
    /// <returns>A <see cref="MultiEmbedPageBuilder"/>.</returns>
    public MultiEmbedPageBuilder ToMultiEmbedPageBuilder() => new MultiEmbedPageBuilder().WithText(Text).WithBuilders(Embeds);

    /// <inheritdoc />
    Embed[] IPage.GetEmbedArray() => _embedArray;
}