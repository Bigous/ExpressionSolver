using BenchmarkDotNet.Attributes;
using ExpressionSolver;
using System.Text;

using ExecutionContext = ExpressionSolver.ExecutionContext;

namespace ExpressionSolverBench;

[MemoryDiagnoser]
public class TokenizationOptimizations
{
    static readonly ExecutionContext context = ExecutionContext.CreateStandardContext_old();
    const string Expression = "1 + 2 * 3 - 4 / 2 + sqrt(16) - 3 ** 2 + max(5, 10) - min(3, 7) + abs(-5) + ln(100)";

    Dictionary<string, Func<IList<IExpression>, IOperator>> _operatorCreators = new() {
        { "+", a => null! },
        { "-", a => null! },
        { "*", a => null! },
        { "/", a => null! },
        { "%", a => null! },
        { "**", a => null! },
        { "&&", a => null! },
        { "||", a => null! },
        { ">", a => null! },
        { "<", a => null! },
        { ">=", a => null! },
        { "<=", a => null! },
        { "==", a => null! },
        { "!=", a => null! },
        { "!", a => null! },
        { "?", a => null! },
        { ":", a => null! },
    };

    private IList<Token> Tokenize_overall_01(string expression)
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

    private IList<Token> Tokenize_overall_02(string expression)
    {
        // Capacidade inicial: metade do tamanho da expressão (estimativa razoável)
        var tokens = new List<Token>(expression.Length / 2 + 4);
        int i = 0;
        bool expectOperand = true;

        while (i < expression.Length)
        {
            char c = expression[i];

            // Espaços em branco
            if (char.IsWhiteSpace(c)) { i++; continue; }

            // Números (inteiros ou decimais)
            if (char.IsDigit(c) || (c == '.' && i + 1 < expression.Length && char.IsDigit(expression[i + 1])))
            {
                int start = i;
                bool hasDecimal = false;
                while (i < expression.Length && (char.IsDigit(expression[i]) || (expression[i] == '.' && !hasDecimal)))
                {
                    if (expression[i] == '.')
                    {
                        if (hasDecimal) break;
                        hasDecimal = true;
                    }
                    i++;
                }
                tokens.Add(new Token(expression.Substring(start, i - start), TokenType.Number));
                expectOperand = false;
                continue;
            }

            // Identificadores e funções
            if (char.IsLetter(c) || c == '_')
            {
                int start = i;
                while (i < expression.Length && (char.IsLetterOrDigit(expression[i]) || expression[i] == '_')) i++;
                string identifier = expression.Substring(start, i - start);

                // Checa se é função (seguida de '(')
                int tempIdx = i;
                while (tempIdx < expression.Length && char.IsWhiteSpace(expression[tempIdx])) tempIdx++;
                if (tempIdx < expression.Length && expression[tempIdx] == '(')
                    tokens.Add(new Token(identifier, TokenType.FunctionCall));
                else
                    tokens.Add(new Token(identifier, TokenType.Identifier));
                expectOperand = false;
                continue;
            }

            // Parênteses e vírgula
            if (c == '(') { tokens.Add(new Token("(", TokenType.LeftParenthesis)); expectOperand = true; i++; continue; }
            if (c == ')') { tokens.Add(new Token(")", TokenType.RightParenthesis)); expectOperand = false; i++; continue; }
            if (c == ',') { tokens.Add(new Token(",", TokenType.Comma)); expectOperand = true; i++; continue; }

            // Operadores unários +-
            if ((c == '+' || c == '-') && expectOperand)
            {
                string unaryOp = "u" + c;
                if (_operatorCreators.ContainsKey(unaryOp))
                    tokens.Add(new Token(unaryOp, TokenType.UnaryOperator));
                else
                    tokens.Add(new Token(c.ToString(), TokenType.Operator));
                i++;
                continue;
            }

            // Operadores de dois caracteres
            if (i + 1 < expression.Length)
            {
                string twoCharOp = expression.Substring(i, 2);
                if (_operatorCreators.ContainsKey(twoCharOp))
                {
                    tokens.Add(new Token(twoCharOp, TokenType.Operator));
                    i += 2;
                    expectOperand = true;
                    continue;
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

    private IList<Token> Tokenize_overall_03(string expression)
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

    private IList<Token> Tokenize_speed_01(string expression)
    {
        // Pré-aloca uma lista de tokens maior do que o necessário para evitar realocações
        var tokens = new List<Token>(expression.Length + 16);
        int i = 0;
        bool expectOperand = true;

        while (i < expression.Length)
        {
            char c = expression[i];

            // Ignora espaços em branco rapidamente
            if (c <= ' ') { i++; continue; }

            // Números (inteiros ou decimais) - sem usar char.IsDigit para performance
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

            // Identificadores e funções
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_')
            {
                int start = i++;
                while (i < expression.Length &&
                      ((expression[i] >= 'a' && expression[i] <= 'z') ||
                       (expression[i] >= 'A' && expression[i] <= 'Z') ||
                       (expression[i] >= '0' && expression[i] <= '9') ||
                       expression[i] == '_')) i++;

                string identifier = expression[start..i];

                // Verifica se é função (seguida de '(')
                int tempIdx = i;
                while (tempIdx < expression.Length && expression[tempIdx] <= ' ') tempIdx++;

                tokens.Add(new Token(identifier,
                    tempIdx < expression.Length && expression[tempIdx] == '('
                        ? TokenType.FunctionCall
                        : TokenType.Identifier));

                expectOperand = false;
                continue;
            }

            // Parênteses e vírgula
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

            // Operadores unários +-
            if ((c == '+' || c == '-') && expectOperand)
            {
                string op = c == '+' ? "u+" : "u-";
                tokens.Add(new Token(
                    _operatorCreators.ContainsKey(op) ? op : c.ToString(),
                    _operatorCreators.ContainsKey(op) ? TokenType.UnaryOperator : TokenType.Operator));
                i++;
                continue;
            }

            // Operadores de dois caracteres (verificação direta)
            if (i + 1 < expression.Length)
            {
                char nextChar = expression[i + 1];
                if ((c == '*' && nextChar == '*') ||
                    (c == '&' && nextChar == '&') ||
                    (c == '|' && nextChar == '|') ||
                    (c == '>' && nextChar == '=') ||
                    (c == '<' && nextChar == '=') ||
                    (c == '=' && nextChar == '=') ||
                    (c == '!' && nextChar == '='))
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

    private IList<Token> Tokenize_speed_02(string expression)
    {
        // Pré-alocação ainda maior para evitar qualquer realocação
        var tokens = new List<Token>(expression.Length + 32);
        int length = expression.Length;

        // Cache de tokens para os mais comuns (evita alocações)
        var leftParenToken = new Token("(", TokenType.LeftParenthesis);
        var rightParenToken = new Token(")", TokenType.RightParenthesis);
        var commaToken = new Token(",", TokenType.Comma);
        var eofToken = new Token("EOF", TokenType.EOF);

        // Arrays de lookup para operadores comuns
        var singleCharOps = new Token[128]; // ASCII
        var doubleCharOps = new (char second, Token token)[128]; // Possíveis segundos caracteres

        // Pré-preencher operadores de um caractere
        foreach (var op in new[] { "+", "-", "*", "/", "%", ">", "<", "!", "?", ":" })
        {
            if (_operatorCreators.ContainsKey(op))
                singleCharOps[op[0]] = new Token(op, TokenType.Operator);
        }

        // Pré-preencher operadores de dois caracteres
        var twoCharPairs = new[] {
        ('*', '*'), ('&', '&'), ('|', '|'),
        ('>', '='), ('<', '='), ('=', '='), ('!', '=')
    };

        foreach (var (first, second) in twoCharPairs)
        {
            string op = new string(new[] { first, second });
            if (_operatorCreators.ContainsKey(op))
                doubleCharOps[first] = (second, new Token(op, TokenType.Operator));
        }

        int i = 0;
        bool expectOperand = true;

        // Ponteiros para funções específicas que lidam com cada tipo de token
        static int SkipWhitespace(string expr, int pos, int len)
        {
            while (pos < len && expr[pos] <= ' ') pos++;
            return pos;
        }

        while (i < length)
        {
            // Otimização: Pular espaços em branco de uma vez
            i = SkipWhitespace(expression, i, length);
            if (i >= length) break;

            char c = expression[i];

            // Números - reconhecimento rápido com lookup direto
            if ((c >= '0' && c <= '9') || (c == '.' && i + 1 < length && expression[i + 1] >= '0' && expression[i + 1] <= '9'))
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
                } while (i < length &&
                       ((expression[i] >= '0' && expression[i] <= '9') ||
                        (expression[i] == '.' && !hasDecimal)));

                tokens.Add(new Token(expression[start..i], TokenType.Number));
                expectOperand = false;
                continue;
            }

            // Identificadores e funções - verificação direta
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_')
            {
                int start = i++;
                while (i < length &&
                      ((expression[i] >= 'a' && expression[i] <= 'z') ||
                       (expression[i] >= 'A' && expression[i] <= 'Z') ||
                       (expression[i] >= '0' && expression[i] <= '9') ||
                       expression[i] == '_')) i++;

                string identifier = expression[start..i];

                // Verificação otimizada para função
                int tempIdx = i;
                while (tempIdx < length && expression[tempIdx] <= ' ') tempIdx++;

                tokens.Add(new Token(identifier,
                    tempIdx < length && expression[tempIdx] == '('
                        ? TokenType.FunctionCall
                        : TokenType.Identifier));

                expectOperand = false;
                continue;
            }

            // Caracteres especiais com tokens cacheados para evitar alocações
            switch (c)
            {
                case '(':
                    tokens.Add(leftParenToken);
                    expectOperand = true;
                    i++;
                    continue;
                case ')':
                    tokens.Add(rightParenToken);
                    expectOperand = false;
                    i++;
                    continue;
                case ',':
                    tokens.Add(commaToken);
                    expectOperand = true;
                    i++;
                    continue;
            }

            // Operadores unários +- (otimizado)
            if ((c == '+' || c == '-') && expectOperand)
            {
                tokens.Add(new Token(c == '+' ? "u+" : "u-", TokenType.UnaryOperator));
                i++;
                continue;
            }

            // Verificação rápida de operadores de dois caracteres com array de lookup
            if (i + 1 < length && doubleCharOps[c].token != null && expression[i + 1] == doubleCharOps[c].second)
            {
                tokens.Add(doubleCharOps[c].token);
                i += 2;
                expectOperand = true;
                continue;
            }

            // Verificação de operadores de um caractere com array de lookup
            if (singleCharOps[c] != null)
            {
                tokens.Add(singleCharOps[c]);
                i++;
                expectOperand = true;
                continue;
            }

            throw new ArgumentException($"Caractere desconhecido ou operador inválido: '{c}' na posição {i}");
        }

        tokens.Add(eofToken);
        return tokens;
    }

    [Benchmark(Baseline = true)]
    public void Tokenize_overall_01_Evaluation() => Tokenize_overall_01(Expression);

    [Benchmark]
    public void Tokenize_overall_02_Evaluation() => Tokenize_overall_02(Expression);

    [Benchmark]
    public void Tokenize_overall_03_Evaluation() => Tokenize_overall_03(Expression);

    [Benchmark]
    public void Tokenize_speed_01_Evaluation() => Tokenize_speed_01(Expression);

    [Benchmark]
    public void Tokenize_speed_02_Evaluation() => Tokenize_speed_02(Expression);

    [Benchmark]
    public void Tokenize_Current_Evaluation() => context.Tokenize(Expression);

}
