using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ExampleBot.Extensions;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;

namespace ExampleBot.Modules;

public partial class CustomModule
{
    // Simple paginator that uses extension methods
    [Command("extension", RunMode = RunMode.Async)]
    public async Task CustomExtensionAsync()
    {
        var texts = new[]
        {
            @"Contented get distrusts certainty nay are frankness concealed ham.
On unaffected resolution on considered of.
No thought me husband or colonel forming effects.
End sitting shewing who saw besides son musical adapted.
Contrasted interested eat alteration pianoforte sympathize was.
He families believed if no elegance interest surprise an.",

            @"Sentiments two occasional affronting solicitude traveling and one contrasted.
Fortune day out married parties.
Happiness remainder joy but earnestly for off.
Took sold add play may none him few.
If as increasing contrasted entreaties be.
Now summer who day looked our behind moment coming.
Pain son rose more park way that.",

            @"As it so contrasted oh estimating instrument.
Size like body some one had.
Are conduct viewing boy minutes warrant expense.
Tolerably behaviour may admitting daughters offending her ask own.
Praise effect wishes change way and any wanted.
Lively use looked latter regard had.
Do he it part more last in.
Merits ye if mr narrow points.",

            @"Saw yet kindness too replying whatever marianne.
Old sentiments resolution admiration unaffected its mrs literature.
Behaviour new set existence dashwoods.
It satisfied to mr commanded consisted disposing engrossed.
Tall snug do of till on easy.
Form not calm new fail.",

            @"Detract yet delight written farther his general.
If in so bred at dare rose lose good.
Feel and make two real miss use easy.
Celebrated delightful an especially increasing instrument am.
Indulgence contrasted sufficient to unpleasant in in insensible favorable.
Latter remark hunted enough say man."
            };

        var pages = texts
            .Select(x => new PageBuilder()
                .WithDescription(x)
                .WithRandomColor());

        var paginator = new StaticPaginatorBuilder()
            .WithActionOnCancellation(ActionOnStop.DisableInput)
            .WithActionOnTimeout(ActionOnStop.DisableInput)
            .AddUser(Context.User)
            .WithRandomizedPages(pages)
            .WithCustomEmotes()
            .Build();

        await _interactive.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(10));
    }
}

// Thanks to the fluent builder pattern and recursive generics, we can have extensions methods in any paginator/selection builder
// that implements IInteractiveBuilder or inherits from PaginatorBuilder or BaseSelectionBuilder.
public static class BuilderExtensions
{
    // Extension method that sets our own custom emotes/actions to the paginator builder.
    // This can be very useful if you have a predefined set/order of emotes and want to use them in your paginators using the builder.
    // This works with any paginator builder that inherits from PaginatorBuilder (like StaticPaginatorBuilder and LazyPaginatorBuilder)
    public static TBuilder WithCustomEmotes<TPaginator, TBuilder>(this PaginatorBuilder<TPaginator, TBuilder> builder)
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

    // Extension method that randomizes the specified pages and sets them.
    public static StaticPaginatorBuilder WithRandomizedPages(this StaticPaginatorBuilder builder, IEnumerable<PageBuilder> pages)
    {
        return builder.WithPages(pages.ToList().Shuffle());
    }

    // Extension method that randomizes the specified options and sets them.
    // This works with any type that implements IInteractiveBuilder.
    public static TBuilder WithRandomizedOptions<TElement, TOption, TBuilder>(this IInteractiveBuilder<TElement, TOption, TBuilder> builder, IEnumerable<TOption> options)
        where TElement : IInteractiveElement<TOption>
        where TBuilder : IInteractiveBuilder<TElement, TOption, TBuilder>
    {
        return builder.WithOptions(options.ToList().Shuffle());
    }

    private static IList<T> Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            int k = Random.Shared.Next(n--);
            (list[n], list[k]) = (list[k], list[n]);
        }

        return list;
    }
}