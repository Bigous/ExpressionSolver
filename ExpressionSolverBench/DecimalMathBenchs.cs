using BenchmarkDotNet.Attributes;
using ExpressionSolver;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ExpressionSolverBench;


[MemoryDiagnoser]
public class DecimalMathBenchs
{
    private const decimal Ln10 = 2.3025850929940456840179914547m;
    decimal AlternativePow_01(decimal value, decimal exponent)
    {
        // Casos especiais comuns para retorno rápido
        if (exponent == 0m) return 1m;
        if (value == 0m)
        {
            if (exponent < 0m)
                throw new DivideByZeroException("Não é possível elevar zero a uma potência negativa.");
            return 0m;
        }
        if (value == 1m) return 1m;
        if (value == -1m)
        {
            // Para -1, o resultado depende se o expoente é par ou ímpar
            if (exponent % 2m == 0m) return 1m;
            return -1m; // O expoente é ímpar
        }

        // Verificar se o expoente é um número inteiro
        bool isIntegerExponent = exponent == decimal.Truncate(exponent);

        // Não permite raiz de número negativo (expoente não inteiro para base negativa)
        if (value < 0m && !isIntegerExponent)
            throw new ArgumentOutOfRangeException(nameof(exponent),
                "Não é possível calcular a potência de um número negativo com expoente não inteiro.");

        // Para expoentes inteiros, usar o algoritmo de exponenciação binária (muito mais eficiente)
        if (isIntegerExponent && exponent >= -50m && exponent <= 50m)
        {
            bool isNegativeResult = value < 0m && exponent % 2m != 0m;
            decimal absValue = Math.Abs(value);
            long intExponent = (long)Math.Abs(decimal.Truncate(exponent));
            decimal result = 1m;
            decimal baseValue = absValue;

            // Algoritmo de exponenciação binária
            while (intExponent > 0)
            {
                if ((intExponent & 1) == 1) // Se o bit menos significativo for 1
                    result *= baseValue;

                baseValue *= baseValue;
                intExponent >>= 1; // Divide por 2
            }

            // Se o expoente era negativo, inverter o resultado
            if (exponent < 0m)
                result = 1m / result;

            return isNegativeResult ? -result : result;
        }

        // Casos especiais para expoentes fracionários comuns
        if (exponent == 0.5m)
            return DecimalMath.Sqrt(value);

        if (exponent == -0.5m)
            return 1m / DecimalMath.Sqrt(value);

        // Verificar limites para evitar overflow/underflow
        if (exponent < -65m || exponent > 66m)
            throw new OverflowException("Expoente muito grande ou muito pequeno para Pow.");

        // Usar logaritmos para calcular potências: value^exponent = e^(exponent * ln(value))
        try
        {
            // Para bases positivas, usamos a fórmula padrão
            if (value > 0m)
                return DecimalMath.Exp(exponent * DecimalMath.Ln(value));

            // Para bases negativas (o expoente já foi verificado como inteiro)
            decimal result = DecimalMath.Exp(exponent * DecimalMath.Ln(-value));
            return exponent % 2m != 0m ? -result : result;
        }
        catch (OverflowException)
        {
            throw new OverflowException("Resultado muito grande ou muito pequeno para ser representado como decimal.");
        }

    }
    decimal AlternativePow_02(decimal value, decimal exponent) => (decimal)Math.Pow((double)value, (double)exponent);

#if !NET20 && !NET35 && !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static decimal Exp(decimal m)
    {
        decimal result;
        decimal nextAdd;
        int iteration;
        bool reciprocal;
        decimal t;

        reciprocal = m < 0;
        m = Math.Abs(m);

        t = Math.Truncate(m);

        if (m == 0)
        {
            result = 1;
        }
        else if (m == 1)
        {
            result = DecimalMath.E;
        }
        else if (Math.Abs(m) > 1 && t != m)
        {
            // Split up into integer and fractional
            result = Exp(t) * Exp(m - t);
        }
        else if (m == t)
        {
            // Integer power
            result = ExpBySquaring(DecimalMath.E, m);
        }
        else
        {
            // Fractional power < 1
            // See http://mathworld.wolfram.com/ExponentialFunction.html
            iteration = 0;
            nextAdd = 0;
            result = 0;

            while (true)
            {
                if (iteration == 0)
                {
                    nextAdd = 1;               // == Pow(d, 0) / Factorial(0) == 1 / 1 == 1
                }
                else
                {
                    nextAdd *= m / iteration;  // == Pow(d, iteration) / Factorial(iteration)
                }

                if (nextAdd == 0)
                {
                    break;
                }

                result += nextAdd;

                iteration += 1;
            }
        }

        // Take reciprocal if this was a negative power
        // Note that result will never be zero at this point.
        if (reciprocal)
        {
            result = 1 / result;
        }

        return result;
    }

#if !NET20 && !NET35 && !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static decimal Log(decimal m)
    {
        if (m < 0)
        {
            throw new ArgumentException("Natural logarithm is a complex number for values less than zero!", nameof(m));
        }

        if (m == 0)
        {
            throw new OverflowException("Natural logarithm is defined as negative infinity at zero which the Decimal data type can't represent!");
        }

        if (m == 1)
        {
            return 0;
        }

        if (m >= 1)
        {
            decimal power = 0m;
            System.Math.Log(0d);

            decimal x = m;
            while (x > 1)
            {
                x /= 10;
                power += 1;
            }

            return Log(x) + (power * Ln10);
        }

        // See http://en.wikipedia.org/wiki/Natural_logarithm#Numerical_value
        // for more information on this faster-converging series.

        decimal y;
        decimal ySquared;

        decimal iteration = 0;
        decimal exponent = 0m;
        decimal nextAdd = 0m;
        decimal result = 0m;

        y = (m - 1) / (m + 1);
        ySquared = y * y;

        while (true)
        {
            if (iteration == 0)
            {
                exponent = 2 * y;
            }
            else
            {
                exponent *= ySquared;
            }

            nextAdd = exponent / ((2 * iteration) + 1);

            if (nextAdd == 0)
            {
                break;
            }

            result += nextAdd;

            iteration += 1;
        }

        return result;
    }

#if !NET20 && !NET35 && !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static decimal ExpBySquaring(decimal x, decimal y)
    {
        Debug.Assert(y >= 0 && decimal.Truncate(y) == y, "Only non-negative, integer powers supported.");
        if (y < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(y), "Negative exponents not supported!");
        }

        if (decimal.Truncate(y) != y)
        {
            throw new ArgumentException("Exponent must be an integer!", nameof(y));
        }

        var result = 1m;
        var multiplier = x;

        while (y > 0)
        {
            if ((y % 2) == 1)
            {
                result *= multiplier;
                y -= 1;
                if (y == 0)
                {
                    break;
                }
            }

            multiplier *= multiplier;
            y /= 2;
        }

        return result;
    }

#if !NET20 && !NET35 && !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static decimal AlternativePow_03(decimal x, decimal y)
    {
        decimal result;
        var isNegativeExponent = false;

        // Handle negative exponents
        if (y < 0)
        {
            isNegativeExponent = true;
            y = Math.Abs(y);
        }

        if (y == 0)
        {
            result = 1;
        }
        else if (y == 1)
        {
            result = x;
        }
        else
        {
            var t = decimal.Truncate(y);

            if (y == t)
            {
                // Integer powers
                result = ExpBySquaring(x, y);
            }
            else
            {
                // Fractional power < 1
                // See http://en.wikipedia.org/wiki/Exponent#Real_powers
                // The next line is an optimization of Exp(y * Log(x)) for better precision
                result = ExpBySquaring(x, t) * Exp((y - t) * Log(x));
            }
        }

        if (isNegativeExponent)
        {
            // Note, for IEEE floats this would be Infinity and not an exception...
            if (result == 0)
            {
                throw new OverflowException("Negative power of 0 is undefined!");
            }

            result = 1 / result;
        }

        return result;
    }

    //[Benchmark]
    //public void MathPow_01() => Math.Pow(2.0, 3.0);
    //[Benchmark]
    //public void DecimalMathPow_01() => DecimalMath.Pow(2.0m, 3.0m);
    //[Benchmark]
    //public void AlternativePow01_01() => AlternativePow_01(2.0m, 3.0m);
    //[Benchmark]
    //public void AlternativePow02_01() => AlternativePow_02(2.0m, 3.0m);
    //[Benchmark]
    //public void AlternativePow03_01() => AlternativePow_03(2.0m, 3.0m);
    //[Benchmark]
    //public void MathPow_02() => Math.Pow(2.8, 3.18);
    //[Benchmark]
    //public void DecimalMathPow_02() => DecimalMath.Pow(2.8m, 3.18m);
    //[Benchmark]
    //public void AlternativePow01_02() => AlternativePow_01(2.8m, 3.18m);
    //[Benchmark]
    //public void AlternativePow02_02() => AlternativePow_02(2.8m, 3.18m);
    //[Benchmark]
    //public void AlternativePow03_02() => AlternativePow_03(2.8m, 3.18m);
    //[Benchmark]
    //public void MathPow_03() => Math.Pow(2.8, -3.18);
    //[Benchmark]
    //public void DecimalMathPow_03() => DecimalMath.Pow(2.8m, -3.18m);
    //[Benchmark]
    //public void AlternativePow01_03() => AlternativePow_01(2.8m, -3.18m);
    //[Benchmark]
    //public void AlternativePow02_03() => AlternativePow_02(2.8m, -3.18m);
    //[Benchmark]
    //public void AlternativePow03_03() => AlternativePow_03(2.8m, -3.18m);
    //[Benchmark]
    //public void MathPow_04() => Math.Pow(-1, 30);
    //[Benchmark]
    //public void DecimalMathPow_04() => DecimalMath.Pow(-1, 30);
    //[Benchmark]
    //public void AlternativePow01_04() => AlternativePow_01(-1, 30);
    //[Benchmark]
    //public void AlternativePow02_04() => AlternativePow_02(-1, 30);
    //[Benchmark]
    //public void AlternativePow03_04() => AlternativePow_03(-1, 30);

    public static decimal Ln_tylor(decimal value)
    {
        if (value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(value), "Argumento para Log deve ser positivo.");
        if (value == 1m) return 0m;

        decimal s = value;
        int p = 0;

        while (s >= 2m) { s /= 2m; p++; }
        while (s < 1m) { s *= 2m; p--; }

        decimal y = (s - 1m) / (s + 1m);
        decimal ySquared = y * y;

        decimal termComponent = y;
        decimal sum_logs_s = y;

        for (int n = 1; n < DecimalMath.MaxTerms; n++)
        {
            termComponent *= ySquared;
            decimal termToAdd = termComponent / (2 * n + 1);

            if (sum_logs_s + termToAdd == sum_logs_s)
                break;
            sum_logs_s += termToAdd;
        }
        return 2m * sum_logs_s + p * DecimalMath.LN2;
    }

    public static decimal Exp_tylor(decimal value)
    {
        if (value == 0m) return 1m;

        if (value < -65m) return 0m;
        if (value > 66m) throw new OverflowException("Argumento muito grande para Exp, resultaria em overflow.");

        decimal k_decimal = Math.Round(value / DecimalMath.LN2);
        int k = (int)k_decimal;
        decimal r = value - k_decimal * DecimalMath.LN2;

        decimal term = 1m;
        decimal sum_er = 1m;

        for (int n = 1; n < DecimalMath.MaxTerms; n++)
        {
            term *= r / n;
            if (sum_er + term == sum_er)
                break;
            sum_er += term;
        }

        decimal twoPowerK = 1m;
        if (k > 0)
        {
            for (int i = 0; i < k; i++) twoPowerK *= 2m;
        }
        else if (k < 0)
        {
            // Usar 0.5m para multiplicação em vez de divisão repetida pode ser marginalmente melhor
            // mas a divisão por 2m é exata para decimal.
            for (int i = 0; i < -k; i++) twoPowerK /= 2m;
        }
        return twoPowerK * sum_er;
    }

    public static decimal Sin_tylor(decimal value)
    {
        // Normalizar para o intervalo [0, 2*PI)
        decimal x = value % DecimalMath.TWO_PI;
        if (x < 0m) x += DecimalMath.TWO_PI;

        // Redução de argumento para [0, PI/2] para melhor convergência
        decimal sign = 1m;
        if (x > DecimalMath.PI_OVER_2 && x <= DecimalMath.PI) // Quadrante II
        {
            x = DecimalMath.PI - x;
        }
        else if (x > DecimalMath.PI && x <= DecimalMath.THREE_PI_OVER_2) // Quadrante III
        {
            x = x - DecimalMath.PI;
            sign = -1m;
        }
        else if (x > DecimalMath.THREE_PI_OVER_2 && x < DecimalMath.TWO_PI) // Quadrante IV
        {
            x = DecimalMath.TWO_PI - x;
            sign = -1m;
        }

        decimal term = x;
        decimal sum = x;
        decimal xSquared = x * x;

        for (int n = 1; n < DecimalMath.MaxTerms; n++)
        {
            long factor1 = 2L * n;
            long factor2 = 2L * n + 1;
            term *= -xSquared / (factor1 * factor2);

            if (sum + term == sum)
                break;
            sum += term;
        }
        return sign * sum;
    }

    public static decimal Cos_tylor(decimal value)
    {
        decimal x = value % DecimalMath.TWO_PI;
        if (x < 0m) x += DecimalMath.TWO_PI;

        decimal sign = 1m;
        if (x > DecimalMath.PI_OVER_2 && x <= DecimalMath.PI)
        {
            x = DecimalMath.PI - x;
            sign = -1m;
        }
        else if (x > DecimalMath.PI && x <= DecimalMath.THREE_PI_OVER_2)
        {
            x = x - DecimalMath.PI;
            sign = -1m;
        }
        else if (x > DecimalMath.THREE_PI_OVER_2 && x < DecimalMath.TWO_PI)
        {
            x = DecimalMath.TWO_PI - x;
        }

        decimal term = 1m;
        decimal sum = 1m;
        decimal xSquared = x * x;

        for (int n = 1; n < DecimalMath.MaxTerms; n++)
        {
            long factor1 = 2L * n - 1;
            long factor2 = 2L * n;
            term *= -xSquared / (factor1 * factor2);

            if (sum + term == sum)
                break;
            sum += term;
        }
        return sign * sum;
    }

    public static decimal Atan(decimal value)
    {
        if (value == 0m) return 0m;

        if (value == 1m) return DecimalMath.PI / 4m;
        if (value == -1m) return -DecimalMath.PI / 4m;

        if (Math.Abs(value) > 1m)
        {
            if (value > 1m)
                return DecimalMath.PI_OVER_2 - Atan(1m / value);
            else // value < -1m
                return -DecimalMath.PI_OVER_2 - Atan(1m / value);
        }

        decimal x = value;
        decimal sum = x;
        decimal term = x;
        decimal xSquared = x * x;

        for (int n = 1; n < DecimalMath.MaxTerms * 2; n++)
        {
            term *= -xSquared * (2L * n - 1) / (2L * n + 1);

            if (sum + term == sum)
                break;
            sum += term;
        }
        return sum;
    }

    // Defina os valores a serem testados
    [Params(0.1, 0.5, 0.7, 1.0, 1.5, 2.0, -0.5)]
    public double Value { get; set; }

    [Benchmark]
    public void Ln_MinMax() => DecimalMath.Ln((decimal)Value);

    [Benchmark]
    public void Ln_Tylor() => Ln_tylor((decimal)Value);

    [Benchmark]
    public void Exp_MinMax() => DecimalMath.Exp((decimal)Value);

    [Benchmark]
    public void Exp_Tylor() => Exp_tylor((decimal)Value);

    [Benchmark]
    public void Sin_MinMax() => DecimalMath.Sin((decimal)Value);

    [Benchmark]
    public void Sin_Tylor() => Sin_tylor((decimal)Value);

    [Benchmark]
    public void Cos_MinMax() => DecimalMath.Cos((decimal)Value);

    [Benchmark]
    public void Cos_Tylor() => Cos_tylor((decimal)Value);
    
    [Benchmark]
    public void Atan_Tylor() => Atan((decimal)Value);
}
