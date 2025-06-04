namespace ExpressionSolver;

public class GeneratorOneArg(string name, IExpression parameter, Func<IExpression, decimal> func) : IFunction
{
    public string Name => name;

    public int Arity => 1;
    public bool IsRightOperator => false;

    public int Precedence => 15;

    public bool ConstantEval => false;

    public IEnumerable<IExpression> GetOperands() { yield return parameter; }

    public decimal Compute() => func(parameter);

    public override string ToString() => $"{Name}({parameter})";
}
