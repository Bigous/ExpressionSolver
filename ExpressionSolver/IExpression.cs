using System.Numerics;

namespace ExpressionSolver;

/// <summary>
/// Defines the basic structure for any expression that can be computed to a decimal value.
/// This interface is central to the expression solver, representing nodes in the expression tree.
/// It aims to solve math expressions over one string, for example:
/// "3 + 5 * ( 80.5 / sin(80) + max(var_a, var_b, var_c) ) * user_func(var_x, cos(var_z)) ^ 3.5"
/// </summary>
public interface IExpression
{
    /// <summary>
    /// Computes the value of the expression.
    /// </summary>
    /// <returns>The decimal result of the computation.</returns>
    decimal Compute();
}
