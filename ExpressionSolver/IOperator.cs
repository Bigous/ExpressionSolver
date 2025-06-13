using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionSolver;

/// <summary>
/// Defines the basic structure for an operator in a mathematical expression.
/// This interface extends IExpression, representing operators in the expression solving system.
/// Operators are elements that perform operations on one or more operands, such as:
/// Binary (+, -, *, /, ^), unary (-x, sin(x), log(x)) or n-ary (max(x,y,z)).
/// </summary>
public interface IOperator : IExpression
{
    /// <summary>
    /// Gets the name or symbol of the operator.
    /// </summary>
    /// <returns>A string representing the operator (example: "+", "-", "sin", "max").</returns>
    string Name { get; }

    /// <summary>
    /// Gets the arity of the operator, which is the number of operands the operator accepts.
    /// </summary>
    /// <returns>An integer representing the arity (1 for unary, 2 for binary, etc. and -1 for variable arity).</returns>
    int Arity { get; }

    /// <summary>
    /// Determines if the operator is a right-associative operator in the expression.
    /// </summary>
    /// <returns>True if the operator is applied from right to left; otherwise, false.</returns>
    bool IsRightOperator { get; }

    /// <summary>
    /// Gets the precedence of the operator to determine the order of evaluation in expressions.
    /// </summary>
    /// <returns>An integer representing the precedence (higher values indicate higher precedence).</returns>
    int Precedence { get; }

    /// <summary>
    /// Gets the collection of operands associated with this operator.
    /// </summary>
    /// <returns>A collection of expressions that represent the operands of the operator.</returns>
    IEnumerable<IExpression> GetOperands();
}
