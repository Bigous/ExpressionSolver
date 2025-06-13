using System;
using System.Globalization;

namespace ExpressionSolver;

/// <summary>
/// Represents a constant value in the expression tree.
/// This class encapsulates a constant and its associated value, participating in the expression computations.
/// </summary>
public class Constant(string name, decimal Value) : IExpression
{
    /// <summary>
    /// Gets the name of the constant.
    /// </summary>
    public string Name { get; } = name;

    /// <inheritdoc/>
    public decimal Compute() => Value;

    /// <summary>
    /// Returns the string representation of the constant's value using invariant culture formatting.
    /// </summary>
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
