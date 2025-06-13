using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionSolver;

/// <summary>
/// Represents a unary operator within an expression tree. 
/// This operator accepts a single operand and performs a computation based on the provided delegate.
/// Implements the <see cref="IBinaryOperator"/> interface, being used for unary operations.
/// </summary>
/// <param name="name">The name or symbol of the operator.</param>
/// <param name="isRightOperation">Determines if the operator is right-associative.</param>
/// <param name="precedence">The precedence of the operator used to define the order of evaluation.</param>
/// <param name="operand">The operand of the operator.</param>
/// <param name="computeOperaton">The delegate that computes the result given the operand.</param>
public class UnaryOperator(string name, bool isRightOperation, int precedence, IExpression operand, Func<IExpression, decimal> computeOperaton) : IBinaryOperator
{
    /// <inheritdoc/>
    public string Name => name;

    /// <inheritdoc/>
    public int Arity => 1;

    /// <inheritdoc/>
    public bool IsRightOperator => isRightOperation;

    /// <inheritdoc/>
    public int Precedence => precedence;

    /// <inheritdoc/>
    public decimal Compute() => computeOperaton(operand);

    /// <inheritdoc/>
    public IEnumerable<IExpression> GetOperands()
    {
        yield return operand;
    }

    /// <summary>
    /// Returns a string that represents the unary operator applied to its operand.
    /// </summary>
    /// <returns>A string representation of the unary operator and its operand.</returns>
    public override string ToString() => IsRightOperator ? $"{operand} {Name}" : $"{Name} {operand}";
}
