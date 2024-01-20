using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Fergun.Interactive.Pagination;

namespace Fergun.Interactive;

/// <summary>
/// Represents a <see cref="Page"/> builder.
/// </summary>
public class PageBuilder : IPageBuilder<Page>, IPageBuilder
{
    private readonly EmbedBuilder _builder;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageBuilder"/> class.
    /// </summary>
    public PageBuilder()
    {
        _builder = new EmbedBuilder();
    }

    internal PageBuilder(EmbedBuilder? builder)
    {
        _builder = builder ?? new EmbedBuilder();
    }

    internal PageBuilder(Page page)
        : this(page.Embed?.ToEmbedBuilder())
    {
        Text = page.Text;
        IsTTS = page.IsTTS;
        AllowedMentions = page.AllowedMentions;
        MessageReference = page.MessageReference;
        Stickers = page.Stickers;
        AttachmentsFactory = page.AttachmentsFactory;
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
    public AllowedMentions? AllowedMentions { get; set; }

    /// <summary>
    /// Gets or sets the message reference of the <see cref="Page"/>.
    /// </summary>
    public MessageReference? MessageReference { get; set; }

    /// <summary>
    /// Gets or sets the stickers of the <see cref="Page"/>.
    /// </summary>
    public IReadOnlyCollection<ISticker> Stickers { get; set; } = Array.Empty<ISticker>();

    /// <summary>
    /// Gets or sets the factory of attachments.
    /// </summary>
    public Func<ValueTask<IEnumerable<FileAttachment>?>>? AttachmentsFactory { get; set; }

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
    /// Gets or sets the thumbnail URL of the <see cref="Page"/>.
    /// </summary>
    /// <returns>The thumbnail URL of the page.</returns>
    public string ThumbnailUrl
    {
        get => _builder.ThumbnailUrl;
        set => _builder.ThumbnailUrl = value;
    }

    /// <summary>
    /// Gets or sets the image URL of the <see cref="Page"/>.
    /// </summary>
    /// <returns>The image URL of the page.</returns>
    public string ImageUrl
    {
        get => _builder.ImageUrl;
        set => _builder.ImageUrl = value;
    }

    /// <summary>
    /// Gets or sets the list of <see cref="PageBuilder"/> of the <see cref="Page"/>.
    /// </summary>
    /// <returns>The list of existing <see cref="EmbedFieldBuilder"/>.</returns>
    public List<EmbedFieldBuilder> Fields
    {
        get => _builder.Fields;
        set => _builder.Fields = value;
    }

    /// <summary>
    /// Gets or sets the timestamp of the <see cref="Page"/>.
    /// </summary>
    /// <returns>The timestamp of the page, or <c>null</c> if none is set.</returns>
    public DateTimeOffset? Timestamp
    {
        get => _builder.Timestamp;
        set => _builder.Timestamp = value;
    }

    /// <summary>
    /// Gets or sets the sidebar color of the <see cref="Page"/>.
    /// </summary>
    /// <returns>The color of the page, or <c>null</c> if none is set.</returns>
    public Color? Color
    {
        get => _builder.Color;
        set => _builder.Color = value;
    }

    /// <summary>
    /// Gets or sets the <see cref="EmbedAuthorBuilder"/> of the <see cref="Page"/>.
    /// </summary>
    /// <returns>The author field builder of the page, or <c>null</c> if none is set.</returns>
    public EmbedAuthorBuilder Author
    {
        get => _builder.Author;
        set => _builder.Author = value;
    }

    /// <summary>
    /// Gets or sets the <see cref="EmbedFooterBuilder"/> of the <see cref="Page"/>.
    /// </summary>
    /// <returns>The footer field builder of the page, or <c>null</c> if none is set.</returns>
    public EmbedFooterBuilder Footer
    {
        get => _builder.Footer;
        set => _builder.Footer = value;
    }

    /// <summary>
    /// Gets the total length of all embed properties.
    /// </summary>
    /// <returns>
    /// The combined length of <see cref="Title"/>, <see cref="EmbedAuthor.Name"/>,
    /// <see cref="Description"/>, <see cref="EmbedFooter.Text"/>, <see cref="EmbedField.Name"/>, and <see cref="EmbedField.Value"/>.
    /// </returns>
    public int Length => _builder.Length;

    /// <summary>
    /// Creates a new <see cref="PageBuilder"/> from an <see cref="IEmbed"/>.
    /// </summary>
    /// <param name="embed">The <see cref="IEmbed"/>.</param>
    /// <returns>A <see cref="PageBuilder"/>.</returns>
    public static PageBuilder FromEmbed(IEmbed embed)
        => FromEmbedBuilder(embed.ToEmbedBuilder());

    /// <summary>
    /// Creates a new <see cref="PageBuilder"/> from an <see cref="EmbedBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="EmbedBuilder"/>.</param>
    /// <returns>A <see cref="PageBuilder"/>.</returns>
    public static PageBuilder FromEmbedBuilder(EmbedBuilder builder)
        => new(builder);

    /// <summary>
    /// Builds this builder to an immutable <see cref="Page"/>.
    /// </summary>
    /// <returns>A <see cref="Page"/>.</returns>
    public Page Build()
        => new(this);

    /// <summary>
    /// Gets the inner <see cref="EmbedBuilder"/> used by this builder.
    /// </summary>
    /// <returns>The inner <see cref="EmbedBuilder"/>.</returns>
    public EmbedBuilder GetEmbedBuilder() => _builder;

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
        _builder.WithThumbnailUrl(thumbnailUrl);
        return this;
    }

    /// <summary>
    /// Sets the image URL of the <see cref="Page"/>.
    /// </summary>
    /// <param name="imageUrl">The image URL to be set.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithImageUrl(string imageUrl)
    {
        _builder.WithImageUrl(imageUrl);
        return this;
    }

    /// <summary>
    /// Sets the timestamp of the <see cref="Page"/> to the current time.
    /// </summary>
    /// <returns>The current builder.</returns>
    public PageBuilder WithCurrentTimestamp()
    {
        _builder.WithCurrentTimestamp();
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
    /// Sets the <see cref="EmbedAuthorBuilder"/> of the <see cref="Page"/>.
    /// </summary>
    /// <param name="author">The author builder class containing the author field properties.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithAuthor(EmbedAuthorBuilder author)
    {
        _builder.WithAuthor(author);
        return this;
    }

    /// <summary>
    /// Sets the author field of the <see cref="Page"/> with the provided properties.
    /// </summary>
    /// <param name="action">The delegate containing the author field properties.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithAuthor(Action<EmbedAuthorBuilder> action)
    {
        _builder.WithAuthor(action);
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
        _builder.WithAuthor(name, iconUrl, url);
        return this;
    }

    /// <summary>
    /// Fills the page author field with the provided user's full username and avatar URL.
    /// </summary>
    /// <param name="user">The user to put into the author.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithAuthor(IUser user)
    {
        InteractiveGuards.NotNull(user);
        return WithAuthor(user.ToString(), user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
    }

    /// <summary>
    /// Sets the <see cref="EmbedFooterBuilder"/> of the <see cref="Page"/>.
    /// </summary>
    /// <param name="footer">The footer builder class containing the footer field properties.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithFooter(EmbedFooterBuilder footer)
    {
        _builder.WithFooter(footer);
        return this;
    }

    /// <summary>
    /// Sets the footer field of the <see cref="Page"/> with the provided properties.
    /// </summary>
    /// <param name="action">The delegate containing the footer field properties.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithFooter(Action<EmbedFooterBuilder> action)
    {
        _builder.WithFooter(action);
        return this;
    }

    /// <summary>Sets the footer field of the <see cref="Page"/> with the provided name, icon URL.</summary>
    /// <param name="text">The title of the footer field.</param>
    /// <param name="iconUrl">The icon URL of the footer field.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithFooter(string text, string? iconUrl = null)
    {
        _builder.WithFooter(text, iconUrl);
        return this;
    }

    /// <summary>
    /// Sets the fields of the <see cref="Page"/>.
    /// </summary>
    /// <param name="fields">The fields.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithFields(params EmbedFieldBuilder[] fields)
    {
        _builder.WithFields(fields);
        return this;
    }

    /// <summary>
    /// Sets the fields of the <see cref="Page"/>.
    /// </summary>
    /// <param name="fields">The fields.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithFields(IEnumerable<EmbedFieldBuilder> fields)
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
        _builder.AddField(name, value, inline);
        return this;
    }

    /// <summary>
    /// Adds a field with the provided <see cref="PageBuilder"/> to a <see cref="Page"/>.
    /// </summary>
    /// <param name="field">The field builder class containing the field properties.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder AddField(EmbedFieldBuilder field)
    {
        _builder.AddField(field);
        return this;
    }

    /// <summary>
    /// Adds a <see cref="Page"/> field with the provided properties.
    /// </summary>
    /// <param name="action">The delegate containing the field properties.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder AddField(Action<EmbedFieldBuilder> action)
    {
        _builder.AddField(action);
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
    public PageBuilder WithAllowedMentions(AllowedMentions? allowedMentions)
    {
        AllowedMentions = allowedMentions;
        return this;
    }

    /// <summary>
    /// Sets the message reference of the <see cref="Page"/>.
    /// </summary>
    /// <param name="messageReference">The message reference.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithMessageReference(MessageReference? messageReference)
    {
        MessageReference = messageReference;
        return this;
    }

    /// <summary>
    /// Sets the stickers of the <see cref="Page"/>.
    /// </summary>
    /// <param name="stickers">The stickers.</param>
    /// <returns>The current builder.</returns>
    public PageBuilder WithStickers(IReadOnlyCollection<ISticker> stickers)
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
    public PageBuilder WithAttachmentFactory(Func<FileAttachment?> attachmentFactory)
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
    public PageBuilder WithAttachmentFactory(Func<ValueTask<FileAttachment?>> attachmentFactory)
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
    public PageBuilder WithAttachmentsFactory(Func<IEnumerable<FileAttachment>?> attachmentsFactory)
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
    public PageBuilder WithAttachmentsFactory(Func<ValueTask<IEnumerable<FileAttachment>?>>? attachmentsFactory)
    {
        AttachmentsFactory = attachmentsFactory;
        return this;
    }

    /// <inheritdoc/>
    IPage IPageBuilder<IPage>.Build() => Build();

    internal PageBuilder WithPaginatorFooter(PaginatorFooter footer, int currentPageIndex, int maxPageIndex, ICollection<IUser>? users)
    {
        if (footer == PaginatorFooter.None)
            return this;

        Footer = new EmbedFooterBuilder();

        if (footer.HasFlag(PaginatorFooter.Users))
        {
            if (users is null || users.Count == 0)
            {
                Footer.Text += "Interactors: Everyone";
            }
            else if (users.Count == 1)
            {
                var user = users.Single();

                Footer.Text += $"Interactor: {user}";
                Footer.IconUrl = user.GetDisplayAvatarUrl();
            }
            else
            {
                Footer.Text += $"Interactors: {string.Join(", ", users)}";
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