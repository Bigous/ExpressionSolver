using System;

namespace ExpressionSolver;

public static class DecimalMath
{
    public const decimal PI = 3.1415926535897932384626433832m;
    public const decimal PI_OVER_2 = PI / 2m; 
    public const decimal THREE_PI_OVER_2 = (3m * PI) / 2m;
    public const decimal TWO_PI = 2m * PI;
    public const decimal E = 2.7182818284590452353602874713m;
    private const decimal LN2 = 0.6931471805599453094172321214m; // ln(2)
    private const decimal INV_LOG10 = 0.4342944819032518276511289189m; // 1 / Log(10)

    private const int MaxIterations = 100;
    private const int MaxTerms = 50;

    public static decimal Sin(decimal value)
    {
        // Normalizar para o intervalo [0, 2*PI)
        decimal x = value % TWO_PI;
        if (x < 0m) x += TWO_PI;

        // Redução de argumento para [0, PI/2] para melhor convergência
        // sin(x) para x em [0, PI/2]
        // sin(x) para x em (PI/2, PI]  =>  sin(PI - x)
        // sin(x) para x em (PI, 3PI/2] => -sin(x - PI)
        // sin(x) para x em (3PI/2, 2PI) => -sin(2PI - x)
        decimal sign = 1m;
        if (x > PI_OVER_2 && x <= PI) // Quadrante II
        {
            x = PI - x;
        }
        else if (x > PI && x <= THREE_PI_OVER_2) // Quadrante III
        {
            x = x - PI;
            sign = -1m;
        }
        else if (x > THREE_PI_OVER_2 && x < TWO_PI) // Quadrante IV
        {
            x = TWO_PI - x;
            sign = -1m;
        }
        // Se x está em [0, PI/2], nenhuma mudança em x ou sinal.

        // Série de Taylor para sin(x): x - x^3/3! + x^5/5! - x^7/7! + ...
        // Agora x está em [0, PI/2], onde a série converge mais rapidamente.
        decimal term = x;
        decimal sum = x;
        decimal xSquared = x * x;
        
        for (int n = 1; n < MaxTerms; n++)
        {
            long factor1 = 2L * n;
            long factor2 = 2L * n + 1;
            term *= -xSquared / (factor1 * factor2);

            if (sum + term == sum) // Convergência alcançada
                break;
            sum += term;
        }
        return sign * sum;
    }

    public static decimal Cos(decimal value)
    {
        // Normalizar para o intervalo [0, 2*PI)
        decimal x = value % TWO_PI;
        if (x < 0m) x += TWO_PI;

        // Redução de argumento para [0, PI/2]
        // cos(x) para x em [0, PI/2]
        // cos(x) para x em (PI/2, PI]  => -cos(PI - x)
        // cos(x) para x em (PI, 3PI/2] => -cos(x - PI)
        // cos(x) para x em (3PI/2, 2PI) =>  cos(2PI - x)
        decimal sign = 1m;
        if (x > PI_OVER_2 && x <= PI) // Quadrante II
        {
            x = PI - x;
            sign = -1m;
        }
        else if (x > PI && x <= THREE_PI_OVER_2) // Quadrante III
        {
            x = x - PI;
            sign = -1m;
        }
        else if (x > THREE_PI_OVER_2 && x < TWO_PI) // Quadrante IV
        {
            x = TWO_PI - x;
            // sinal permanece 1m
        }
        // Se x está em [0, PI/2], nenhuma mudança em x ou sinal.

        // Série de Taylor para cos(x): 1 - x^2/2! + x^4/4! - x^6/6! + ...
        // Agora x está em [0, PI/2]
        decimal term = 1m; // Primeiro termo é 1 (x^0/0!)
        decimal sum = 1m;
        decimal xSquared = x * x;

        for (int n = 1; n < MaxTerms; n++)
        {
            long factor1 = 2L * n - 1;
            long factor2 = 2L * n;
            term *= -xSquared / (factor1 * factor2);

            if (sum + term == sum) // Convergência alcançada
                break;
            sum += term;
        }
        return sign * sum;
    }

    public static decimal Tan(decimal value)
    {
        decimal cosVal = Cos(value);
        if (cosVal == 0m)
            throw new DivideByZeroException("Tangente não definida: cosseno do argumento é zero.");
        return Sin(value) / cosVal;
    }

    public static decimal Atan(decimal value)
    {
        if (value == 0m) return 0m;
        
        if (value == 1m) return PI / 4m;
        if (value == -1m) return -PI / 4m;

        if (Math.Abs(value) > 1m)
        {
            if (value > 1m)
                return PI_OVER_2 - Atan(1m / value);
            else // value < -1m
                return -PI_OVER_2 - Atan(1m / value);
        }

        decimal x = value;
        decimal sum = x; 
        decimal term = x; 
        decimal xSquared = x * x;

        for (int n = 1; n < MaxTerms * 2 ; n++) 
        {
            term *= -xSquared * (2L * n - 1) / (2L * n + 1);
            
            if (sum + term == sum) 
                break;
            sum += term;
        }
        return sum;
    }

    public static decimal Asin(decimal value)
    {
        if (value < -1m || value > 1m)
            throw new ArgumentOutOfRangeException(nameof(value), "Argumento para Asin deve estar entre -1 e 1.");
        if (value == 0m) return 0m;
        if (value == 1m) return PI_OVER_2;
        if (value == -1m) return -PI_OVER_2;

        decimal temp = 1m - value * value;
        if (temp == 0m) { 
             return value > 0 ? PI_OVER_2 : -PI_OVER_2; 
        }
        decimal sqrtPart = Sqrt(temp);
         if (sqrtPart == 0m) { 
            return value > 0 ? PI_OVER_2 : -PI_OVER_2;
        }
        return Atan(value / sqrtPart);
    }

    public static decimal Acos(decimal value)
    {
        if (value < -1m || value > 1m)
            throw new ArgumentOutOfRangeException(nameof(value), "Argumento para Acos deve estar entre -1 e 1.");
        if (value == 1m) return 0m;
        if (value == -1m) return PI;
        if (value == 0m) return PI_OVER_2;

        return PI_OVER_2 - Asin(value);
    }

    public static decimal Atan2(decimal y, decimal x)
    {
        if (x > 0m)
        {
            return Atan(y / x);
        }
        else if (x < 0m)
        {
            if (y >= 0m)
                return Atan(y / x) + PI;
            else // y < 0m
                return Atan(y / x) - PI;
        }
        else // x == 0m
        {
            if (y > 0m)
                return PI_OVER_2;
            else if (y < 0m)
                return -PI_OVER_2;
            else // y == 0m, x == 0m
                return 0m; 
        }
    }

    public static decimal Ln(decimal value)
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

        for (int n = 1; n < MaxTerms; n++)
        {
            termComponent *= ySquared; 
            decimal termToAdd = termComponent / (2 * n + 1);
            
            if (sum_logs_s + termToAdd == sum_logs_s) 
                break;
            sum_logs_s += termToAdd;
        }
        return 2m * sum_logs_s + p * LN2;
    }

    public static decimal Log10(decimal value)
    {
        return Ln(value) * INV_LOG10;
    }

    public static decimal Log(decimal value, decimal baseValue)
    {
        if (baseValue <= 0m || baseValue == 1m) // Corrigido: baseValue não pode ser 1.
            throw new ArgumentOutOfRangeException(nameof(baseValue), "Base do logaritmo deve ser positiva e diferente de 1.");
        if (value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(value), "Valor do logaritmo deve ser positivo.");
        return Ln(value) / Ln(baseValue);
    }

    public static decimal Exp(decimal value)
    {
        if (value == 0m) return 1m;
        
        if (value < -65m) return 0m; 
        if (value > 66m) throw new OverflowException("Argumento muito grande para Exp, resultaria em overflow."); 

        decimal k_decimal = Math.Round(value / LN2);
        int k = (int)k_decimal; 
        decimal r = value - k_decimal * LN2;

        decimal term = 1m;
        decimal sum_er = 1m;

        for (int n = 1; n < MaxTerms; n++) 
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

    public static decimal Sqrt(decimal value)
    {
        if (value < 0m) throw new ArgumentOutOfRangeException(nameof(value), "Não é possível calcular a raiz quadrada de um número negativo.");
        if (value == 0m) return 0m;

        decimal x = value > 1m ? value / 2m : (value + 1m) / 2m;
        if (x == 0m && value > 0m) x = 1m; 

        decimal lastX;
        for (int i = 0; i < MaxIterations; i++)
        {
            lastX = x;
            if (x == 0m) break; 
            x = (x + value / x) / 2m;
            if (x == lastX) 
                break;
        }
        return x;
    }
    
    // Método auxiliar para exponenciação por quadratura (para expoentes inteiros não negativos)
    private static decimal PowBySquaring(decimal baseValue, long exponent)
    {
        if (exponent < 0)
            throw new ArgumentOutOfRangeException(nameof(exponent), "Expoente para PowBySquaring deve ser não negativo.");
        if (exponent == 0) return 1m;
        if (baseValue == 0m) return 0m; // Já tratado em Pow, mas por segurança

        decimal result = 1m;
        decimal currentPower = baseValue;

        while (exponent > 0)
        {
            if ((exponent % 2) == 1) // Se o bit menos significativo for 1
            {
                result *= currentPower;
            }
            if (exponent > 1) // Evita calcular currentPower *= currentPower na última iteração se não for necessário
            {
                currentPower *= currentPower; // Quadrado da base
            }
            exponent /= 2; // Desloca bits do expoente para a direita
        }
        return result;
    }

    public static decimal Pow(decimal value, decimal exponent)
    {
        if (exponent == 0m) return 1m;
        if (value == 0m)
        {
            if (exponent < 0m)
                throw new ArgumentOutOfRangeException(nameof(exponent), "Não é possível calcular 0 elevado a uma potência negativa.");
            return 0m; 
        }

        // Caso para expoente inteiro
        if (exponent == Math.Truncate(exponent)) 
        {
            long intExponent = (long)exponent;
            const long practicalExponentLimit = 100000; 

            if (Math.Abs(intExponent) > practicalExponentLimit)
                 throw new OverflowException($"Expoente inteiro ({intExponent}) excede o limite prático para Pow.");

            if (intExponent < 0)
            {
                return 1m / PowBySquaring(value, -intExponent);
            }
            else
            {
                return PowBySquaring(value, intExponent);
            }
        }

        // Caso para expoente não inteiro
        if (value < 0m) 
            throw new ArgumentOutOfRangeException(nameof(exponent), "Não é possível calcular a potência de um número negativo com expoente não inteiro.");
        
        return Exp(exponent * Ln(value));
    }
}