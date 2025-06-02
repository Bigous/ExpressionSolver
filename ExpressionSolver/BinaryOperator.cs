using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionSolver;

public class BinaryOperator(string name, int precedence, IExpression right, IExpression left, Func<IExpression, IExpression, decimal> computeOperation) : IOperator
{
    public string Name => name;

    public int Arity => 2;

    public bool IsRightOperator => false;

    public int Precedence => precedence;


    public decimal Compute() => computeOperation(right, left);

    public IEnumerable<IExpression> GetOperands()
    {
        yield return right;
        yield return left;
    }

    public override string ToString() => $"{right} {Name} {left}";
}
