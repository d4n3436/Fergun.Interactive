using JetBrains.Annotations;

namespace Fergun.Interactive.Selection;

/// <inheritdoc/>
[PublicAPI]
public class Selection<TOption> : BaseSelection<TOption>
{
    internal Selection(IBaseSelectionBuilderProperties<TOption> properties)
        : base(properties)
    {
    }
}