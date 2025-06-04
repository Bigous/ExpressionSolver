namespace ExpressionSolver.Operators;

public class Subtract(IExpression Left, IExpression Right) : IOperator
{
    public string Name => "-";

    public int Arity => 2;

    public bool IsRightOperator => false;

    public int Precedence => 11;

    public decimal Compute() => Left.Compute() - Right.Compute();

    public IEnumerable<IExpression> GetOperands()
    {
        yield return Left;
        yield return Right;
    }
}
