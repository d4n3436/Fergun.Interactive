using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NetCord;
using NetCord.Rest;

namespace Fergun.Interactive;

/// <summary>
/// Represents a page used in an interactive element.
/// </summary>
[PublicAPI]
public interface IPage
{
    /// <summary>
    /// Gets the text (content) of this page.
    /// </summary>
    string? Text { get; }

    /// <summary>
    /// Gets a value indicating whether the text of this page should be read aloud by Discord.
    /// </summary>
    bool IsTTS { get; }

    /// <summary>
    /// Gets the allowed mentions of this page.
    /// </summary>
    AllowedMentionsProperties? AllowedMentions { get; }

    /// <summary>
    /// Gets the message reference of this page.
    /// </summary>
    MessageReferenceProperties? MessageReference { get; }

    /// <summary>
    /// Gets the stickers of this page.
    /// </summary>
    IReadOnlyCollection<ulong> StickerIds { get; }

    /// <summary>
    /// Gets the embeds of this page.
    /// </summary>
    IReadOnlyCollection<EmbedProperties> Embeds { get; }

    /// <summary>
    /// Gets the factory of attachments.
    /// </summary>
    Func<ValueTask<IEnumerable<AttachmentProperties>?>>? AttachmentsFactory { get; }

    /// <summary>
    /// Gets the message flags.
    /// </summary>
    MessageFlags? MessageFlags { get; }

    /// <summary>
    /// Gets the components of this page.
    /// </summary>
    /// <remarks>This property is only used on component paginators. Using the new components (components V2) requires leaving <see cref="Text"/>, <see cref="Embeds"/> and <see cref="StickerIds"/> empty.</remarks>
    List<IMessageComponentProperties>? Components { get; }
}