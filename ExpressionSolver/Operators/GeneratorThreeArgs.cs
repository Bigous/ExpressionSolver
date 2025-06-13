namespace ExpressionSolver.Operators;

/// <summary>
/// Represents a generator operator that accepts three arguments in a mathematical expression.
/// A Generator is a Function that can return different values when called multiple times with the same arguments.
/// Implements the <see cref="IFunction"/> interface.
/// </summary>
/// <param name="name">The name of the generator.</param>
/// <param name="parameter1">The first expression parameter for the generator.</param>
/// <param name="parameter2">The second expression parameter for the generator.</param>
/// <param name="parameter3">The third expression parameter for the generator.</param>
/// <param name="func">The delegate that computes the generator's result based on the three parameters.</param>
public class GeneratorThreeArgs(string name, IExpression parameter1, IExpression parameter2, IExpression parameter3, Func<IExpression, IExpression, IExpression, decimal> func) : IFunction
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
    public bool ConstantEval => false;

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
    /// Returns a string that represents the current generator call with its three arguments.
    /// </summary>
    /// <returns>A string representing the generator call.</returns>
    public override string ToString() => $"{Name}({parameter1}, {parameter2}, {parameter3})";
}
