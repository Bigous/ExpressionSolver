namespace ExpressionSolver;

public class GeneratorManyArgs(string name, IList<IExpression> parameters, Func<IList<IExpression>, decimal> func) : IFunction
{
    public string Name => name;

    public int Arity => parameters.Count;
    public bool IsRightOperator => false;

    public int Precedence => 15;

    public bool ConstantEval => false;

    public IEnumerable<IExpression> GetOperands() => parameters;

    public decimal Compute() => func(parameters);

    public override string ToString() => $"{Name}({string.Join(", ", parameters.Select(p => p.ToString()))})";
}
