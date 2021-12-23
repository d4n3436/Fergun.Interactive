using System.Collections.Generic;
using Discord;

namespace Fergun.Interactive
{
    /// <summary>
    /// Represents a page used in an interactive element.
    /// </summary>
    public interface IPage
    {
        /// <summary>
        /// Gets the text (content) of this page.
        /// </summary>
        string? Text { get; }

        /// <summary>
        /// Gets the embeds of this page.
        /// </summary>
        IReadOnlyCollection<Embed> Embeds { get; } // Unfortunately we have to use Embed here because we can't send or modify messages using IEmbed.
    }
}