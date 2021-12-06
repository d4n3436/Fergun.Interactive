using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;

namespace ExampleBot.Modules;

public partial class CustomModule
{
    // Simple paginator that uses custom extensions from an extension method
    [Command("extension", RunMode = RunMode.Async)]
    public async Task CustomExtensionAsync()
    {
        var paginator = new LazyPaginatorBuilder()
            .AddUser(Context.User)
            .WithFergunEmotes()
            .Build();

        await Interactive.SendPaginatorAsync(paginator, Context.Channel);
    }
}

public static class BuilderExtensions
{
    public static TBuilder WithFergunEmotes<TPaginator, TBuilder>(this PaginatorBuilder<TPaginator, TBuilder> builder)
        where TPaginator : Paginator
        where TBuilder : PaginatorBuilder<TPaginator, TBuilder>
    {
        builder.Options.Clear();

        builder.AddOption(Emoji.Parse("⏮️"), PaginatorAction.SkipToStart);
        builder.AddOption(Emoji.Parse("◀️"), PaginatorAction.Backward);
        builder.AddOption(Emoji.Parse("▶️"), PaginatorAction.Forward);
        builder.AddOption(Emoji.Parse("⏭️"), PaginatorAction.SkipToEnd);
        builder.AddOption(Emoji.Parse("🛑"), PaginatorAction.Exit);

        return (TBuilder)builder;
    }

    public static TBuilder WithRandomizedOptions<TElement, TOption, TBuilder>(this IInteractiveBuilder<TElement, TOption, TBuilder> builder, IEnumerable<TOption> options)
        where TElement : IInteractiveElement<TOption>
        where TBuilder : IInteractiveBuilder<TElement, TOption, TBuilder>
    {
        builder.Options.Clear();

        var optionsArr = options.ToArray();
        optionsArr.Shuffle();

        return builder.WithOptions(optionsArr);
    }

    private static void Shuffle<T>(this T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = Random.Shared.Next(n--);
            (array[n], array[k]) = (array[k], array[n]);
        }
    }
}