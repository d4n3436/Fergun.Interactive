using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using JetBrains.Annotations;
using NetCord;
using NetCord.Rest;

namespace Fergun.Interactive;

/// <summary>
/// Represents a page with multiple embeds.
/// </summary>
[PublicAPI]
public class MultiEmbedPage : IPage
{
    private readonly EmbedProperties[] _embedArray;

    internal MultiEmbedPage(MultiEmbedPageBuilder builder)
    {
        InteractiveGuards.NotNull(builder);
        InteractiveGuards.NotNull(builder.StickerIds);
        InteractiveGuards.NotNull(builder.Builders);
        InteractiveGuards.EmbedCountInRange(builder.Builders);
        if (string.IsNullOrEmpty(builder.Text) && builder.Builders.Count == 0 && builder.AttachmentsFactory is null)
        {
            throw new ArgumentException("Either a text, at least one embed builder, or an AttachmentsFactory is required.", nameof(builder));
        }

        Text = builder.Text;
        IsTTS = builder.IsTTS;
        AllowedMentions = builder.AllowedMentions;
        MessageReference = builder.MessageReference;
        StickerIds = builder.StickerIds;
        AttachmentsFactory = builder.AttachmentsFactory;
        Components = builder.Components;
        MessageFlags = builder.MessageFlags;
        _embedArray = builder.Builders.ToArray();
    }

    /// <inheritdoc/>
    public string? Text { get; }

    /// <inheritdoc/>
    public bool IsTTS { get; }

    /// <inheritdoc/>
    public AllowedMentionsProperties? AllowedMentions { get; }

    /// <inheritdoc/>
    public MessageReferenceProperties? MessageReference { get; }

    /// <inheritdoc/>
    public IReadOnlyCollection<ulong> StickerIds { get; }

    /// <inheritdoc/>
    public IReadOnlyCollection<EmbedProperties> Embeds => _embedArray;

    /// <inheritdoc/>
    public Func<ValueTask<IEnumerable<AttachmentProperties>?>>? AttachmentsFactory { get; }

    /// <inheritdoc/>
    public List<IMessageComponentProperties>? Components { get; }

    /// <inheritdoc/>
    public MessageFlags? MessageFlags { get; }

    /// <summary>
    /// Converts this <see cref="MultiEmbedPage"/> into a <see cref="MultiEmbedPageBuilder"/>.
    /// </summary>
    /// <returns>A <see cref="MultiEmbedPageBuilder"/>.</returns>
    public MultiEmbedPageBuilder ToMultiEmbedPageBuilder()
        => new MultiEmbedPageBuilder()
        .WithText(Text)
        .WithIsTTS(IsTTS)
        .WithAllowedMentions(AllowedMentions)
        .WithMessageReference(MessageReference)
        .WithStickerIds(StickerIds)
        .WithBuilders(Embeds)
        .WithAttachmentsFactory(AttachmentsFactory)
        .WithComponents(Components)
        .WithMessageFlags(MessageFlags);

    /// <inheritdoc />
    Embed[] IPage.Embeds => _embedArray;
}