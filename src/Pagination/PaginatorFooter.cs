using System;

namespace Fergun.Interactive.Pagination;

/// <summary>
/// Specifies which contents should be displayed in the footer of a paginator using an embed.
/// </summary>
[Flags]
public enum PaginatorFooter
{
    /// <summary>
    /// Do not display anything.
    /// </summary>
    None = 0,

    /// <summary>
    /// Display the current page number and maximum page number in the footer.
    /// </summary>
    PageNumber = 1 << 0,

    /// <summary>
    /// Display the users who can interact with the paginator.
    /// </summary>
    Users = 1 << 1
}