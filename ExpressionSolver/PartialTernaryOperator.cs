
namespace ExpressionSolver;

/// <summary>
/// Representa uma expressão ternária parcialmente formada (condição ? expressãoVerdadeira).
/// Esta é uma classe auxiliar para o processo de parsing.
/// </summary>
internal class PartialTernaryOperator(IExpression condition, IExpression trueBranch) : IOperator
{
    public IExpression Condition { get; } = condition;
    public IExpression TrueBranch { get; } = trueBranch;

    public string Name => "?";

    public int Arity => 2;

    public bool IsRightOperator => true;

    public int Precedence => 2;

    /// <summary>
    /// Esta expressão não deve ser computada diretamente.
    /// </summary>
    public decimal Compute() => throw new InvalidOperationException("PartialTernaryExpression não deve ser computada diretamente.");

    public IEnumerable<IExpression> GetOperands()
    {
        yield return Condition;
        yield return TrueBranch;
    }
}