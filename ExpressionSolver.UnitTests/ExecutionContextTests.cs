using System.Reflection;

namespace ExpressionSolver.UnitTests;

[TestClass]
public class ExecutionContextTests
{
    [TestMethod]
    [DataRow("1 + 2", 4)] // 1, +, 2, EOF
    [DataRow("1 - 2", 4)]
    [DataRow("1 * 2", 4)]
    [DataRow("1 / 2", 4)]
    [DataRow("1 % 2", 4)]
    [DataRow("1 ** 2", 4)]
    [DataRow("1 && 2", 4)]
    [DataRow("1 || 2", 4)]
    [DataRow("1 == 2", 4)]
    [DataRow("(1 * 2)", 6)]
    [DataRow("sqrt(4)", 5)]
    [DataRow("max(4, 3, 2)", 9)]
    public void StandardContext_Tokenize_ShouldReturnCorrectTokens(string expression, int numberOfTokens)
    {
        // Arrange
        var context = ExecutionContext.CreateStandardContext();
        var tokenize = context.GetType().GetMethod("Tokenize", BindingFlags.Instance | BindingFlags.NonPublic);
        // Act
        Assert.IsNotNull(tokenize);
        var result = tokenize.Invoke(context, new object[] { expression });
        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(List<Token>), "Result should be a list of tokens.");
        var tokenList = (List<Token>)result;
        Assert.AreEqual(numberOfTokens, tokenList.Count, "Number of tokens is not correct!");
    }

    [TestMethod]
    [DataRow("1 + 2", 3)] // Constant, Operator, Constant
    [DataRow("1 - 2", 3)]
    [DataRow("1 * 2", 3)]
    [DataRow("1 / 2", 3)]
    [DataRow("1 % 2", 3)]
    [DataRow("1 ** 2", 3)]
    [DataRow("1 && 2", 3)]
    [DataRow("1 || 2", 3)]
    [DataRow("1 == 2", 3)]
    [DataRow("(1 * 2)", 3)]
    [DataRow("sqrt(4)", 2)]
    [DataRow("max(4, 3)", 3)]
    [DataRow("if(2, 3, 4)", 4)]
    public void StandardContext_Compile_ShouldCompileExpression(string expression, int expectedExpressions)
    {
        // Arrange
        var context = ExecutionContext.CreateStandardContext();
        // Act
        var result = context.Compile(expression);
        // Assert
        Assert.IsNotNull(result);
        var numExpressions = GetNumberOfExpressions(result);

        Assert.AreEqual(expectedExpressions, numExpressions, "Number of expressions in the compiled result is not correct!");
    }


    private int GetNumberOfExpressions(IExpression expression)
    {
        if (expression is null) return 0;
        int count = 1; // Count this expression
        switch(expression)
        {
            case IOperator op:
                count += op.GetOperands()
                    .Sum(GetNumberOfExpressions); // Recursively count operands
                break;
        }
        return count;
    }
}
