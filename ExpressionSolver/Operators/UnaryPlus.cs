namespace ExpressionSolver.Operators;

internal class UnaryPlus(IExpression Operand) : IUnaryOperator
{
    public string Name => "u+";

    public int Arity => 1;

    public bool IsRightOperator => false;

    public int Precedence => 13;

    public decimal Compute() => Operand.Compute();

    public IEnumerable<IExpression> GetOperands()
    {
        yield return Operand;
    }

    public override string ToString() => $"-{Operand}";
}
