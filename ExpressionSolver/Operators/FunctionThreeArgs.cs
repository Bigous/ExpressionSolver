using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionSolver;

public class FunctionThreeArg(string name, bool constantEval, IExpression parameter1, IExpression parameter2, IExpression parameter3, Func<IExpression, IExpression, IExpression, decimal> func) : IFunction
{
    public string Name => name;

    public int Arity => 3;
    public bool IsRightOperator => false;

    public int Precedence => 15;

    public bool ConstantEval => constantEval;

    public IEnumerable<IExpression> GetOperands()
    {
        yield return parameter1;
        yield return parameter2;
        yield return parameter3;
    }

    public decimal Compute() => func(parameter1, parameter2, parameter3);

    public override string ToString() => $"{Name}({parameter1}, {parameter2}, {parameter3})";
}
