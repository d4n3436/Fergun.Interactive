namespace Fergun.Interactive.Selection;

/// <inheritdoc/>
public class Selection<TOption> : BaseSelection<TOption>
{
    internal Selection(IBaseSelectionBuilderProperties<TOption> properties)
        : base(properties)
    {
    }
}