using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace Fergun.Interactive;

/// <summary>
/// Represents a message page. A page consists of a <see cref="Text"/> and an <see cref="Embed"/>.
/// </summary>
public class Page : IPage
{
    private readonly Lazy<Embed[]> _lazyEmbeds;

    internal Page(PageBuilder page)
    {
        InteractiveGuards.NotNull(page);
        InteractiveGuards.NotNull(page.Stickers);

        Text = page.Text;
        IsTTS = page.IsTTS;
        AllowedMentions = page.AllowedMentions;
        MessageReference = page.MessageReference;
        Stickers = page.Stickers;
        AttachmentsFactory = page.AttachmentsFactory;
        
        bool isEmpty = false;
        var builder = page.GetEmbedBuilder();

        if (builder?.Author is null &&
            builder?.Color is null &&
            builder?.Description is null &&
            (builder?.Fields is null || builder.Fields.Count == 0) &&
            builder?.Footer is null &&
            builder?.ImageUrl is null &&
            builder?.ThumbnailUrl is null &&
            builder?.Timestamp is null &&
            builder?.Title is null &&
            builder?.Url is null)
        {
            if (string.IsNullOrEmpty(page.Text) && AttachmentsFactory is null)
            {
                throw new InvalidOperationException("Either a text, a valid EmbedBuilder or an AttachmentsFactory must be present.");
            }

            isEmpty = true;
        }

        Embed = isEmpty ? null : builder!.Build();
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