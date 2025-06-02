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

    public static IExpression CompileExpression(this string expression, ExecutionContext context) => context.Compile(expression);

    public static IExpression CompileExpression(this string expression) => ExecutionContext.DefaulContext.Compile(expression);

    public static IExpression OptimizeExpression(this IExpression expression, ExecutionContext context) => context.Optimize(expression);

    public static IExpression OptimizeExpression(this IExpression expression) => ExecutionContext.DefaulContext.Optimize(expression);

    public static IExpression OptimizeExpression(this string expression, ExecutionContext context) => expression.CompileExpression(context).OptimizeExpression(context);

    public static IExpression OptimizeExpression(this string expression) => expression.CompileExpression().OptimizeExpression();
}
