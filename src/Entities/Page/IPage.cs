using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using JetBrains.Annotations;

namespace Fergun.Interactive;

/// <summary>
/// Represents a page used in an interactive element.
/// </summary>
/// <typeparam name="TEmbed">The type of the embeds.</typeparam>
[PublicAPI]
public interface IPage<out TEmbed> where TEmbed : IEmbed
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
    AllowedMentions? AllowedMentions { get; }

    /// <summary>
    /// Gets the message reference of this page.
    /// </summary>
    MessageReference? MessageReference { get; }

    /// <summary>
    /// Gets the stickers of this page.
    /// </summary>
    IReadOnlyCollection<ISticker> Stickers { get; }

    /// <summary>
    /// Gets the embeds of this page.
    /// </summary>
    IReadOnlyCollection<TEmbed> Embeds { get; }

    /// <summary>
    /// Gets the factory of attachments.
    /// </summary>
    Func<ValueTask<IEnumerable<FileAttachment>?>>? AttachmentsFactory { get; }

    /// <summary>
    /// Gets the message flags.
    /// </summary>
    MessageFlags? MessageFlags { get; }

    /// <summary>
    /// Gets the components of this page.
    /// </summary>
    /// <remarks>This property is only used on component paginators. Using the new components (components V2) requires leaving <see cref="Text"/>, <see cref="Embeds"/> and <see cref="Stickers"/> empty.</remarks>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
    MessageComponent? Components => null;
#else
    MessageComponent? Components { get; }
#endif
}

/// <inheritdoc/>
public interface IPage : IPage<Embed> // Unfortunately we have to use Embed here because we can't send or modify messages using IEmbed.
{
    /// <summary>
    /// Gets the array of <see cref="Embed"/> of this page.
    /// </summary>
    /// <remarks>This is used for sending and modifying messages via Discord.Net, since it requires an array of <see cref="Embed"/>.</remarks>
    /// <returns>An <see cref="Embed"/>[].</returns>
    Embed[] GetEmbedArray();
}