using Discord;
using Discord.Net;
using Discord.Rest;
using Fergun.Interactive.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents a component-based paginator that can be used to paginate messages with buttons or select menus.
/// </summary>
public class ComponentPaginator : IComponentPaginator
{
    /// <summary>
    /// Returns the prefix used on the custom ID of components owned by <see cref="ComponentPaginator"/>.
    /// </summary>
    public const string IdPrefix = "component_paginator_";

    /// <summary>
    /// Returns the custom ID for the <see cref="PaginatorAction.Forward"/> component.
    /// </summary>
    public const string NextPageId = $"{IdPrefix}next";

    /// <summary>
    /// Represents the custom ID for the <see cref="PaginatorAction.Backward"/> component.
    /// </summary>
    public const string PreviousPageId = $"{IdPrefix}previous";

    /// <summary>
    /// Returns the custom ID for the <see cref="PaginatorAction.SkipToStart"/> component.
    /// </summary>
    public const string FirstPageId = $"{IdPrefix}first";

    /// <summary>
    /// Returns the custom ID for the <see cref="PaginatorAction.SkipToEnd"/> component.
    /// </summary>
    public const string LastPageId = $"{IdPrefix}last";

    /// <summary>
    /// Returns the custom ID for the <see cref="PaginatorAction.Exit"/> component.
    /// </summary>
    public const string StopId = $"{IdPrefix}stop";

    /// <summary>
    /// Returns the custom ID for the <see cref="PaginatorAction.Jump"/> to page component.
    /// </summary>
    public const string JumpId = $"{IdPrefix}jump";

    /// <summary>
    /// Returns the custom ID for the <see cref="PaginatorAction.Jump"/> modal.
    /// </summary>
    public const string JumpModalId = $"{IdPrefix}jump_modal";

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentPaginator"/> class, copying the properties from the specified builder.
    /// </summary>
    /// <param name="builder">The paginator builder.</param>
    /// <exception cref="ArgumentException">Thrown when <see cref="IComponentPaginatorBuilder.ActionOnCancellation"/> or <see cref="IComponentPaginatorBuilder.ActionOnTimeout"/> have invalid values.</exception>
    /// <exception cref="ArgumentNullException">Thrown when a required property is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of a property is outside the valid range.</exception>
    internal ComponentPaginator(IComponentPaginatorBuilder builder)
    {
        InteractiveGuards.NotNull(builder);
        InteractiveGuards.NotNull(builder.PageFactory);

        if (builder.RestrictedInputBehavior == RestrictedInputBehavior.SendMessage)
        {
            InteractiveGuards.NotNull(builder.RestrictedPageFactory);
        }

        InteractiveGuards.ValidActionOnStop(builder.ActionOnCancellation, true);
        InteractiveGuards.ValidActionOnStop(builder.ActionOnTimeout, true);
        InteractiveGuards.LessThan(builder.PageCount, 1);
        InteractiveGuards.ValueInRange(0, builder.PageCount, builder.InitialPageIndex);

        PageCount = builder.PageCount;
        CurrentPageIndex = builder.InitialPageIndex;
        PageFactory = builder.PageFactory;
        UserState = builder.UserState;
        Users = new ReadOnlyCollection<IUser>(builder.Users.ToArray());
        ActionOnCancellation = builder.ActionOnCancellation;
        ActionOnTimeout = builder.ActionOnTimeout;
        RestrictedInputBehavior = builder.RestrictedInputBehavior;
        CanceledPage = builder.CanceledPage;
        TimeoutPage = builder.TimeoutPage;
        JumpModalFactory = builder.JumpModalFactory;
        RestrictedPage = builder.RestrictedPageFactory?.Invoke(Users);
    }

    /// <inheritdoc />
    public int CurrentPageIndex { get; private set; }

    /// <inheritdoc />
    public int PageCount { get; set; }

    /// <inheritdoc />
    public PaginatorStatus Status { get; set; } = PaginatorStatus.Active;

    /// <inheritdoc />
    public Func<IComponentPaginator, ValueTask<IPage>> PageFactory { get; }

    /// <inheritdoc />
    public object? UserState { get; set; }

    /// <inheritdoc />
    public IReadOnlyCollection<IUser> Users { get; }

    /// <inheritdoc />
    public ActionOnStop ActionOnCancellation { get; }

    /// <inheritdoc />
    public ActionOnStop ActionOnTimeout { get; }

    /// <inheritdoc />
    public RestrictedInputBehavior RestrictedInputBehavior { get; }

    /// <inheritdoc />
    public IPage? CanceledPage { get; }

    /// <inheritdoc />
    public IPage? TimeoutPage { get; }

    ///<inheritdoc />
    public Func<IComponentPaginator, ModalBuilder>? JumpModalFactory { get; }

    /// <inheritdoc />
    public IPage? RestrictedPage { get; }

    /// <summary>
    /// Increments the <see cref="CurrentPageIndex"/>, so it points to the next page if possible.
    /// </summary>
    /// <returns><see langword="true"/> if <see cref="CurrentPageIndex"/> was updated; otherwise, <see langword="false"/>. It returns <see langword="false"/> if it's already the last page.</returns>
    public virtual bool NextPage() => SetPage(CurrentPageIndex + 1);

    /// <summary>
    /// Decrements the <see cref="CurrentPageIndex"/>, so it points to the previous page if possible.
    /// </summary>
    /// <returns><see langword="true"/> if <see cref="CurrentPageIndex"/> was updated; otherwise, <see langword="false"/>. It returns <see langword="false"/> if it's already the first page.</returns>
    public virtual bool PreviousPage() => SetPage(CurrentPageIndex - 1);

    /// <summary>
    /// Sets the <see cref="CurrentPageIndex"/> to 0, so it points to the first page if possible.
    /// </summary>
    /// <returns><see langword="true"/> if <see cref="CurrentPageIndex"/> was updated; otherwise, <see langword="false"/>. It returns <see langword="false"/> if it's already the first page.</returns>
    public virtual bool FirstPage() => SetPage(0);

    /// <summary>
    /// Sets the <see cref="CurrentPageIndex"/> to <see cref="PageCount"/> minus 1, so it points to the last page if possible.
    /// </summary>
    /// <returns><see langword="true"/> if <see cref="CurrentPageIndex"/> was updated; otherwise, <see langword="false"/>. It returns <see langword="false"/> if it's already the last page.</returns>
    public virtual bool LastPage() => SetPage(PageCount - 1);

    ///<inheritdoc />
    public virtual bool SetPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < PageCount)
        {
            CurrentPageIndex = pageIndex;
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public virtual bool ShouldDisable(PaginatorAction? action = null)
    {
        bool shouldDisable = action switch
        {
            PaginatorAction.Backward => CurrentPageIndex == 0,
            PaginatorAction.Forward => CurrentPageIndex == PageCount - 1,
            PaginatorAction.SkipToStart => CurrentPageIndex == 0,
            PaginatorAction.SkipToEnd => CurrentPageIndex == PageCount - 1,
            PaginatorAction.Exit => false,
            PaginatorAction.Jump => PageCount == 1,
            _ => false
        };

        return shouldDisable || Status switch
        {
            PaginatorStatus.Canceled => ActionOnCancellation.HasFlag(ActionOnStop.DisableInput),
            PaginatorStatus.TimedOut => ActionOnTimeout.HasFlag(ActionOnStop.DisableInput),
            _ => false
        };
    }

    /// <summary>
    /// Creates a standard <see cref="PaginatorAction.Forward"/> button component.
    /// </summary>
    /// <returns></returns>
    public ButtonComponent CreatePreviousButton()
        => new ButtonBuilder(null, PreviousPageId, ButtonStyle.Primary, null, Emoji.Parse("◀"), ShouldDisable(PaginatorAction.Backward))
            .Build();

    /// <inheritdoc/>
    public virtual async ValueTask<InteractiveInputStatus> HandleInteractionAsync(IComponentInteraction interaction)
    {
        if (!this.CanInteract(interaction.User))
        {
            return RestrictedInputBehavior switch
            {
                RestrictedInputBehavior.Ignore or RestrictedInputBehavior.Auto when RestrictedPage is null => InteractiveInputStatus.Ignored,
                RestrictedInputBehavior.SendMessage or RestrictedInputBehavior.Auto when RestrictedPage is not null => await SendRestrictedPageAsync(interaction).ConfigureAwait(false),
                RestrictedInputBehavior.Defer => await DeferInteractionAsync(interaction).ConfigureAwait(false),
                _ => InteractiveInputStatus.Ignored
            };
        }

        bool pageChanged;

        switch (interaction.Data.CustomId)
        {
            case NextPageId:
                pageChanged = NextPage();
                break;
            case PreviousPageId:
                pageChanged = PreviousPage();
                break;
            case FirstPageId:
                pageChanged = FirstPage();
                break;
            case LastPageId:
                pageChanged = LastPage();
                break;
            case StopId:
                return InteractiveInputStatus.Canceled;
            case JumpId:
                await SendJumpPromptAsync(interaction).ConfigureAwait(false);
                return InteractiveInputStatus.Success;
            default:
                return InteractiveInputStatus.Ignored;
        }

        if (pageChanged)
        {
            await RenderPageAsync(interaction).ConfigureAwait(false);
        }
        else
        {
            await interaction.DeferAsync().ConfigureAwait(false);
        }

        return InteractiveInputStatus.Success;
    }

    /// <inheritdoc/>
    public virtual async Task<IUserMessage> RenderPageAsync(IDiscordInteraction interaction, InteractionResponseType responseType, bool isEphemeral, IPage? page = null)
    {
        page ??= await PageFactory(this).ConfigureAwait(false);
        var attachments = page.AttachmentsFactory is null ? [] : await page.AttachmentsFactory().ConfigureAwait(false);

        switch (responseType)
        {
            case InteractionResponseType.ChannelMessageWithSource:
                await interaction.RespondWithFilesAsync(attachments, page.Text, page.GetEmbedArray(), page.IsTTS,
                    isEphemeral, page.AllowedMentions, page.Components, null, null, null, page.MessageFlags ?? MessageFlags.None).ConfigureAwait(false);

                return await interaction.GetOriginalResponseAsync().ConfigureAwait(false);

            case InteractionResponseType.DeferredChannelMessageWithSource:
                return await interaction.FollowupWithFilesAsync(attachments ?? [],
                    page.Text, page.GetEmbedArray(), page.IsTTS, isEphemeral, page.AllowedMentions, page.Components, flags: page.MessageFlags ?? MessageFlags.None).ConfigureAwait(false);

            case InteractionResponseType.DeferredUpdateMessage:
                return await interaction.ModifyOriginalResponseAsync(UpdateMessage).ConfigureAwait(false);

            case InteractionResponseType.UpdateMessage:
                InteractiveGuards.ValidResponseType(responseType, interaction);
                if (interaction is IComponentInteraction componentInteraction)
                {
                    await componentInteraction.UpdateAsync(UpdateMessage).ConfigureAwait(false);
                    return componentInteraction.Message;
                }

                await ((IModalInteraction)interaction).UpdateAsync(UpdateMessage).ConfigureAwait(false);
                return ((IModalInteraction)interaction).Message;

            default:
                throw new ArgumentException("Invalid interaction response type.", nameof(responseType));
        }

        void UpdateMessage(MessageProperties props)
        {
            props.Content = page.Text;
            props.Embeds = page.GetEmbedArray();
            props.Components = page.Components;
            props.AllowedMentions = page.AllowedMentions;
            props.Attachments = attachments is null ? new Optional<IEnumerable<FileAttachment>>() : new Optional<IEnumerable<FileAttachment>>(attachments);
            props.Flags = page.MessageFlags;
        }
    }

    /// <inheritdoc/>
    public virtual async Task RenderPageAsync(IComponentInteraction interaction, IPage? page = null)
    {
        page ??= await PageFactory(this).ConfigureAwait(false);
        var attachments = page.AttachmentsFactory is null ? [] : await page.AttachmentsFactory().ConfigureAwait(false);

        await interaction.UpdateAsync(x =>
        {
            x.Content = page.Text;
            x.Embeds = page.GetEmbedArray();
            x.Components = page.Components;
            x.AllowedMentions = page.AllowedMentions;
            x.Attachments = attachments is null ? new Optional<IEnumerable<FileAttachment>>() : new Optional<IEnumerable<FileAttachment>>(attachments);
            x.Flags = page.MessageFlags;
        }).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<IUserMessage> RenderPageAsync(IMessageChannel channel, IPage? page = null)
    {
        page ??= await PageFactory(this).ConfigureAwait(false);
        var attachments = page.AttachmentsFactory is null ? [] : await page.AttachmentsFactory().ConfigureAwait(false);

        return await channel.SendFilesAsync(attachments, page.Text, page.IsTTS, null, null, page.AllowedMentions, page.MessageReference,
            page.Components, page.Stickers.ToArray(), page.GetEmbedArray(), page.MessageFlags ?? MessageFlags.None).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task RenderPageAsync(IUserMessage message, IPage? page = null)
    {
        page ??= await PageFactory(this).ConfigureAwait(false);
        var attachments = page.AttachmentsFactory is null ? [] : await page.AttachmentsFactory().ConfigureAwait(false);

        // REST interaction messages need to be special-cased unfortunately
        bool retry = false;
        try
        {
            await (message switch
            {
                RestInteractionMessage im => im.ModifyAsync(UpdateMessage).ConfigureAwait(false),
                RestFollowupMessage fm => fm.ModifyAsync(UpdateMessage).ConfigureAwait(false),
                _ => message.ModifyAsync(UpdateMessage).ConfigureAwait(false)
            });
        }
        catch (HttpException e) when (e.DiscordCode == DiscordErrorCode.InvalidWebhookToken)
        {
            retry = true;
        }

        if (retry)
        {
            await message.ModifyAsync(UpdateMessage).ConfigureAwait(false);
        }

        void UpdateMessage(MessageProperties props)
        {
            props.Content = page.Text;
            props.Embeds = page.GetEmbedArray();
            props.AllowedMentions = page.AllowedMentions;
            props.Attachments = attachments is null ? new Optional<IEnumerable<FileAttachment>>() : new Optional<IEnumerable<FileAttachment>>(attachments);
            props.Components = page.Components;
            props.Flags = page.MessageFlags;
        }
    }

    /// <inheritdoc/>
    public virtual async Task SendJumpPromptAsync(IComponentInteraction interaction)
    {
        var builder = JumpModalFactory?
            .Invoke(this)?
            .WithCustomId(JumpModalId) ?? new ModalBuilder()
            .WithTitle("Enter a page number")
            .AddTextInput($"Page number (1-{PageCount})", "jump_modal_text_input", minLength: 1, maxLength: (int)Math.Floor(Math.Log10(PageCount) + 1), required: true)
            .WithCustomId(JumpModalId);

        await interaction.RespondWithModalAsync(builder.Build()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async ValueTask<InteractiveInputStatus> HandleModalInteractionAsync(IModalInteraction interaction)
    {
        if (!this.CanInteract(interaction.User))
        {
            return RestrictedInputBehavior switch
            {
                RestrictedInputBehavior.Ignore or RestrictedInputBehavior.Auto when RestrictedPage is null => InteractiveInputStatus.Ignored,
                RestrictedInputBehavior.SendMessage or RestrictedInputBehavior.Auto when RestrictedPage is not null => await SendRestrictedPageAsync(interaction).ConfigureAwait(false),
                RestrictedInputBehavior.Defer => await DeferInteractionAsync(interaction).ConfigureAwait(false),
                _ => InteractiveInputStatus.Ignored
            };
        }

        if (interaction.Data.CustomId != JumpModalId)
            return InteractiveInputStatus.Ignored;

        string? rawInput = interaction.Data.Components.FirstOrDefault(x => x.Type == ComponentType.TextInput)?.Value;
        if (rawInput is null || !int.TryParse(rawInput, out int pageNumber) || !SetPage(pageNumber - 1))
        {
            return await DeferInteractionAsync(interaction).ConfigureAwait(false);
        }

        await RenderPageAsync(interaction, InteractionResponseType.UpdateMessage, false).ConfigureAwait(false);
        return InteractiveInputStatus.Success;
    }

    /// <inheritdoc/>
    public virtual async ValueTask ApplyActionOnStopAsync(IUserMessage message, IComponentInteraction? stopInteraction, bool deferInteraction)
    {
        var action = Status switch
        {
            PaginatorStatus.Canceled => ActionOnCancellation,
            PaginatorStatus.TimedOut => ActionOnTimeout,
            _ => ActionOnStop.None
        };

        if (action is ActionOnStop.None or ActionOnStop.DeleteInput)
        {
            if (deferInteraction && stopInteraction is not null)
            {
                await stopInteraction.DeferAsync().ConfigureAwait(false);
            }

            return;
        }

        if (action.HasFlag(ActionOnStop.DeleteMessage))
        {
            try
            {
                if (stopInteraction is not null)
                {
                    await stopInteraction.DeleteOriginalResponseAsync().ConfigureAwait(false);
                }
                else
                {
                    await message.DeleteAsync().ConfigureAwait(false);
                }
            }
            catch (HttpException e) when (e.DiscordCode == DiscordErrorCode.UnknownMessage)
            {
                // Ignored, message was already deleted
            }

            return;
        }

        IPage? page = null;
        if (action.HasFlag(ActionOnStop.ModifyMessage))
        {
            page = Status switch
            {
                PaginatorStatus.Canceled => CanceledPage,
                PaginatorStatus.TimedOut => TimeoutPage,
                _ => null
            };

            if (page is null)
            {
                if (stopInteraction is not null && deferInteraction)
                {
                    await stopInteraction.DeferAsync();
                }

                return;
            }
        }

        if (action.HasFlag(ActionOnStop.ModifyMessage) || action.HasFlag(ActionOnStop.DisableInput))
        {
            try
            {
                if (stopInteraction is not null)
                {
                    await RenderPageAsync(stopInteraction, page).ConfigureAwait(false);
                }
                else
                {
                    await RenderPageAsync(message, page).ConfigureAwait(false);
                }
            }
            catch (HttpException e) when (e.DiscordCode == DiscordErrorCode.UnknownMessage)
            {
                // Ignored, message was already deleted
            }
        }
    }

    private async Task<InteractiveInputStatus> SendRestrictedPageAsync(IDiscordInteraction interaction)
    {
        var page = RestrictedPage ?? throw new InvalidOperationException($"Expected {nameof(RestrictedPage)} to be non-null.");
        var attachments = page.AttachmentsFactory is null ? null : await page.AttachmentsFactory().ConfigureAwait(false);

        await interaction.RespondWithFilesAsync(attachments ?? [], page.Text, page.GetEmbedArray(), page.IsTTS, true, page.AllowedMentions, flags: page.MessageFlags ?? MessageFlags.None).ConfigureAwait(false);
        return InteractiveInputStatus.Success;
    }

    private static async Task<InteractiveInputStatus> DeferInteractionAsync(IDiscordInteraction interaction)
    {
        await interaction.DeferAsync().ConfigureAwait(false);
        return InteractiveInputStatus.Success;
    }
}