namespace ExpressionSolver.Operators;

/// <summary>
/// Represents a generator operator that accepts one argument in a mathematical expression.
/// A Generator is a Function that can return different values when called multiple times with the same arguments.
/// Implements the <see cref="IFunction"/> interface.
/// </summary>
/// <param name="name">The name of the generator.</param>
/// <param name="parameter">The expression parameter for the generator.</param>
/// <param name="func">The delegate that computes the generator's result based on the parameter.</param>
public class GeneratorOneArg(string name, IExpression parameter, Func<IExpression, decimal> func) : IFunction
{
    /// <inheritdoc/>
    public string Name => name;

    /// <inheritdoc/>
    public int Arity => 1;

    /// <inheritdoc/>
    public bool IsRightOperator => false;

    /// <inheritdoc/>
    public int Precedence => 15;

    /// <inheritdoc/>
    public bool ConstantEval => false;

    /// <inheritdoc/>
    public IEnumerable<IExpression> GetOperands() { yield return parameter; }

    /// <inheritdoc/>
    public decimal Compute() => func(parameter);

    /// <summary>
    /// Returns a string that represents the generator call with its single argument.
    /// </summary>
    /// <returns>A string representing the generator call.</returns>
    public override string ToString() => $"{Name}({parameter})";
}
