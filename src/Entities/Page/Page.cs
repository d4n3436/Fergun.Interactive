using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace Fergun.Interactive;

/// <summary>
/// Represents a message page. A page consists of a <see cref="Text"/>, <see cref="Embed"/> or <see cref="Components"/>.
/// </summary>
public class Page : IPage
{
    private readonly Lazy<Embed[]> _lazyEmbeds;

    internal Page(PageBuilder builder)
    {
        InteractiveGuards.NotNull(builder);
        InteractiveGuards.NotNull(builder.Stickers);

        Text = builder.Text;
        IsTTS = builder.IsTTS;
        AllowedMentions = builder.AllowedMentions;
        MessageReference = builder.MessageReference;
        Stickers = builder.Stickers;
        AttachmentsFactory = builder.AttachmentsFactory;
        Components = builder.Components;
        MessageFlags = builder.MessageFlags;

        bool isEmpty = false;
        var embedBuilder = builder.GetEmbedBuilder();

        if (embedBuilder.Author is null &&
            embedBuilder.Color is null &&
            embedBuilder.Description is null &&
            (embedBuilder.Fields is null || embedBuilder.Fields.Count == 0) &&
            embedBuilder.Footer is null &&
            embedBuilder.ImageUrl is null &&
            embedBuilder.ThumbnailUrl is null &&
            embedBuilder.Timestamp is null &&
            embedBuilder.Title is null &&
            embedBuilder.Url is null)
        {
            if (string.IsNullOrEmpty(builder.Text) && Components is null && AttachmentsFactory is null)
            {
                throw new InvalidOperationException("Either a text, a valid EmbedBuilder, Components or an AttachmentsFactory must be present.");
            }

            isEmpty = true;
        }

        Embed = isEmpty ? null : embedBuilder.Build();
        _lazyEmbeds = new Lazy<Embed[]>(() => Embed is null ? [] : [Embed]);
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
    public Func<ValueTask<IEnumerable<FileAttachment>?>>? AttachmentsFactory { get; }

    /// <inheritdoc/>
    public MessageComponent? Components { get; }

    /// <inheritdoc/>
    public MessageFlags MessageFlags { get; }

    /// <summary>
    /// Gets the embed of this page.
    /// </summary>
    public Embed? Embed { get; }

    /// <inheritdoc/>
    IReadOnlyCollection<Embed> IPage<Embed>.Embeds => _lazyEmbeds.Value;

    /// <inheritdoc />
    Embed[] IPage.GetEmbedArray() => _lazyEmbeds.Value;

    /// <summary>
    /// Creates a new <see cref="Page"/> from an <see cref="Discord.Embed"/>.
    /// </summary>
    /// <param name="embed">The embed.</param>
    /// <returns>A <see cref="Page"/>.</returns>
    public static Page FromEmbed(Embed embed)
        => new(PageBuilder.FromEmbed(embed));

    /// <summary>
    /// Creates a new <see cref="Page"/> from an <see cref="EmbedBuilder"/>.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>A <see cref="Page"/>.</returns>
    public static Page FromEmbedBuilder(EmbedBuilder builder)
        => new(PageBuilder.FromEmbedBuilder(builder));

    /// <summary>
    /// Creates a <see cref="PageBuilder"/> with all the values of this <see cref="Page"/>.
    /// </summary>
    /// <returns>A <see cref="PageBuilder"/>.</returns>
    public PageBuilder ToPageBuilder()
        => new(this);
}