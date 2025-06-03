using BenchmarkDotNet.Attributes;
using ExpressionSolver;
using System.Globalization;
using ExecutionContext = ExpressionSolver.ExecutionContext;

namespace ExpressionSolverBench;

[MemoryDiagnoser]
public class BuildExpressionTreeOptimizations
{
    static readonly ExecutionContext context = ExecutionContext.CreateStandardContext();
    const string expression = "1 + 2 * 3 - 4 / 2 + sqrt(16) - 3 ** 2 + max(5, 10) - min(3, 7) + abs(-5) + ln(100)";
    IList<Token> tokens = context.Tokenize(expression);

    internal IExpression BuildExpressionTree_01(IList<Token> tokens)
    {
        // Pré-alocação otimizada das pilhas baseada no tamanho dos tokens
        int estimatedCapacity = tokens.Count / 2 + 2;
        var outputStack = new Stack<IExpression>(estimatedCapacity);
        var operatorStack = new Stack<Token>(estimatedCapacity);

        // Cache de metadados para evitar recálculos
        var operatorMetadataCache = new Dictionary<string, ExecutionContext.OperatorMetadata?>(16);

        int tokenIndex = 0;
        int tokenCount = tokens.Count;

        while (tokenIndex < tokenCount)
        {
            Token token = tokens[tokenIndex++];
            if (token.Type == TokenType.EOF) break;

            switch (token.Type)
            {
                case TokenType.Number:
                    if (decimal.TryParse(token.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal numValue))
                        outputStack.Push(new Constant(token.Value, numValue));
                    else
                        throw new ArgumentException($"Número inválido: {token.Value}");
                    break;

                case TokenType.Identifier:
                    if (context.TryGetVariable(token.Value, out var variable))
                        outputStack.Push(variable);
                    else if (context.TryGetConstant(token.Value, out var constant))
                        outputStack.Push(constant);
                    else
                        throw new ArgumentException($"Identificador desconhecido: {token.Value}");
                    break;

                case TokenType.FunctionCall:
                    operatorStack.Push(token);
                    break;

                case TokenType.Comma:
                    while (operatorStack.Count > 0)
                    {
                        var top = operatorStack.Peek();
                        if (top.Type == TokenType.LeftParenthesis || top.Type == TokenType.FunctionCall)
                            break;
                        context.ApplyOperatorFromStack(outputStack, operatorStack);
                    }
                    break;

                case TokenType.UnaryOperator:
                case TokenType.Operator:
                    string opName = token.Value;
                    ExecutionContext.OperatorMetadata? op1Meta;

                    // Usar cache de metadados
                    if (!operatorMetadataCache.TryGetValue(opName, out op1Meta))
                    {
                        op1Meta = context.GetOperatorMetadata(token);
                        operatorMetadataCache[opName] = op1Meta;
                    }

                    if (op1Meta == null)
                        throw new ArgumentException($"Operador desconhecido: {opName}");

                    while (operatorStack.Count > 0)
                    {
                        var topOpToken = operatorStack.Peek();
                        if (topOpToken.Type == TokenType.LeftParenthesis ||
                            topOpToken.Type == TokenType.FunctionCall)
                            break;

                        string topOpName = topOpToken.Value;
                        ExecutionContext.OperatorMetadata? op2Meta;

                        // Usar cache para operador no topo da pilha
                        if (!operatorMetadataCache.TryGetValue(topOpName, out op2Meta))
                        {
                            op2Meta = context.GetOperatorMetadata(topOpToken);
                            operatorMetadataCache[topOpName] = op2Meta;
                        }

                        if (op2Meta == null)
                            break;

                        if ((!op1Meta.IsRightAssociative && op1Meta.Precedence <= op2Meta.Precedence) ||
                            (op1Meta.IsRightAssociative && op1Meta.Precedence < op2Meta.Precedence))
                            context.ApplyOperatorFromStack(outputStack, operatorStack);
                        else
                            break;
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
                            throw new ArgumentException("Operador ternário '?' desbalanceado dentro de parênteses.");

                        context.ApplyOperatorFromStack(outputStack, operatorStack);
                    }

                    if (!foundLeftParen)
                        throw new ArgumentException("Parênteses desbalanceados ( '(' esperado ).");

                    if (operatorStack.Count > 0 && operatorStack.Peek().Type == TokenType.FunctionCall)
                        context.ApplyFunctionCall(outputStack, operatorStack);

                    break;
            }
        }

        while (operatorStack.Count > 0)
        {
            Token topOp = operatorStack.Peek();
            if (topOp.Type == TokenType.LeftParenthesis)
                throw new ArgumentException("Parênteses desbalanceados ( ')' esperado ).");
            if (topOp.Value == "?" || topOp.Value == ":")
                throw new ArgumentException($"Operador ternário incompleto: '{topOp.Value}'");
            if (topOp.Type == TokenType.FunctionCall)
                throw new ArgumentException("Chamada de função mal formada");

            context.ApplyOperatorFromStack(outputStack, operatorStack);
        }

        return outputStack.Count switch
        {
            1 => outputStack.Pop(),
            0 => throw new ArgumentException("Expressão inválida: pilha de saída vazia."),
            _ => throw new ArgumentException($"Expressão inválida. Pilha contém {outputStack.Count} elementos.")
        };
    }

    [Benchmark(Baseline = true)]
    public IExpression BuildExpressionTree_Standard_Eval() => context.BuildExpressionTree(tokens);

    [Benchmark]
    public IExpression BuildExpressionTree_01_Eval() => BuildExpressionTree_01(tokens);
}
