using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NetCord;
using NetCord.Rest;

namespace Fergun.Interactive;

/// <summary>
/// Represents a message page. A page consists of a <see cref="Text"/>, <see cref="Embed"/> or <see cref="Components"/>.
/// </summary>
[PublicAPI]
public class Page : IPage
{
    private readonly Lazy<EmbedProperties[]> _lazyEmbeds;

    internal Page(PageBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(builder.StickerIds);

        Text = builder.Text;
        IsTTS = builder.IsTTS;
        AllowedMentions = builder.AllowedMentions;
        MessageReference = builder.MessageReference;
        StickerIds = builder.StickerIds;
        AttachmentsFactory = builder.AttachmentsFactory;
        Components = builder.Components;
        MessageFlags = builder.MessageFlags;

        bool isEmpty = false;
        var embedProperties = builder.GetEmbedProperties();

        if (embedProperties.Author is null &&
            embedProperties.Description is null &&
            (embedProperties.Fields is null || !embedProperties.Fields.Any()) &&
            embedProperties.Footer is null &&
            embedProperties.Image is null &&
            embedProperties.Thumbnail is null &&
            embedProperties.Timestamp is null &&
            embedProperties.Title is null &&
            embedProperties.Url is null)
        {
            if (string.IsNullOrEmpty(builder.Text) && Components is null && AttachmentsFactory is null)
            {
                throw new InvalidOperationException("Either a text, a valid EmbedProperties, Components or an AttachmentsFactory must be present.");
            }

            isEmpty = true;
        }

        Embed = isEmpty ? null : embedProperties;
        _lazyEmbeds = new Lazy<EmbedProperties[]>(() => Embed is null ? [] : [Embed]);
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
    public Func<ValueTask<IEnumerable<AttachmentProperties>?>>? AttachmentsFactory { get; }

    /// <inheritdoc/>
    public List<IMessageComponentProperties>? Components { get; }

    /// <inheritdoc/>
    public MessageFlags? MessageFlags { get; }

    /// <summary>
    /// Gets the embed of this page.
    /// </summary>
    public EmbedProperties? Embed { get; }

    /// <inheritdoc/>
    public IReadOnlyCollection<EmbedProperties> Embeds => _lazyEmbeds.Value;

    /// <summary>
    /// Creates a new <see cref="Page"/> from an <see cref="EmbedProperties"/>.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>A <see cref="Page"/>.</returns>
    public static Page FromEmbedProperties(EmbedProperties builder)
        => new(PageBuilder.FromEmbedProperties(builder));

    /// <summary>
    /// Creates a <see cref="PageBuilder"/> with all the values of this <see cref="Page"/>.
    /// </summary>
    /// <returns>A <see cref="PageBuilder"/>.</returns>
    public PageBuilder ToPageBuilder()
        => new(this);
}