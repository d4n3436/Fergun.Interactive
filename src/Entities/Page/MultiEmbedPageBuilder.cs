using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace Fergun.Interactive;

/// <summary>
/// Represents a builder of pages with multiple embeds.
/// </summary>
public class MultiEmbedPageBuilder : IPageBuilder<MultiEmbedPage>, IPageBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiEmbedPageBuilder"/> class.
    /// </summary>
    public MultiEmbedPageBuilder()
    {
    }

    /// <summary>
    /// Gets or sets the text of the <see cref="MultiEmbedPage"/>.
    /// </summary>
    /// <returns>The text of the page.</returns>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets a value that determines whether the text in <see cref="MultiEmbedPage"/> should be read aloud by Discord.
    /// </summary>
    public bool IsTTS { get; set; }

    /// <summary>
    /// Gets or sets the allowed mentions of the <see cref="MultiEmbedPage"/>.
    /// </summary>
    public AllowedMentions? AllowedMentions { get; set; }

    /// <summary>
    /// Gets or sets the message reference of the <see cref="MultiEmbedPage"/>.
    /// </summary>
    public MessageReference? MessageReference { get; set; }

    /// <summary>
    /// Gets or sets the stickers of the <see cref="MultiEmbedPage"/>.
    /// </summary>
    public IReadOnlyCollection<ISticker> Stickers { get; set; } = Array.Empty<ISticker>();

    /// <summary>
    /// Gets or sets the builders that will be in the page.
    /// </summary>
    /// <returns>The list of builders.</returns>
    public IList<EmbedBuilder> Builders { get; set; } = [];

    /// <summary>
    /// Gets or sets the factory of attachments.
    /// </summary>
    public Func<ValueTask<IEnumerable<FileAttachment>?>>? AttachmentsFactory { get; set; }

    /// <summary>
    /// Builds this builder into a <see cref="MultiEmbedPage"/>.
    /// </summary>
    /// <returns>A <see cref="MultiEmbedPage"/>.</returns>
    public MultiEmbedPage Build() => new(this);

    /// <summary>
    /// Sets the text of a <see cref="MultiEmbedPage"/>.
    /// </summary>
    /// <param name="text">The text to be set.</param>
    /// <returns>This builder.</returns>
    public MultiEmbedPageBuilder WithText(string? text)
    {
        Text = text;
        return this;
    }

    /// <summary>
    /// Sets the embed builders.
    /// </summary>
    /// <param name="builders">The embed builders.</param>
    /// <returns>This builder.</returns>
    public MultiEmbedPageBuilder WithBuilders(params EmbedBuilder[] builders)
    {
        var list = new List<EmbedBuilder>(builders);
        InteractiveGuards.EmbedCountInRange(list);
        Builders = list;
        return this;
    }

    /// <summary>
    /// Sets the embed builders.
    /// </summary>
    /// <param name="embeds">The embeds.</param>
    /// <returns>This builder.</returns>
    public MultiEmbedPageBuilder WithBuilders(params Embed[] embeds)
    {
        InteractiveGuards.NotNull(embeds);
        return WithBuilders(embeds.AsEnumerable());
    }

    /// <summary>
    /// Sets the embed builders.
    /// </summary>
    /// <param name="builders">The embed builders.</param>
    /// <returns>This builder.</returns>
    public MultiEmbedPageBuilder WithBuilders(IEnumerable<EmbedBuilder> builders)
    {
        var list = new List<EmbedBuilder>(builders);
        InteractiveGuards.EmbedCountInRange(list);
        Builders = list;
        return this;
    }

    /// <summary>
    /// Sets the embed builders.
    /// </summary>
    /// <param name="embeds">The embeds.</param>
    /// <returns>This builder.</returns>
    public MultiEmbedPageBuilder WithBuilders(IEnumerable<Embed> embeds)
    {
        InteractiveGuards.NotNull(embeds);
        return WithBuilders(embeds.Select(x => x.ToEmbedBuilder()));
    }

    /// <summary>
    /// Adds an embed builder.
    /// </summary>
    /// <param name="builder">The embed builder.</param>
    /// <returns>This builder.</returns>
    public MultiEmbedPageBuilder AddBuilder(EmbedBuilder builder)
    {
        InteractiveGuards.NotNull(builder);
        InteractiveGuards.EmbedCountInRange(Builders, true);
        Builders.Add(builder);
        return this;
    }

    /// <summary>
    /// Adds an embed builder from an <see cref="Embed"/>.
    /// </summary>
    /// <param name="embed">The embed.</param>
    /// <returns>This builder.</returns>
    public MultiEmbedPageBuilder AddBuilder(Embed embed)
    {
        InteractiveGuards.NotNull(embed);
        return AddBuilder(embed.ToEmbedBuilder());
    }

    /// <summary>
    /// Sets the <see cref="IsTTS"/> value of the <see cref="MultiEmbedPage"/>.
    /// </summary>
    /// <param name="isTTS">Whether the text should be read aloud by Discord.</param>
    /// <returns>The current builder.</returns>
    public MultiEmbedPageBuilder WithIsTTS(bool isTTS)
    {
        IsTTS = isTTS;
        return this;
    }

    /// <summary>
    /// Sets the allowed mentions the <see cref="MultiEmbedPage"/>.
    /// </summary>
    /// <param name="allowedMentions">The allowed mentions.</param>
    /// <returns>The current builder.</returns>
    public MultiEmbedPageBuilder WithAllowedMentions(AllowedMentions? allowedMentions)
    {
        AllowedMentions = allowedMentions;
        return this;
    }

    /// <summary>
    /// Sets the message reference of the <see cref="MultiEmbedPage"/>.
    /// </summary>
    /// <param name="messageReference">The message reference.</param>
    /// <returns>The current builder.</returns>
    public MultiEmbedPageBuilder WithMessageReference(MessageReference? messageReference)
    {
        MessageReference = messageReference;
        return this;
    }

    /// <summary>
    /// Sets the stickers of the <see cref="MultiEmbedPage"/>.
    /// </summary>
    /// <param name="stickers">The stickers.</param>
    /// <returns>The current builder.</returns>
    public MultiEmbedPageBuilder WithStickers(IReadOnlyCollection<ISticker> stickers)
    {
        InteractiveGuards.NotNull(stickers);
        Stickers = stickers;
        return this;
    }

    /// <summary>
    /// Sets the function that generates the attachment.
    /// </summary>
    /// <remarks>To leave the attachment in the message unmodified, <paramref name="attachmentFactory"/> must return <see langword="null"/> instead of a <see cref="FileAttachment"/> object.</remarks>
    /// <param name="attachmentFactory">The attachment factory. To leave the attachment in the message unmodified, <paramref name="attachmentFactory"/> must return <see langword="null"/> instead of a <see cref="FileAttachment"/> object.</param>
    /// <returns>The current builder.</returns>
    public MultiEmbedPageBuilder WithAttachmentFactory(Func<FileAttachment?> attachmentFactory)
    {
        InteractiveGuards.NotNull(attachmentFactory);
        return WithAttachmentsFactory(() =>
        {
            var attachment = attachmentFactory();
            return new ValueTask<IEnumerable<FileAttachment>?>(attachment is null ? null : new[] { attachment.Value });
        });
    }

    /// <summary>
    /// Sets the function that generates the attachment.
    /// </summary>
    /// <remarks>To leave the attachment in the message unmodified, <paramref name="attachmentFactory"/> must return <see langword="null"/> instead of a <see cref="FileAttachment"/> object.</remarks>
    /// <param name="attachmentFactory">The attachment factory. To leave the attachment in the message unmodified, <paramref name="attachmentFactory"/> must return <see langword="null"/> instead of a <see cref="FileAttachment"/> object.</param>
    /// <returns>The current builder.</returns>
    public MultiEmbedPageBuilder WithAttachmentFactory(Func<ValueTask<FileAttachment?>> attachmentFactory)
    {
        InteractiveGuards.NotNull(attachmentFactory);
        return WithAttachmentsFactory(async () =>
        {
            var attachment = await attachmentFactory().ConfigureAwait(false);
            return attachment is null ? null : new[] { attachment.Value };
        });
    }

    /// <summary>
    /// Sets the function that generates the attachments.
    /// </summary>
    /// <remarks>To leave the attachments in the message unmodified, <paramref name="attachmentsFactory"/> must return <see langword="null"/> instead of a <see cref="FileAttachment"/> object.</remarks>
    /// <param name="attachmentsFactory">The attachments factory. To leave the attachments in the message unmodified, <paramref name="attachmentsFactory"/> must return <see langword="null"/> instead of a <see cref="FileAttachment"/> object.</param>
    /// <returns>The current builder.</returns>
    public MultiEmbedPageBuilder WithAttachmentsFactory(Func<IEnumerable<FileAttachment>?> attachmentsFactory)
    {
        InteractiveGuards.NotNull(attachmentsFactory);
        return WithAttachmentsFactory(() => new ValueTask<IEnumerable<FileAttachment>?>(attachmentsFactory()));
    }

    /// <summary>
    /// Sets the function that generates the attachments.
    /// </summary>
    /// <remarks>To leave the attachments in the message unmodified, <paramref name="attachmentsFactory"/> must return <see langword="null"/> instead of a <see cref="FileAttachment"/> object.</remarks>
    /// <param name="attachmentsFactory">The attachments factory. To leave the attachments in the message unmodified, <paramref name="attachmentsFactory"/> must return <see langword="null"/> instead of a <see cref="FileAttachment"/> object.</param>
    /// <returns>The current builder.</returns>
    public MultiEmbedPageBuilder WithAttachmentsFactory(Func<ValueTask<IEnumerable<FileAttachment>?>>? attachmentsFactory)
    {
        AttachmentsFactory = attachmentsFactory;
        return this;
    }

    /// <inheritdoc/>
    IPage IPageBuilder<IPage>.Build() => Build();
}