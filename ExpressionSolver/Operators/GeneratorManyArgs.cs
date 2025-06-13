namespace ExpressionSolver.Operators;

/// <summary>
/// Represents a function operator with a variable number of arguments in a mathematical expression.
/// A Generator is a Function that can return different values when called multiple times with the same arguments.
/// Implements the <see cref="IFunction"/> interface.
/// </summary>
/// <param name="name">The name of the function.</param>
/// <param name="parameters">The list of expressions serving as arguments for the function.</param>
/// <param name="func">The delegate that computes the function result based on the parameters.</param>
public class GeneratorManyArgs(string name, IList<IExpression> parameters, Func<IList<IExpression>, decimal> func) : IFunction
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
    public bool ConstantEval => false;

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
