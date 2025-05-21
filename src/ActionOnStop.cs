using System;
using Discord;

namespace Fergun.Interactive;

/// <summary>
/// Specifies the actions that will be applied to a message after a timeout or a cancellation.
/// </summary>
[Flags]
public enum ActionOnStop
{
    /// <summary>
    /// Do nothing.
    /// </summary>
    None = 0,

    /// <summary>
    /// Modify the message to contain the timeout or canceled page.
    /// </summary>
    /// <remarks>This action is mutually exclusive with <see cref="DeleteMessage"/>. On component paginators, it's mutually exclusive with all other options.</remarks>
    ModifyMessage = 1 << 0,

    /// <summary>
    /// Delete the reactions/buttons/select menus from the message.
    /// </summary>
    /// <remarks>
    /// This action is mutually exclusive with <see cref="DisableInput"/> and it's not supported on component paginators.<br/>
    /// If reactions are used as input, this requires the <see cref="ChannelPermission.ManageMessages"/> permission.
    /// </remarks>
    DeleteInput = 1 << 1,

    /// <summary>
    /// Disable the buttons or select menus in the message. Only applicable to messages using buttons or select menus.
    /// </summary>
    /// <remarks>This action is mutually exclusive with <see cref="DeleteInput"/>.</remarks>
    DisableInput = 1 << 2,

    /// <summary>
    /// Delete the message.
    /// </summary>
    /// <remarks>
    /// This action takes the highest precedence over any other flag.<br/>
    /// </remarks>
    DeleteMessage = 1 << 3
}