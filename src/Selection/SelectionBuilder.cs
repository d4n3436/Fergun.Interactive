using JetBrains.Annotations;

namespace Fergun.Interactive.Selection;

/// <summary>
/// Represents a builder class for constructing instances of <see cref="Selection{TOption}"/>.
/// </summary>
/// <typeparam name="TOption">The type of the options the selection will have.</typeparam>
[PublicAPI]
public class SelectionBuilder<TOption> : BaseSelectionBuilder<Selection<TOption>, TOption, SelectionBuilder<TOption>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectionBuilder{TOption}"/> class.
    /// </summary>
    public SelectionBuilder()
    {
    }

    /// <summary>
    /// Builds this <see cref="SelectionBuilder{TOption}"/> into an immutable <see cref="Selection{TOption}"/>.
    /// </summary>
    /// <returns>A <see cref="Selection{TOption}"/>.</returns>
    public override Selection<TOption> Build() => new(this);
}