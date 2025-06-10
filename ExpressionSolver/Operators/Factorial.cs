namespace ExpressionSolver.Operators;

public class Factorial(IExpression Operand) : IUnaryOperator
{
    public string Name => "!";

    public int Arity => 1;

    public bool IsRightOperator => true;

    public int Precedence => 14;

    public decimal Compute() {
        decimal fact = Operand.Compute();
        if (!decimal.IsInteger(fact)) throw new ArgumentException("Factorial is only defined for non-negative integers.");
        if (fact == 0m) return 1m;
        decimal factRet = 1;
        for (int i = Convert.ToInt32(fact); i > 1; --i) factRet *= i;
        return factRet;
    }

    public IEnumerable<IExpression> GetOperands()
    {
        yield return Operand;
    }

    public override string ToString() => $"{Operand}{Name}";
}
