using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionSolver;

public interface IOperator : IExpression
{
    string Name { get; }

    int Arity { get; }

    bool IsRightOperator { get; }

    int Precedence { get; }

    IEnumerable<IExpression> GetOperands();
}
