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
        /// <inheritdoc/>
        public string? Text { get; }

        /// <inheritdoc/>
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