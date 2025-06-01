using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionSolver;

public class UnaryOperator(string name, bool isRightOperation, int precedence, IExpression operand, Func<IExpression, decimal> computeOperaton) : IOperator
{
    public string Name => name;

    public int Arity => 1;

    public bool IsRightOperator => isRightOperation;

    public int Precedence => precedence;

    public decimal Compute() => computeOperaton(operand);

    public IEnumerable<IExpression> GetOperands()
    {
        yield return operand;
    }
}
