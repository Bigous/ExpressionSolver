namespace ExpressionSolver.Operators;

public class Power(IExpression Left, IExpression Right) : IBinaryOperator
{
    public string Name => "**";

    public int Arity => 2;

    public bool IsRightOperator => false;

    public int Precedence => 14;

    public decimal Compute() => DecimalMath.Pow(Left.Compute(), Right.Compute());

    public IEnumerable<IExpression> GetOperands()
    {
        yield return Left;
        yield return Right;
    }

    public override string ToString() => $"{Left} {Name} {Right}";
}
