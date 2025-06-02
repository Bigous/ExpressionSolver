using System.Reflection;

namespace ExpressionSolver.UnitTests;

[TestClass]
public class ExecutionContextTests
{
    [DataTestMethod]
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
    [DataRow("1 + 2 * 3 - 4 / 2 + sqrt(16) - 3 ** 2 + max(5, 10) - min(3, 7) + abs(-5) + log(100)", 44)]
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

    [DataTestMethod]
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
    [DataRow("abs(-5)", 3)]
    [DataRow("1 + 2 * 3 - 4 / 2 + sqrt(16) - 3 ** 2 + max(5, 10) - min(3, 7) + abs(-5) + log(100)", 31)]
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

    [TestMethod]
    public void StandardContext_Compile_ShouldThrowOnInvalidExpression()
    {
        // Arrange
        var context = ExecutionContext.CreateStandardContext();
        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() => context.Compile("1 + "));
    }

    [TestMethod]
    public void StandardContext_Compile_ShouldThrowOnInvalidFunction()
    {
        // Arrange
        var context = ExecutionContext.CreateStandardContext();
        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() => context.Compile("invalidFunction(1, 2)"));
    }

    [TestMethod]
    public void StandardContext_Optimize_ShouldOptimizeExpression()
    {
        // Arrange
        var context = ExecutionContext.CreateStandardContext();
        var expression = "1 + 2 * 3 - 4 / 2"; // Should optimize to 5
        // Act
        var compiledExpression = context.Compile(expression);
        var optimizedExpression = context.Optimize(compiledExpression);
        // Assert
        Assert.IsNotNull(optimizedExpression);
        var numExpressions = GetNumberOfExpressions(optimizedExpression);
        
        Assert.AreEqual(1, numExpressions, "Optimized expression did not minimize expressions.");
        Assert.IsInstanceOfType<Constant>(optimizedExpression, "Optimized expression should be a Constant.");
        Assert.AreEqual(5m, optimizedExpression.Compute(), "Optimized expression did not compute to the expected value.");
    }

    [TestMethod]
    public void CustomContexWithVariable_ShouldCompileAndCompute()
    {
        // Arrange
        var context = ExecutionContext.CreateStandardContext();
        context.TryAddVariable("x", 10m);
        context.TryAddVariable("y", 20m);
        
        // Act
        var expression = "x + y"; // Should compile to a BinaryOperator
        var compiledExpression = context.Compile(expression);
        
        // Assert
        Assert.IsNotNull(compiledExpression);
        Assert.IsInstanceOfType<BinaryOperator>(compiledExpression, "Compiled expression should be a BinaryOperator.");
        Assert.AreEqual(30m, compiledExpression.Compute(), "Compiled expression did not compute to the expected value.");
    }

    [TestMethod]
    public void CustomContextWithVariable_ShouldThrowOnUndefinedVariable()
    {
        // Arrange
        var context = ExecutionContext.CreateStandardContext();
        
        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => context.Compile("undefinedVariable + 1"));
    }

    [TestMethod]
    public void CustomContextWihVariable_ShoudNotOptimizeAndCompute()
    {
        // Arrange
        var context = ExecutionContext.CreateStandardContext();
        context.TryAddVariable("x", 10m);
        context.TryAddVariable("y", 20m);
        
        // Act
        var expression = "10 + x + y * 4"; // Should compile to a BinaryOperator and cannot be optimized
        var compiledExpression = context.Compile(expression);
        var numExpressions = GetNumberOfExpressions(compiledExpression);
        var optimizedExpression = context.Optimize(compiledExpression);
        var numExpressionsOptimized = GetNumberOfExpressions(optimizedExpression);

        // Assert
        Assert.IsNotNull(optimizedExpression);
        Assert.IsInstanceOfType<BinaryOperator>(optimizedExpression, "Optimized expression should be a Constant.");
        Assert.AreEqual(100m, optimizedExpression.Compute(), "Optimized expression did not compute to the expected value.");
        Assert.AreEqual(numExpressions, numExpressionsOptimized, "Optimize over optimized the expression.");
    }

    [DataTestMethod]
    [DataRow("44 * 38 + x * y ** 2 * 7")]
    [DataRow("x * y * 25 * 7 + 44 * 38")]
    [DataRow("sqrt(x * x + y * y + abs(-5))")]
    public void CustomContextWithVariable_ShouldOptimizeAndCompute(string expression)
    {
        // Arrange
        var context = ExecutionContext.CreateStandardContext();
        context.TryAddVariable("x", 10m);
        context.TryAddVariable("y", 20m);
        
        // Act
        var compiledExpression = context.Compile(expression);
        var numExpressions = GetNumberOfExpressions(compiledExpression);
        var expectedValue = compiledExpression.Compute();
        var optimizedExpression = context.Optimize(compiledExpression);
        var numExpressionsOptimized = GetNumberOfExpressions(optimizedExpression);
        // Assert
        Assert.IsNotNull(optimizedExpression);
        Assert.AreEqual(expectedValue, optimizedExpression.Compute(), "Optimized expression did not compute to the expected value.");
        Assert.IsTrue(numExpressionsOptimized < numExpressions, "Optimize did not reduce the number of expressions as expected.");
    }

    [TestMethod]
    public void CustomContextWithVariable_ShouldThrowOnInvalidExpression()
    {
        // Arrange
        var context = ExecutionContext.CreateStandardContext();
        context.TryAddVariable("x", 10m);
        
        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() => context.Compile("x + "));
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
