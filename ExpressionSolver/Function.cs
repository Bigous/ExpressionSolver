using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionSolver;

/// <summary>
/// Represents a function operator in a mathematical expression.
/// Implements the <see cref="IFunction"/> interface.
/// </summary>
/// <param name="name">The name of the function.</param>
/// <param name="precedence">The precedence of the function in the expression evaluation.</param>
/// <param name="arity">The number of operands the function accepts.</param>
/// <param name="constantEval">Indicates whether the function can be evaluated at compile time.</param>
/// <param name="parameters">The list of expressions that serve as parameters to the function.</param>
/// <param name="func">The delegate that computes the function result given the parameters.</param>
public class Function(string name, int precedence, int arity, bool constantEval, IList<IExpression> parameters, Func<IList<IExpression>, decimal> func) : IFunction
{
    /// <inheritdoc/>
    public string Name => name;

    /// <inheritdoc/>
    public int Arity => arity;

    /// <inheritdoc/>
    public bool IsRightOperator => false;

    /// <inheritdoc/>
    public int Precedence => precedence;

    /// <inheritdoc/>
    public bool ConstantEval => constantEval;

    /// <inheritdoc/>
    public IEnumerable<IExpression> GetOperands() => parameters;

    /// <inheritdoc/>
    public decimal Compute() => func(parameters);

    /// <summary>
    /// Returns a string that represents the current function including its parameters.
    /// </summary>
    /// <returns>A string representation of the function call.</returns>
    public override string ToString() => $"{Name}({string.Join(", ", parameters.Select(p => p.ToString()))})";
}
