namespace ExpressionSolver;

/// <summary>
/// Represents a variable expression within the expression tree, holding a specific decimal value.
/// </summary>
/// <param name="name">The name of the variable.</param>
/// <param name="value">The initial decimal value of the variable.</param>
public class Variable(string name, decimal value) : IExpression
{
    /// <summary>
    /// Gets the name of the variable.
    /// </summary>
    public string Name => name;

    /// <summary>
    /// Gets or sets the value of the variable.
    /// </summary>
    public decimal Value { get; set; } = value;

    /// <inheritdoc/>
    public decimal Compute() => Value;

    /// <summary>
    /// Returns a string that represents the variable.
    /// </summary>
    /// <returns>The name of the variable.</returns>
    public override string ToString() => Name;
}