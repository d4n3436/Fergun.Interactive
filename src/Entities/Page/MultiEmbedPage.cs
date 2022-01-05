﻿using System;
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

        internal MultiEmbedPage(MultiEmbedPageBuilder builder)
        {
            InteractiveGuards.NotNull(builder, nameof(builder));
            InteractiveGuards.NotNull(builder.Builders, nameof(builder.Builders));
            InteractiveGuards.EmbedCountInRange(builder.Builders, nameof(builder.Builders));
            if (string.IsNullOrEmpty(builder.Text) && builder.Builders.Count == 0)
            {
                throw new ArgumentException("At least 1 EmbedBuilder is required when Text is null or empty.", nameof(builder));
            }

            Text = builder.Text;
            Embeds = builder.Builders.Select(x => x.Build()).ToArray();
        }

        /// <summary>
        /// Converts this <see cref="MultiEmbedPage"/> into a <see cref="MultiEmbedPageBuilder"/>.
        /// </summary>
        /// <returns>A <see cref="MultiEmbedPageBuilder"/>.</returns>
        public MultiEmbedPageBuilder ToMultiEmbedPageBuilder() => new MultiEmbedPageBuilder().WithText(Text).WithBuilders(Embeds);
    }
}