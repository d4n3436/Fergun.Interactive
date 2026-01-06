using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NetCord;
using NetCord.Rest;

namespace Fergun.Interactive;

/// <summary>
/// Represents a builder of pages with multiple embeds.
/// </summary>
[PublicAPI]
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
    public AllowedMentionsProperties? AllowedMentions { get; set; }

    /// <summary>
    /// Gets or sets the message reference of the <see cref="MultiEmbedPage"/>.
    /// </summary>
    public MessageReferenceProperties? MessageReference { get; set; }

    /// <summary>
    /// Gets or sets the stickers of the <see cref="MultiEmbedPage"/>.
    /// </summary>
    public IReadOnlyCollection<ulong> StickerIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the builders that will be in the page.
    /// </summary>
    /// <returns>The list of builders.</returns>
    public IList<EmbedProperties> Builders { get; set; } = [];

    /// <summary>
    /// Gets or sets the factory of attachments.
    /// </summary>
    public Func<ValueTask<IEnumerable<AttachmentProperties>?>>? AttachmentsFactory { get; set; }

    /// <summary>
    /// Gets or sets the components of this page.
    /// </summary>
    /// <remarks>This property is only used on component paginators. Using the new components (components V2) requires not using <see cref="Text"/>, <see cref="StickerIds"/> or any embed property.</remarks>
    public List<IMessageComponentProperties>? Components { get; set; }

    /// <summary>
    /// Gets or sets the message flags.
    /// </summary>
    public MessageFlags? MessageFlags { get; set; }

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
    public MultiEmbedPageBuilder WithBuilders(params EmbedProperties[] builders)
    {
        var list = new List<EmbedProperties>(builders);
        InteractiveGuards.EmbedCountInRange(list);
        Builders = list;
        return this;
    }

    /// <summary>
    /// Sets the embed builders.
    /// </summary>
    /// <param name="builders">The embed builders.</param>
    /// <returns>This builder.</returns>
    public MultiEmbedPageBuilder WithBuilders(IEnumerable<EmbedProperties> builders)
    {
        var list = new List<EmbedProperties>(builders);
        InteractiveGuards.EmbedCountInRange(list);
        Builders = list;
        return this;
    }

    /// <summary>
    /// Adds an embed builder.
    /// </summary>
    /// <param name="builder">The embed builder.</param>
    /// <returns>This builder.</returns>
    public MultiEmbedPageBuilder AddBuilder(EmbedProperties builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        InteractiveGuards.EmbedCountInRange(Builders, ensureMaxCapacity: true);
        Builders.Add(builder);
        return this;
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
    public MultiEmbedPageBuilder WithAllowedMentions(AllowedMentionsProperties? allowedMentions)
    {
        AllowedMentions = allowedMentions;
        return this;
    }

    /// <summary>
    /// Sets the message reference of the <see cref="MultiEmbedPage"/>.
    /// </summary>
    /// <param name="messageReference">The message reference.</param>
    /// <returns>The current builder.</returns>
    public MultiEmbedPageBuilder WithMessageReference(MessageReferenceProperties? messageReference)
    {
        MessageReference = messageReference;
        return this;
    }

    /// <summary>
    /// Sets the stickers of the <see cref="MultiEmbedPage"/>.
    /// </summary>
    /// <param name="stickerIds">The stickers.</param>
    /// <returns>The current builder.</returns>
    public MultiEmbedPageBuilder WithStickerIds(IReadOnlyCollection<ulong> stickerIds)
    {
        ArgumentNullException.ThrowIfNull(stickerIds);
        StickerIds = stickerIds;
        return this;
    }

    /// <summary>
    /// Sets the function that generates the attachment.
    /// </summary>
    /// <remarks>To leave the attachment in the message unmodified, <paramref name="attachmentFactory"/> must return <see langword="null"/> instead of a <see cref="AttachmentProperties"/> object.</remarks>
    /// <param name="attachmentFactory">The attachment factory. To leave the attachment in the message unmodified, <paramref name="attachmentFactory"/> must return <see langword="null"/> instead of a <see cref="AttachmentProperties"/> object.</param>
    /// <returns>The current builder.</returns>
    public MultiEmbedPageBuilder WithAttachmentFactory(Func<AttachmentProperties?> attachmentFactory)
    {
        ArgumentNullException.ThrowIfNull(attachmentFactory);
        return WithAttachmentsFactory(() =>
        {
            var attachment = attachmentFactory();
            return new ValueTask<IEnumerable<AttachmentProperties>?>(attachment is null ? null : new[] { attachment });
        });
    }

    /// <summary>
    /// Sets the function that generates the attachment.
    /// </summary>
    /// <remarks>To leave the attachment in the message unmodified, <paramref name="attachmentFactory"/> must return <see langword="null"/> instead of a <see cref="AttachmentProperties"/> object.</remarks>
    /// <param name="attachmentFactory">The attachment factory. To leave the attachment in the message unmodified, <paramref name="attachmentFactory"/> must return <see langword="null"/> instead of a <see cref="AttachmentProperties"/> object.</param>
    /// <returns>The current builder.</returns>
    public MultiEmbedPageBuilder WithAttachmentFactory(Func<ValueTask<AttachmentProperties?>> attachmentFactory)
    {
        ArgumentNullException.ThrowIfNull(attachmentFactory);
        return WithAttachmentsFactory(async () =>
        {
            var attachment = await attachmentFactory().ConfigureAwait(false);
            return attachment is null ? null : new[] { attachment };
        });
    }

    /// <summary>
    /// Sets the function that generates the attachments.
    /// </summary>
    /// <remarks>To leave the attachments in the message unmodified, <paramref name="attachmentsFactory"/> must return <see langword="null"/> instead of a <see cref="AttachmentProperties"/> object.</remarks>
    /// <param name="attachmentsFactory">The attachments factory. To leave the attachments in the message unmodified, <paramref name="attachmentsFactory"/> must return <see langword="null"/> instead of a <see cref="AttachmentProperties"/> object.</param>
    /// <returns>The current builder.</returns>
    public MultiEmbedPageBuilder WithAttachmentsFactory(Func<IEnumerable<AttachmentProperties>?> attachmentsFactory)
    {
        ArgumentNullException.ThrowIfNull(attachmentsFactory);
        return WithAttachmentsFactory(() => new ValueTask<IEnumerable<AttachmentProperties>?>(attachmentsFactory()));
    }

    /// <summary>
    /// Sets the function that generates the attachments.
    /// </summary>
    /// <remarks>To leave the attachments in the message unmodified, <paramref name="attachmentsFactory"/> must return <see langword="null"/> instead of a <see cref="AttachmentProperties"/> object.</remarks>
    /// <param name="attachmentsFactory">The attachments factory. To leave the attachments in the message unmodified, <paramref name="attachmentsFactory"/> must return <see langword="null"/> instead of a <see cref="AttachmentProperties"/> object.</param>
    /// <returns>The current builder.</returns>
    public MultiEmbedPageBuilder WithAttachmentsFactory(Func<ValueTask<IEnumerable<AttachmentProperties>?>>? attachmentsFactory)
    {
        AttachmentsFactory = attachmentsFactory;
        return this;
    }

    /// <summary>
    /// Sets the components of the <see cref="Page"/>.
    /// </summary>
    /// <remarks>The <see cref="Components"/> property is only used on component paginators. Using the new components (components V2) requires not setting <see cref="Text"/>, <see cref="StickerIds"/> or any embed property.</remarks>
    /// <param name="components">The components.</param>
    /// <returns>The current builder.</returns>
    public MultiEmbedPageBuilder WithComponents(List<IMessageComponentProperties>? components)
    {
        Components = components;
        return this;
    }

    /// <summary>
    /// Sets the message flags.
    /// </summary>
    /// <param name="flags">The message flags.</param>
    /// <returns>The current builder.</returns>
    public MultiEmbedPageBuilder WithMessageFlags(MessageFlags? flags)
    {
        MessageFlags = flags;
        return this;
    }

    /// <inheritdoc/>
    IPage IPageBuilder<IPage>.Build() => Build();
}