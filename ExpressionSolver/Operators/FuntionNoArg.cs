using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionSolver;

public class FunctionNoArg(string name, bool constantEval, Func<decimal> func) : IFunction
{
    public string Name => name;

    public int Arity => 0;
    public bool IsRightOperator => false;

    public int Precedence => 15;

    public bool ConstantEval => constantEval;

    public IEnumerable<IExpression> GetOperands() { yield break; }

    public decimal Compute() => func();

    public override string ToString() => $"{Name}()";
}
