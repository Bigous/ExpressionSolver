namespace ExpressionSolver.Operators;

/// <summary>
/// Represents a generator operator that accepts two arguments in a mathematical expression.
/// A Generator is a Function that can return different values when called multiple times with the same arguments.
/// Implements the <see cref="IFunction"/> interface.
/// </summary>
/// <param name="name">The name of the generator.</param>
/// <param name="parameter1">The first expression parameter for the generator.</param>
/// <param name="parameter2">The second expression parameter for the generator.</param>
/// <param name="func">The delegate that computes the generator's result based on the two parameters.</param>
public class GeneratorTwoArgs(string name, IExpression parameter1, IExpression parameter2, Func<IExpression, IExpression, decimal> func) : IFunction
{
    /// <inheritdoc/>
    public string Name => name;

    /// <inheritdoc/>
    public int Arity => 2;

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
    }

    /// <inheritdoc/>
    public decimal Compute() => func(parameter1, parameter2);

    /// <summary>
    /// Returns a string that represents the current generator call with its two arguments.
    /// </summary>
    /// <returns>A string representing the generator call.</returns>
    public override string ToString() => $"{Name}({parameter1}, {parameter2})";
}
