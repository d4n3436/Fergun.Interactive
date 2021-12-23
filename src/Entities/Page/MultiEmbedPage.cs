using System.Collections.Generic;
using System.Linq;
using Discord;

namespace Fergun.Interactive
{
    /// <summary>
    /// Represents a page with multiple embeds.
    /// </summary>
    public class MultiEmbedPage : IPage
    {
        /// <summary>
        /// Gets the text of this <see cref="MultiEmbedPage"/>.
        /// </summary>
        public string? Text { get; }

        /// <summary>
        /// Gets the embeds of this <see cref="MultiEmbedPage"/>.
        /// </summary>
        public IReadOnlyCollection<Embed> Embeds { get; }

        internal MultiEmbedPage(string? text, ICollection<EmbedBuilder> builders)
        {
            InteractiveGuards.NotNull(builders, nameof(builders));
            InteractiveGuards.EmbedCountInRange(builders, nameof(builders));

            Text = text;
            Embeds = builders.Select(x => x.Build()).ToArray();
        }
    }
}