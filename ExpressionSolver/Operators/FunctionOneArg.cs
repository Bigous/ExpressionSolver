using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionSolver;

public class FunctionOneArg(string name, bool constantEval, IExpression parameter, Func<IExpression, decimal> func) : IOperator
{
    public string Name => name;

    public int Arity => 1;
    public bool IsRightOperator => false;

    public int Precedence => 15;

    public bool ConstantEval => constantEval;

    public IEnumerable<IExpression> GetOperands() { yield return parameter; }

    public decimal Compute() => func(parameter);

    public override string ToString() => $"{Name}({parameter})";
}
