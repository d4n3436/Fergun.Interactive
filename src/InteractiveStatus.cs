namespace Fergun.Interactive
{
    /// <summary>
    /// Specifies the possible status of an <see cref="IInteractiveResult{TStatus}"/> whose status is <see cref="InteractiveStatus"/>.
    /// </summary>
    public enum InteractiveStatus
    {
        /// <summary>
        /// The interactive action status is unknown.
        /// </summary>
        Unknown,
        /// <summary>
        /// The interactive action was successful.
        /// </summary>
        Success,
        /// <summary>
        /// The interactive action timed out.
        /// </summary>
        Timeout,
        /// <summary>
        /// The interactive action was canceled.
        /// </summary>
        Canceled
    }
}