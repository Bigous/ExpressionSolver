using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ExpressionSolver;

public partial class ExecutionContext // Adicionado partial para o caso de Token.cs ser separado
{
    internal Dictionary<string, Func<IList<IExpression>, IOperator>> _operatorCreators = new();
    internal Dictionary<string, Constant> _constants = new();
    internal Dictionary<string, Variable> _variables = new();
    // Dicionário de criadores de função modificado para usar FunctionMetadata
    internal Dictionary<string, FunctionMetadata> _functionCreators = new();

    // Definição do registro para metadados da função
    public bool TryAddOperatorCreator(string name, Func<IList<IExpression>, IOperator> opCreator) => HasIdentifier(name) ? false : _operatorCreators.TryAdd(name, opCreator);
    public bool TryRemoveOperatorCreator(string name) =>_operatorCreators.Remove(name);
    public bool TryGetOperatorCreator(string name, [MaybeNullWhen(false)] out Func<IList<IExpression>, IOperator> opCreator) => _operatorCreators.TryGetValue(name, out opCreator);

    public bool TryAddConstant(string name, Constant customConstant) => HasIdentifier(name) ? false : _constants.TryAdd(customConstant.Name, customConstant);
    public bool TryAddConstant(string name, decimal value) => HasIdentifier(name) ? false : _constants.TryAdd(name, new(name, value));
    public bool TryRemoveConstant(string name) => _constants.Remove(name);
    public bool TryGetConstant(string name, [MaybeNullWhen(false)] out Constant constant) => _constants.TryGetValue(name, out constant);

    public bool TryAddVariable(Variable customVariable) => HasIdentifier(customVariable.Name) ? false : _variables.TryAdd(customVariable.Name, customVariable);
    public bool TryAddVariable(string name, decimal value) => HasIdentifier(name) ? false : _variables.TryAdd(name, new(name, value));
    public bool TryRemoveVariable(string name) => _variables.Remove(name);
    public bool TryGetVariable(string name, [MaybeNullWhen(false)] out Variable variable) => _variables.TryGetValue(name, out variable);

    public bool TryAddFunctionCreator(string name, int arity, Func<IList<IExpression>, IFunction> funcCreator)
    {
        if (HasIdentifier(name)) return false;
        return _functionCreators.TryAdd(name, new FunctionMetadata(arity, funcCreator));
    }
    public bool TryRemoveFunctionCreator(string name) => _functionCreators.Remove(name);
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

    public decimal Solve(string expression) => Compile(expression).Compute();

    public IExpression Compile(string expression) => BuildExpressionTree(Tokenize(expression));

    /// <summary>
    /// Solves constant expressions, such as "2 + 2" or "sin(0)" and clears as much as possible from the expression tree, leaving only the necessary operators and operands.
    /// At maximum, it will leave a single constant.
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public IExpression Optimize(IExpression expression)
    {
        if (expression is Constant || expression is Variable)
        {
            return expression;
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
                var optimizedOperand = Optimize(originalOperand); 
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

            // 1. Tentativa de colapso total para constante se todos os operandos forem constantes
            bool canConstantEval = (op is Function func && func.ConstantEval) || op is BinaryOperator || op is UnaryOperator;
            if (allOperandsAreConstant && canConstantEval)
            {
                IExpression? tempNodeToCompute = null;
                if (_operatorCreators.TryGetValue(op.Name, out var opCreatorEval))
                {
                    tempNodeToCompute = opCreatorEval(optimizedOperands);
                }
                else if (op is Function opFuncEval && _functionCreators.TryGetValue(opFuncEval.Name, out var funcMetaEval))
                {
                    tempNodeToCompute = funcMetaEval.Creator(optimizedOperands);
                }

                if (tempNodeToCompute != null)
                {
                    try
                    {
                        return new Constant(string.Empty, tempNodeToCompute.Compute());
                    }
                    catch
                    {
                        // A avaliação falhou (ex: divisão por zero em tempo de otimização).
                        // Não otimizar para constante, prosseguir para outras otimizações.
                    }
                }
            }

            // 2. Otimizações específicas de reestruturação (ex: para '*')
            if (op.Name == "*" && optimizedOperands.Count == 2)
            {
                var opA = optimizedOperands[0];
                var opB = optimizedOperands[1];

                // Padrão: (X * C1) * C2  =>  X * (C1 * C2)
                if (opA is IOperator innerOpA && innerOpA.Name == "*" && innerOpA.GetOperands().Count() == 2 && opB is Constant constB_val)
                {
                    var innerOperandsA = innerOpA.GetOperands().ToList();
                    if (innerOperandsA[1] is Constant constA1_val)
                    {
                        return _operatorCreators["*"](new List<IExpression> { innerOperandsA[0], new Constant("", constA1_val.Compute() * constB_val.Compute()) });
                    }
                }
                // Padrão: C1 * (X * C2)  =>  X * (C1 * C2)
                if (opA is Constant constA_val && opB is IOperator innerOpB && innerOpB.Name == "*" && innerOpB.GetOperands().Count() == 2)
                {
                    var innerOperandsB = innerOpB.GetOperands().ToList();
                    if (innerOperandsB[1] is Constant constB2_val) // X * C2
                    {
                        return _operatorCreators["*"](new List<IExpression> { innerOperandsB[0], new Constant("", constA_val.Compute() * constB2_val.Compute()) });
                    }
                    // Padrão: C1 * (C2 * X) => X * (C1*C2) (garante que X venha primeiro se possível)
                    else if (innerOperandsB[0] is Constant constB1_val && !(innerOperandsB[1] is Constant)) // C2 * X
                    {
                         return _operatorCreators["*"](new List<IExpression> { innerOperandsB[1], new Constant("", constA_val.Compute() * constB1_val.Compute()) });
                    }
                }
            }

            // 3. Se nenhuma otimização específica de reestruturação retornou,
            //    e se algum dos operandos originais mudou para sua forma otimizada,
            //    recrie o operador atual com os operandos otimizados.
            if (anyOperandChangedFromOriginal)
            {
                if (_operatorCreators.TryGetValue(op.Name, out var opCreatorRebuild))
                {
                    return opCreatorRebuild(optimizedOperands);
                }
                else if (op is Function opFuncRebuild && _functionCreators.TryGetValue(opFuncRebuild.Name, out var funcMetaRebuild))
                {
                    return funcMetaRebuild.Creator(optimizedOperands);
                }
            }

            // 4. Se nada mudou (nem colapso para constante, nem reestruturação, nem operandos alterados),
            //    retorne a expressão original.
            return expression;
        }
        return expression;
    }
}
