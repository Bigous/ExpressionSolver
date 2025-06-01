namespace ExpressionSolver;

internal enum TokenType
{
    Number,         // Ex: 123, 45.67
    Identifier,     // Ex: sin, myVar, PI
    Operator,       // Ex: +, -, *, ?, :
    LeftParenthesis,// (
    RightParenthesis,// )
    Comma,          // ,
    UnaryOperator,  // Para distinguir operadores unários como -5 de binários a - b
    FunctionCall,   // Marcador para nome de função antes de seus argumentos
    EOF             // Fim da expressão
}

internal record Token(string Value, TokenType Type);