using NetCord;

namespace ExampleBot.Extensions;

internal static class EmojiExtensions
{
    public static string GetValue(this EmojiProperties properties)
    {
        return properties.Id is null ? properties.Name! : $"<:{properties.Name}:{properties.Id}>";
    }

    public static string GetValue(this MessageReactionEmoji emoji)
    {
        return emoji.Id is null ? emoji.Name! : $"<:{emoji.Name}:{emoji.Id}>";
    }
}