﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ExampleBot.Extensions;
using Fergun.Interactive;
using Fergun.Interactive.Extensions;
using Fergun.Interactive.Pagination;
using GScraper.Google;

namespace ExampleBot.Modules;

[Group("paginator")]
public class PaginatorModule : ModuleBase
{
    private static readonly GoogleScraper Scraper = new();
    private readonly InteractiveService _interactive;

    public PaginatorModule(InteractiveService interactive)
    {
        _interactive = interactive;
    }

    // Sends a message that contains a static paginator with pages that can be changed with reactions or buttons.
    [Command("static", RunMode = RunMode.Async)]
    public async Task PaginatorAsync()
    {
        IPageBuilder[] pages =
        [
            new PageBuilder().WithDescription("This is the first page of the static paginator. Use the provided buttons to navigate through the pages.").WithColor(Color.Blue),
            new MultiEmbedPageBuilder
            {
                Builders =
                [
                    new EmbedBuilder().WithTitle("Customization").WithColor(Color.Gold),
                    new EmbedBuilder().WithDescription("You can customize the paginator with various settings, including user restrictions, timeouts and the ability to have multiple embeds.").WithColor(Color.LightGrey)
                ]
            },
            new PageBuilder().WithDescription($"This paginator is restricted to {Context.User.Mention} and will expire in 10 minutes.").WithColor(Color.Green),
            new PageBuilder().WithDescription("Once the paginator reaches the last page, the 'Forward' and 'Skip to end' buttons are automatically disabled since there are no further pages to navigate.").WithColor(Color.Red)
        ];

        var paginator = new StaticPaginatorBuilder()
            .AddUser(Context.User) // Only allow the user that executed the command to interact with the selection.
            .WithPages(pages) // Set the pages the paginator will use. This is the only required component.
            .Build();

        // Send the paginator to the source channel and wait until it times out after 10 minutes.
        await _interactive.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(10));

        // By default, SendPaginatorAsync sends the paginator and waits for a timeout or a cancellation.
        // If you want the method to return after sending the paginator, you can set the
        // ReturnAfterSendingPaginator option to true in the InteractiveService configuration, InteractiveConfig.

        // Example in ServiceCollection:
        /*
        var collection = new ServiceCollection()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(new InteractiveConfig { ReturnAfterSendingPaginator = true })
            .AddSingleton<InteractiveService>()
            ...
        */
    }

    // Sends a lazy paginator. The pages are generated using a page factory.
    [Command("lazy", RunMode = RunMode.Async)]
    public async Task LazyPaginatorAsync()
    {
        var paginator = new LazyPaginatorBuilder()
            .AddUser(Context.User)
            .WithDefaultRestrictedPage() // Set a page that a user will see if they are not allowed to use the paginator. This is an extension method that provides a default page for convenience.
            .WithPageFactory(GeneratePage) // The pages are now generated on demand using a local method.
            .WithMaxPageIndex(9) // You must specify the max. index the page factory can go. max. index 9 = 10 pages
            .Build();

        await _interactive.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(10));

        static PageBuilder GeneratePage(int index)
            => new PageBuilder()
                .WithDescription($"This is page {index + 1}.")
                .WithRandomColor();
    }

    // Sends a lazy paginator that displays images and uses more complex buttons.
    [Command("img", RunMode = RunMode.Async)]
    public async Task ImgAsync(string query = "discord")
    {
        // Get images from Google Images.
        var images = (await Scraper.GetImagesAsync(query)).ToList();

        var paginator = new LazyPaginatorBuilder()
            .AddUser(Context.User)
            .WithPageFactory(GeneratePage)
            .WithMaxPageIndex(images) // Convenience extension method that sets the max. page index based on the number of items in a collection.
            .AddOption(context =>
            {
                // Factory method that creates a disabled blurple button with text "Page x / y"
                return new PaginatorButton(PaginatorAction.Backward, null,
                    $"Page {context.CurrentPageIndex + 1} / {context.MaxPageIndex + 1}", ButtonStyle.Primary, true);
            })
            .AddOption(new Emoji("◀"), PaginatorAction.Backward, ButtonStyle.Secondary) // Gray buttons
            .AddOption(new Emoji("❌"), PaginatorAction.Exit, ButtonStyle.Secondary)
            .AddOption(new Emoji("▶"), PaginatorAction.Forward, ButtonStyle.Secondary)
            .AddOption(new Emoji("🔢"), PaginatorAction.Jump, ButtonStyle.Secondary) // Use the jump feature
            .WithCacheLoadedPages(false) // The lazy paginator caches generated pages by default but it's possible to disable this.
            .WithActionOnCancellation(ActionOnStop.DeleteMessage) // Delete the message after pressing the stop emoji.
            .WithActionOnTimeout(ActionOnStop.DisableInput) // Disable the input (buttons) after a timeout.
            .WithFooter(PaginatorFooter.None) // Do not override the page footer. This allows us to write our own page footer in the page factory.
            .Build();

        await _interactive.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(10));

        PageBuilder GeneratePage(int index)
            => new PageBuilder()
                .WithAuthor(Context.User)
                .WithTitle(images[index].Title)
                .WithUrl(images[index].SourceUrl)
                .WithDescription("Image paginator example")
                .WithImageUrl(images[index].Url)
                .WithRandomColor();
    }
}