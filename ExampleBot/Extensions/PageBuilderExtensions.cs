using Fergun.Interactive;

namespace ExampleBot.Extensions;

public static class PageBuilderExtensions
{
    public static PageBuilder WithRandomColor(this PageBuilder builder) => builder.WithColor(Utils.GetRandomColor());
}