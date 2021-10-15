using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using GScraper.Google;

namespace ExampleBot.Modules
{
    [Group("paginator")]
    public class PaginatorModule : ModuleBase
    {
        private static readonly GoogleScraper _scraper = new();

        public InteractiveService Interactive { get; set; }

        public Random Rng { get; set; }

        // Sends a message that contains a static paginator with pages that can be changed with reactions or buttons.
        [Command(RunMode = RunMode.Async)]
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
            await Interactive.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(10));

            // Most of the time you won't need the result of the paginator so you can safely discard the task:
            // _ = Interactive.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(10));
        }

        // Sends a lazy paginator. The pages are generated using a page factory.
        [Command("lazy", RunMode = RunMode.Async)]
        public async Task LazyPaginatorAsync()
        {
            var paginator = new LazyPaginatorBuilder()
                .AddUser(Context.User)
                .WithPageFactory(GeneratePageAsync) // The pages are now generated on demand using a local method.
                .WithMaxPageIndex(9) // You must specify the max. index the page factory can go. max. index 9 = 10 pages
                .Build();

            await Interactive.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(10));

            Task<PageBuilder> GeneratePageAsync(int index)
            {
                var page = new PageBuilder()
                    .WithDescription($"This is page {index + 1}.")
                    .WithColor(GetRandomColor());

                return Task.FromResult(page);
            }
        }

        // Sends a lazy paginator that displays images and uses more options.
        [Command("img", RunMode = RunMode.Async)]
        public async Task ImgAsync(string query = "discord")
        {
            // Get images from Google Images.
            var images = (await _scraper.GetImagesAsync(query)).ToList();

            // If we can use interactions, prefer disabling the input (buttons, select menus) instead of removing them from the message.
            var actionOnTimeout = Program.CanUseInteractions ? ActionOnStop.DisableInput : ActionOnStop.DeleteInput;

            var paginator = new LazyPaginatorBuilder()
                .AddUser(Context.User)
                .WithPageFactory(GeneratePageAsync)
                .WithMaxPageIndex(images.Count - 1) // You must specify the max. index the page factory can go.
                .AddOption(new Emoji("⏪"), PaginatorAction.SkipToStart) // Use different emojis and option order.
                .AddOption(new Emoji("◀"), PaginatorAction.Backward)
                .AddOption(new Emoji("▶"), PaginatorAction.Forward)
                .AddOption(new Emoji("⏩"), PaginatorAction.SkipToEnd)
                .AddOption(new Emoji("🛑"), PaginatorAction.Exit)
                .WithCacheLoadedPages(false) // The lazy paginator caches generated pages by default but it's possible to disable this.
                .WithActionOnCancellation(ActionOnStop.DeleteMessage) // Delete the message after pressing the stop emoji.
                .WithActionOnTimeout(actionOnTimeout) // Disable/Delete the input after a timeout.
                .WithFooter(PaginatorFooter.None) // Do not override the page footer. This allows us to write our own page footer in the page factory.
                .Build();

            await Interactive.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(10));

            Task<PageBuilder> GeneratePageAsync(int index)
            {
                var page = new PageBuilder()
                    .WithAuthor(Context.User)
                    .WithTitle(images[index].Title)
                    .WithUrl(images[index].SourceUrl)
                    .WithDescription("Image paginator example")
                    .WithImageUrl(images[index].Url)
                    .WithFooter($"Page {index + 1}/{images.Count}")
                    .WithColor(GetRandomColor());

                return Task.FromResult(page);
            }
        }

        private Color GetRandomColor() => new(Rng.Next(0, 255), Rng.Next(0, 255), Rng.Next(0, 255));
    }
}