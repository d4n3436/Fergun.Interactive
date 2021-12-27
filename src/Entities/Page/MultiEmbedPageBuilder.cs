using System.Collections.Generic;
using Discord;

namespace Fergun.Interactive
{
    /// <summary>
    /// Represents a builder of pages with multiple embeds.
    /// </summary>
    public class MultiEmbedPageBuilder : IPageBuilder<MultiEmbedPage>, IPageBuilder
    {
        /// <summary>
        /// Gets or sets the text of a <see cref="MultiEmbedPage"/>.
        /// </summary>
        /// <returns>The text of the page.</returns>
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the builders that will be in the page.
        /// </summary>
        /// <returns>The list of builders.</returns>
        public IList<EmbedBuilder> Builders { get; set; } = new List<EmbedBuilder>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiEmbedPageBuilder"/> class.
        /// </summary>
        public MultiEmbedPageBuilder()
        {
        }

        /// <summary>
        /// Builds this builder into an <see cref="MultiEmbedPage"/>.
        /// </summary>
        /// <returns>A <see cref="MultiEmbedPage"/>.</returns>
        public MultiEmbedPage Build() => new(Text, Builders);

        /// <summary>
        /// Sets the text of a <see cref="MultiEmbedPage"/>.
        /// </summary>
        /// <param name="text">The text to be set.</param>
        /// <returns>This builder.</returns>
        public MultiEmbedPageBuilder WithText(string text)
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
            InteractiveGuards.EmbedCountInRange(list, nameof(builders));
            Builders = list;
            return this;
        }

        /// <summary>
        /// Sets the embed builders.
        /// </summary>
        /// <param name="builders">The embed builders.</param>
        /// <returns>This builder.</returns>
        public MultiEmbedPageBuilder WithBuilders(IEnumerable<EmbedBuilder> builders)
        {
            var list = new List<EmbedBuilder>(builders);
            InteractiveGuards.EmbedCountInRange(list, nameof(builders));
            Builders = list;
            return this;
        }

        /// <summary>
        /// Adds an embed builder.
        /// </summary>
        /// <param name="builder">The embed builder.</param>
        /// <returns>This builder.</returns>
        public MultiEmbedPageBuilder AddBuilder(EmbedBuilder builder)
        {
            InteractiveGuards.NotNull(builder, nameof(builder));
            InteractiveGuards.EmbedCountInRange(Builders.Count + 1, nameof(builder));
            Builders.Add(builder);
            return this;
        }

        /// <inheritdoc/>
        IPage IPageBuilder<IPage>.Build() => Build();
    }
}