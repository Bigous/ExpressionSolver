namespace ExpressionSolver.Operators;

public class LogicLessThan(IExpression Left, IExpression Right) : IOperator
{
    public string Name => "<";

    public int Arity => 2;

    public bool IsRightOperator => false;

    public int Precedence => 9;

    public decimal Compute() => Left.Compute() < Right.Compute() ? 1m : 0m;

    public IEnumerable<IExpression> GetOperands()
    {
        yield return Left;
        yield return Right;
    }

    public override string ToString() => $"{Left} {Name} {Right}";
}
