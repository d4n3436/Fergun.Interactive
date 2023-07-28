using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ExampleBot.Extensions;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using GScraper.Google;

namespace ExampleBot.Modules;

[Group("paginator")]
public class PaginatorModule : ModuleBase
{
    private static readonly GoogleScraper _scraper = new();
    private readonly InteractiveService _interactive;

    public PaginatorModule(InteractiveService interactive)
    {
        _interactive = interactive;
    }

    // Sends a message that contains a static paginator with pages that can be changed with reactions or buttons.
    [Command("static", RunMode = RunMode.Async)]
    public async Task PaginatorAsync()
    {
        var pages = new[]
        {
            new PageBuilder().WithDescription("Lorem ipsum dolor sit amet, consectetur adipiscing elit."),
            new PageBuilder().WithDescription("Praesent eu est vitae dui sollicitudin volutpat."),
            new PageBuilder().WithDescription("Etiam in ex sed turpis imperdiet viverra id eget nunc."),
            new PageBuilder().WithDescription("Donec eget feugiat nisi. Praesent faucibus malesuada nulla, a vulputate velit eleifend ut.")
        };

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
            .WithPageFactory(GeneratePage) // The pages are now generated on demand using a local method.
            .WithMaxPageIndex(9) // You must specify the max. index the page factory can go. max. index 9 = 10 pages
            .Build();

        await _interactive.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(10));

        static PageBuilder GeneratePage(int index)
        {
            return new PageBuilder()
                .WithDescription($"This is page {index + 1}.")
                .WithRandomColor();
        }
    }

    // Sends a lazy paginator that displays images and uses more complex buttons.
    [Command("img", RunMode = RunMode.Async)]
    public async Task ImgAsync(string query = "discord")
    {
        // Get images from Google Images.
        var images = (await _scraper.GetImagesAsync(query)).ToList();

        var paginator = new LazyPaginatorBuilder()
            .AddUser(Context.User)
            .WithPageFactory(GeneratePage)
            .WithMaxPageIndex(images.Count - 1) // You must specify the max. index the page factory can go.
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
        {
            return new PageBuilder()
                .WithAuthor(Context.User)
                .WithTitle(images[index].Title)
                .WithUrl(images[index].SourceUrl)
                .WithDescription("Image paginator example")
                .WithImageUrl(images[index].Url)
                .WithRandomColor();
        }
    }
}