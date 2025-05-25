using Discord;

namespace ExampleBot.Extensions;

public static class EmbedBuilderExtensions
{
    public static EmbedBuilder WithRandomColor(this EmbedBuilder builder) => builder.WithColor(Utils.GetRandomColor());
}