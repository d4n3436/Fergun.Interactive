using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ExampleBot.Extensions;
using Fergun.Interactive;
using Fergun.Interactive.Extensions;
using Fergun.Interactive.Pagination;
using Fergun.Interactive.Selection;
using GScraper;
using GScraper.Brave;
using GScraper.DuckDuckGo;
using GScraper.Google;

namespace ExampleBot.Modules
{
    public partial class CustomModule
    {
        private static readonly GoogleScraper _googleScraper = new();
        private static readonly DuckDuckGoScraper _ddgScraper = new();
        private static readonly BraveScraper _braveScraper = new();

        // Sends a paginated (paged) selection
        // A paged selection is a selection where each options contains a paginator
        // Here, 3 different image scrapers are used to get images, then their results are grouped in a selection.
        [Command("paginator", RunMode = RunMode.Async)]
        public async Task PagedSelection2Async(string query = "Discord")
        {
            await Context.Channel.TriggerTypingAsync();

            var googleTask = _googleScraper.GetImagesAsync(query);
            var ddgTask = _ddgScraper.GetImagesAsync(query);
            var braveTask = _braveScraper.GetImagesAsync(query);

            try
            {
                await Task.WhenAll(googleTask, ddgTask, braveTask);
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
                return;
            }

            var results = new Dictionary<string, IReadOnlyList<IImageResult>>
            {
                { "Google", googleTask.Result.ToArray() },
                { "DuckDuckGo", ddgTask.Result.ToArray() },
                { "Brave", braveTask.Result.ToArray() }
            };

            var options = results
                .Where(x => x.Value.Count > 0)
                .ToDictionary(x => x.Key, x => new LazyPaginatorBuilder()
                    .WithPageFactory(index => GeneratePage(x.Value, x.Key, index))
                    .WithMaxPageIndex(x.Value.Count - 1)
                    .WithActionOnCancellation(ActionOnStop.DisableInput)
                    .WithActionOnTimeout(ActionOnStop.DisableInput)
                    .WithFooter(PaginatorFooter.None)
                    .AddUser(Context.User)
                    .WithCustomEmotes()
                    .Build() as Paginator);

            if (options.Count == 0)
            {
                await ReplyAsync("No results.");
                return;
            }

            // We have to provide an initial page to the selection, there's no easy way do to this within the selection
            string first = options.First().Key;
            var initialPage = GeneratePage(results[first], first, 0);

            var pagedSelection = new PagedSelectionBuilder<string>()
                .WithOptions(options)
                .AddUser(Context.User)
                .WithSelectionPage(initialPage)
                .WithActionOnTimeout(ActionOnStop.DisableInput)
                .WithActionOnCancellation(ActionOnStop.DisableInput)
                .Build();

            await Interactive.SendSelectionAsync(pagedSelection, Context.Channel, TimeSpan.FromMinutes(10));

            PageBuilder GeneratePage(IReadOnlyList<IImageResult> images, string scraper, int index)
            {
                return new PageBuilder()
                    .WithAuthor(Context.User)
                    .WithTitle(images[index].Title)
                    .WithDescription($"{scraper} Images")
                    .WithImageUrl(images[index].Url)
                    .WithFooter($"Page {index + 1}/{images.Count}")
                    .WithRandomColor();
            }
        }
    }
    public class PagedSelectionBuilder<TOption> : BaseSelectionBuilder<PagedSelection<TOption>, KeyValuePair<TOption, Paginator>, PagedSelectionBuilder<TOption>>
    {
        public PagedSelection<TOption> Build(PageBuilder startPage)
        {
            SelectionPage = startPage;
            return Build();
        }

        /// <inheritdoc />
        public override PagedSelection<TOption> Build()
        {
            base.Options = Options;
            return new PagedSelection<TOption>(this);
        }

        /// <summary>
        /// Gets a dictionary of options and their paginators.
        /// </summary>
        public new IDictionary<TOption, Paginator> Options { get; set; } = new Dictionary<TOption, Paginator>();

        public override Func<KeyValuePair<TOption, Paginator>, string> StringConverter { get; set; } = option => option.Key?.ToString();

        public PagedSelectionBuilder<TOption> WithOptions<TPaginator>(IDictionary<TOption, TPaginator> options) where TPaginator : Paginator
        {
            Options = options as IDictionary<TOption, Paginator> ?? throw new ArgumentNullException(nameof(options));
            return this;
        }

        public PagedSelectionBuilder<TOption> AddOption(TOption option, Paginator paginator)
        {
            Options.Add(option, paginator);
            return this;
        }
    }

    public class PagedSelection<TOption> : BaseSelection<KeyValuePair<TOption, Paginator>>
    {
        /// <inheritdoc />
        public PagedSelection(PagedSelectionBuilder<TOption> builder) : base(builder)
        {
            Options = new ReadOnlyDictionary<TOption, Paginator>(builder.Options);
            CurrentOption = Options.Keys.First();
        }

        /// <summary>
        /// Gets a dictionary of options and their paginators.
        /// </summary>
        public new IReadOnlyDictionary<TOption, Paginator> Options { get; }

        /// <summary>
        /// Gets the current option.
        /// </summary>
        public TOption CurrentOption { get; private set; }

        public override ComponentBuilder GetOrAddComponents(bool disableAll, ComponentBuilder builder = null)
        {
            builder ??= new ComponentBuilder();
            var paginator = Options[CurrentOption];

            // add paginator components to the builder
            paginator.GetOrAddComponents(disableAll, builder);

            // select menu
            var options = new List<SelectMenuOptionBuilder>();

            foreach (var selection in Options)
            {
                var emote = EmoteConverter?.Invoke(selection);
                string label = StringConverter?.Invoke(selection);
                if (emote is null && label is null)
                {
                    throw new InvalidOperationException($"Neither {nameof(EmoteConverter)} nor {nameof(StringConverter)} returned a valid emote or string.");
                }

                var option = new SelectMenuOptionBuilder()
                    .WithLabel(label)
                    .WithEmote(emote)
                    .WithDefault(Equals(selection.Key, CurrentOption))
                    .WithValue(emote?.ToString() ?? label);

                options.Add(option);
            }

            var selectMenu = new SelectMenuBuilder()
                .WithCustomId("foobar")
                .WithOptions(options)
                .WithDisabled(disableAll);

            builder.WithSelectMenu(selectMenu);

            return builder;
        }

        public override async Task<InteractiveInputResult<KeyValuePair<TOption, Paginator>>> HandleInteractionAsync(SocketMessageComponent input, IUserMessage message)
        {
            if (input.Message.Id != message.Id || !this.CanInteract(input.User))
            {
                return InteractiveInputStatus.Ignored;
            }

            string option = input.Data.Values?.FirstOrDefault();

            if (input.Data.Type == ComponentType.SelectMenu && option is not null)
            {
                KeyValuePair<TOption, Paginator> selected = default;
                string selectedString = null;

                foreach (var value in Options)
                {
                    string stringValue = EmoteConverter?.Invoke(value)?.ToString() ?? StringConverter?.Invoke(value);
                    if (option != stringValue) continue;
                    selected = value;
                    selectedString = stringValue;
                    break;
                }

                if (selectedString is null)
                {
                    return InteractiveInputStatus.Ignored;
                }

                CurrentOption = selected.Key;

                bool isCanceled = AllowCancel && (EmoteConverter?.Invoke(CancelOption)?.ToString() ?? StringConverter?.Invoke(CancelOption)) == selectedString;

                if (isCanceled)
                {
                    return new(InteractiveInputStatus.Canceled, selected);
                }
            }

            var paginator = Options[CurrentOption];
            var (emote, action) = paginator.Emotes.FirstOrDefault(x => x.Key.ToString() == input.Data.CustomId);

            if (emote is not null)
            {
                if (action == PaginatorAction.Exit)
                {
                    return InteractiveInputStatus.Canceled;
                }

                await paginator.ApplyActionAsync(action).ConfigureAwait(false);
            }

            var currentPage = await paginator.GetOrLoadCurrentPageAsync().ConfigureAwait(false);

            await input.UpdateAsync(x =>
            {
                x.Content = currentPage.Text ?? "";
                x.Embeds = currentPage.GetEmbedArray();
                x.Components = GetOrAddComponents(false).Build();
            }).ConfigureAwait(false);

            return InteractiveInputStatus.Ignored;
        }
    }
}
