namespace ExpressionSolver;

public interface IFunction : IOperator
{
    public bool ConstantEval { get; } // Indicates if the function can be evaluated to a constant value when all operands are constants
}
