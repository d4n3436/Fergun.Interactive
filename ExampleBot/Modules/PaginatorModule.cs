using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExampleBot.Extensions;
using Fergun.Interactive;
using Fergun.Interactive.Extensions;
using Fergun.Interactive.Pagination;
using GScraper.DuckDuckGo;
using GScraper.Google;
using JetBrains.Annotations;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace ExampleBot.Modules;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class PaginatorModule : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly InteractiveService _interactive;
    private readonly GoogleScraper _googleScraper;
    private readonly DuckDuckGoScraper _duckDuckGoScraper;

    public PaginatorModule(InteractiveService interactive, GoogleScraper googleScraper, DuckDuckGoScraper duckDuckGoScraper)
    {
        _interactive = interactive;
        _googleScraper = googleScraper;
        _duckDuckGoScraper = duckDuckGoScraper;
    }

    [SlashCommand("paginator-static", "Sends a message with a static paginator. The paginator has pages that can be changed using buttons.")]
    public async Task StaticAsync()
    {
        IPageBuilder[] pages =
        [
            new PageBuilder().WithDescription("This is the first page of the static paginator. Use the provided buttons to navigate through the pages.").WithColor(new Color(0x0000FF)),
            new MultiEmbedPageBuilder
            {
                Builders =
                [
                    new EmbedProperties().WithTitle("Customization").WithColor(new(0xF1C40F)),
                    new EmbedProperties().WithDescription("You can customize the paginator with various settings, including user restrictions, timeouts and the ability to have multiple embeds.").WithColor(new(0x979C9F))
                ]
            },
            new PageBuilder().WithDescription($"This paginator is restricted to <@{Context.User.Id}> and will expire in 10 minutes.").WithColor(new(0x2ECC71)),
            new PageBuilder().WithDescription("Once the paginator reaches the last page, the 'Forward' and 'Skip to end' buttons are automatically disabled since there are no further pages to navigate.").WithColor(new(0xE74C3C))
        ];

        var paginator = new StaticPaginatorBuilder()
            .AddUser(Context.User) // Only allow the user that executed the command to interact with the selection.
            .WithPages(pages) // Set the pages the paginator will use. This is the only required component.
            .Build();

        // Respond to the interaction with the paginator and wait until it times out after 10 minutes.
        await _interactive.SendPaginatorAsync(paginator, Context.Interaction, TimeSpan.FromMinutes(10));

        // By default, SendPaginatorAsync sends the paginator and waits for a timeout or a cancellation.
        // If you want the method to return after sending the paginator, you can set the
        // ReturnAfterSendingPaginator option to true in the InteractiveService configuration, InteractiveServiceOptions.

    }

    [SlashCommand("paginator-lazy", "Sends a lazy-loaded paginator. The pages are generated using a page factory.")]
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

    [SlashCommand("paginator-img", "Sends a lazy paginator that displays images and uses more complex buttons.")]
    public async Task ImgAsync([SlashCommandParameter(Description = "The search query.")] string query = "discord")
    {
        // Get images from Google Images.
        var images = (await _googleScraper.GetImagesAsync(query)).ToList();

        var paginator = new LazyPaginatorBuilder()
            .AddUser(Context.User)
            .WithPageFactory(GeneratePage)
            .WithMaxPageIndex(images) // Convenience extension method that sets the max. page index based on the number of items in a collection.
            .AddOption(context => // Factory method that creates a disabled blurple button with text "Page x / y"
            new PaginatorButton(PaginatorAction.Backward, emote: null, $"Page {context.CurrentPageIndex + 1} / {context.MaxPageIndex + 1}", ButtonStyle.Primary, isDisabled: true))
            .AddOption(EmojiProperties.Standard("◀"), PaginatorAction.Backward, ButtonStyle.Secondary) // Gray buttons
            .AddOption(EmojiProperties.Standard("❌"), PaginatorAction.Exit, ButtonStyle.Secondary)
            .AddOption(EmojiProperties.Standard("▶"), PaginatorAction.Forward, ButtonStyle.Secondary)
            .AddOption(EmojiProperties.Standard("🔢"), PaginatorAction.Jump, ButtonStyle.Secondary) // Use the jump feature
            .WithCacheLoadedPages(false) // The lazy paginator caches generated pages by default, but it's possible to disable this.
            .WithActionOnCancellation(ActionOnStop.DeleteMessage) // Delete the message after pressing the stop EmojiProperties.
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

    [SlashCommand("paginator-component", "Sends a paginator that allows switching over a set of pages.")]
    public async Task ComponentAsync()

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
        
        var paginator = new ComponentPaginatorBuilder()
            .WithUsers(Context.User)
            .WithPageCount(state.Sections[state.CurrentSectionName].Length)
            .WithUserState(state) // Attach the state into the paginator so we can retrieve it elsewhere
            .WithPageFactory(GeneratePage)
            .WithActionOnCancellation(ActionOnStop.DeleteMessage)
            .WithActionOnTimeout(ActionOnStop.DisableInput)
            .Build();
        
        await _interactive.SendPaginatorAsync(paginator, Context.Interaction, TimeSpan.FromMinutes(10));
        
        IPage GeneratePage(IComponentPaginator p)
        {
            // Create the select menu options from the dictionary keys
            var options = state.Sections.Keys
                .Select(x => new StringMenuSelectOptionProperties(x, x).WithDefault(x == state.CurrentSectionName))
                .ToList();
        
            var section = state.Sections[state.CurrentSectionName];

            var buttonActionRow = new ActionRowProperties()
                .AddPreviousButton(p, style: ButtonStyle.Secondary)
                .AddNextButton(p, style: ButtonStyle.Secondary)
                .AddStopButton(p);
        
            var components = new ComponentContainerProperties(
            [
                new TextDisplayProperties($"## Just-in-time compilation\n{section[p.CurrentPageIndex]}"),
                new StringMenuProperties("paginator-select-section", options).WithDisabled(p), // Interactions targeting this select menu will be handled on SelectSectionAsync
                buttonActionRow,
                new ComponentSeparatorProperties(),
                new TextDisplayProperties($"Info by Wikipedia | Page {p.CurrentPageIndex + 1} of {p.PageCount}")
             ]).WithAccentColor(new Color(0x0000FF));
        
            return new PageBuilder()
                .WithComponents([components]) // Using components V2 requires not setting the page text, stickers, or any embed property
                .WithMessageFlags(MessageFlags.IsComponentsV2)
                .Build();
        }
    }

    internal class WikipediaState
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