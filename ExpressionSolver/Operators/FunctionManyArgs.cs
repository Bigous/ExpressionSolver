using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionSolver;

public class FunctionManyArgs(string name, bool constantEval, IList<IExpression> parameters, Func<IList<IExpression>, decimal> func) : IFunction
{
    public string Name => name;

    public int Arity => parameters.Count;
    public bool IsRightOperator => false;

    public int Precedence => 15;

    public bool ConstantEval => constantEval;

    public IEnumerable<IExpression> GetOperands() => parameters;

    public decimal Compute() => func(parameters);

    public override string ToString() => $"{Name}({string.Join(", ", parameters.Select(p => p.ToString()))})";
}
