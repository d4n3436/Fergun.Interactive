using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using ExampleBot.Extensions;
using Fergun.Interactive;
using Fergun.Interactive.Extensions;
using Fergun.Interactive.Pagination;
using GScraper;
using GScraper.DuckDuckGo;
using GScraper.Google;
using JetBrains.Annotations;

namespace ExampleBot.Modules;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Group("paginator", "Paginator commands.")]
public class PaginatorModule : InteractionModuleBase
{
    private const string SelectOptionId = "select_option";

    private readonly InteractiveService _interactive;
    private readonly GoogleScraper _googleScraper;
    private readonly DuckDuckGoScraper _duckDuckGoScraper;

    public PaginatorModule(InteractiveService interactive, GoogleScraper googleScraper, DuckDuckGoScraper duckDuckGoScraper)
    {
        _interactive = interactive;
        _googleScraper = googleScraper;
        _duckDuckGoScraper = duckDuckGoScraper;
    }

    [SlashCommand("static", "Sends a message with a static paginator. The paginator has pages that can be changed using buttons.")]
    public async Task StaticAsync()
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

        // Respond to the interaction with the paginator and wait until it times out after 10 minutes.
        await _interactive.SendPaginatorAsync(paginator, Context.Interaction, TimeSpan.FromMinutes(10));

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

    [SlashCommand("lazy", "Sends a lazy-loaded paginator. The pages are generated using a page factory.")]
    public async Task LazyAsync()
    {
        var paginator = new LazyPaginatorBuilder()
            .AddUser(Context.User)
            .WithDefaultRestrictedPage() // Set a page that a user will see if they are not allowed to use the paginator. This is an extension method that provides a default page for convenience.
            .WithPageFactory(GeneratePage) // The pages are now generated on demand using a local method.
            .WithMaxPageIndex(9) // You must specify the max. index the page factory can go. max. index 9 = 10 pages
            .Build();

        await _interactive.SendPaginatorAsync(paginator, Context.Interaction, TimeSpan.FromMinutes(10));
        return;

        static PageBuilder GeneratePage(int index)
            => new PageBuilder()
                .WithDescription($"This is page {index + 1}.")
                .WithRandomColor();
    }

    [SlashCommand("img", "Sends a lazy paginator that displays images and uses more complex buttons.")]
    public async Task ImgAsync([Summary(description: "The search query.")] string query = "discord")
    {
        // Get images from Google Images.
        var images = (await _googleScraper.GetImagesAsync(query)).ToList();

        var paginator = new LazyPaginatorBuilder()
            .AddUser(Context.User)
            .WithPageFactory(GeneratePage)
            .WithMaxPageIndex(images) // Convenience extension method that sets the max. page index based on the number of items in a collection.
            .AddOption(context => // Factory method that creates a disabled blurple button with text "Page x / y"
            new PaginatorButton(PaginatorAction.Backward, emote: null, $"Page {context.CurrentPageIndex + 1} / {context.MaxPageIndex + 1}", ButtonStyle.Primary, isDisabled: true))
            .AddOption(new Emoji("◀"), PaginatorAction.Backward, ButtonStyle.Secondary) // Gray buttons
            .AddOption(new Emoji("❌"), PaginatorAction.Exit, ButtonStyle.Secondary)
            .AddOption(new Emoji("▶"), PaginatorAction.Forward, ButtonStyle.Secondary)
            .AddOption(new Emoji("🔢"), PaginatorAction.Jump, ButtonStyle.Secondary) // Use the jump feature
            .WithCacheLoadedPages(false) // The lazy paginator caches generated pages by default, but it's possible to disable this.
            .WithActionOnCancellation(ActionOnStop.DeleteMessage) // Delete the message after pressing the stop emoji.
            .WithActionOnTimeout(ActionOnStop.DisableInput) // Disable the input (buttons) after a timeout.
            .WithFooter(PaginatorFooter.None) // Do not override the page footer. This allows us to write our own page footer in the page factory.
            .Build();

        await _interactive.SendPaginatorAsync(paginator, Context.Interaction, TimeSpan.FromMinutes(10));
        return;

        PageBuilder GeneratePage(int index)
            => new PageBuilder()
                .WithAuthor(Context.User)
                .WithTitle(images[index].Title)
                .WithUrl(images[index].SourceUrl)
                .WithDescription("Image paginator example")
                .WithImageUrl(images[index].Url)
                .WithRandomColor();
    }

    [SlashCommand("component", "Sends a component paginator (new paginator type) containing images from Google & DuckDuckGo.")]
    public async Task ComponentAsync([Summary(description: "The search query.")] string query = "discord")
    {
        await DeferAsync();

        var googleTask = _googleScraper.GetImagesAsync(query);
        var ddgTask = _duckDuckGoScraper.GetImagesAsync(query);

        try
        {
            await Task.WhenAll(googleTask, ddgTask);
        }
        catch (Exception ex)
        {
            await FollowupAsync(ex.Message);
            return;
        }

        var googleImages = (await googleTask).ToList();
        var ddgImages = (await ddgTask).ToList();

        if (googleImages.Count == 0 || ddgImages.Count == 0)
        {
            await FollowupAsync("No images found.");
            return;
        }

        var info = new PaginatorInfo
        {
            SelectedOption = "Google",
            Options =
            {
                ["Google"] = new PaginatorOption(googleImages),
                ["DuckDuckGo"] = new PaginatorOption(ddgImages)
            }
        };

        // ComponentPaginator is a new type of paginator written from scratch with customization and flexibility in mind
        // Now the components are decoupled from the paginator, and the paginator loosely owns the navigation buttons

        var paginator = new ComponentPaginatorBuilder()
            .AddUser(Context.User)
            .WithPageFactory(GeneratePage)
            .WithUserState(info) // Now it's possible to store arbitrary state in the paginator. This is useful for storing data that needs to be retrieved elsewhere
            .WithActionOnCancellation(ActionOnStop.DeleteMessage)
            .WithActionOnTimeout(ActionOnStop.DisableInput)
            .WithPageCount(info.Options[info.SelectedOption].Images.Count) // Component paginators have a page count instead of a max. page index
            .Build();

        await _interactive.SendPaginatorAsync(paginator, Context.Interaction, TimeSpan.FromMinutes(20), InteractionResponseType.DeferredChannelMessageWithSource);
        return;

        IPage GeneratePage(IComponentPaginator p)
        {
            var selected = info.Options[info.SelectedOption];
            var imageResult = selected.Images[p.CurrentPageIndex];

            // Create the select menu options from the dictionary keys
            var options = info.Options.Keys
                .Select(x => new SelectMenuOptionBuilder(x, x, isDefault: x == info.SelectedOption))
                .ToList();

            var components = new ComponentBuilderV2()
                .WithContainer(new ContainerBuilder()
                    .WithTextDisplay($"### {imageResult.Title}\n{info.SelectedOption} Images")
                    .WithMediaGallery(new MediaGalleryBuilder()
                        .WithItems([new MediaGalleryItemProperties(imageResult.Url)]))
                    .WithTextDisplay($"Page {p.CurrentPageIndex + 1} of {p.PageCount}")
                    .WithSeparator()
                    .WithActionRow(new ActionRowBuilder() // The navigation buttons need to added manually; extension methods were added to make this easier
                        .AddPreviousButton(p, style: ButtonStyle.Secondary)
                        .AddNextButton(p, style: ButtonStyle.Secondary)
                        .AddJumpButton(p, style: ButtonStyle.Secondary)
                        .AddStopButton(p))
                    .WithActionRow(new ActionRowBuilder()
                        .WithSelectMenu(SelectOptionId, options, disabled: p.ShouldDisable())) // Interactions targeting this select menu will be handled on SelectOptionAsync
                    .WithAccentColor(Color.Blue))
                .Build();

            return new PageBuilder()
                .WithComponents(components) // Using components V2 requires not setting the page text, stickers, or any embed property
                .Build();
        }
    }

    [ComponentInteraction(SelectOptionId, ignoreGroupNames: true)]
    public async Task SelectOptionAsync(string option)
    {
        var interaction = (IComponentInteraction)Context.Interaction;

        if (!_interactive.TryGetComponentPaginator(interaction.Message, out var paginator) || !paginator.CanInteract(interaction.User))
        {
            await DeferAsync();
            return;
        }

        var info = paginator.GetUserState<PaginatorInfo>(); // Extension method that gets the user state from the paginator as PaginatorInfo

        info.Options[info.SelectedOption].PageIndex = paginator.CurrentPageIndex; // Save the current page index of the current paginator
        info.SelectedOption = option; // Set the new option

        var selected = info.Options[option];

        // Set the new page count and page index
        paginator.PageCount = selected.Images.Count; 
        paginator.SetPage(selected.PageIndex);

        await paginator.RenderPageAsync(interaction); // Render the current page of the paginator, this will call the GeneratePage method
    }

    private sealed class PaginatorInfo
    {
        public string SelectedOption { get; set; } = string.Empty;

        public Dictionary<string, PaginatorOption> Options { get; } = [];
    }

    private sealed class PaginatorOption
    {
        public PaginatorOption(IReadOnlyList<IImageResult> images)
        {
            Images = images;
        }

        public IReadOnlyList<IImageResult> Images { get; }

        public int PageIndex { get; set; }
    }
}