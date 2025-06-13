using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionSolver;

public partial class ExecutionContext
{
    /// <summary>
    /// Holds metadata for a function, including its arity and a creator delegate.
    /// </summary>
    /// <param name="Arity">The number of arguments the function expects. A value of -1 indicates variable arity.</param>
    /// <param name="Creator">A delegate that creates an instance of the function (<see cref="IFunction"/>) given a list of argument expressions.</param>
    internal record FunctionMetadata(int Arity, Func<IList<IExpression>, IFunction> Creator);

    /// <summary>
    /// Holds metadata for an operator, such as its name, precedence, associativity, and arity.
    /// </summary>
    /// <param name="Name">The string representation of the operator (e.g., "+", "u-").</param>
    /// <param name="Precedence">The operator's precedence level. Higher values typically indicate higher precedence.</param>
    /// <param name="IsRightAssociative"><c>true</c> if the operator is right-associative (e.g., exponentiation); <c>false</c> if left-associative (e.g., addition).</param>
    /// <param name="Arity">The number of operands the operator takes (e.g., 1 for unary, 2 for binary).</param>
    /// <param name="IsUnary"><c>true</c> if the operator is unary; otherwise, <c>false</c>. Defaults to <c>false</c>.</param>
    internal record OperatorMetadata(string Name, int Precedence, bool IsRightAssociative, int Arity, bool IsUnary = false);

    /// <summary>
    /// A marker used during parsing to identify the start of arguments for functions with variable arity.
    /// This class is an internal implementation detail of the parsing process and is not meant to be computed.
    /// </summary>
    internal class EndArgumentsMarker : IExpression
    {
        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> as this marker should not be computed.
        /// </summary>
        /// <returns>This method does not return a value as it always throws an exception.</returns>
        /// <exception cref="InvalidOperationException">Always thrown to indicate that an <see cref="EndArgumentsMarker"/> cannot be computed.</exception>
        public decimal Compute()
        {
            throw new InvalidOperationException($"EndArgumentsMarker should not be computed.");
        }
    }

    /// <summary>
    /// Checks if the given identifier exists in the current execution context.
    /// An identifier can be a function, variable, constant, or operator.
    /// </summary>
    /// <param name="identifier">The identifier string to check.</param>
    /// <returns><c>true</c> if the identifier is found in the context; otherwise, <c>false</c>.</returns>
    internal bool HasIdentifier(string identifier) => _functionCreators.ContainsKey(identifier) || _variables.ContainsKey(identifier) || _constants.ContainsKey(identifier) || _operatorCreators.ContainsKey(identifier);

    /// <summary>
    /// Tokenizes a mathematical expression string into a list of <see cref="Token"/> objects.
    /// This is the first step in parsing an expression.
    /// </summary>
    /// <param name="expression">The raw string representation of the mathematical expression.</param>
    /// <returns>A list of tokens derived from the input expression string.</returns>
    /// <exception cref="ArgumentException">Thrown if an unknown character or invalid operator is encountered in the expression string.</exception>
    internal IList<Token> Tokenize(string expression)
    {
        var tokens = new List<Token>(expression.Length / 2 + 4); // Optimized pre-allocation
        int i = 0;
        bool expectOperand = true; // True if an operand is expected next, false if an operator is expected.

        while (i < expression.Length)
        {
            char c = expression[i];

            if (c <= ' ') { i++; continue; } // Skip whitespace

            // Numbers (integer or decimal)
            if ((c >= '0' && c <= '9') || (c == '.' && i + 1 < expression.Length &&
                expression[i + 1] >= '0' && expression[i + 1] <= '9'))
            {
                int start = i;
                bool hasDecimal = false;
                do
                {
                    if (expression[i] == '.')
                    {
                        if (hasDecimal) break; // Second decimal point
                        hasDecimal = true;
                    }
                    i++;
                } while (i < expression.Length &&
                        ((expression[i] >= '0' && expression[i] <= '9') ||
                         (expression[i] == '.' && !hasDecimal))); // Allow one decimal point
                tokens.Add(new Token(expression[start..i], TokenType.Number));
                expectOperand = false;
                continue;
            }

            // Identifiers (variables, constants, function names)
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_')
            {
                int start = i++;
                while (i < expression.Length &&
                      ((expression[i] >= 'a' && expression[i] <= 'z') ||
                       (expression[i] >= 'A' && expression[i] <= 'Z') ||
                       (expression[i] >= '0' && expression[i] <= '9') ||
                       expression[i] == '_')) i++;
                string identifier = expression[start..i];

                // Check if it's a function call (identifier followed by '(')
                int tempIdx = i;
                while (tempIdx < expression.Length && expression[tempIdx] <= ' ') tempIdx++; // Skip whitespace
                tokens.Add(new Token(identifier,
                    tempIdx < expression.Length && expression[tempIdx] == '('
                        ? TokenType.FunctionCall
                        : TokenType.Identifier));
                expectOperand = false;
                continue;
            }

            // Parentheses and comma
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
                    expectOperand = true; // Expect an operand after a comma (next function argument)
                    i++;
                    continue;
            }

            // Unary plus/minus operators
            if ((c == '+' || c == '-') && expectOperand)
            {
                string op = c == '+' ? "u+" : "u-"; // Distinguish unary from binary operators
                tokens.Add(new Token(
                    _operatorCreators.ContainsKey(op) ? op : c.ToString(), // Use "u+" or "u-" if defined, else fallback to "+" or "-"
                    _operatorCreators.ContainsKey(op) ? TokenType.UnaryOperator : TokenType.Operator));
                // expectOperand remains true if unary operator is followed by another operand (e.g. u- (another expression))
                // but for simplicity, the shunting yard will handle this. Here, we've tokenized the unary op.
                i++;
                continue;
            }

            // Two-character operators
            if (i + 1 < expression.Length)
            {
                char nextChar = expression[i + 1];
                string twoCharOpCandidate = new string(new[] { c, nextChar });
                if (_operatorCreators.ContainsKey(twoCharOpCandidate))
                {
                    tokens.Add(new Token(twoCharOpCandidate, TokenType.Operator));
                    i += 2;
                    expectOperand = true;
                    continue;
                }
            }

            // One-character operators
            string oneCharOpCandidate = c.ToString();
            if (_operatorCreators.ContainsKey(oneCharOpCandidate))
            {
                tokens.Add(new Token(oneCharOpCandidate, TokenType.Operator));
                i++;
                expectOperand = true;
                continue;
            }

            throw new ArgumentException($"Unknown character or invalid operator: '{c}' at position {i}");
        }

        tokens.Add(new Token("EOF", TokenType.EOF)); // End-of-expression marker
        return tokens;
    }

    /// <summary>
    /// Retrieves metadata for a given operator token.
    /// This metadata includes precedence, associativity, and arity.
    /// </summary>
    /// <param name="token">The token representing the operator.</param>
    /// <returns>An <see cref="OperatorMetadata"/> record for the token, or <c>null</c> if the operator is not recognized or its metadata cannot be determined.</returns>
    internal OperatorMetadata? GetOperatorMetadata(Token token)
    {
        string opName = token.Value;
        // Standard operators with predefined metadata
        switch (opName)
        {
            case "u+": return new OperatorMetadata(opName, 13, true, 1, true); // Unary Plus
            case "u-": return new OperatorMetadata(opName, 13, true, 1, true); // Unary Minus
            case "||": return new OperatorMetadata(opName, 3, false, 2);  // Logical OR
            case "&&": return new OperatorMetadata(opName, 4, false, 2);  // Logical AND
            case "?": return new OperatorMetadata(opName, 2, true, 2);   // Ternary conditional (part 1)
            case ":": return new OperatorMetadata(opName, 1, false, 2);  // Ternary conditional (part 2)
            case "==": case "!=": return new OperatorMetadata(opName, 8, false, 2); // Equality
            case ">": case "<": case ">=": case "<=": return new OperatorMetadata(opName, 9, false, 2); // Relational
            case "+": case "-": return new OperatorMetadata(opName, 11, false, 2); // Addition, Subtraction
            case "*": case "/": case "%": return new OperatorMetadata(opName, 12, false, 2); // Multiplication, Division, Modulo
            case "**": return new OperatorMetadata(opName, 14, true, 2);  // Exponentiation (Right-associative)
            case "!": return new OperatorMetadata(opName, 14, false, 1, true); // Factorial (typically unary, higher precedence)
            default:
                // For custom operators, try to instantiate them to get metadata
                if (_operatorCreators.TryGetValue(opName, out var creator))
                {
                    try
                    {
                        // Create a dummy instance to retrieve metadata.
                        // This assumes operators can be created with placeholder operands for metadata retrieval.
                        // A more robust way might be to store metadata alongside the creator.
                        var dummyOperands = new IExpression[2] { new Constant(string.Empty, 0), new Constant(string.Empty, 0) };
                        var tempOp = creator(dummyOperands);
                        return new OperatorMetadata(tempOp.Name, tempOp.Precedence, tempOp.IsRightOperator, tempOp.Arity, tempOp is IUnaryOperator);
                    }
                    catch
                    {
                        // If instantiation fails, metadata cannot be determined this way.
                        return null;
                    }
                }
                return null; // Operator not found
        }
    }

    /// <summary>
    /// Builds an expression tree from a list of tokens using the Shunting-yard algorithm.
    /// </summary>
    /// <param name="tokens">A list of <see cref="Token"/> objects representing the tokenized expression.</param>
    /// <returns>The root <see cref="IExpression"/> node of the constructed expression tree.</returns>
    /// <exception cref="ArgumentException">Thrown for various parsing errors, such as mismatched parentheses, invalid number format, or unknown identifiers/operators.</exception>
    /// <exception cref="InvalidOperationException">Thrown for internal errors during tree construction, like missing operator metadata or insufficient operands.</exception>
    internal IExpression BuildExpressionTree(IList<Token> tokens)
    {
        var outputStack = new Stack<IExpression>();    // For operands and sub-expressions
        var operatorStack = new Stack<Token>();        // For operators and function calls
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
                        throw new ArgumentException($"Invalid number format: {token.Value}");
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
                        throw new ArgumentException($"Unknown identifier: {token.Value}");
                    }
                    break;

                case TokenType.FunctionCall:
                    operatorStack.Push(token);
                    // If the next token is a left parenthesis and the function has variable arity,
                    // push a marker onto the output stack to denote the start of its arguments.
                    if (tokenIndex < tokens.Count && tokens[tokenIndex].Type == TokenType.LeftParenthesis &&
                        _functionCreators.TryGetValue(token.Value, out var metadata) && metadata.Arity == -1)
                    {
                        outputStack.Push(new EndArgumentsMarker());
                    }
                    break;

                case TokenType.Comma:
                    // Pop operators from operator stack to output stack until a left parenthesis or function call is found.
                    while (operatorStack.Count > 0 && operatorStack.Peek().Type != TokenType.LeftParenthesis)
                    {
                        if (operatorStack.Peek().Type == TokenType.FunctionCall) break; // Stop if it's the function call itself
                        ApplyOperatorFromStack(outputStack, operatorStack);
                        if (operatorStack.Count > 0 && operatorStack.Peek().Type == TokenType.LeftParenthesis)
                            break; // Stop if a left parenthesis is found (should be part of the current function call)
                    }
                    // Error if no left parenthesis or function call is found (e.g. misplaced comma)
                    if (operatorStack.Count == 0 ||
                        (operatorStack.Peek().Type != TokenType.LeftParenthesis && operatorStack.Peek().Type != TokenType.FunctionCall))
                    {
                        // This condition might indicate a misplaced comma or an issue with function parsing.
                        // Depending on strictness, could throw an error here.
                        // For now, it implies the comma is correctly separating arguments for a function whose token is on the stack.
                    }
                    break;

                case TokenType.UnaryOperator:
                case TokenType.Operator:
                    var op1Meta = GetOperatorMetadata(token);
                    if (op1Meta == null) throw new ArgumentException($"Unknown operator or metadata not found for: {token.Value}");

                    // Shunting-yard: pop operators with higher or equal precedence (for left-associative)
                    // or higher precedence (for right-associative)
                    while (operatorStack.Count > 0)
                    {
                        var topOpToken = operatorStack.Peek();
                        if (topOpToken.Type == TokenType.LeftParenthesis || topOpToken.Type == TokenType.FunctionCall) break;

                        var op2Meta = GetOperatorMetadata(topOpToken);
                        if (op2Meta == null)
                        {
                            // This should ideally not happen if all tokenized operators are valid.
                            throw new InvalidOperationException($"Could not get metadata for operator on stack: {topOpToken.Value}");
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
                            operatorStack.Pop(); // Discard the left parenthesis
                            foundLeftParen = true;
                            break;
                        }
                        // Special handling for ternary '?' to ensure it's not popped prematurely inside parentheses
                        // if its corresponding ':' hasn't been processed.
                        // This check might need refinement based on full ternary operator logic.
                        if (topOp.Value == "?")
                        {
                             throw new ArgumentException("Unbalanced ternary operator '?' found within parentheses without a matching ':'.");
                        }
                        ApplyOperatorFromStack(outputStack, operatorStack);
                    }
                    if (!foundLeftParen) throw new ArgumentException("Mismatched parentheses (expected '(').");

                    // If a function call is at the top of the operator stack, apply it.
                    if (operatorStack.Count > 0 && operatorStack.Peek().Type == TokenType.FunctionCall)
                    {
                        ApplyFunctionCall(outputStack, operatorStack);
                    }
                    break;
            }
        }

        // Pop any remaining operators from the stack to the output.
        while (operatorStack.Count > 0)
        {
            Token topOp = operatorStack.Peek();
            if (topOp.Type == TokenType.LeftParenthesis) throw new ArgumentException("Mismatched parentheses (expected ')').");
            // Check for incomplete ternary operators
            if (topOp.Value == "?" || topOp.Value == ":") throw new ArgumentException($"Incomplete ternary operator: '{topOp.Value}' found at end of expression.");
            if (topOp.Type == TokenType.FunctionCall) throw new ArgumentException("Malformed function call (likely missing parentheses or arguments).");

            ApplyOperatorFromStack(outputStack, operatorStack);
        }

        // The final expression tree should be the single item left on the output stack.
        if (outputStack.Count == 1)
        {
            return outputStack.Pop();
        }
        if (outputStack.Count == 0 && tokens.Count > 1 && tokens[0].Type != TokenType.EOF) // More than just EOF token
        {
            throw new ArgumentException("Invalid expression: resulted in an empty output stack.");
        }
        if (outputStack.Count == 0 && (tokens.Count == 0 || (tokens.Count == 1 && tokens[0].Type == TokenType.EOF))) // Empty or just EOF
        {
            throw new ArgumentException("The expression is empty.");
        }
        // If more than one item, the expression was malformed (e.g., "2 3" without an operator).
        throw new ArgumentException($"Invalid or malformed expression. Output stack has {outputStack.Count} items, expected 1.");
    }

    /// <summary>
    /// Applies an operator from the operator stack to operands from the output stack.
    /// Pops the operator, pops the required number of operands, creates the expression node,
    /// and pushes the new node back onto the output stack.
    /// </summary>
    /// <param name="outputStack">The stack containing operands and sub-expressions.</param>
    /// <param name="operatorStack">The stack containing operators.</param>
    /// <exception cref="InvalidOperationException">Thrown if operator metadata or creator is not found, or if there are insufficient operands.</exception>
    internal void ApplyOperatorFromStack(Stack<IExpression> outputStack, Stack<Token> operatorStack)
    {
        Token opToken = operatorStack.Pop();
        var opMeta = GetOperatorMetadata(opToken);

        if (opMeta == null) throw new InvalidOperationException($"Metadata not found for operator {opToken.Value}");

        if (!_operatorCreators.TryGetValue(opToken.Value, out var creator))
        {
            // This case should be rare if GetOperatorMetadata and Tokenize are consistent.
            throw new InvalidOperationException($"Creator not found for operator {opToken.Value}, though metadata exists.");
        }

        var operands = new List<IExpression>();
        if (outputStack.Count < opMeta.Arity) throw new InvalidOperationException($"Insufficient operands for operator {opToken.Value}. Expected: {opMeta.Arity}, Found: {outputStack.Count}");

        for (int i = 0; i < opMeta.Arity; i++)
        {
            operands.Add(outputStack.Pop());
        }
        operands.Reverse(); // Operands are popped in reverse order

        IExpression newExpressionNode = creator(operands);
        outputStack.Push(newExpressionNode);
    }

    /// <summary>
    /// Applies a function call from the operator stack to its arguments from the output stack.
    /// Pops the function token, collects its arguments (handling fixed and variable arity),
    /// creates the function expression node, and pushes it onto the output stack.
    /// </summary>
    /// <param name="outputStack">The stack containing arguments and sub-expressions.</param>
    /// <param name="operatorStack">The stack containing the function call token.</param>
    /// <exception cref="InvalidOperationException">Thrown if the function is unknown, arity mismatches, or parsing errors occur (like unexpected EndArgumentsMarker).</exception>
    internal void ApplyFunctionCall(Stack<IExpression> outputStack, Stack<Token> operatorStack)
    {
        Token funcToken = operatorStack.Pop();
        string funcName = funcToken.Value;

        if (!_functionCreators.TryGetValue(funcName, out var metadata))
        {
            throw new InvalidOperationException($"Unknown function: {funcName}");
        }

        List<IExpression> args = new();

        if (metadata.Arity == -1) // Variable arity function
        {
            // Pop arguments until an EndArgumentsMarker is found or stack is empty (error if marker not found appropriately)
            while (outputStack.Count > 0)
            {
                var expr = outputStack.Pop();
                if (expr is EndArgumentsMarker)
                {
                    break; // Found the marker for this function call
                }
                args.Insert(0, expr); // Add to the beginning to maintain order
            }
            // If EndArgumentsMarker was expected but not found, it's an error (e.g. function(a,b without closing parenthesis)
            // This is implicitly handled by the main loop's parenthesis matching.
        }
        else // Fixed arity function
        {
            if (metadata.Arity > 0) // Functions with 1 or more arguments
            {
                if (outputStack.Count < metadata.Arity)
                {
                    throw new InvalidOperationException($"Insufficient operands on stack for fixed-arity function '{funcName}'. Expected: {metadata.Arity}, Found on stack: {outputStack.Count}");
                }
                for (int i = 0; i < metadata.Arity; i++)
                {
                    // Crucial check to prevent consuming a marker from an outer variable-arity function
                    if (outputStack.Peek() is EndArgumentsMarker)
                    {
                        throw new InvalidOperationException($"Parsing error: Fixed-arity function '{funcName}' encountered an unexpected EndArgumentsMarker while collecting argument {i + 1} of {metadata.Arity}. This indicates a problem with function nesting or a stray EndArgumentsMarker.");
                    }
                    args.Insert(0, outputStack.Pop());
                }
            }
            // If metadata.Arity == 0, args list remains empty, which is correct for no-argument functions.
        }

        IFunction function = metadata.Creator(args);

        // Validate arity if the function itself has a fixed arity defined (post-creation)
        // This is a safeguard, as metadata.Arity should align.
        if (function.Arity >= 0 && function.Arity != args.Count)
        {
            throw new InvalidOperationException($"Function '{funcName}' expects {function.Arity} {(function.Arity == 1 ? "parameter" : "parameters")}, but received {args.Count}.");
        }

        outputStack.Push(function);
    }
}
