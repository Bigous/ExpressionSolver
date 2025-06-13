using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ExpressionSolver;

/// <summary>
/// Manages the context for parsing and evaluating mathematical expressions.
/// This includes storing and managing custom operators, constants, variables, and functions.
/// </summary>
public partial class ExecutionContext
{
    internal Dictionary<string, Func<IList<IExpression>, IOperator>> _operatorCreators = new();
    internal Dictionary<string, Constant> _constants = new();
    internal Dictionary<string, Variable> _variables = new();
    internal Dictionary<string, FunctionMetadata> _functionCreators = new();

    /// <summary>
    /// Tries to add a custom operator creator to the context.
    /// </summary>
    /// <param name="name">The name of the operator (e.g., "+", "**").</param>
    /// <param name="opCreator">A function that takes a list of operand expressions and returns an <see cref="IOperator"/> instance.</param>
    /// <returns><c>true</c> if the operator creator was added successfully; <c>false</c> if an identifier with the same name already exists.</returns>
    public bool TryAddOperatorCreator(string name, Func<IList<IExpression>, IOperator> opCreator) => HasIdentifier(name) ? false : _operatorCreators.TryAdd(name, opCreator);

    /// <summary>
    /// Tries to remove an operator creator from the context.
    /// </summary>
    /// <param name="name">The name of the operator to remove.</param>
    /// <returns><c>true</c> if the operator creator was removed successfully; <c>false</c> otherwise.</returns>
    public bool TryRemoveOperatorCreator(string name) =>_operatorCreators.Remove(name);

    /// <summary>
    /// Tries to get an operator creator from the context.
    /// </summary>
    /// <param name="name">The name of the operator.</param>
    /// <param name="opCreator">When this method returns, contains the operator creator associated with the specified name, if the name is found; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the context contains an operator creator with the specified name; otherwise, <c>false</c>.</returns>
    public bool TryGetOperatorCreator(string name, [MaybeNullWhen(false)] out Func<IList<IExpression>, IOperator> opCreator) => _operatorCreators.TryGetValue(name, out opCreator);

    /// <summary>
    /// Tries to add a custom constant to the context.
    /// </summary>
    /// <param name="name">The name of the constant (e.g., "pi"). This name is ignored if <paramref name="customConstant"/>'s name is used.</param>
    /// <param name="customConstant">The <see cref="Constant"/> object to add.</param>
    /// <returns><c>true</c> if the constant was added successfully; <c>false</c> if an identifier with the same name already exists.</returns>
    public bool TryAddConstant(string name, Constant customConstant) => HasIdentifier(customConstant.Name) ? false : _constants.TryAdd(customConstant.Name, customConstant);

    /// <summary>
    /// Tries to add a custom constant with a specified value to the context.
    /// </summary>
    /// <param name="name">The name of the constant.</param>
    /// <param name="value">The decimal value of the constant.</param>
    /// <returns><c>true</c> if the constant was added successfully; <c>false</c> if an identifier with the same name already exists.</returns>
    public bool TryAddConstant(string name, decimal value) => HasIdentifier(name) ? false : _constants.TryAdd(name, new(name, value));

    /// <summary>
    /// Tries to remove a constant from the context.
    /// </summary>
    /// <param name="name">The name of the constant to remove.</param>
    /// <returns><c>true</c> if the constant was removed successfully; <c>false</c> otherwise.</returns>
    public bool TryRemoveConstant(string name) => _constants.Remove(name);

    /// <summary>
    /// Tries to get a constant from the context.
    /// </summary>
    /// <param name="name">The name of the constant.</param>
    /// <param name="constant">When this method returns, contains the constant associated with the specified name, if the name is found; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the context contains a constant with the specified name; otherwise, <c>false</c>.</returns>
    public bool TryGetConstant(string name, [MaybeNullWhen(false)] out Constant constant) => _constants.TryGetValue(name, out constant);

    /// <summary>
    /// Tries to add a custom variable to the context.
    /// </summary>
    /// <param name="customVariable">The <see cref="Variable"/> object to add.</param>
    /// <returns><c>true</c> if the variable was added successfully; <c>false</c> if an identifier with the same name already exists.</returns>
    public bool TryAddVariable(Variable customVariable) => HasIdentifier(customVariable.Name) ? false : _variables.TryAdd(customVariable.Name, customVariable);

    /// <summary>
    /// Tries to add a custom variable with a specified value to the context.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="value">The initial decimal value of the variable.</param>
    /// <returns><c>true</c> if the variable was added successfully; <c>false</c> if an identifier with the same name already exists.</returns>
    public bool TryAddVariable(string name, decimal value) => HasIdentifier(name) ? false : _variables.TryAdd(name, new(name, value));

    /// <summary>
    /// Tries to remove a variable from the context.
    /// </summary>
    /// <param name="name">The name of the variable to remove.</param>
    /// <returns><c>true</c> if the variable was removed successfully; <c>false</c> otherwise.</returns>
    public bool TryRemoveVariable(string name) => _variables.Remove(name);

    /// <summary>
    /// Tries to get a variable from the context.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="variable">When this method returns, contains the variable associated with the specified name, if the name is found; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the context contains a variable with the specified name; otherwise, <c>false</c>.</returns>
    public bool TryGetVariable(string name, [MaybeNullWhen(false)] out Variable variable) => _variables.TryGetValue(name, out variable);

    /// <summary>
    /// Tries to add a custom function creator to the context.
    /// </summary>
    /// <param name="name">The name of the function (e.g., "sin", "max").</param>
    /// <param name="arity">The number of arguments the function takes. Use -1 for variable arity.</param>
    /// <param name="funcCreator">A function that takes a list of argument expressions and returns an <see cref="IFunction"/> instance.</param>
    /// <returns><c>true</c> if the function creator was added successfully; <c>false</c> if the name is invalid, an identifier with the same name already exists, or arity is out of range.</returns>
    public bool TryAddFunctionCreator(string name, int arity, Func<IList<IExpression>, IFunction> funcCreator)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        if (HasIdentifier(name))
            return false;

        if (arity < -1 || arity > 100) // Assuming a practical limit for arity
            return false;

        _functionCreators[name] = new FunctionMetadata(arity, funcCreator);
        return true;
    }

    /// <summary>
    /// Tries to remove a function creator from the context.
    /// </summary>
    /// <param name="name">The name of the function to remove.</param>
    /// <returns><c>true</c> if the function creator was removed successfully; <c>false</c> otherwise.</returns>
    public bool TryRemoveFunctionCreator(string name) => _functionCreators.Remove(name);

    /// <summary>
    /// Tries to get a function creator from the context.
    /// </summary>
    /// <param name="name">The name of the function.</param>
    /// <param name="funcCreator">When this method returns, contains the function creator associated with the specified name, if the name is found; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the context contains a function creator with the specified name; otherwise, <c>false</c>.</returns>
    public bool TryGetFunctionCreator(string name, [MaybeNullWhen(false)] out Func<IList<IExpression>, IFunction> funcCreator)
    {
        if (_functionCreators.TryGetValue(name, out var funcMetadata))
        {
            funcCreator = funcMetadata.Creator;
            return true;
        }

        funcCreator = null;
        return false;
    }

    /// <summary>
    /// Solves a mathematical expression string and returns its decimal result.
    /// This is a convenience method that combines <see cref="Compile"/> and <see cref="IExpression.Compute"/>.
    /// </summary>
    /// <param name="expression">The mathematical expression string to solve.</param>
    /// <returns>The decimal result of the evaluated expression.</returns>
    public decimal Solve(string expression) => Compile(expression).Compute();

    /// <summary>
    /// Compiles a mathematical expression string into an <see cref="IExpression"/> tree.
    /// The resulting expression tree can be computed or further optimized.
    /// </summary>
    /// <param name="expression">The mathematical expression string to compile.</param>
    /// <returns>An <see cref="IExpression"/> representing the root of the compiled expression tree.</returns>
    public IExpression Compile(string expression) => BuildExpressionTree(Tokenize(expression));

    /// <summary>
    /// Optimizes a given expression tree by evaluating constant sub-expressions and applying simplification rules.
    /// For example, an expression like "2 + 3 * x" might be optimized if "2+3" were part of a constant sub-expression.
    /// If the entire expression can be resolved to a constant, it will return a <see cref="Constant"/> node.
    /// </summary>
    /// <param name="expression">The expression tree to optimize.</param>
    /// <returns>An optimized <see cref="IExpression"/> tree. This might be the original expression if no optimizations were applicable, a new simplified tree, or a <see cref="Constant"/> node.</returns>
    public IExpression Optimize(IExpression expression)
    {
        if (expression is Constant || expression is Variable)
        {
            return expression; // Constants and variables are already optimized.
        }

        if (expression is IOperator op)
        {
            var originalOperands = op.GetOperands().ToList();
            var optimizedOperands = new List<IExpression>(originalOperands.Count);
            bool allOperandsAreConstant = true;
            bool anyOperandChangedFromOriginal = false;

            for (int i = 0; i < originalOperands.Count; i++)
            {
                var originalOperand = originalOperands[i];
                var optimizedOperand = Optimize(originalOperand); // Recursively optimize operands
                optimizedOperands.Add(optimizedOperand);

                if (!ReferenceEquals(originalOperand, optimizedOperand))
                {
                    anyOperandChangedFromOriginal = true;
                }
                if (!(optimizedOperand is Constant))
                {
                    allOperandsAreConstant = false;
                }
            }

            // 1. Attempt to collapse to a constant if all operands are constants and the operator supports constant evaluation.
            bool canConstantEval = (op is IFunction func && func.ConstantEval) || op is IBinaryOperator || op is IUnaryOperator;
            if (allOperandsAreConstant && canConstantEval)
            {
                IExpression? tempNodeToCompute = null;
                // Try to create the operator/function with the now constant operands
                if (_operatorCreators.TryGetValue(op.Name, out var opCreatorEval))
                {
                    tempNodeToCompute = opCreatorEval(optimizedOperands);
                }
                else if (op is IFunction opFuncEval && _functionCreators.TryGetValue(opFuncEval.Name, out var funcMetaEval))
                {
                    tempNodeToCompute = funcMetaEval.Creator(optimizedOperands);
                }

                if (tempNodeToCompute != null)
                {
                    try
                    {
                        // If successfully created, compute its value and return as a new Constant
                        return new Constant(string.Empty, tempNodeToCompute.Compute());
                    }
                    catch
                    {
                        // Evaluation failed (e.g., division by zero during optimization).
                        // Do not optimize to constant, proceed to other optimizations.
                    }
                }
            }

            // 2. Specific restructuring optimizations (example for '*')
            if (op.Name == "*" && optimizedOperands.Count == 2)
            {
                var opA = optimizedOperands[0];
                var opB = optimizedOperands[1];

                // Pattern: (X * C1) * C2  =>  X * (C1 * C2)
                if (opA is IOperator innerOpA && innerOpA.Name == "*" && innerOpA.GetOperands().Count() == 2 && opB is Constant constB_val)
                {
                    var innerOperandsA = innerOpA.GetOperands().ToList();
                    if (innerOperandsA[1] is Constant constA1_val)
                    {
                        // Recreate the multiplication with X and the combined constant.
                        return _operatorCreators["*"](new List<IExpression> { innerOperandsA[0], new Constant("", constA1_val.Compute() * constB_val.Compute()) });
                    }
                }
                // Pattern: C1 * (X * C2)  =>  X * (C1 * C2)
                if (opA is Constant constA_val && opB is IOperator innerOpB && innerOpB.Name == "*" && innerOpB.GetOperands().Count() == 2)
                {
                    var innerOperandsB = innerOpB.GetOperands().ToList();
                    if (innerOperandsB[1] is Constant constB2_val) // X * C2
                    {
                        return _operatorCreators["*"](new List<IExpression> { innerOperandsB[0], new Constant("", constA_val.Compute() * constB2_val.Compute()) });
                    }
                    // Pattern: C1 * (C2 * X) => X * (C1*C2) (ensures X comes first if possible)
                    else if (innerOperandsB[0] is Constant constB1_val && !(innerOperandsB[1] is Constant)) // C2 * X
                    {
                         return _operatorCreators["*"](new List<IExpression> { innerOperandsB[1], new Constant("", constA_val.Compute() * constB1_val.Compute()) });
                    }
                }
            }

            // 3. If no specific restructuring optimization returned,
            //    and if any of the original operands changed to their optimized form,
            //    recreate the current operator with the optimized operands.
            if (anyOperandChangedFromOriginal)
            {
                if (_operatorCreators.TryGetValue(op.Name, out var opCreatorRebuild))
                {
                    return opCreatorRebuild(optimizedOperands);
                }
                else if (op is IFunction opFuncRebuild && _functionCreators.TryGetValue(opFuncRebuild.Name, out var funcMetaRebuild))
                {
                    return funcMetaRebuild.Creator(optimizedOperands);
                }
            }

            // 4. If nothing changed (neither collapsed to constant, nor restructured, nor operands changed),
            //    return the original expression.
            return expression;
        }
        // If not an operator (e.g., already a Constant or Variable handled at the start, or some other IExpression type), return as is.
        return expression;
    }
}
