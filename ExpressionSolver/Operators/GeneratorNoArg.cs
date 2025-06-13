namespace ExpressionSolver.Operators;

/// <summary>
/// Represents a function operator with no arguments in a mathematical expression.
/// A Generator is a Function that can return different values when called multiple times with the same arguments.
/// Implements the <see cref="IFunction"/> interface.
/// </summary>
/// <param name="name">The name of the function.</param>
/// <param name="func">The delegate that computes the function result.</param>
public class GeneratorNoArg(string name, Func<decimal> func) : IFunction
{
    /// <inheritdoc/>
    public string Name => name;

    /// <inheritdoc/>
    public int Arity => 0;

    /// <inheritdoc/>
    public bool IsRightOperator => false;

    /// <inheritdoc/>
    public int Precedence => 15;

    /// <inheritdoc/>
    public bool ConstantEval => false;

    /// <inheritdoc/>
    public IEnumerable<IExpression> GetOperands() { yield break; }

    /// <inheritdoc/>
    public decimal Compute() => func();

    /// <summary>
    /// Returns a string that represents the current function call with no arguments.
    /// </summary>
    /// <returns>A string representing the function call.</returns>
    public override string ToString() => $"{Name}()";
}
