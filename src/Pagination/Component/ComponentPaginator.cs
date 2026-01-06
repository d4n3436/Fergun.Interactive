using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NetCord;
using NetCord.Rest;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Represents a component-based paginator. This is a new type of paginator that offers more flexibility than <see cref="Paginator"/> and supports components V2.
/// </summary>
[PublicAPI]
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
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(builder.PageFactory);

        if (builder.RestrictedInputBehavior == RestrictedInputBehavior.SendMessage)
        {
            ArgumentNullException.ThrowIfNull(builder.RestrictedPageFactory);
        }

        InteractiveGuards.ValidActionOnStop(builder.ActionOnCancellation);
        InteractiveGuards.ValidActionOnStop(builder.ActionOnTimeout);
        ArgumentOutOfRangeException.ThrowIfLessThan(builder.PageCount, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(builder.InitialPageIndex, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(builder.InitialPageIndex, builder.PageCount - 1);

        PageCount = builder.PageCount;
        CurrentPageIndex = builder.InitialPageIndex;
        PageFactory = builder.PageFactory;
        UserState = builder.UserState;
        Users = builder.Users.ToArray().AsReadOnly();
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
    public IReadOnlyCollection<User> Users { get; }

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

    /// <inheritdoc />
    public Func<IComponentPaginator, ModalProperties>? JumpModalFactory { get; }

    /// <inheritdoc />
    public Func<IComponentPaginator, IPage>? RestrictedPageFactory { get; }

    /// <inheritdoc />
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
        ArgumentNullException.ThrowIfNull(customId);

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
    public virtual async ValueTask<InteractiveInputStatus> HandleInteractionAsync(MessageComponentInteraction interaction)
    {
        ArgumentNullException.ThrowIfNull(interaction);

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
            await interaction.SendResponseAsync(InteractionCallback.DeferredModifyMessage).ConfigureAwait(false);
        }

        return InteractiveInputStatus.Success;
    }

    /// <inheritdoc/>
    public virtual async Task<RestMessage> RenderPageAsync(Interaction interaction, InteractionCallbackType responseType, bool isEphemeral, IPage? page = null)
    {
        ArgumentNullException.ThrowIfNull(interaction);

        page ??= await PageFactory(this).ConfigureAwait(false);
        var attachments = page.AttachmentsFactory is null ? [] : await page.AttachmentsFactory().ConfigureAwait(false);

        var message = new InteractionMessageProperties
        {
            Content = page.Text,
            Tts = page.IsTTS,
            Embeds = page.Embeds,
            Components = page.Components,
            AllowedMentions = page.AllowedMentions,
            Attachments = attachments ?? [],
            Flags = page.MessageFlags
        };

        var props = InteractionCallback.Message(message);

        switch (responseType)
        {
            case InteractionCallbackType.Message:
                await interaction.SendResponseAsync(props).ConfigureAwait(false);
                return await interaction.GetResponseAsync().ConfigureAwait(false);

            case InteractionCallbackType.DeferredMessage:
                return await interaction.SendFollowupMessageAsync(message).ConfigureAwait(false);

            case InteractionCallbackType.DeferredModifyMessage:
                return await interaction.ModifyResponseAsync(UpdateMessage).ConfigureAwait(false);

            case InteractionCallbackType.ModifyMessage:
                InteractiveGuards.ValidResponseType(responseType, interaction);
                if (interaction is MessageComponentInteraction componentInteraction)
                {
                    await componentInteraction.SendResponseAsync(InteractionCallback.ModifyMessage(UpdateMessage)).ConfigureAwait(false);
                    return componentInteraction.Message;
                }

                await ((ModalInteraction)interaction).SendResponseAsync(InteractionCallback.ModifyMessage(UpdateMessage)).ConfigureAwait(false);
                return await interaction.GetResponseAsync().ConfigureAwait(false);

            default:
                throw new ArgumentException("Unsupported interaction response type.", nameof(responseType));
        }

        void UpdateMessage(MessageOptions options)
        {
            options.Content = page.Text;
            options.Embeds = page.Embeds;
            options.Components = page.Components;
            options.AllowedMentions = page.AllowedMentions;
            options.Attachments = attachments;
            options.Flags = page.MessageFlags;
        }
    }

    /// <inheritdoc/>
    public virtual async Task<RestMessage> RenderPageAsync(TextChannel channel, IPage? page = null)
    {
        ArgumentNullException.ThrowIfNull(channel);

        page ??= await PageFactory(this).ConfigureAwait(false);
        var attachments = page.AttachmentsFactory is null ? [] : await page.AttachmentsFactory().ConfigureAwait(false);

        var message = new MessageProperties
        {
            Content = page.Text,
            Tts = page.IsTTS,
            Embeds = page.Embeds,
            MessageReference = page.MessageReference,
            StickerIds = page.StickerIds,
            Components = page.Components,
            AllowedMentions = page.AllowedMentions,
            Attachments = attachments ?? [],
            Flags = page.MessageFlags
        };

        return await channel.SendMessageAsync(message).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task RenderPageAsync(RestMessage message, IPage? page = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        page ??= await PageFactory(this).ConfigureAwait(false);
        var attachments = page.AttachmentsFactory is null ? [] : await page.AttachmentsFactory().ConfigureAwait(false);

        await message.ModifyAsync(UpdateMessage).ConfigureAwait(false);

        return;

        void UpdateMessage(MessageOptions props)
        {
            props.Content = page.Text;
            props.Embeds = page.Embeds;
            props.AllowedMentions = page.AllowedMentions;
            props.Attachments = attachments;
            props.Components = page.Components;
            props.Flags = page.MessageFlags;
        }
    }

    /// <inheritdoc/>
    public virtual async Task SendJumpPromptAsync(MessageComponentInteraction interaction)
    {
        ArgumentNullException.ThrowIfNull(interaction);

        var builder = JumpModalFactory?
            .Invoke(this)?
            .WithCustomId(JumpModalId);

        if (builder is null)
        {
            var label = new LabelProperties($"Page number (1-{PageCount})", new TextInputProperties("jump_modal_text_input", TextInputStyle.Short)
            {
                MinLength = 1,
                MaxLength = (int)Math.Floor(Math.Log10(PageCount) + 1),
                Required = true
            });

            builder = new ModalProperties(JumpModalId, "Enter a page number")
                .AddComponents(label);
        }

        await interaction.SendResponseAsync(InteractionCallback.Modal(builder)).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async ValueTask<InteractiveInputStatus> HandleModalInteractionAsync(ModalInteraction interaction)
    {
        ArgumentNullException.ThrowIfNull(interaction);

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

        string? rawInput = interaction.Data.Components
            .OfType<Label>()
            .Select(x => x.Component)
            .OfType<TextInput>()
            .FirstOrDefault()?
            .Value;

        if (rawInput is null || !int.TryParse(rawInput, NumberStyles.Integer, CultureInfo.InvariantCulture, out int pageNumber) || !SetPage(Clamp(pageNumber - 1, 0, PageCount - 1)))
        {
            return await DeferInteractionAsync(interaction).ConfigureAwait(false);
        }

        await RenderPageAsync(interaction, InteractionCallbackType.ModifyMessage, isEphemeral: false).ConfigureAwait(false);
        return InteractiveInputStatus.Success;
    }

    /// <inheritdoc/>
    public virtual async ValueTask ApplyActionOnStopAsync(RestMessage message, MessageComponentInteraction? stopInteraction, bool deferInteraction)
    {
        ArgumentNullException.ThrowIfNull(message);

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
                await stopInteraction.SendResponseAsync(InteractionCallback.DeferredModifyMessage).ConfigureAwait(false);
            }

            return;
        }

        if (action.HasFlag(ActionOnStop.DeleteMessage))
        {
            if (!message.Flags.HasFlag(MessageFlags.Ephemeral))
            {
                try
                {
                    await message.DeleteAsync().ConfigureAwait(false);
                }
                catch (RestException e) when (e.Error?.Code == 10008)
                {
                    // Ignored, message was already deleted
                }
            }
            else if (stopInteraction is not null)
            {
                await stopInteraction.SendResponseAsync(InteractionCallback.DeferredModifyMessage).ConfigureAwait(false);
                await stopInteraction.DeleteResponseAsync().ConfigureAwait(false);
            }
            else
            {
                try
                {
                    await message.DeleteAsync();
                }
                catch (RestException e) when (e.Error?.Code == 50027) // Invalid webhook token
                {
                    // Ignored, token expired
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
            catch (RestException e) when (e.Error?.Code == 10008)
            {
                // Ignored, message was already deleted
            }
        }
    }

    private static async Task<InteractiveInputStatus> DeferInteractionAsync(Interaction interaction)
    {
        await interaction.SendResponseAsync(InteractionCallback.DeferredModifyMessage).ConfigureAwait(false);
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

    private async Task<InteractiveInputStatus> SendRestrictedPageAsync(Interaction interaction)
    {
        var page = RestrictedPageFactory?.Invoke(this) ?? throw new InvalidOperationException($"Expected result of {nameof(RestrictedPageFactory)} to be non-null.");
        var attachments = page.AttachmentsFactory is null ? null : await page.AttachmentsFactory().ConfigureAwait(false);

        var message = new InteractionMessageProperties
        {
            Content = page.Text,
            Tts = page.IsTTS,
            Embeds = page.Embeds,
            Components = page.Components,
            AllowedMentions = page.AllowedMentions,
            Attachments = attachments,
            Flags = page.MessageFlags
        };

        await interaction.SendResponseAsync(InteractionCallback.Message(message)).ConfigureAwait(false);

        return InteractiveInputStatus.Success;
    }
}