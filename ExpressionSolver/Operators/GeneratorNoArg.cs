namespace ExpressionSolver;

public class GeneratorNoArg(string name, Func<decimal> func) : IFunction
{
    public string Name => name;

    public int Arity => 0;
    public bool IsRightOperator => false;

    public int Precedence => 15;

    public bool ConstantEval => false;

    public IEnumerable<IExpression> GetOperands() { yield break; }

    public decimal Compute() => func();

    public override string ToString() => $"{Name}()";
}
