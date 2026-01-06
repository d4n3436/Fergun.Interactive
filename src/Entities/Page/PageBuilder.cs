using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fergun.Interactive.Pagination;
using JetBrains.Annotations;
using NetCord;
using NetCord.Rest;

namespace Fergun.Interactive;

/// <summary>
/// Represents a <see cref="Page"/> builder.
/// </summary>
[PublicAPI]
public class PageBuilder : IPageBuilder<Page>, IPageBuilder
{
    private readonly EmbedProperties _builder;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageBuilder"/> class.
    /// </summary>
    public PageBuilder()
    {
        _builder = new EmbedProperties();
    }

    internal PageBuilder(EmbedProperties? builder)
    {
        _builder = builder ?? new EmbedProperties();
    }

    internal PageBuilder(Page page)
        : this()
    {
        Text = page.Text;
        IsTTS = page.IsTTS;
        AllowedMentions = page.AllowedMentions;
        MessageReference = page.MessageReference;
        StickerIds = page.StickerIds;
        AttachmentsFactory = page.AttachmentsFactory;
        Components = page.Components;
        MessageFlags = page.MessageFlags;
    }

    /// <summary>
    /// Gets or sets the text of the <see cref="Page"/>.
    /// </summary>
    /// <returns>The text of the page.</returns>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets a value that determines whether the text of the <see cref="Page"/> should be read aloud by Discord.
    /// </summary>
    public bool IsTTS { get; set; }

    /// <summary>
    /// Gets or sets the allowed mentions of the <see cref="Page"/>.
    /// </summary>
    public AllowedMentionsProperties? AllowedMentions { get; set; }

    /// <summary>
    /// Gets or sets the message reference of the <see cref="Page"/>.
    /// </summary>
    public MessageReferenceProperties? MessageReference { get; set; }

    /// <summary>
    /// Gets or sets the stickers of the <see cref="Page"/>.
    /// </summary>
    public IReadOnlyCollection<ulong> StickerIds { get; set; } = [];

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

#nullable disable
    /// <summary>
    /// Gets or sets the title of the <see cref="Page"/>.
    /// </summary>
    /// <returns>The title of the page.</returns>
    public string Title
    {
        get => _builder.Title;
        set => _builder.Title = value;
    }

    /// <summary>
    /// Gets or sets the description of the <see cref="Page"/>.
    /// </summary>
    /// <returns>The description of the page.</returns>
    public string Description
    {
        get => _builder.Description;
        set => _builder.Description = value;
    }

    /// <summary>
    /// Gets or sets the URL of the <see cref="Page"/>.
    /// </summary>
    /// <returns>The URL of the page.</returns>
    public string Url
    {
        get => _builder.Url;
        set => _builder.Url = value;
    }

    /// <summary>
    /// Gets or sets the thumbnail of the <see cref="Page"/>.
    /// </summary>
    /// <returns>The thumbnail of the page.</returns>
    public EmbedThumbnailProperties Thumbnail
    {
        get => _builder.Thumbnail;
        set => _builder.Thumbnail = value;
    }

    /// <summary>
    /// Gets or sets the image of the <see cref="Page"/>.
    /// </summary>
    /// <returns>The image of the page.</returns>
    public EmbedImageProperties ImageUrl
    {
        get => _builder.Image;
        set => _builder.Image = value;
    }

    /// <summary>
    /// Gets or sets the list of <see cref="PageBuilder"/> of the <see cref="Page"/>.
    /// </summary>
    /// <returns>The list of existing <see cref="EmbedFieldProperties"/>.</returns>
    public IEnumerable<EmbedFieldProperties> Fields
    {
        get => _builder.Fields;
        set => _builder.Fields = value;
    }

    /// <summary>
    /// Gets or sets the timestamp of the <see cref="Page"/>.
    /// </summary>
    /// <returns>The timestamp of the page, or <see langword="null"/> if none is set.</returns>
    public DateTimeOffset? Timestamp
    {
        get => _builder.Timestamp;
        set => _builder.Timestamp = value;
    }

    /// <summary>
    /// Gets or sets the sidebar color of the <see cref="Page"/>.
    /// </summary>
    /// <returns>The color of the page, or <see langword="null"/> if none is set.</returns>
    public Color Color
    {
        get => _builder.Color;
        set => _builder.Color = value;
    }

    /// <summary>
    /// Gets or sets the <see cref="EmbedAuthorProperties"/> of the <see cref="Page"/>.
    /// </summary>
    /// <returns>The author field builder of the page, or <see langword="null"/> if none is set.</returns>
    public EmbedAuthorProperties Author
    {
        get => _builder.Author;
        set => _builder.Author = value;
    }

    /// <summary>
    /// Gets or sets the <see cref="EmbedFooterProperties"/> of the <see cref="Page"/>.
    /// </summary>
    /// <returns>The footer field builder of the page, or <see langword="null"/> if none is set.</returns>
    public EmbedFooterProperties Footer
    {
        get => _builder.Footer;
        set => _builder.Footer = value;
    }
#nullable restore

    /// <summary>
    /// Creates a new <see cref="PageBuilder"/> from an <see cref="EmbedProperties"/>.
    /// </summary>
    /// <param name="builder">The <see cref="EmbedProperties"/>.</param>
    /// <returns>A <see cref="PageBuilder"/>.</returns>
    public static PageBuilder FromEmbedProperties(EmbedProperties builder)
        => new(builder);

    /// <summary>
    /// Builds this builder to an immutable <see cref="Page"/>.
    /// </summary>
    /// <returns>A <see cref="Page"/>.</returns>
    public Page Build()
        => new(this);

    /// <summary>
    /// Gets the inner <see cref="EmbedProperties"/> used by this builder.
    /// </summary>
    /// <returns>The inner <see cref="EmbedProperties"/>.</returns>
    public EmbedProperties GetEmbedProperties() => _builder;

    /// <summary>
    /// Sets the text of the <see cref="Page"/>.
    /// </summary>
    /// <param name="text">The text to be set.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithText(string text)
    {
        Text = text;
        return this;
    }

    /// <summary>
    /// Sets the title of the <see cref="Page"/>.
    /// </summary>
    /// <param name="title">The title to be set.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithTitle(string title)
    {
        _builder.WithTitle(title);
        return this;
    }

    /// <summary>
    /// Sets the description of the <see cref="Page"/>.
    /// </summary>
    /// <param name="description"> The description to be set.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithDescription(string description)
    {
        _builder.WithDescription(description);
        return this;
    }

    /// <summary>
    /// Sets the URL of the <see cref="Page"/>.
    /// </summary>
    /// <param name="url"> The URL to be set.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithUrl(string url)
    {
        _builder.WithUrl(url);
        return this;
    }

    /// <summary>
    /// Sets the thumbnail URL of the <see cref="Page"/>.
    /// </summary>
    /// <param name="thumbnailUrl"> The thumbnail URL to be set.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithThumbnailUrl(string thumbnailUrl)
    {
        _builder.WithThumbnail(thumbnailUrl);
        return this;
    }

    /// <summary>
    /// Sets the thumbnail of the <see cref="Page"/>.
    /// </summary>
    /// <param name="thumbnail"> The thumbnail to be set.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithThumbnailUrl(EmbedThumbnailProperties thumbnail)
    {
        _builder.WithThumbnail(thumbnail);
        return this;
    }

    /// <summary>
    /// Sets the image URL of the <see cref="Page"/>.
    /// </summary>
    /// <param name="imageUrl">The image URL to be set.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithImageUrl(string imageUrl)
    {
        _builder.WithImage(imageUrl);
        return this;
    }

    
    /// <summary>
    /// Sets the image of the <see cref="Page"/>.
    /// </summary>
    /// <param name="image">The image to be set.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithImage(EmbedImageProperties image)
    {
        _builder.WithImage(image);
        return this;
    }

    /// <summary>
    /// Sets the timestamp of the <see cref="Page"/> to the current time.
    /// </summary>
    /// <returns>The current builder.</returns>
    public PageBuilder WithCurrentTimestamp()
    {
        _builder.WithTimestamp(DateTimeOffset.UtcNow);
        return this;
    }

    /// <summary>
    /// Sets the timestamp of the <see cref="Page"/>.
    /// </summary>
    /// <param name="dateTimeOffset">The timestamp to be set.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithTimestamp(DateTimeOffset? dateTimeOffset)
    {
        _builder.Timestamp = dateTimeOffset;
        return this;
    }

    /// <summary>
    /// Sets the sidebar color of the <see cref="Page"/>.
    /// </summary>
    /// <param name="color">The color to be set.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithColor(Color color)
    {
        _builder.WithColor(color);
        return this;
    }

    /// <summary>
    /// Sets the <see cref="EmbedAuthorProperties"/> of the <see cref="Page"/>.
    /// </summary>
    /// <param name="author">The author builder class containing the author field properties.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithAuthor(EmbedAuthorProperties author)
    {
        _builder.WithAuthor(author);
        return this;
    }

    /// <summary>
    /// Sets the author field of the <see cref="Page"/> with the provided properties.
    /// </summary>
    /// <param name="action">The delegate containing the author field properties.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithAuthor(Action<EmbedAuthorProperties> action)
    {
        var author = new EmbedAuthorProperties();
        action(author);

        _builder.WithAuthor(author);
        return this;
    }

    /// <summary>
    /// Sets the author field of the <see cref="Page"/> with the provided name, icon URL, and URL.
    /// </summary>
    /// <param name="name">The title of the author field.</param>
    /// <param name="iconUrl">The icon URL of the author field.</param>
    /// <param name="url">The URL of the author field.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithAuthor(string name, string? iconUrl = null, string? url = null)
    {
        ArgumentNullException.ThrowIfNull(name);
        var author = new EmbedAuthorProperties()
            .WithName(name)
            .WithIconUrl(iconUrl)
            .WithUrl(url);

        _builder.WithAuthor(author);
        return this;
    }

    /// <summary>
    /// Fills the page author field with the provided user's full username and avatar URL.
    /// </summary>
    /// <param name="user">The user to put into the author.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithAuthor(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return WithAuthor(user.Username, (user.GetAvatarUrl() ?? user.DefaultAvatarUrl).ToString());
    }

    /// <summary>
    /// Sets the <see cref="EmbedFooterProperties"/> of the <see cref="Page"/>.
    /// </summary>
    /// <param name="footer">The footer builder class containing the footer field properties.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithFooter(EmbedFooterProperties footer)
    {
        _builder.WithFooter(footer);
        return this;
    }

    /// <summary>
    /// Sets the footer field of the <see cref="Page"/> with the provided properties.
    /// </summary>
    /// <param name="action">The delegate containing the footer field properties.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithFooter(Action<EmbedFooterProperties> action)
    {
        var footer = new EmbedFooterProperties();
        action(footer);

        _builder.WithFooter(footer);
        return this;
    }

    /// <summary>Sets the footer field of the <see cref="Page"/> with the provided name, icon URL.</summary>
    /// <param name="text">The title of the footer field.</param>
    /// <param name="iconUrl">The icon URL of the footer field.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithFooter(string text, string? iconUrl = null)
    {
        var footer = new EmbedFooterProperties()
            .WithText(text)
            .WithIconUrl(iconUrl);

        _builder.WithFooter(footer);
        return this;
    }

    /// <summary>
    /// Sets the fields of the <see cref="Page"/>.
    /// </summary>
    /// <param name="fields">The fields.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithFields(params EmbedFieldProperties[] fields)
    {
        _builder.WithFields(fields);
        return this;
    }

    /// <summary>
    /// Sets the fields of the <see cref="Page"/>.
    /// </summary>
    /// <param name="fields">The fields.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithFields(IEnumerable<EmbedFieldProperties> fields)
    {
        _builder.WithFields(fields);
        return this;
    }

    /// <summary>
    /// Adds a <see cref="Page"/> field with the provided name and value.
    /// </summary>
    /// <param name="name">The title of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <param name="inline">Indicates whether the field is in-line or not.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder AddField(string name, object value, bool inline = false)
    {
        var field = new EmbedFieldProperties()
            .WithName(name)
            .WithValue(value.ToString()!)
            .WithInline(inline);

        _builder.AddFields(field);
        return this;
    }

    /// <summary>
    /// Adds a field with the provided <see cref="PageBuilder"/> to a <see cref="Page"/>.
    /// </summary>
    /// <param name="field">The field builder class containing the field properties.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder AddField(EmbedFieldProperties field)
    {
        _builder.AddFields(field);
        return this;
    }

    /// <summary>
    /// Adds a <see cref="Page"/> field with the provided properties.
    /// </summary>
    /// <param name="action">The delegate containing the field properties.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder AddField(Action<EmbedFieldProperties> action)
    {
        var field = new EmbedFieldProperties();
        action(field);

        _builder.AddFields(field);
        return this;
    }

    /// <summary>
    /// Sets the <see cref="IsTTS"/> value of the <see cref="Page"/>.
    /// </summary>
    /// <param name="isTTS">Whether the text of the page should be read aloud by Discord.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithIsTTS(bool isTTS)
    {
        IsTTS = isTTS;
        return this;
    }

    /// <summary>
    /// Sets the allowed mentions the <see cref="Page"/>.
    /// </summary>
    /// <param name="allowedMentions">The allowed mentions.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithAllowedMentions(AllowedMentionsProperties? allowedMentions)
    {
        AllowedMentions = allowedMentions;
        return this;
    }

    /// <summary>
    /// Sets the message reference of the <see cref="Page"/>.
    /// </summary>
    /// <param name="messageReference">The message reference.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithMessageReference(MessageReferenceProperties? messageReference)
    {
        MessageReference = messageReference;
        return this;
    }

    /// <summary>
    /// Sets the stickers of the <see cref="Page"/>.
    /// </summary>
    /// <param name="stickerIds">The stickers.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithStickerIds(IReadOnlyCollection<ulong> stickerIds)
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
    public PageBuilder WithAttachmentFactory(Func<AttachmentProperties?> attachmentFactory)
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
    public PageBuilder WithAttachmentFactory(Func<ValueTask<AttachmentProperties?>> attachmentFactory)
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
    public PageBuilder WithAttachmentsFactory(Func<IEnumerable<AttachmentProperties>?> attachmentsFactory)
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
    public PageBuilder WithAttachmentsFactory(Func<ValueTask<IEnumerable<AttachmentProperties>?>>? attachmentsFactory)
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
    public PageBuilder WithComponents(List<IMessageComponentProperties>? components)
    {
        Components = components;
        return this;
    }

    /// <summary>
    /// Sets the message flags.
    /// </summary>
    /// <param name="flags">The message flags.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithMessageFlags(MessageFlags? flags)
    {
        MessageFlags = flags;
        return this;
    }

    /// <inheritdoc/>
    IPage IPageBuilder<IPage>.Build() => Build();

    internal PageBuilder WithPaginatorFooter(PaginatorFooter footer, int currentPageIndex, int maxPageIndex, ICollection<User>? users)
    {
        if (footer == PaginatorFooter.None)
            return this;

        Footer = new EmbedFooterProperties();

        if (footer.HasFlag(PaginatorFooter.Users))
        {
            if (users is null || users.Count == 0)
            {
                Footer.Text += "Interactors: Everyone";
            }
            else if (users.Count == 1)
            {
                var user = users.Single();

                Footer.Text += $"Interactor: {user.Username}";
                Footer.IconUrl =  ((user as GuildUser)?.GetGuildAvatarUrl() ?? user.GetAvatarUrl() ?? user.DefaultAvatarUrl).ToString();
            }
            else
            {
                Footer.Text += $"Interactors: {string.Join(", ", users.Select(x => x.Username))}";
            }

            Footer.Text += '\n';
        }

        if (footer.HasFlag(PaginatorFooter.PageNumber))
        {
            Footer.Text += $"Page {currentPageIndex + 1}/{maxPageIndex + 1}";
        }

        return this;
    }
}