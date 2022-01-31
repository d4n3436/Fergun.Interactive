using System.Collections.Generic;
using System.Linq;
using Discord;

namespace Fergun.Interactive;

/// <summary>
/// Represents a builder of pages with multiple embeds.
/// </summary>
public class MultiEmbedPageBuilder : IPageBuilder<MultiEmbedPage>, IPageBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiEmbedPageBuilder"/> class.
    /// </summary>
    public MultiEmbedPageBuilder()
    {
    }

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
    public MultiEmbedPageBuilder WithBuilders(params EmbedBuilder[] builders)
    {
        var list = new List<EmbedBuilder>(builders);
        InteractiveGuards.EmbedCountInRange(list);
        Builders = list;
        return this;
    }

    /// <summary>
    /// Sets the embed builders.
    /// </summary>
    /// <param name="embeds">The embeds.</param>
    /// <returns>This builder.</returns>
    public MultiEmbedPageBuilder WithBuilders(params Embed[] embeds)
    {
        InteractiveGuards.NotNull(embeds);
        return WithBuilders(embeds.AsEnumerable());
    }

    /// <summary>
    /// Sets the embed builders.
    /// </summary>
    /// <param name="builders">The embed builders.</param>
    /// <returns>This builder.</returns>
    public MultiEmbedPageBuilder WithBuilders(IEnumerable<EmbedBuilder> builders)
    {
        var list = new List<EmbedBuilder>(builders);
        InteractiveGuards.EmbedCountInRange(list);
        Builders = list;
        return this;
    }

    /// <summary>
    /// Sets the embed builders.
    /// </summary>
    /// <param name="embeds">The embeds.</param>
    /// <returns>This builder.</returns>
    public MultiEmbedPageBuilder WithBuilders(IEnumerable<Embed> embeds)
    {
        InteractiveGuards.NotNull(embeds);
        return WithBuilders(embeds.Select(x => x.ToEmbedBuilder()));
    }

    /// <summary>
    /// Adds an embed builder.
    /// </summary>
    /// <param name="builder">The embed builder.</param>
    /// <returns>This builder.</returns>
    public MultiEmbedPageBuilder AddBuilder(EmbedBuilder builder)
    {
        InteractiveGuards.NotNull(builder);
        InteractiveGuards.EmbedCountInRange(Builders, true);
        Builders.Add(builder);
        return this;
    }

    /// <summary>
    /// Adds an embed builder from an <see cref="Embed"/>.
    /// </summary>
    /// <param name="embed">The embed.</param>
    /// <returns>This builder.</returns>
    public MultiEmbedPageBuilder AddBuilder(Embed embed)
    {
        InteractiveGuards.NotNull(embed);
        return AddBuilder(embed.ToEmbedBuilder());
    }

    /// <inheritdoc/>
    IPage IPageBuilder<IPage>.Build() => Build();
}