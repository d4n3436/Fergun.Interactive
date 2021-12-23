using System;
using System.Collections.Generic;
using Discord;

namespace Fergun.Interactive.Selection
{
    /// <summary>
    /// Represents a selection of values.
    /// </summary>
    /// <typeparam name="TOption">The type of the options.</typeparam>
    public class Selection<TOption> : BaseSelection<TOption>
    {
        internal Selection(Func<TOption, IEmote>? emoteConverter, Func<TOption, string>? stringConverter,
            IEqualityComparer<TOption> equalityComparer, bool allowCancel, IPage selectionPage,
            IReadOnlyCollection<IUser> users, IReadOnlyCollection<TOption> options, IPage? canceledPage,
            IPage? timeoutPage, IPage? successPage, DeletionOptions deletion, InputType inputType,
            ActionOnStop actionOnCancellation, ActionOnStop actionOnTimeout, ActionOnStop actionOnSuccess)
            : base(emoteConverter, stringConverter, equalityComparer, allowCancel, selectionPage, users, options,
                canceledPage, timeoutPage, successPage, deletion, inputType, actionOnCancellation, actionOnTimeout,
                actionOnSuccess)
        {
        }
    }
}