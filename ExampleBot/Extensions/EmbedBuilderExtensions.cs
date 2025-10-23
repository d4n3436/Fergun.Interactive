
using NetCord.Rest;

namespace ExampleBot.Extensions;

public static class EmbedPropertiesExtensions
{
    public static EmbedProperties WithRandomColor(this EmbedProperties builder) => builder.WithColor(Utils.GetRandomColor());
}