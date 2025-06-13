namespace ExpressionSolver.Operators;

/// <summary>
/// Represents a function operator that accepts three arguments in a mathematical expression.
/// Implements the <see cref="IFunction"/> interface.
/// </summary>
/// <param name="name">The name of the function.</param>
/// <param name="parameter1">The first expression parameter for the function.</param>
/// <param name="parameter2">The second expression parameter for the function.</param>
/// <param name="parameter3">The third expression parameter for the function.</param>
/// <param name="func">The delegate that computes the function result given the three parameters.</param>
public class FunctionThreeArgs(string name, IExpression parameter1, IExpression parameter2, IExpression parameter3, Func<IExpression, IExpression, IExpression, decimal> func) : IFunction
{
    /// <inheritdoc/>
    public string Name => name;

    /// <inheritdoc/>
    public int Arity => 3;

    /// <inheritdoc/>
    public bool IsRightOperator => false;

    /// <inheritdoc/>
    public int Precedence => 15;

    /// <inheritdoc/>
    public bool ConstantEval => true;

    /// <inheritdoc/>
    public IEnumerable<IExpression> GetOperands()
    {
        yield return parameter1;
        yield return parameter2;
        yield return parameter3;
    }

    /// <inheritdoc/>
    public decimal Compute() => func(parameter1, parameter2, parameter3);

    /// <summary>
    /// Returns a string that represents the current function call with its three arguments.
    /// </summary>
    /// <returns>A string representing the function call.</returns>
    public override string ToString() => $"{Name}({parameter1}, {parameter2}, {parameter3})";
}
