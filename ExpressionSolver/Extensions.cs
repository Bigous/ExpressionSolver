using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionSolver;

public static class Extensions
{
    public static decimal SolveExpression(this string expression, ExecutionContext context) => context.Solve(expression);

    public static decimal SolveExpression(this string expression) => ExecutionContext.DefaulContext.Solve(expression);
}
