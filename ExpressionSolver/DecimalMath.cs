using System;

namespace ExpressionSolver;

public static class DecimalMath
{
    public const decimal PI = 3.14159265358979323846264338327950288419716939937510m;
    public const decimal E = 2.71828182845904523536028747135266249775724709369995m;
    private const decimal LN2 = 0.69314718055994530941723212145818m; // ln(2)
    private const decimal INV_LOG10 = 0.43429448190325182765112891891661m; // 1 / Log(10)

    private const int MaxIterations = 50;
    private const int MaxTerms = 25;

    public static decimal Sin(decimal value)
    {
        // Normalizar para o intervalo [-PI, PI) para melhor convergência e precisão
        decimal x = value % (2 * PI);
        if (x > PI)
            x -= 2 * PI;
        else if (x < -PI)
            x += 2 * PI;

        // Série de Taylor para sin(x): x - x^3/3! + x^5/5! - x^7/7! + ...
        decimal term = x;
        decimal sum = x;
        decimal xSquared = x * x;
        
        for (int n = 1; n < MaxTerms; n++)
        {
            // Próximo termo = -termo_anterior * x^2 / ((2n)*(2n+1))
            // (2n) e (2n+1) são os novos fatores no fatorial.
            // Ex: de x^1/1! para -x^3/3!: termo_novo = termo_velho * (-x^2 / (2*3))
            // Ex: de -x^3/3! para +x^5/5!: termo_novo = termo_velho * (-x^2 / (4*5))
            long factor1 = 2L * n;
            long factor2 = 2L * n + 1;
            term *= -xSquared / (factor1 * factor2);

            if (sum + term == sum) // Convergência alcançada
                break;
            sum += term;
        }
        return sum;
    }

    public static decimal Cos(decimal value)
    {
        // Normalizar para o intervalo [-PI, PI)
        decimal x = value % (2 * PI);
        if (x > PI)
            x -= 2 * PI;
        else if (x < -PI)
            x += 2 * PI;

        // Série de Taylor para cos(x): 1 - x^2/2! + x^4/4! - x^6/6! + ...
        decimal term = 1m; // Primeiro termo é 1 (x^0/0!)
        decimal sum = 1m;
        decimal xSquared = x * x;

        for (int n = 1; n < MaxTerms; n++)
        {
            // Próximo termo = -termo_anterior * x^2 / ((2n-1)*(2n))
            // Ex: de 1 para -x^2/2!: termo_novo = termo_velho * (-x^2 / (1*2))
            // Ex: de -x^2/2! para +x^4/4!: termo_novo = termo_velho * (-x^2 / (3*4))
            long factor1 = 2L * n - 1;
            long factor2 = 2L * n;
            term *= -xSquared / (factor1 * factor2);

            if (sum + term == sum) // Convergência alcançada
                break;
            sum += term;
        }
        return sum;
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
        
        // Usar identidades para reduzir o argumento para |x| <= 1
        // Atan(x) = PI/2 - Atan(1/x) para x > 1
        // Atan(x) = -PI/2 - Atan(1/x) para x < -1
        // Atan(1) = PI/4, Atan(-1) = -PI/4
        if (value == 1m) return PI / 4m;
        if (value == -1m) return -PI / 4m;

        if (Math.Abs(value) > 1m)
        {
            if (value > 1m)
                return PI / 2m - Atan(1m / value);
            else // value < -1m
                return -PI / 2m - Atan(1m / value);
        }

        // Série de Taylor para atan(x): x - x^3/3 + x^5/5 - ... para |x| <= 1
        // Converge lentamente perto de |x|=1, mas a redução ajuda.
        decimal x = value;
        decimal sum = x; // Primeiro termo (n=0)
        decimal term = x; // Termo atual que foi somado
        decimal xSquared = x * x;

        for (int n = 1; n < MaxTerms * 2 ; n++) // Pode precisar de mais termos para Atan
        {
            // term_n = (-1)^n * x^(2n+1) / (2n+1)
            // Próximo termo = -termo_anterior_base * x^2 * (2n-1)/(2n+1)
            // Se 'term' é o termo anterior (-1)^(n-1) * x^(2(n-1)+1) / (2(n-1)+1)
            // O novo termo é term * (-1) * x^2 * (2(n-1)+1) / (2n+1)
            // Simplificando: term_k = x^(2k-1)/(2k-1). O próximo é x^(2k+1)/(2k+1)
            // term_new = term_old_component * x_squared * (old_denominator / new_denominator) * sign_change
            
            // Mais simples:
            // term_0 = x
            // term_1 = -x^3/3
            // term_2 = +x^5/5
            // O termo anterior adicionado foi (-1)^(n-1) * x^(2(n-1)+1) / (2(n-1)+1)
            // O novo termo a ser adicionado é (-1)^n * x^(2n+1) / (2n+1)
            // term_to_add_new = term_to_add_old * (-xSquared) * (2n-1)/(2n+1)
            term *= -xSquared * (2L * n - 1) / (2L * n + 1);
            
            if (sum + term == sum) // Convergência
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
        if (value == 1m) return PI / 2m;
        if (value == -1m) return -PI / 2m;

        // Asin(x) = Atan(x / Sqrt(1 - x^2))
        decimal temp = 1m - value * value;
        // Devido à precisão, temp pode ser zero se value estiver muito próximo de 1 ou -1.
        if (temp == 0m) { // Deveria ter sido pego por value == 1 ou value == -1
             return value > 0 ? PI / 2m : -PI / 2m; // Comportamento consistente
        }
        decimal sqrtPart = Sqrt(temp);
         if (sqrtPart == 0m) { // Denominador zero
            return value > 0 ? PI / 2m : -PI / 2m;
        }
        return Atan(value / sqrtPart);
    }

    public static decimal Acos(decimal value)
    {
        if (value < -1m || value > 1m)
            throw new ArgumentOutOfRangeException(nameof(value), "Argumento para Acos deve estar entre -1 e 1.");
        if (value == 1m) return 0m;
        if (value == -1m) return PI;
        if (value == 0m) return PI / 2m;

        // Acos(x) = PI/2 - Asin(x)
        return PI / 2m - Asin(value);
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
                return PI / 2m;
            else if (y < 0m)
                return -PI / 2m;
            else // y == 0m, x == 0m
                return 0m; // Math.Atan2(0,0) retorna 0.
        }
    }

    public static decimal Log(decimal value)
    {
        if (value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(value), "Argumento para Log deve ser positivo.");
        if (value == 1m) return 0m;

        // Normalizar 'value' para 's * 2^p' onde s está em [1, 2)
        // log(value) = log(s) + p * LN2
        decimal s = value;
        int p = 0; // expoente para base 2

        while (s >= 2m) { s /= 2m; p++; }
        while (s < 1m) { s *= 2m; p--; }

        // Calcular log(s) usando a série para log((1+y)/(1-y)) = 2 * (y + y^3/3 + y^5/5 + ...)
        // Onde y = (s-1)/(s+1). Como s está em [1,2), y está em [0, 1/3).
        decimal y = (s - 1m) / (s + 1m);
        decimal ySquared = y * y;
        
        decimal termComponent = y; // Componente do termo (y, y^3, y^5...)
        decimal sum_logs_s = y;    // Soma da série (y + y^3/3 + ...)

        for (int n = 1; n < MaxTerms; n++)
        {
            termComponent *= ySquared; // Próximo componente y^(2n+1)
            decimal termToAdd = termComponent / (2 * n + 1);
            
            if (sum_logs_s + termToAdd == sum_logs_s) // Convergência
                break;
            sum_logs_s += termToAdd;
        }
        return 2m * sum_logs_s + p * LN2;
    }

    public static decimal Log10(decimal value)
    {
        // Log10(x) = Log(x) / Log(10) = Log(x) * INV_LOG10
        return Log(value) * INV_LOG10;
    }

    public static decimal Exp(decimal value)
    {
        if (value == 0m) return 1m;
        
        // Limites aproximados para evitar overflow/underflow com decimal
        if (value < -65m) return 0m; // e^-65 é muito pequeno
        if (value > 66m) throw new OverflowException("Argumento muito grande para Exp, resultaria em overflow."); // e^66 é muito grande

        // Redução do argumento: x = k * ln(2) + r, onde r está em [-ln(2)/2, ln(2)/2]
        // e^x = 2^k * e^r
        decimal k_decimal = Math.Round(value / LN2);
        int k = (int)k_decimal; // Arredondar para o inteiro mais próximo
        decimal r = value - k_decimal * LN2;

        // Calcular e^r usando a série de Taylor: 1 + r + r^2/2! + r^3/3! + ...
        // r está agora em um intervalo pequeno (aprox. [-0.346, 0.346]), bom para convergência.
        decimal term = 1m;
        decimal sum_er = 1m;

        for (int n = 1; n < MaxTerms; n++) // r é pequeno, converge rápido
        {
            term *= r / n; // term_n = term_{n-1} * r / n
            if (sum_er + term == sum_er) // Precisão alcançada
                break;
            sum_er += term;
        }

        // Calcular 2^k
        decimal twoPowerK = 1m;
        if (k > 0)
        {
            for (int i = 0; i < k; i++) twoPowerK *= 2m;
        }
        else if (k < 0)
        {
            for (int i = 0; i < -k; i++) twoPowerK /= 2m; // Pode ser otimizado para evitar divisão repetida
        }
        return twoPowerK * sum_er;
    }

    public static decimal Sqrt(decimal value)
    {
        if (value < 0m) throw new ArgumentOutOfRangeException(nameof(value), "Não é possível calcular a raiz quadrada de um número negativo.");
        if (value == 0m) return 0m;

        // Método de Newton-Raphson: x_n+1 = (x_n + value / x_n) / 2
        // Estimativa inicial:
        decimal x = value > 1m ? value / 2m : (value + 1m) / 2m;
        if (x == 0m && value > 0m) x = 1m; // Evitar divisão por zero com estimativa ruim para valores muito pequenos

        decimal lastX;
        for (int i = 0; i < MaxIterations; i++)
        {
            lastX = x;
            if (x == 0m) break; // Evitar divisão por zero se x convergir para 0 (improvável se value > 0)
            x = (x + value / x) / 2m;
            if (x == lastX) // Convergência alcançada
                break;
        }
        return x;
    }
}