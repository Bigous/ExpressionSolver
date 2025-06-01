using System.Numerics;

namespace ExpressionSolver;

// Aims to solve math expressions over one string:
// eg: "3 + 5 * ( 80.5 / sin(80) + max(var_a, var_b, var_c) ) * user_func(var_x, cos(var_z)) ^ 3.5"

public interface IExpression
{
    decimal Compute();
}
