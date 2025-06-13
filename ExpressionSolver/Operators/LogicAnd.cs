namespace ExpressionSolver.Operators;

internal class LogicAnd(IExpression Left, IExpression Right) : IBinaryOperator
{
    public string Name => "&&";

    public int Arity => 2;

    public bool IsRightOperator => false;

    public int Precedence => 4;

    public decimal Compute() => Left.Compute() != 0m && Right.Compute() != 0m ? 1m : 0m;

    public IEnumerable<IExpression> GetOperands()
    {
        yield return Left;
        yield return Right;
    }

    public override string ToString() => $"{Left} {Name} {Right}";
}
