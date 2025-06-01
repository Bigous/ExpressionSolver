using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ExpressionSolver;

public partial class ExecutionContext // Adicionado partial para o caso de Token.cs ser separado
{
    // Definição do registro para metadados da função
    public record FunctionMetadata(int Arity, Func<IList<IExpression>, Function> Creator);

    private Dictionary<string, Func<IList<IExpression>, IOperator>> _operatorCreators = new();
    private Dictionary<string, Constant> _constants = new();
    private Dictionary<string, Variable> _variables = new();
    // Dicionário de criadores de função modificado para usar FunctionMetadata
    private Dictionary<string, FunctionMetadata> _functionCreators = new();

    private bool HasIdentifier(string identifier) => _functionCreators.ContainsKey(identifier) || _variables.ContainsKey(identifier) || _constants.ContainsKey(identifier) || _operatorCreators.ContainsKey(identifier);

    public bool TryAddOperatorCreator(string name, Func<IList<IExpression>, IOperator> opCreator) => HasIdentifier(name) ? false : _operatorCreators.TryAdd(name, opCreator);
    public bool TryRemoveOperatorCreator(string name) => _operatorCreators.Remove(name);
    public bool TryGetOperatorCreator(string name, [MaybeNullWhen(false)] out Func<IList<IExpression>, IOperator> opCreator) => _operatorCreators.TryGetValue(name, out opCreator);

    public bool TryAddConstant(string name, Constant customConstant) => HasIdentifier(name) ? false : _constants.TryAdd(customConstant.Name, customConstant);
    public bool TryRemoveConstant(string name) => _constants.Remove(name);
    public bool TryGetConstant(string name, [MaybeNullWhen(false)] out Constant constant) => _constants.TryGetValue(name, out constant);

    public bool TryAddVariable(Variable customVariable) => HasIdentifier(customVariable.Name) ? false : _variables.TryAdd(customVariable.Name, customVariable);
    public bool TryRemoveVariable(string name) => _variables.Remove(name);
    public bool TryGetVariable(string name, [MaybeNullWhen(false)] out Variable variable) => _variables.TryGetValue(name, out variable);

    // TryAddFunctionCreator modificado para aceitar aridade e usar FunctionMetadata
    public bool TryAddFunctionCreator(string name, int arity, Func<IList<IExpression>, Function> funcCreator)
    {
        if (HasIdentifier(name)) return false;
        return _functionCreators.TryAdd(name, new FunctionMetadata(arity, funcCreator));
    }
    public bool TryRemoveFunctionCreator(string name) => _functionCreators.Remove(name);
    // TryGetFunctionCreator modificado para retornar FunctionMetadata
    public bool TryGetFunctionCreator(string name, [MaybeNullWhen(false)] out FunctionMetadata funcMetadata) =>
        _functionCreators.TryGetValue(name, out funcMetadata);

    public decimal Solve(string expression) => Compile(expression).Compute();

    public IExpression Compile(string expression) => BuildExpressionTree(Tokenize(expression));

    private IList<Token> Tokenize(string expression)
    {
        var tokens = new List<Token>();
        int i = 0;
        bool expectOperand = true; // No início, ou após um operador/parêntese esquerdo, esperamos um operando

        while (i < expression.Length)
        {
            char c = expression[i];

            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            if (char.IsDigit(c) || (c == '.' && i + 1 < expression.Length && char.IsDigit(expression[i + 1])))
            {
                var sb = new StringBuilder();
                bool hasDecimal = false;
                while (i < expression.Length && (char.IsDigit(expression[i]) || (expression[i] == '.' && !hasDecimal)))
                {
                    if (expression[i] == '.')
                    {
                        if (hasDecimal) break; // Segundo ponto decimal
                        hasDecimal = true;
                    }
                    sb.Append(expression[i]);
                    i++;
                }
                tokens.Add(new Token(sb.ToString(), TokenType.Number));
                expectOperand = false;
                continue;
            }

            if (char.IsLetter(c) || c == '_')
            {
                var sb = new StringBuilder();
                while (i < expression.Length && (char.IsLetterOrDigit(expression[i]) || expression[i] == '_'))
                {
                    sb.Append(expression[i]);
                    i++;
                }
                string identifier = sb.ToString();
                // Verifica se é uma função (seguida por '(' )
                int tempIdx = i;
                while (tempIdx < expression.Length && char.IsWhiteSpace(expression[tempIdx])) tempIdx++;
                if (tempIdx < expression.Length && expression[tempIdx] == '(')
                {
                    tokens.Add(new Token(identifier, TokenType.FunctionCall));
                }
                else
                {
                    tokens.Add(new Token(identifier, TokenType.Identifier));
                }
                expectOperand = false;
                continue;
            }

            if (c == '(')
            {
                tokens.Add(new Token("(", TokenType.LeftParenthesis));
                expectOperand = true;
                i++;
                continue;
            }

            if (c == ')')
            {
                tokens.Add(new Token(")", TokenType.RightParenthesis));
                expectOperand = false;
                i++;
                continue;
            }

            if (c == ',')
            {
                tokens.Add(new Token(",", TokenType.Comma));
                expectOperand = true;
                i++;
                continue;
            }

            // Operadores
            // Tratar unário +-
            if ((c == '+' || c == '-') && expectOperand)
            {
                string unaryOp = "u" + c.ToString();
                if (_operatorCreators.ContainsKey(unaryOp))
                {
                    tokens.Add(new Token(unaryOp, TokenType.UnaryOperator));
                }
                else
                {
                    tokens.Add(new Token(c.ToString(), TokenType.Operator));
                }
                i++;
                continue;
            }


            // Operadores de múltiplos caracteres
            string twoCharOp = "";
            if (i + 1 < expression.Length)
            {
                twoCharOp = expression.Substring(i, 2);
            }

            if (!string.IsNullOrEmpty(twoCharOp) && _operatorCreators.ContainsKey(twoCharOp))
            {
                tokens.Add(new Token(twoCharOp, TokenType.Operator));
                i += 2;
                expectOperand = true;
                continue;
            }

            string oneCharOp = c.ToString();
            if (_operatorCreators.ContainsKey(oneCharOp))
            {
                tokens.Add(new Token(oneCharOp, TokenType.Operator));
                i++;
                expectOperand = true;
                continue;
            }
            throw new ArgumentException($"Caractere desconhecido ou operador inválido: '{c}' na posição {i}");
        }
        tokens.Add(new Token("EOF", TokenType.EOF));
        return tokens;
    }

    private record OperatorMetadata(string Name, int Precedence, bool IsRightAssociative, int Arity, bool IsUnary = false);

    private OperatorMetadata? GetOperatorMetadata(Token token)
    {
        string opName = token.Value;
        switch (opName)
        {
            case "u+": return new OperatorMetadata(opName, 13, true, 1, true);
            case "u-": return new OperatorMetadata(opName, 13, true, 1, true);
            case "||": return new OperatorMetadata(opName, 3, false, 2);
            case "&&": return new OperatorMetadata(opName, 4, false, 2);
            case "?": return new OperatorMetadata(opName, 2, true, 2);
            case ":": return new OperatorMetadata(opName, 1, false, 2);
            case "==": case "!=": return new OperatorMetadata(opName, 8, false, 2);
            case ">": case "<": case ">=": case "<=": return new OperatorMetadata(opName, 9, false, 2);
            case "+": case "-": return new OperatorMetadata(opName, 11, false, 2);
            case "*": case "/": case "%": return new OperatorMetadata(opName, 12, false, 2);
            case "**": return new OperatorMetadata(opName, 14, true, 2);
            case "!": return new OperatorMetadata(opName, 14, false, 1, true);
            default:
                if (_operatorCreators.TryGetValue(opName, out var creator))
                {
                    try
                    {
                        var dummyOperands = new List<IExpression>();
                        var tempOp = creator(new IExpression[2] { new Constant("d1", 0), new Constant("d2", 0) });
                        return new OperatorMetadata(tempOp.Name, tempOp.Precedence, tempOp.IsRightOperator, tempOp.Arity);
                    }
                    catch
                    {
                        return null;
                    }
                }
                return null;
        }
    }

    private IExpression BuildExpressionTree(IList<Token> tokens)
    {
        var outputStack = new Stack<IExpression>();
        var operatorStack = new Stack<Token>();
        int tokenIndex = 0;

        Token GetNextToken() => tokens[tokenIndex++];

        while (tokenIndex < tokens.Count && tokens[tokenIndex].Type != TokenType.EOF)
        {
            Token token = GetNextToken();

            switch (token.Type)
            {
                case TokenType.Number:
                    if (decimal.TryParse(token.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal numValue))
                    {
                        outputStack.Push(new Constant(token.Value, numValue));
                    }
                    else
                    {
                        throw new ArgumentException($"Número inválido: {token.Value}");
                    }
                    break;

                case TokenType.Identifier:
                    if (TryGetVariable(token.Value, out var variable))
                    {
                        outputStack.Push(variable);
                    }
                    else if (TryGetConstant(token.Value, out var constant))
                    {
                        outputStack.Push(constant);
                    }
                    else
                    {
                        throw new ArgumentException($"Identificador desconhecido: {token.Value}");
                    }
                    break;

                case TokenType.FunctionCall:
                    operatorStack.Push(token);
                    break;

                case TokenType.Comma:
                    while (operatorStack.Count > 0 && operatorStack.Peek().Type != TokenType.LeftParenthesis)
                    {
                        if (operatorStack.Peek().Type == TokenType.FunctionCall) break;
                        ApplyOperatorFromStack(outputStack, operatorStack);
                        if (operatorStack.Count > 0 && operatorStack.Peek().Type == TokenType.LeftParenthesis)
                            break;
                    }
                    if (operatorStack.Count == 0 ||
                        (operatorStack.Peek().Type != TokenType.LeftParenthesis && operatorStack.Peek().Type != TokenType.FunctionCall))
                    {
                    }
                    break;

                case TokenType.UnaryOperator:
                case TokenType.Operator:
                    var op1Meta = GetOperatorMetadata(token);
                    if (op1Meta == null) throw new ArgumentException($"Operador desconhecido ou metadados não encontrados para: {token.Value}");

                    while (operatorStack.Count > 0)
                    {
                        var topOpToken = operatorStack.Peek();
                        if (topOpToken.Type == TokenType.LeftParenthesis || topOpToken.Type == TokenType.FunctionCall) break;

                        var op2Meta = GetOperatorMetadata(topOpToken);
                        if (op2Meta == null)
                        {
                            throw new InvalidOperationException($"Não foi possível obter metadados para o operador no topo da pilha: {topOpToken.Value}");
                        }


                        if ((!op1Meta.IsRightAssociative && op1Meta.Precedence <= op2Meta.Precedence) ||
                            (op1Meta.IsRightAssociative && op1Meta.Precedence < op2Meta.Precedence))
                        {
                            ApplyOperatorFromStack(outputStack, operatorStack);
                        }
                        else
                        {
                            break;
                        }
                    }
                    operatorStack.Push(token);
                    break;

                case TokenType.LeftParenthesis:
                    operatorStack.Push(token);
                    break;

                case TokenType.RightParenthesis:
                    bool foundLeftParen = false;
                    while (operatorStack.Count > 0)
                    {
                        Token topOp = operatorStack.Peek();
                        if (topOp.Type == TokenType.LeftParenthesis)
                        {
                            operatorStack.Pop();
                            foundLeftParen = true;
                            break;
                        }
                        if (topOp.Value == "?")
                        {
                            throw new ArgumentException("Operador ternário '?' desbalanceado encontrado dentro de parênteses sem ':' correspondente.");
                        }
                        ApplyOperatorFromStack(outputStack, operatorStack);
                    }
                    if (!foundLeftParen) throw new ArgumentException("Parênteses desbalanceados ( '(' esperado ).");

                    if (operatorStack.Count > 0 && operatorStack.Peek().Type == TokenType.FunctionCall)
                    {
                        ApplyFunctionCall(outputStack, operatorStack);
                    }
                    break;
            }
        }

        while (operatorStack.Count > 0)
        {
            Token topOp = operatorStack.Peek();
            if (topOp.Type == TokenType.LeftParenthesis) throw new ArgumentException("Parênteses desbalanceados ( ')' esperado ).");
            if (topOp.Value == "?" || topOp.Value == ":") throw new ArgumentException($"Operador ternário incompleto: '{topOp.Value}' encontrado no final da expressão.");
            if (topOp.Type == TokenType.FunctionCall) throw new ArgumentException("Chamada de função mal formada (provavelmente faltam parênteses ou argumentos).");
            ApplyOperatorFromStack(outputStack, operatorStack);
        }

        if (outputStack.Count == 1)
        {
            return outputStack.Pop();
        }
        if (outputStack.Count == 0 && tokens.Count > 1 && tokens[0].Type != TokenType.EOF)
        {
            throw new ArgumentException("Expressão inválida: resultou em uma pilha de saída vazia.");
        }
        if (outputStack.Count == 0 && (tokens.Count == 0 || (tokens.Count == 1 && tokens[0].Type == TokenType.EOF)))
        {
            throw new ArgumentException("A expressão está vazia.");
        }

        throw new ArgumentException($"Expressão inválida ou mal formada. Pilha de saída contém {outputStack.Count} elementos, esperado 1.");
    }

    private void ApplyOperatorFromStack(Stack<IExpression> outputStack, Stack<Token> operatorStack)
    {
        Token opToken = operatorStack.Pop();
        var opMeta = GetOperatorMetadata(opToken);

        if (opMeta == null) throw new InvalidOperationException($"Metadados não encontrados para operador {opToken.Value}");

        if (!_operatorCreators.TryGetValue(opToken.Value, out var creator))
        {
            throw new InvalidOperationException($"Criador não encontrado para o operador {opToken.Value}, embora metadados existam.");
        }

        var operands = new List<IExpression>();
        if (outputStack.Count < opMeta.Arity) throw new InvalidOperationException($"Operandos insuficientes para o operador {opToken.Value}. Esperado: {opMeta.Arity}, Encontrado: {outputStack.Count}");

        for (int i = 0; i < opMeta.Arity; i++)
        {
            operands.Add(outputStack.Pop());
        }
        operands.Reverse();

        IExpression newExpressionNode = creator(operands);
        outputStack.Push(newExpressionNode);
    }

    private void ApplyFunctionCall(Stack<IExpression> outputStack, Stack<Token> operatorStack)
    {
        Token funcToken = operatorStack.Pop();
        if (!_functionCreators.TryGetValue(funcToken.Value, out var funcMetadata))
        {
            throw new InvalidOperationException($"Metadados da função não encontrados para {funcToken.Value}");
        }

        int expectedArity = funcMetadata.Arity;
        var funcCreator = funcMetadata.Creator;

        var args = new List<IExpression>();
        if (expectedArity > 0)
        {
            if (outputStack.Count < expectedArity)
            {
                throw new InvalidOperationException($"Argumentos insuficientes na pilha para a função {funcToken.Value}. Esperado: {expectedArity}, Pilha contém: {outputStack.Count}");
            }
            for (int i = 0; i < expectedArity; i++)
            {
                args.Add(outputStack.Pop());
            }
            args.Reverse();
        }

        IExpression funcExpressionNode = funcCreator(args);
        outputStack.Push(funcExpressionNode);
    }

    /// <summary>
    /// Solves constant expressions, such as "2 + 2" or "sin(0)" and clears as much as possible from the expression tree, leaving only the necessary operators and operands.
    /// At maximum, it will leave a single constant.
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IExpression Optimize(IExpression expression)
    {
        if (expression is Constant || expression is Variable)
        {
            return expression;
        }

        if (expression is IOperator op)
        {
            var operands = op.GetOperands().ToList();
            var optimizedOperands = new List<IExpression>();
            bool allOperandsAreConstant = true;

            foreach (var operand in operands)
            {
                var optimizedOperand = Optimize(operand);
                optimizedOperands.Add(optimizedOperand);
                if (!(optimizedOperand is Constant))
                {
                    allOperandsAreConstant = false;
                }
            }

            bool canConstantEval = false;
            if (op is Function func) canConstantEval = func.ConstantEval;
            else if (op is BinaryOperator || op is UnaryOperator) canConstantEval = true;

            if (allOperandsAreConstant && canConstantEval)
            {
                if (_operatorCreators.TryGetValue(op.Name, out var creator))
                {
                    try
                    {
                        var tempOpWithConstants = creator(optimizedOperands);
                        return new Constant(string.Empty, tempOpWithConstants.Compute());
                    }
                    catch { }
                }
                // Lógica de otimização de função modificada para usar FunctionMetadata
                else if (op is Function opFunc && _functionCreators.TryGetValue(opFunc.Name, out var funcMetadata))
                {
                    try
                    {
                        var funcCreator = funcMetadata.Creator;
                        var tempFuncWithConstants = funcCreator(optimizedOperands);
                        return new Constant(string.Empty, tempFuncWithConstants.Compute());
                    }
                    catch { }
                }
            }

            return expression;
        }
        return expression;
    }
}
