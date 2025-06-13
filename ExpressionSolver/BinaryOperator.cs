using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionSolver;

/// <summary>
/// Represents a binary operator within an expression tree. 
/// This operator accepts two operands and performs a computation based on the provided operation delegate.
/// </summary>
/// <param name="name">The name or symbol of the operator.</param>
/// <param name="precedence">The precedence of the operator, used to determine the evaluation order.</param>
/// <param name="right">The right-hand side operand of the operator.</param>
/// <param name="left">The left-hand side operand of the operator.</param>
/// <param name="computeOperation">The delegate that performs the computation using the two operands.</param>
public class BinaryOperator(string name, int precedence, IExpression right, IExpression left, Func<IExpression, IExpression, decimal> computeOperation) : IBinaryOperator
{
    /// <inheritdoc/>
    public string Name => name;

    /// <inheritdoc/>
    public int Arity => 2;

    /// <inheritdoc/>
    public bool IsRightOperator => false;

    /// <inheritdoc/>
    public int Precedence => precedence;

    /// <inheritdoc/>
    public decimal Compute() => computeOperation(right, left);

    /// <inheritdoc/>
    public IEnumerable<IExpression> GetOperands()
    {
        yield return right;
        yield return left;
    }

    /// <summary>
    /// Returns a string that represents the binary operator with its two operands.
    /// </summary>
    /// <returns>A string representation of the binary operation.</returns>
    public override string ToString() => $"{right} {Name} {left}";
}
