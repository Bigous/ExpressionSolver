namespace ExpressionSolver.Operators;

/// <summary>
/// Represents a function operator that accepts many arguments in a mathematical expression.
/// Implements the <see cref="IFunction"/> interface.
/// </summary>
/// <param name="name">The name of the function.</param>
/// <param name="parameters">The list of expressions that serve as arguments to the function.</param>
/// <param name="func">The delegate that computes the function result given the list of parameters.</param>
public class FunctionManyArgs(string name, IList<IExpression> parameters, Func<IList<IExpression>, decimal> func) : IFunction
{
    /// <inheritdoc/>
    public string Name => name;

    /// <inheritdoc/>
    public int Arity => parameters.Count;

    /// <inheritdoc/>
    public bool IsRightOperator => false;

    /// <inheritdoc/>
    public int Precedence => 15;

    /// <inheritdoc/>
    public bool ConstantEval => true;

    /// <inheritdoc/>
    public IEnumerable<IExpression> GetOperands() => parameters;

    /// <inheritdoc/>
    public decimal Compute() => func(parameters);

    /// <summary>
    /// Returns a string that represents the current function call with its arguments.
    /// </summary>
    /// <returns>A string representing the function call.</returns>
    public override string ToString() => $"{Name}({string.Join(", ", parameters.Select(p => p.ToString()))})";
}
