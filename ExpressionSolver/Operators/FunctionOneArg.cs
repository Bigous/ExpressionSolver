namespace ExpressionSolver.Operators;

/// <summary>
/// Represents a function operator that accepts one argument in a mathematical expression.
/// Implements the <see cref="IFunction"/> interface.
/// </summary>
/// <param name="name">The name of the function.</param>
/// <param name="parameter">The single expression parameter for the function.</param>
/// <param name="func">The delegate that computes the function result given the parameter.</param>
public class FunctionOneArg(string name, IExpression parameter, Func<IExpression, decimal> func) : IFunction
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
    public bool ConstantEval => true;

    /// <inheritdoc/>
    public IEnumerable<IExpression> GetOperands() { yield return parameter; }

    /// <inheritdoc/>
    public decimal Compute() => func(parameter);

    /// <summary>
    /// Returns a string that represents the function call with its argument.
    /// </summary>
    /// <returns>A string representing the function call.</returns>
    public override string ToString() => $"{Name}({parameter})";
}
