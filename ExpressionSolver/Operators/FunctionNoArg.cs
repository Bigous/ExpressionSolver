namespace ExpressionSolver;

public class FunctionNoArg(string name, Func<decimal> func) : IFunction
{
    public string Name => name;

    public int Arity => 0;
    public bool IsRightOperator => false;

    public int Precedence => 15;

    public bool ConstantEval => true;

    public IEnumerable<IExpression> GetOperands() { yield break; }

    public decimal Compute() => func();

    public override string ToString() => $"{Name}()";
}
