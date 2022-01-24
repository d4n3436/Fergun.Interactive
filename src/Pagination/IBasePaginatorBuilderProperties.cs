using System.Collections.Generic;
using Discord;

namespace Fergun.Interactive.Pagination
{
    /// <summary>
    /// Represents the properties of a <see cref="PaginatorBuilder{TPaginator, TBuilder}"/>.
    /// </summary>
    public interface IBasePaginatorBuilderProperties : IInteractiveBuilderProperties<KeyValuePair<IEmote, PaginatorAction>>
    {
        /// <summary>
        /// Gets a value indicating whether the paginator is restricted to <see cref="IInteractiveBuilderProperties{TOption}.Users"/>.
        /// </summary>
        bool IsUserRestricted { get; }

        /// <summary>
        /// Gets or sets the index of the page the paginator should start.
        /// </summary>
        int StartPageIndex { get; set; }

        /// <summary>
        /// Gets or sets the footer format in the <see cref="Embed"/> of the paginator.
        /// </summary>
        /// <remarks>Setting this to other than <see cref="PaginatorFooter.None"/> will override any other footer in the pages.</remarks>
        PaginatorFooter Footer { get; set; }

        /// <summary>
        /// Gets or sets the emotes and their related actions of the paginator.
        /// </summary>
        new IDictionary<IEmote, PaginatorAction> Options { get; set; }
    }
}