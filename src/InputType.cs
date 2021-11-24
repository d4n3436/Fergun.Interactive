using System;

namespace Fergun.Interactive
{
    /// <summary>
    /// Specifies the types of inputs that are used to interact with the interactive elements.
    /// </summary>
    [Flags]
    public enum InputType
    {
        /// <summary>
        /// Use reactions as input.
        /// </summary>
        Reactions = 1 << 0,
        /// <summary>
        /// Use messages as input.
        /// </summary>
        Messages = 1 << 1,
        /// <summary>
        /// Use buttons as input.
        /// </summary>
        Buttons = 1 << 2,
        /// <summary>
        /// Use select menus as input.
        /// </summary>
        SelectMenus = 1 << 3
    }
}