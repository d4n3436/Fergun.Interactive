using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Discord;
using Fergun.Interactive.Pagination;
using Fergun.Interactive.Selection;

namespace Fergun.Interactive.Extensions;

internal static class InteractiveExtensions
{
    public static async ValueTask<bool> CurrentUserHasManageMessagesAsync(this IMessageChannel channel)
    {
        if (channel is not ITextChannel textChannel)
            return false;

        var currentUser = await textChannel.Guild.GetCurrentUserAsync(CacheMode.CacheOnly).ConfigureAwait(false);
        return currentUser?.GetPermissions(textChannel).ManageMessages == true;
    }

    public static TimeSpan GetElapsedTime(this PaginatorCallback callback, InteractiveStatus status)
        => status.GetElapsedTime(callback.StartTime, callback.TimeoutTaskSource.Delay);

    public static TimeSpan GetElapsedTime<TOption>(this SelectionCallback<TOption> callback, InteractiveStatus status)
        => status.GetElapsedTime(callback.StartTime, callback.TimeoutTaskSource.Delay);

    public static TimeSpan GetElapsedTime<TInput>(this FilteredCallback<TInput> callback, InteractiveStatus status)
        => status.GetElapsedTime(callback.StartTime, callback.TimeoutTaskSource.Delay);

    public static TimeSpan GetElapsedTime(this DateTimeOffset startTime)
        => DateTimeOffset.UtcNow - startTime;

    // Using just startTime.GetElapsedTime() would return a slightly incorrect elapsed time if the status is Timeout
    public static TimeSpan GetElapsedTime(this InteractiveStatus status, DateTimeOffset startTime, TimeSpan timeoutDelay)
        => status == InteractiveStatus.Timeout ? timeoutDelay : startTime.GetElapsedTime();

    public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        => new ReadOnlyDictionary<TKey, TValue>(dictionary);
}