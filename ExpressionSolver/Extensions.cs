using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionSolver;

/// <summary>
/// Provides extension methods for solving, compiling, and optimizing mathematical expressions represented as strings or as <see cref="IExpression"/> trees.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Solves a mathematical expression string using the specified <see cref="ExecutionContext"/>.
    /// </summary>
    /// <param name="expression">The mathematical expression string to solve.</param>
    /// <param name="context">The execution context containing operators, functions, and constants.</param>
    /// <returns>The decimal result of the evaluated expression.</returns>
    public static decimal SolveExpression(this string expression, ExecutionContext context) => context.Solve(expression);

    /// <summary>
    /// Solves a mathematical expression string using the default <see cref="ExecutionContext"/>.
    /// </summary>
    /// <param name="expression">The mathematical expression string to solve.</param>
    /// <returns>The decimal result of the evaluated expression.</returns>
    public static decimal SolveExpression(this string expression) => ExecutionContext.DefaulContext.Solve(expression);

    /// <summary>
    /// Compiles a mathematical expression string into an <see cref="IExpression"/> tree using the specified <see cref="ExecutionContext"/>.
    /// </summary>
    /// <param name="expression">The mathematical expression string to compile.</param>
    /// <param name="context">The execution context containing operators, functions, and constants.</param>
    /// <returns>An <see cref="IExpression"/> representing the root of the compiled expression tree.</returns>
    public static IExpression CompileExpression(this string expression, ExecutionContext context) => context.Compile(expression);

    /// <summary>
    /// Compiles a mathematical expression string into an <see cref="IExpression"/> tree using the default <see cref="ExecutionContext"/>.
    /// </summary>
    /// <param name="expression">The mathematical expression string to compile.</param>
    /// <returns>An <see cref="IExpression"/> representing the root of the compiled expression tree.</returns>
    public static IExpression CompileExpression(this string expression) => ExecutionContext.DefaulContext.Compile(expression);

    /// <summary>
    /// Optimizes a given <see cref="IExpression"/> tree using the specified <see cref="ExecutionContext"/>.
    /// </summary>
    /// <param name="expression">The <see cref="IExpression"/> tree to optimize.</param>
    /// <param name="context">The execution context with optimization rules and settings.</param>
    /// <returns>An optimized <see cref="IExpression"/> tree.</returns>
    public static IExpression OptimizeExpression(this IExpression expression, ExecutionContext context) => context.Optimize(expression);

    /// <summary>
    /// Optimizes a given <see cref="IExpression"/> tree using the default <see cref="ExecutionContext"/>.
    /// </summary>
    /// <param name="expression">The <see cref="IExpression"/> tree to optimize.</param>
    /// <returns>An optimized <see cref="IExpression"/> tree.</returns>
    public static IExpression OptimizeExpression(this IExpression expression) => ExecutionContext.DefaulContext.Optimize(expression);

    /// <summary>
    /// Compiles and then optimizes a mathematical expression string using the specified <see cref="ExecutionContext"/>.
    /// </summary>
    /// <param name="expression">The mathematical expression string to compile and optimize.</param>
    /// <param name="context">The execution context containing operators, functions, and constants.</param>
    /// <returns>An optimized <see cref="IExpression"/> tree representing the expression.</returns>
    public static IExpression OptimizeExpression(this string expression, ExecutionContext context) => expression.CompileExpression(context).OptimizeExpression(context);

    /// <summary>
    /// Compiles and then optimizes a mathematical expression string using the default <see cref="ExecutionContext"/>.
    /// </summary>
    /// <param name="expression">The mathematical expression string to compile and optimize.</param>
    /// <returns>An optimized <see cref="IExpression"/> tree representing the expression.</returns>
    public static IExpression OptimizeExpression(this string expression) => expression.CompileExpression().OptimizeExpression();
}
