using System;

using JetBrains.Annotations;

namespace Fergun.Interactive;

/// <summary>
/// Specifies the actions that will be applied to a message after a timeout or a cancellation.
/// </summary>
[Flags]
[PublicAPI]
public enum ActionOnStop
{
    /// <summary>
    /// Do nothing.
    /// </summary>
    None = 0,

    /// <summary>
    /// Modify the message to contain the timeout or canceled page.
    /// </summary>
    /// <remarks>This action is mutually exclusive with <see cref="DeleteMessage"/>.<br/>
    /// For component paginators, this option can also be used to force a page render, provided there are no canceled or timed-out pages.</remarks>
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