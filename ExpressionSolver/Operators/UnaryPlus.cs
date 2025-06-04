namespace ExpressionSolver.Operators;

public class UnaryPlus(IExpression Operand) : IOperator
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

    public override string ToString() => $"{Name}{Operand}";
}
