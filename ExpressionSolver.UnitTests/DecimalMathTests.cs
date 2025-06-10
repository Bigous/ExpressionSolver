using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ExpressionSolver;

namespace ExpressionSolver.UnitTests;

[TestClass]
public class DecimalMathTests
{
    private const decimal Tolerance = 0.0000000000001m;

    private decimal RoundInput(double value)
    {
        return Math.Round((decimal)value, 13);
    }

    [DataTestMethod]
    [DataRow(0.0)]
    [DataRow(0.52360)]  // PI/6
    [DataRow(0.78540)]  // PI/4
    [DataRow(1.04720)]  // PI/3
    [DataRow(1.57080)]  // PI/2
    [DataRow(3.14159)]  // PI
    [DataRow(-0.52360)] // -PI/6
    [DataRow(0.25000)]
    [DataRow(-0.25000)]
    public void Sin_ShouldMatchMathSin(double val)
    {
        decimal inputValue = RoundInput(val);
        decimal expected = (decimal)Math.Sin((double)inputValue);
        decimal actual = DecimalMath.Sin(inputValue);
        Assert.AreEqual(expected, actual, Tolerance, $"Sin({inputValue})");
    }

    [DataTestMethod]
    [DataRow(0.0)]
    [DataRow(0.52360)]  // PI/6
    [DataRow(0.78540)]  // PI/4
    [DataRow(1.04720)]  // PI/3
    [DataRow(1.57080)]  // PI/2
    [DataRow(3.14159)]  // PI
    [DataRow(-0.52360)] // -PI/6
    [DataRow(0.25000)]
    [DataRow(-0.25000)]
    public void Cos_ShouldMatchMathCos(double val)
    {
        decimal inputValue = RoundInput(val);
        decimal expected = (decimal)Math.Cos((double)inputValue);
        decimal actual = DecimalMath.Cos(inputValue);
        Assert.AreEqual(expected, actual, Tolerance, $"Cos({inputValue})");
    }

    [DataTestMethod]
    [DataRow(0.0)]
    [DataRow(0.52360)]  // PI/6
    [DataRow(0.78540)]  // PI/4
    [DataRow(-0.52360)] // -PI/6
    [DataRow(0.25000)]
    [DataRow(-0.25000)]
    [DataRow(1.00000)]
    public void Tan_ShouldMatchMathTan(double val)
    {
        decimal inputValue = RoundInput(val);
        decimal expected = (decimal)Math.Tan((double)inputValue);
        decimal actual = DecimalMath.Tan(inputValue);
        Assert.AreEqual(expected, actual, Tolerance, $"Tan({inputValue})");
    }

    [DataTestMethod]
    [DataRow(0.0)]
    [DataRow(0.50000)]
    [DataRow(1.00000)]
    [DataRow(-0.50000)]
    [DataRow(-1.00000)]
    [DataRow(2.00000)]
    [DataRow(-2.00000)]
    [DataRow(0.25000)]
    public void Atan_ShouldMatchMathAtan(double val)
    {
        decimal inputValue = RoundInput(val);
        decimal expected = (decimal)Math.Atan((double)inputValue);
        decimal actual = DecimalMath.Atan(inputValue);
        Assert.AreEqual(expected, actual, Tolerance, $"Atan({inputValue})");
    }

    [DataTestMethod]
    [DataRow(0.0)]
    [DataRow(0.25000)]
    [DataRow(0.50000)]
    [DataRow(0.75000)]
    [DataRow(1.00000)]
    [DataRow(-0.25000)]
    [DataRow(-0.50000)]
    [DataRow(-0.75000)]
    [DataRow(-1.00000)]
    public void Asin_ShouldMatchMathAsin(double val)
    {
        decimal inputValue = RoundInput(val);
        // Math.Asin pode retornar NaN para valores fora de [-1,1] após conversão double->decimal->double.
        // A implementação DecimalMath.Asin lança exceção para valores fora do intervalo.
        // Garantir que o input está estritamente dentro do intervalo para Math.Asin se o input original for +/-1.0
        double doubleInput = (double)inputValue;
        if (doubleInput > 1.0) doubleInput = 1.0;
        if (doubleInput < -1.0) doubleInput = -1.0;

        decimal expected = (decimal)Math.Asin(doubleInput);
        decimal actual = DecimalMath.Asin(inputValue);
        Assert.AreEqual(expected, actual, Tolerance, $"Asin({inputValue})");
    }

    [DataTestMethod]
    [DataRow(0.0)]
    [DataRow(0.25000)]
    [DataRow(0.50000)]
    [DataRow(0.75000)]
    [DataRow(1.00000)]
    [DataRow(-0.25000)]
    [DataRow(-0.50000)]
    [DataRow(-0.75000)]
    [DataRow(-1.00000)]
    public void Acos_ShouldMatchMathAcos(double val)
    {
        decimal inputValue = RoundInput(val);
        // Similar ao Asin, garantir que o input para Math.Acos está no intervalo.
        double doubleInput = (double)inputValue;
        if (doubleInput > 1.0) doubleInput = 1.0;
        if (doubleInput < -1.0) doubleInput = -1.0;

        decimal expected = (decimal)Math.Acos(doubleInput);
        decimal actual = DecimalMath.Acos(inputValue);
        Assert.AreEqual(expected, actual, Tolerance, $"Acos({inputValue})");
    }

    [DataTestMethod]
    [DataRow(0.0, 1.0)]
    [DataRow(1.0, 1.0)]
    [DataRow(1.0, 0.0)]
    [DataRow(1.0, -1.0)]
    [DataRow(0.0, -1.0)]
    [DataRow(-1.0, -1.0)]
    [DataRow(-1.0, 0.0)]
    [DataRow(-1.0, 1.0)]
    [DataRow(0.0, 0.0)]
    [DataRow(0.50000, 0.50000)]
    [DataRow(0.25000, -0.75000)]
    [DataRow(-0.60000, -0.30000)]
    public void Atan2_ShouldMatchMathAtan2(double y, double x)
    {
        decimal inputY = RoundInput(y);
        decimal inputX = RoundInput(x);
        decimal expected = (decimal)Math.Atan2((double)inputY, (double)inputX);
        decimal actual = DecimalMath.Atan2(inputY, inputX);
        Assert.AreEqual(expected, actual, Tolerance, $"Atan2({inputY}, {inputX})");
    }

    [DataTestMethod]
    [DataRow(1.0)]
    [DataRow(2.71828)]  // E
    [DataRow(2.00000)]
    [DataRow(10.00000)]
    [DataRow(0.50000)]
    [DataRow(0.10000)]
    [DataRow(25.00000)]
    public void Ln_ShouldMatchMathLog(double val)
    {
        decimal inputValue = RoundInput(val);
        decimal expected = (decimal)Math.Log((double)inputValue);
        decimal actual = DecimalMath.Ln_MinMax(inputValue);
        Assert.AreEqual(expected, actual, Tolerance, $"Log({inputValue})");
    }

    [DataTestMethod]
    [DataRow(1.0)]
    [DataRow(10.00000)]
    [DataRow(100.00000)]
    [DataRow(0.50000)]
    [DataRow(0.10000)]
    [DataRow(2.00000)]
    [DataRow(50.00000)]
    public void Log10_ShouldMatchMathLog10(double val)
    {
        decimal inputValue = RoundInput(val);
        decimal expected = (decimal)Math.Log10((double)inputValue);
        decimal actual = DecimalMath.Log10(inputValue);
        Assert.AreEqual(expected, actual, Tolerance, $"Log10({inputValue})");
    }

    [DataTestMethod]
    [DataRow(1.0, 2)]
    [DataRow(10.00000, 4)]
    [DataRow(100.00000, 3)]
    [DataRow(0.50000, 5)]
    [DataRow(0.10000, 8)]
    [DataRow(2.00000, 10)]
    [DataRow(50.00000,12)]
    public void Log_ShouldMatchMathLog10(double val, double b)
    {
        decimal inputValue = RoundInput(val);
        decimal expected = (decimal)Math.Log((double)inputValue, b);
        decimal actual = DecimalMath.Log(inputValue, RoundInput(b));
        Assert.AreEqual(expected, actual, Tolerance, $"Log({inputValue}, {b})");
    }
    [DataTestMethod]
    [DataRow(0.0)]
    [DataRow(1.00000)]
    [DataRow(-1.00000)]
    [DataRow(2.00000)]
    [DataRow(-2.00000)]
    [DataRow(0.50000)]
    [DataRow(-0.50000)]
    [DataRow(0.69315)]  // ln(2)
    public void Exp_ShouldMatchMathExp(double val)
    {
        decimal inputValue = RoundInput(val);
        // DecimalMath.Exp tem limites para evitar overflow que são mais restritos que Math.Exp
        // Testar apenas dentro da faixa de DecimalMath.Exp (aprox -65 a 66)
        if (inputValue > 60m || inputValue < -60m) // Ajustar se necessário, para ficar bem dentro dos limites
        {
            Assert.Inconclusive($"Valor de entrada {inputValue} está fora da faixa de teste segura para Exp.");
            return;
        }

        decimal expected = (decimal)Math.Exp((double)inputValue);
        decimal actual = DecimalMath.Exp_MinMax(inputValue);
        Assert.AreEqual(expected, actual, Tolerance, $"Exp({inputValue})");
    }

    [DataTestMethod]
    [DataRow(0.0)]
    [DataRow(1.00000)]
    [DataRow(4.00000)]
    [DataRow(2.00000)]
    [DataRow(0.25000)]
    [DataRow(0.50000)]
    [DataRow(100.00000)]
    [DataRow(9.00000)]
    public void Sqrt_ShouldMatchMathSqrt(double val)
    {
        decimal inputValue = RoundInput(val);
        decimal expected = (decimal)Math.Sqrt((double)inputValue);
        decimal actual = DecimalMath.Sqrt(inputValue);
        Assert.AreEqual(expected, actual, Tolerance, $"Sqrt({inputValue})");
    }

    [DataTestMethod]
    [DataRow(2.0, 3.0)]
    [DataRow(5.0, 2.0)]
    [DataRow(10.0, -1.0)]
    [DataRow(0.0, 0.0)] // Caso especial, 0^0 é indefinido, mas DecimalMath.Pow define como 1
    [DataRow(100.0, 0.5)] // Testando raiz quadrada
    public void Pow_ShouldMatchMathPow(double val, double exp)
    {
        decimal inputValue = RoundInput(val);
        decimal exponent = RoundInput(exp);
        // DecimalMath.Pow tem limites para evitar overflow que são mais restritos que Math.Pow
        decimal expected = (decimal)Math.Pow((double)inputValue, (double)exponent);
        decimal actual = DecimalMath.Pow(inputValue, exponent);
        Assert.AreEqual(expected, actual, Tolerance, $"Pow({inputValue}, {exponent})");
    }
}
