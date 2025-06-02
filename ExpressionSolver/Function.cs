using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionSolver;

public class Function(string name, int precedence, int arity, bool constantEval, IList<IExpression> parameters, Func<IList<IExpression>, decimal> func) : IOperator
{
    public string Name => name;

    public int Arity => arity;
    public bool IsRightOperator => false;

    public int Precedence => precedence;

    public bool ConstantEval => constantEval;

    public IEnumerable<IExpression> GetOperands() => parameters;

    public decimal Compute() => func(parameters);

    public override string ToString() => $"{Name}({string.Join(", ", parameters.Select(p => p.ToString()))})";
}
