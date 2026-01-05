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

    public PaginatorModule(InteractiveService interactive, GoogleScraper googleScraper)
    {
        _interactive = interactive;
        _googleScraper = googleScraper;
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

    [SlashCommand("wikipedia", "Sends a component paginator (new paginator type) that allows switching over a set of pages.")]
    public async Task WikipediaAsync()
    {
        string[] description =
        [
            "In computing, just-in-time (JIT) compilation (also dynamic translation or run-time compilations) is compilation (of computer code) during execution of a program (at run time) rather than before execution.",
            "This may consist of source code translation but is more commonly bytecode translation to machine code, which is then executed directly."
        ];
    
        string[] history =
        [
            "The earliest published JIT compiler is generally attributed to work on LISP by John McCarthy in 1960.",
            "Smalltalk (c. 1983) pioneered new aspects of JIT compilations. For example, translation to machine code was done on demand, and the result was cached for later use.",
            "Sun's Self language improved these techniques extensively and was at one point the fastest Smalltalk system in the world, achieving up to half the speed of optimized C but with a fully object-oriented language.",
            """
            Self was abandoned by Sun, but the research went into the Java language. The term "Just-in-time compilation" was borrowed from the manufacturing term "Just in time" and popularized by Java, with James Gosling using the term from 1993.
            Currently JITing is used by most implementations of the Java Virtual Machine, as HotSpot builds on, and extensively uses, this research base.
            """
        ];
    
        string[] design =
        [
            """
            In a bytecode-compiled system, source code is translated to an intermediate representation known as bytecode. Bytecode is not the machine code for any particular computer, and may be portable among computer architectures.
            The bytecode may then be interpreted by, or run on a virtual machine. The JIT compiler reads the bytecodes in many sections (or in full, rarely) and compiles them dynamically into machine code so the program can run faster.
            """,
            """
            By contrast, a traditional interpreted virtual machine will simply interpret the bytecode, generally with much lower performance. Some interpreters even interpret source code, without the step of first compiling to bytecode, with even worse performance.
            Statically-compiled code or native code is compiled prior to deployment. A dynamic compilation environment is one in which the compiler can be used during execution.
            """
        ];
    
        var sections = new Dictionary<string, string[]>
        {
            ["Description"] = description,
            ["History"] = history,
            ["Design"] = design
        };
    
        var state = new WikipediaState(sections, sections.Keys.First());
    
        // ComponentPaginator is a new type of paginator written from scratch with customization and flexibility in mind
        // Now the components are decoupled from the paginator, and the paginator loosely owns the navigation buttons

        var paginator = new ComponentPaginatorBuilder()
            .AddUser(Context.User)
            .WithPageCount(state.Sections[state.CurrentSectionName].Length) // Component paginators have a page count instead of a max. page index
            .WithUserState(state) // Now it's possible to store arbitrary state in the paginator. This is useful for storing data that needs to be retrieved elsewhere
            .WithPageFactory(GeneratePage)
            .WithActionOnCancellation(ActionOnStop.DeleteMessage)
            .WithActionOnTimeout(ActionOnStop.DisableInput)
            .Build();
    
        await _interactive.SendPaginatorAsync(paginator, Context.Interaction, TimeSpan.FromMinutes(10));
    
        IPage GeneratePage(IComponentPaginator p)
        {
            // Create the select menu options from the dictionary keys
            var options = state.Sections.Keys
                .Select(x => new SelectMenuOptionBuilder(x, x, isDefault: x == state.CurrentSectionName))
                .ToList();
    
            var section = state.Sections[state.CurrentSectionName];
        
            var components = new ComponentBuilderV2()
                .WithContainer(new ContainerBuilder()
                    .WithTextDisplay($"## Just-in-time compilation\n{section[p.CurrentPageIndex]}")
                    .WithActionRow(new ActionRowBuilder() // Interactions targeting this select menu will be handled on SelectSectionAsync
                        .WithSelectMenu("paginator-select-section", options, disabled: p.ShouldDisable()))
                    .WithActionRow(new ActionRowBuilder()
                        .AddPreviousButton(p, style: ButtonStyle.Secondary)  // The navigation buttons need to added manually; extension methods were added to make this easier
                        .AddNextButton(p, style: ButtonStyle.Secondary)
                        .AddStopButton(p))
                    .WithSeparator()
                    .WithTextDisplay($"Info by Wikipedia | Page {p.CurrentPageIndex + 1} of {p.PageCount}")
                    .WithAccentColor(Color.Blue))
                .Build();
    
            return new PageBuilder()
                .WithComponents(components) // Using components V2 requires not setting the page text, stickers, or any embed property
                .Build();
        }
    }
    
    // This method handles the select menu from the paginator and stores the new section name in the attached state
    // It also sets the page count, current page index, and renders the page
    [ComponentInteraction("paginator-select-section", ignoreGroupNames: true)]
    public async Task SelectSectionAsync(string sectionName)
    {
        var interaction = (IComponentInteraction)Context.Interaction;
    
        // RenderPageAsync bypasses the user check, so we need to call CanInteract here.
        if (!_interactive.TryGetComponentPaginator(interaction.Message, out var paginator) || !paginator.CanInteract(interaction.User))
        {
            await DeferAsync();
            return;
        }
    
        var state = paginator.GetUserState<WikipediaState>(); // Extension method that gets the user state from the paginator as WikipediaState
    
        state.CurrentSectionName = sectionName;
        paginator.SetPage(0); // Reset the page index to 0
        paginator.PageCount = state.Sections[sectionName].Length; // Set the new page count
    
        await paginator.RenderPageAsync(interaction); // Render the current page of the paginator, this will call the GeneratePage method
    }
    
    public class WikipediaState
    {
        public WikipediaState(Dictionary<string, string[]> sections, string currentSectionName)
        {
            Sections = sections;
            CurrentSectionName = currentSectionName;
        }
    
        public Dictionary<string, string[]> Sections { get; }
    
        public string CurrentSectionName { get; set; }
    }
}