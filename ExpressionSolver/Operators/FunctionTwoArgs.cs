using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionSolver;

public class FunctionTwoArg(string name, bool constantEval, IExpression parameter1, IExpression parameter2, Func<IExpression, IExpression, decimal> func) : IOperator
{
    public string Name => name;

    public int Arity => 2;
    public bool IsRightOperator => false;

    public int Precedence => 15;

    public bool ConstantEval => constantEval;

    public IEnumerable<IExpression> GetOperands()
    {
        yield return parameter1;
        yield return parameter2;
    }

    public decimal Compute() => func(parameter1, parameter2);

    public override string ToString() => $"{Name}({parameter1}, {parameter2})";
}
