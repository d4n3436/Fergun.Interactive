using System.Threading.Tasks;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace ExampleBot.Modules;

public class SelectMenuModule : ComponentInteractionModule<StringMenuInteractionContext>
{
    private readonly InteractiveService _interactive;
    public const string SelectSectionId = "paginator-select-section";

    public SelectMenuModule(InteractiveService interactive)
    {
        _interactive = interactive;
    }

    // This method handles the select menu from the paginator and stores the new section name in the attached state
    // It also sets the page count, current page index, and renders the page
    [ComponentInteraction(SelectSectionId)]
    public async Task SelectSectionAsync()
    {
        var interaction = Context.Interaction;

        // RenderPageAsync bypasses the user check, so we need to call CanInteract here.
        if (!_interactive.TryGetComponentPaginator(interaction.Message, out var paginator) || !paginator.CanInteract(interaction.User))
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            return;
        }

        var state = paginator.GetUserState<PaginatorModule.WikipediaState>(); // Extension method that gets the user state from the paginator as WikipediaState

        state.CurrentSectionName = Context.SelectedValues[0];
        paginator.SetPage(0); // Reset the page index to 0
        paginator.PageCount = state.Sections[Context.SelectedValues[0]].Length; // Set the new page count

        await paginator.RenderPageAsync(interaction); // Render the current page of the paginator, this will call the GeneratePage method
    }
}