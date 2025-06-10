namespace ExpressionSolver.UnitTests;

[TestClass]
public sealed class ExtensionsTests
{
    [DataTestMethod]
    [DataRow("1 + 2", 3)]
    [DataRow("10 - 5", 5)]
    [DataRow("3 * 4", 12)]
    [DataRow("8 / 2", 4)]
    [DataRow("5 + 3 * 2", 11)]
    [DataRow("10 - 2 + 1", 9)]
    [DataRow("2 * 3 + 4", 10)]
    [DataRow("10 / 2 - 1", 4)]
    [DataRow("5 + 2 * 3 - 1", 10)]
    [DataRow("10 / 2 + 3 * 2", 11)]
    [DataRow("2 * (3 + 4)", 14)]
    [DataRow("10 - (2 + 3)", 5)]
    [DataRow("5 + (2 * 3) - 1", 10)]
    [DataRow("10 / (2 + 3) + 1", 3)]
    [DataRow("2 * (3 + 4) - 1", 13)]
    [DataRow("10 - (2 + 3) * 2", 0)]
    [DataRow("5 + (2 * 3) / 2", 8)]
    [DataRow("10 / (2 + 3) * 2", 4)]
    [DataRow("2 * (3 + 4) / 2", 7)]
    [DataRow("10 - (2 + 3) / 2", 7.5)]
    [DataRow("10.5 - (2 + 3) / 2", 8)]
    [DataRow("10 % 3", 1)]
    [DataRow("10 ** 2", 100)]
    [DataRow("100 ** 0.5", 10)]
    [DataRow("sqrt(100)", 10)]
    [DataRow("if(sqrt(100) > 9, 15, 16)", 15)]
    public void DefaultContextTests(string expression, double expectedResult)
    {
        decimal result = expression.SolveExpression();
        decimal er = Convert.ToDecimal(expectedResult);
        Assert.AreEqual(er, result, "Default context should solve simple expressions correctly.");
    }

    [DataTestMethod]
    [DataRow("min(5,8) % max(2,3)", 2)]
    public void DefaultContextVariadicArityTests(string expression, double expectedResult)
    {
        decimal result = expression.SolveExpression();
        decimal er = Convert.ToDecimal(expectedResult);
        Assert.AreEqual(er, result, "Default context should solve variadic arity expressions correctly.");
    }
}
