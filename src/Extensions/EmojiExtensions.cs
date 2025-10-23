using NetCord;
using NetCord.Rest;

namespace Fergun.Interactive.Extensions;

internal static class EmojiExtensions
{
    public static EmojiProperties ToEmojiProperties(this MessageReactionEmoji reactionEmoji)
    {
        return reactionEmoji.Id is null
            ? EmojiProperties.Standard(reactionEmoji.Name!)
            : EmojiProperties.Custom(reactionEmoji.Id.Value);
    }

    public static ReactionEmojiProperties ToReactionEmojiProperties(this EmojiProperties properties)
    {
        return properties.Id is null
            ? new ReactionEmojiProperties(properties.Name!)
            : new ReactionEmojiProperties(properties.Name!, properties.Id.Value);
    }

    public static string GetValue(this EmojiProperties properties)
    {
        return properties.Id is null ? properties.Name! : $"<:{properties.Name}:{properties.Id}>";
    }
}