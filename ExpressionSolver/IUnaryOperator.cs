namespace ExpressionSolver;

/// <summary>
/// Defines the basic structure for any unary operator in a mathematical expression.
/// Unary operators accept exactly one operand and perform a specific operation.
/// This interface inherits from <see cref="IOperator"/>, and by extension <see cref="IExpression"/>.
/// </summary>
public interface IUnaryOperator : IOperator
{
}
