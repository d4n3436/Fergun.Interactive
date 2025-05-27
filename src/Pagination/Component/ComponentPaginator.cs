using Discord;
using Discord.Net;
using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents a component-based paginator. This is a new type of paginator that offers more flexibility than <see cref="Paginator"/> and supports components V2.
/// </summary>
public class ComponentPaginator : IComponentPaginator
{
    private const string IdPrefix = "component_paginator_";

    private const string NextPageId = $"{IdPrefix}next";

    private const string PreviousPageId = $"{IdPrefix}previous";

    private const string FirstPageId = $"{IdPrefix}first";

    private const string LastPageId = $"{IdPrefix}last";

    private const string StopId = $"{IdPrefix}stop";

    private const string JumpId = $"{IdPrefix}jump";

    private const string JumpModalId = $"{IdPrefix}jump_modal";

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentPaginator"/> class, copying the properties from the specified builder.
    /// </summary>
    /// <param name="builder">The paginator builder.</param>
    /// <exception cref="ArgumentException">Thrown when <see cref="IComponentPaginatorBuilder.ActionOnCancellation"/> or <see cref="IComponentPaginatorBuilder.ActionOnTimeout"/> have invalid values.</exception>
    /// <exception cref="ArgumentNullException">Thrown when a required property is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value of a property is outside the valid range.</exception>
    protected internal ComponentPaginator(IComponentPaginatorBuilder builder)
    {
        InteractiveGuards.NotNull(builder);
        InteractiveGuards.NotNull(builder.PageFactory);

        if (builder.RestrictedInputBehavior == RestrictedInputBehavior.SendMessage)
        {
            InteractiveGuards.NotNull(builder.RestrictedPageFactory);
        }

        InteractiveGuards.ValidActionOnStop(builder.ActionOnCancellation);
        InteractiveGuards.ValidActionOnStop(builder.ActionOnTimeout);
        InteractiveGuards.LessThan(builder.PageCount, 1);
        InteractiveGuards.ValueInRange(0, builder.PageCount - 1, builder.InitialPageIndex);

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
        RestrictedPageFactory = builder.RestrictedPageFactory;
    }

    /// <inheritdoc />
    public int CurrentPageIndex { get; private set; }

    /// <inheritdoc />
    public int PageCount
    {
        get;
        set => field = value >= 1 ? value : throw new ArgumentOutOfRangeException(nameof(value), "Page count must be at least 1.");
    }

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
    public Func<IComponentPaginator, IPage>? RestrictedPageFactory { get; }

    ///<inheritdoc />
    public virtual bool SetPage(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= PageCount) return false;
        CurrentPageIndex = pageIndex;
        return true;
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

    /// <inheritdoc/>
    public virtual bool OwnsComponent(string customId)
    {
        InteractiveGuards.NotNull(customId);

        return customId is NextPageId or PreviousPageId or FirstPageId or LastPageId or StopId or JumpId or JumpModalId;
    }

    /// <inheritdoc/>
    public virtual string GetCustomId(PaginatorAction action)
        => action switch
        {
            PaginatorAction.Backward => PreviousPageId,
            PaginatorAction.Forward => NextPageId,
            PaginatorAction.SkipToStart => FirstPageId,
            PaginatorAction.SkipToEnd => LastPageId,
            PaginatorAction.Exit => StopId,
            PaginatorAction.Jump => JumpId,
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        };

    /// <inheritdoc/>
    public virtual async ValueTask<InteractiveInputStatus> HandleInteractionAsync(IComponentInteraction interaction)
    {
        InteractiveGuards.NotNull(interaction);

        if (!this.CanInteract(interaction.User))
        {
            return RestrictedInputBehavior switch
            {
                RestrictedInputBehavior.Ignore or RestrictedInputBehavior.Auto when RestrictedPageFactory is null => InteractiveInputStatus.Ignored,
                RestrictedInputBehavior.SendMessage or RestrictedInputBehavior.Auto when RestrictedPageFactory is not null => await SendRestrictedPageAsync(interaction).ConfigureAwait(false),
                RestrictedInputBehavior.Defer => await DeferInteractionAsync(interaction).ConfigureAwait(false),
                _ => InteractiveInputStatus.Ignored
            };
        }

        bool pageChanged;
        switch (interaction.Data.CustomId)
        {
            case NextPageId:
                pageChanged = this.NextPage();
                break;
            case PreviousPageId:
                pageChanged = this.PreviousPage();
                break;
            case FirstPageId:
                pageChanged = this.FirstPage();
                break;
            case LastPageId:
                pageChanged = this.LastPage();
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
            await this.RenderPageAsync(interaction).ConfigureAwait(false);
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
        InteractiveGuards.NotNull(interaction);

        page ??= await PageFactory(this).ConfigureAwait(false);
        var attachments = page.AttachmentsFactory is null ? [] : await page.AttachmentsFactory().ConfigureAwait(false);

        switch (responseType)
        {
            case InteractionResponseType.ChannelMessageWithSource:
                await interaction.RespondWithFilesAsync(attachments, page.Text, page.GetEmbedArray(), page.IsTTS,
                    isEphemeral, page.AllowedMentions, page.Components, embed: null, options: null, poll: null, page.MessageFlags ?? MessageFlags.None).ConfigureAwait(false);

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
                throw new ArgumentException("Unsupported interaction response type.", nameof(responseType));
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
    public virtual async Task<IUserMessage> RenderPageAsync(IMessageChannel channel, IPage? page = null)
    {
        InteractiveGuards.NotNull(channel);

        page ??= await PageFactory(this).ConfigureAwait(false);
        var attachments = page.AttachmentsFactory is null ? [] : await page.AttachmentsFactory().ConfigureAwait(false);

        return await channel.SendFilesAsync(attachments, page.Text, page.IsTTS, embed: null, options: null, page.AllowedMentions, page.MessageReference,
            page.Components, page.Stickers.ToArray(), page.GetEmbedArray(), page.MessageFlags ?? MessageFlags.None).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task RenderPageAsync(IUserMessage message, IPage? page = null)
    {
        InteractiveGuards.NotNull(message);

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
        InteractiveGuards.NotNull(interaction);

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
        InteractiveGuards.NotNull(interaction);

        if (!this.CanInteract(interaction.User))
        {
            return RestrictedInputBehavior switch
            {
                RestrictedInputBehavior.Ignore or RestrictedInputBehavior.Auto when RestrictedPageFactory is null => InteractiveInputStatus.Ignored,
                RestrictedInputBehavior.SendMessage or RestrictedInputBehavior.Auto when RestrictedPageFactory is not null => await SendRestrictedPageAsync(interaction).ConfigureAwait(false),
                RestrictedInputBehavior.Defer => await DeferInteractionAsync(interaction).ConfigureAwait(false),
                _ => InteractiveInputStatus.Ignored
            };
        }

        if (interaction.Data.CustomId != JumpModalId)
            return InteractiveInputStatus.Ignored;

        string? rawInput = interaction.Data.Components.FirstOrDefault(x => x.Type == ComponentType.TextInput)?.Value;
        if (rawInput is null || !int.TryParse(rawInput, NumberStyles.Integer, CultureInfo.InvariantCulture, out int pageNumber) || !SetPage(Clamp(pageNumber - 1, 0, PageCount - 1)))
        {
            return await DeferInteractionAsync(interaction).ConfigureAwait(false);
        }

        await RenderPageAsync(interaction, InteractionResponseType.UpdateMessage, isEphemeral: false).ConfigureAwait(false);
        return InteractiveInputStatus.Success;
    }

    /// <inheritdoc/>
    public virtual async ValueTask ApplyActionOnStopAsync(IUserMessage message, IComponentInteraction? stopInteraction, bool deferInteraction)
    {
        InteractiveGuards.NotNull(message);

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
            if (message.Flags?.HasFlag(MessageFlags.Ephemeral) != true)
            {
                try
                {
                    await message.DeleteAsync().ConfigureAwait(false);
                }
                catch (HttpException e) when (e.DiscordCode == DiscordErrorCode.UnknownMessage)
                {
                    // Ignored, message was already deleted
                }
            }
            else
            {
                if (stopInteraction is not null)
                {
                    await stopInteraction.DeferAsync().ConfigureAwait(false);
                    await stopInteraction.DeleteOriginalResponseAsync().ConfigureAwait(false);
                }
                else
                {
                    try
                    {
                        await (message switch
                        {
                            RestInteractionMessage im => im.DeleteAsync().ConfigureAwait(false),
                            RestFollowupMessage fm => fm.DeleteAsync().ConfigureAwait(false),
                            _ => Task.CompletedTask.ConfigureAwait(false)
                        });
                    }
                    catch (HttpException e) when (e.DiscordCode == DiscordErrorCode.InvalidWebhookToken)
                    {
                        // Ignored, token expired
                    }
                }
            }
            
            return;
        }

        var page = !action.HasFlag(ActionOnStop.ModifyMessage) ? null : Status switch
        {
            PaginatorStatus.Canceled => CanceledPage,
            PaginatorStatus.TimedOut => TimeoutPage,
            _ => null
        };

        if (action.HasFlag(ActionOnStop.ModifyMessage) || action.HasFlag(ActionOnStop.DisableInput))
        {
            try
            {
                if (stopInteraction is not null)
                {
                    await this.RenderPageAsync(stopInteraction, page).ConfigureAwait(false);
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
        var page = RestrictedPageFactory?.Invoke(this) ?? throw new InvalidOperationException($"Expected result of {nameof(RestrictedPageFactory)} to be non-null.");
        var attachments = page.AttachmentsFactory is null ? null : await page.AttachmentsFactory().ConfigureAwait(false);

        await interaction.RespondWithFilesAsync(attachments ?? [], page.Text, page.GetEmbedArray(), page.IsTTS,
            ephemeral: true, page.AllowedMentions, flags: page.MessageFlags ?? MessageFlags.None).ConfigureAwait(false);

        return InteractiveInputStatus.Success;
    }

    private static async Task<InteractiveInputStatus> DeferInteractionAsync(IDiscordInteraction interaction)
    {
        await interaction.DeferAsync().ConfigureAwait(false);
        return InteractiveInputStatus.Success;
    }

    private static int Clamp(int value, int min, int max)
    {
        if (min > max)
        {
            throw new ArgumentException($"'{min}' cannot be greater than {max}.");
        }

        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }
}