using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionSolver;

public partial class ExecutionContext
{
    internal record FunctionMetadata(int Arity, Func<IList<IExpression>, IFunction> Creator);
    internal record OperatorMetadata(string Name, int Precedence, bool IsRightAssociative, int Arity, bool IsUnary = false);

    /// <summary>
    /// Marcador utilizado para identificar o início dos argumentos de funções com aridade variável
    /// </summary>
    internal class EndArgumentsMarker : IExpression
    {
        public decimal Compute()
        {
            throw new InvalidOperationException($"Marcador de fim de argumentos não deve ser computado");
        }
    }

    internal bool HasIdentifier(string identifier) => _functionCreators.ContainsKey(identifier) || _variables.ContainsKey(identifier) || _constants.ContainsKey(identifier) || _operatorCreators.ContainsKey(identifier);

    internal IList<Token> Tokenize(string expression)
    {
        // Pré-alocação otimizada da lista de tokens
        var tokens = new List<Token>(expression.Length / 2 + 4);
        int i = 0;
        bool expectOperand = true;

        while (i < expression.Length)
        {
            char c = expression[i];

            // Espaços em branco - verificação rápida
            if (c <= ' ') { i++; continue; }

            // Números (inteiros ou decimais) - verificação direta em vez de char.IsDigit()
            if ((c >= '0' && c <= '9') || (c == '.' && i + 1 < expression.Length &&
                expression[i + 1] >= '0' && expression[i + 1] <= '9'))
            {
                int start = i;
                bool hasDecimal = false;

                do
                {
                    if (expression[i] == '.')
                    {
                        if (hasDecimal) break;
                        hasDecimal = true;
                    }
                    i++;
                } while (i < expression.Length &&
                        ((expression[i] >= '0' && expression[i] <= '9') ||
                         (expression[i] == '.' && !hasDecimal)));

                tokens.Add(new Token(expression[start..i], TokenType.Number));
                expectOperand = false;
                continue;
            }

            // Identificadores e funções - verificação otimizada
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_')
            {
                int start = i++;

                while (i < expression.Length &&
                      ((expression[i] >= 'a' && expression[i] <= 'z') ||
                       (expression[i] >= 'A' && expression[i] <= 'Z') ||
                       (expression[i] >= '0' && expression[i] <= '9') ||
                       expression[i] == '_')) i++;

                string identifier = expression[start..i];

                // Verificar se é função (otimização de verificação)
                int tempIdx = i;
                while (tempIdx < expression.Length && expression[tempIdx] <= ' ') tempIdx++;

                tokens.Add(new Token(identifier,
                    tempIdx < expression.Length && expression[tempIdx] == '('
                        ? TokenType.FunctionCall
                        : TokenType.Identifier));

                expectOperand = false;
                continue;
            }

            // Parênteses e vírgula - switch para melhor desempenho
            switch (c)
            {
                case '(':
                    tokens.Add(new Token("(", TokenType.LeftParenthesis));
                    expectOperand = true;
                    i++;
                    continue;
                case ')':
                    tokens.Add(new Token(")", TokenType.RightParenthesis));
                    expectOperand = false;
                    i++;
                    continue;
                case ',':
                    tokens.Add(new Token(",", TokenType.Comma));
                    expectOperand = true;
                    i++;
                    continue;
            }

            // Operadores unários +- (otimizado)
            if ((c == '+' || c == '-') && expectOperand)
            {
                string op = c == '+' ? "u+" : "u-";
                tokens.Add(new Token(
                    _operatorCreators.ContainsKey(op) ? op : c.ToString(),
                    _operatorCreators.ContainsKey(op) ? TokenType.UnaryOperator : TokenType.Operator));
                i++;
                continue;
            }

            // Operadores de dois caracteres - verificação direta
            if (i + 1 < expression.Length)
            {
                char nextChar = expression[i + 1];
                // Verificação rápida para operadores comuns de 2 caracteres
                if ((c == '*' && nextChar == '*') ||  // **
                    (c == '&' && nextChar == '&') ||  // &&
                    (c == '|' && nextChar == '|') ||  // ||
                    (c == '>' && nextChar == '=') ||  // >=
                    (c == '<' && nextChar == '=') ||  // <=
                    (c == '=' && nextChar == '=') ||  // ==
                    (c == '!' && nextChar == '='))    // !=
                {
                    string op = new string(new[] { c, nextChar });
                    if (_operatorCreators.ContainsKey(op))
                    {
                        tokens.Add(new Token(op, TokenType.Operator));
                        i += 2;
                        expectOperand = true;
                        continue;
                    }
                }
            }

            // Operadores de um caractere
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

    internal OperatorMetadata? GetOperatorMetadata(Token token)
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
                        var tempOp = creator(new IExpression[2] { new Constant(string.Empty, 0), new Constant(string.Empty, 0) });
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

    internal IExpression BuildExpressionTree(IList<Token> tokens)
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
                    // Se a próxima token for um parêntese esquerdo e a função tiver aridade variável,
                    // adiciona um marcador na pilha de saída
                    if (tokenIndex < tokens.Count && tokens[tokenIndex].Type == TokenType.LeftParenthesis &&
                        _functionCreators.TryGetValue(token.Value, out var metadata) && metadata.Arity == -1)
                    {
                        outputStack.Push(new EndArgumentsMarker());
                    }
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

    internal void ApplyOperatorFromStack(Stack<IExpression> outputStack, Stack<Token> operatorStack)
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

    internal void ApplyFunctionCall(Stack<IExpression> outputStack, Stack<Token> operatorStack)
    {
        Token funcToken = operatorStack.Pop();
        string funcName = funcToken.Value;

        if (!_functionCreators.TryGetValue(funcName, out var metadata))
        {
            throw new InvalidOperationException($"Função desconhecida: {funcName}");
        }

        List<IExpression> args = new();

        if (metadata.Arity == -1) // Função de aridade variável
        {
            while (outputStack.Count > 0)
            {
                var expr = outputStack.Pop();
                if (expr is EndArgumentsMarker)
                {
                    break;
                }
                args.Insert(0, expr);
            }
        }
        else // Função de aridade fixa
        {
            if (metadata.Arity > 0)
            {
                if (outputStack.Count < metadata.Arity)
                {
                    throw new InvalidOperationException($"Operandos insuficientes na pilha para a função de aridade fixa '{funcName}'. Esperado: {metadata.Arity}, Encontrado na pilha: {outputStack.Count}");
                }
                for (int i = 0; i < metadata.Arity; i++)
                {
                    // Verificação crucial para evitar consumir um marcador de uma função externa
                    if (outputStack.Peek() is EndArgumentsMarker)
                    {
                        throw new InvalidOperationException($"Erro de parsing: Função de aridade fixa '{funcName}' encontrou um EndArgumentsMarker inesperadamente ao coletar o argumento {i + 1} de {metadata.Arity}. Isso indica um problema com o aninhamento de funções ou um EndArgumentsMarker perdido.");
                    }
                    args.Insert(0, outputStack.Pop());
                }
            }
            // Se metadata.Arity == 0, args permanece vazio, o que é correto.
        }

        IFunction function = metadata.Creator(args);

        if (function.Arity >= 0 && function.Arity != args.Count)
        {
            throw new InvalidOperationException($"Função '{funcName}' espera {function.Arity} {(function.Arity == 1 ? "parâmetro" : "parâmetros")}, mas recebeu {args.Count}.");
        }

        outputStack.Push(function);
    }
}
