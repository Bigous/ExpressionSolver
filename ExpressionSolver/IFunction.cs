namespace ExpressionSolver;

/// <summary>
/// Represents a function operator in a mathematical expression.
/// Inherits from <see cref="IOperator"/> and adds additional behavior specific to functions.
/// </summary>
public interface IFunction : IOperator
{
    /// <summary>
    /// Gets a value indicating whether the function can be evaluated to a constant value when all operands are constant.
    /// </summary>
    bool ConstantEval { get; }
}
