using Discord;
using Fergun.Interactive;

namespace ExampleBot.Extensions;

public static class EmbedBuilderExtensions
{
    public static EmbedBuilder WithRandomColor(this EmbedBuilder builder) => builder.WithColor(Utils.GetRandomColor());
}

public static class PageBuilderExtensions
{
    public static PageBuilder WithRandomColor(this PageBuilder builder) => builder.WithColor(Utils.GetRandomColor());
}