namespace ExpressionSolver;

/// <summary>
/// Represents a partially formed ternary expression (condition ? trueBranch).
/// This is an intermediate helper class used during the parsing process of ternary operators.
/// It is not intended to be directly computed but holds the first two parts of a ternary expression
/// until the false branch (after ':') is encountered.
/// </summary>
internal class PartialTernaryOperator(IExpression condition, IExpression trueBranch) : IOperator
{
    /// <summary>
    /// Gets the condition part of the ternary expression.
    /// </summary>
    public IExpression Condition { get; } = condition;

    /// <summary>
    /// Gets the true branch (expression to evaluate if the condition is true) of the ternary expression.
    /// </summary>
    public IExpression TrueBranch { get; } = trueBranch;

    /// <summary>
    /// Gets the name of this partial operator, which is "?".
    /// </summary>
    public string Name => "?";

    /// <summary>
    /// Gets the arity of this partial operator, which is 2 (condition and true branch).
    /// </summary>
    public int Arity => 2;

    /// <summary>
    /// Gets a value indicating whether this operator is right-associative.
    /// Ternary operators are right-associative.
    /// </summary>
    public bool IsRightOperator => true;

    /// <summary>
    /// Gets the precedence of the '?' part of the ternary operator.
    /// </summary>
    public int Precedence => 2; // Precedence for ?: is typically low.

    /// <summary>
    /// This expression should not be computed directly as it's an intermediate parsing structure.
    /// Attempting to compute it will result in an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <returns>Does not return; always throws an exception.</returns>
    /// <exception cref="InvalidOperationException">Thrown because a <see cref="PartialTernaryOperator"/> is not a complete, computable expression.</exception>
    public decimal Compute() => throw new InvalidOperationException("PartialTernaryOperator should not be computed directly.");

    /// <summary>
    /// Gets the operands of this partial ternary operator.
    /// </summary>
    /// <returns>An enumerable containing the condition and the true branch expressions.</returns>
    public IEnumerable<IExpression> GetOperands()
    {
        yield return Condition;
        yield return TrueBranch;
    }
}