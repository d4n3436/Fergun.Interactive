namespace Fergun.Interactive.Selection
{
    /// <summary>
    /// Represents a selection of options.
    /// </summary>
    /// <typeparam name="TOption">The type of the options.</typeparam>
    public class Selection<TOption> : BaseSelection<TOption>
    {
        internal Selection(BaseSelectionBuilderProperties<TOption> builder)
            : base(builder)
        {
        }
    }
}