using NetCord;

namespace Fergun.Interactive.Extensions;

internal static class UserExtensions
{
    extension(User user)
    {
        public string Mention => $"<@{user.Id}>";
    }

    extension(ulong userId)
    {
        public string Mention => $"<@{userId}>";
    }
}