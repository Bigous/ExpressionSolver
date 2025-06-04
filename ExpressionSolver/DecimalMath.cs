using System;

namespace ExpressionSolver;

public static class DecimalMath
{
    public const decimal PI = 3.1415926535897932384626433832m;
    public const decimal PI_OVER_2 = PI / 2m;
    public const decimal THREE_PI_OVER_2 = (3m * PI) / 2m;
    public const decimal TWO_PI = 2m * PI;
    public const decimal E = 2.7182818284590452353602874713m;
    public const decimal LN2 = 0.6931471805599453094172321214m; // ln(2)
    public const decimal INV_LOG10 = 0.4342944819032518276511289189m; // 1 / Log(10)

    public const int MaxIterations = 100; // Ainda pode ser usado por Sqrt
    public const int MaxTerms = 50;    // Ainda pode ser usado por Sin, Cos, Atan

    // Grau do polinômio: 20
    // Intervalo de aproximação para x_poly: (0.5, 1.0]
    // Função aproximada: ln(x_poly)
    // Precisão de trabalho mpmath (dps): 50
    // Número de pontos de ajuste: 10000
    // Erro maximo aferido: 3.783002313e-17
    internal static readonly decimal[] LnCoefficients = new decimal[]
    {
        -27.7820315854339204174599445676875473991892085m,
        438.037437366718703774562014492789921512128798m,
        -3280.75439930850425698092423501934067990851792m,
        15524.3648482283015457635106218447102907364758m,
        -52070.934850915329908194394680547654381561877m,
        131651.968437459610951150883745402991648461708m,
        -260474.43167012822998215660328552413547815159m,
        413225.898599094091121720442678920621461457563m,
        -534289.253998163616138914010930167868054495957m,
        569161.782147278003599907874578033523511870939m,
        -502927.048037110942867952670069593118735055521m,
        369912.843895236976793401215142507216301587112m,
        -226611.972006125250484630411552202841540702065m,
        115378.785619632680910020496924408421668724995m,
        -48582.2403025131592255698879960001426373323988m,
        16783.3913084336510498193085215213693358962407m,
        -4704.6069018851726692802360420085648739734602m,
        1056.03040496846445301475483069925209223408744m,
        -188.337595457091367050901157378904004594437288m,
        28.2019756515320613992408124270308741570566715m,
        -3.94288015730036885177992682296129125213130049m
    };

    // Coeficientes calculados com mpmath(alta precisão):
    // Grau do polinômio: 18
    // Intervalo de aproximação para x_poly: (-0.34657359027997265470861606072908828403775006718013, 0.34657359027997265470861606072908828403775006718013]
    // Função aproximada: exp(x_poly)
    // Precisão de trabalho mpmath (dps): 50
    // Número de pontos de ajuste: 10000
    internal static readonly decimal[] ExpCoefficients = new decimal[]
    {
        0.00000000000000015643281607979890113134415758309075331910053m,
        0.00000000000000281602510137494514395524411525232147680166594m,
        0.0000000000000477947135098845946714303606890422967563343993m,
        0.000000000000764715306730313419444327644132494358575570434m,
        0.000000000011470745605943162254935065419279522615161163m,
        0.000000000160590438504118912916059511410513087855078934m,
        0.0000000020876756987861295963833926161438809286365235m,
        0.0000000250521083854314477938430458195046554794922566m,
        0.00000027557319223985894132820719158342322262209707m,
        0.00000275573192239858953332075445465777058413855885m,
        0.0000248015873015873015862202336780050702226850516m,
        0.000198412698412698412685916242355623749818816723m,
        0.00138888888888888888888890813542304722626539849m,
        0.00833333333333333333333351349289544547007825213m,
        0.0416666666666666666666666664932464055602701821m,
        0.166666666666666666666666665490345152196417609m,
        0.500000000000000000000000000000603899966336168m,
        1.00000000000000000000000000000224321124224682m,
        0.999999999999999999999999999999999654515407177m
    };

    // Grau do polinômio: 20
    // Intervalo de aproximação para x_poly: (0, 1.5707963267948966192313216916397514420985846996876]
    // Função aproximada: sin(x_poly)
    // Precisão de trabalho mpmath (dps): 50
    // Número de pontos de ajuste: 10000
    private static readonly decimal[] SinCoefficients = new decimal[]
    {
        0.000000000000000000288565372967092080154910283554541578014434566m,
        -0.0000000000000000103020613832564404063466084302914382514783212m,
        0.00000000000000000947136763506471723503643888938116479276465938m,
        0.00000000000000278134468141772025648080944177219239433082883m,
        0.0000000000000000706106557676478754247100952007639228335174385m,
        -0.000000000000764842466555375155050852162680558619320503477m,
        0.000000000000000174877325186593568435259188413628363147230693m,
        0.000000000160590247810318726119754451469113162854113794m,
        0.000000000000000164086250092263268782682323128178864974009561m,
        -0.0000000250521084972089544380015783118878271153145241m,
        0.0000000000000000600266596360082826553049804953711783984341511m,
        0.00000275573192237336048418404398192733637116295208m,
        0.00000000000000000819674816624199076105066977741091078583313959m,
        -0.000198412698412700434812690513991005357572954697m,
        0.000000000000000000369314310377482510741962284121311221281237488m,
        0.00833333333333333328514479699927832482955240726m,
        0.00000000000000000000426792936897961832897942732306761820482039266m,
        -0.166666666666666666666904250936577382330473209m,
        0.00000000000000000000000733742415171493728887050293378265681734234652m,
        0.999999999999999999999999900337008328456308806m,
        0.00000000000000000000000000033440456681639416397223173331817625024433154m
    };

    // Grau do polinômio: 20
    // Intervalo de aproximação para x_poly: (0, 1.5707963267948966192313216916397514420985846996876]
    // Função aproximada: cos(x_poly)
    // Precisão de trabalho mpmath (dps): 50
    // Número de pontos de ajuste: 10000
    private static readonly decimal[] CosCoefficients = new decimal[]
    {
        0.000000000000000000288565372964675876144757721059369296791409032m,
        0.00000000000000000123651282535632288308409647339533272800706901m,
        -0.000000000000000162713757206367805794589848571568524226496727m,
        0.0000000000000000225663802737054005185905230900166258873027645m,
        0.0000000000000477388496870792266858161359855504940316057927m,
        0.000000000000000103844166481127080506763727299180822094809675m,
        -0.0000000000114708938721217246038198392343435334822920342m,
        0.000000000000000165254729016519175245451674814746930572371154m,
        0.00000000208767555390491284539611388910175251405397841m,
        0.000000000000000100147269494981989716299526088884850547421463m,
        -0.000000275573192294306787084632995311936231124080892m,
        0.0000000000000000231217389050778387092678585433660248491745217m,
        0.0000248015873015797225153744924764691925011133464m,
        0.00000000000000000188413663700905685307874941278559893745235451m,
        -0.00138888888888888923532054824849077900829709715m,
        0.000000000000000000045471203612873632340007214168102017384538835m,
        0.0416666666666666666626181555448518626673263089m,
        0.000000000000000000000226433594823851701543876673367751884043424392m,
        -0.500000000000000000000007022767414177101786699m,
        0.0000000000000000000000000957563050225891392316764498697182221805826807m,
        0.999999999999999999999999999677571165776348051m
    };

    public static decimal Sin(decimal value)
    {
        // Normalizar para o intervalo [0, 2*PI)
        decimal x = value % TWO_PI;
        if (x < 0m) x += TWO_PI;

        // Redução de argumento para [0, PI/2] para melhor convergência
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

        // Avaliação do polinômio usando o método de Horner
        decimal result = 0;
        foreach (var coeff in SinCoefficients)
        {
            result = result * x + coeff;
        }

        return sign * result;
    }

    public static decimal Cos(decimal value)
    {
        decimal x = value % TWO_PI;
        if (x < 0m) x += TWO_PI;

        decimal sign = 1m;
        if (x > PI_OVER_2 && x <= PI)
        {
            x = PI - x;
            sign = -1m;
        }
        else if (x > PI && x <= THREE_PI_OVER_2)
        {
            x = x - PI;
            sign = -1m;
        }
        else if (x > THREE_PI_OVER_2 && x < TWO_PI)
        {
            x = TWO_PI - x;
        }

        // Avaliação do polinômio usando o método de Horner
        decimal result = 0;
        foreach (var coeff in CosCoefficients)
        {
            result = result * x + coeff;
        }

        return sign * result;
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

        for (int n = 1; n < MaxTerms * 2; n++)
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
        if (temp == 0m)
        {
            return value > 0 ? PI_OVER_2 : -PI_OVER_2;
        }
        decimal sqrtPart = Sqrt(temp);
        if (sqrtPart == 0m)
        {
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
        if (value == E) return 1m; // Otimização para Ln(E) = 1

        // Normalizar 'value' para 's * 2^p' onde s está em [1, 2)
        // ln(value) = p * LN2 + ln(s)
        decimal s = value;
        int p = 0;

        while (s >= 2m) { s /= 2m; p++; }
        while (s < 1m) { s *= 2m; p--; }

        // Agora s está no intervalo [1, 2).
        // Se s == 1m após a normalização (ex: value era uma potência de 2), então ln(s) é 0.
        if (s == 1m) return p * LN2;

        // Para usar o polinômio que aproxima ln(x_poly) para x_poly em (0, 1],
        // usamos a identidade ln(s) = -ln(1/s).
        // Seja x_poly = 1/s. Como s está em (1, 2) (s=1 já tratado),
        // x_poly estará em (0.5, 1). Este intervalo está dentro do domínio do polinômio (0, 1].
        decimal x_poly = 1m / s;

        // Avaliação do polinômio P(x_poly) usando o método de Horner
        // P(x) = c[0]*x^N + c[1]*x^(N-1) + ... + c[N]
        // N = grau = LnMinimaxCoefficients.Length - 1
        // Coeficientes são c[0] para x^30, ..., c[30] para x^0
        //decimal poly_eval_ln_one_over_s = LnCoefficients[0];
        //for (int i = 1; i < LnCoefficients.Length; i++)
        //{
        //    poly_eval_ln_one_over_s = poly_eval_ln_one_over_s * x_poly + LnCoefficients[i];
        //}
        decimal poly_eval_ln_one_over_s = 0;
        foreach (var coeff in LnCoefficients)
        {
            poly_eval_ln_one_over_s = poly_eval_ln_one_over_s * x_poly + coeff;
        }

        // ln(s) = - poly_eval_ln_one_over_s
        decimal ln_s = -poly_eval_ln_one_over_s;

        return p * LN2 + ln_s;
    }

    public static decimal Log10(decimal value)
    {
        return Ln(value) * INV_LOG10;
    }

    public static decimal Log(decimal value, decimal baseValue)
    {
        if (baseValue <= 0m || baseValue == 1m)
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

        // Normalizar para o intervalo válido do polinômio
        // Usamos a identidade: exp(x) = exp(k*ln(2) + r) = 2^k * exp(r)
        // onde r está no intervalo do polinômio (-0.34657... a 0.34657...)
        decimal k_decimal = Math.Round(value / LN2);
        int k = (int)k_decimal;
        decimal r = value - k_decimal * LN2;

        // Verificar se r está dentro do intervalo válido
        const decimal validRange = 0.34657359027997265470861606072908828403775006718013m;
        if (r < -validRange || r > validRange)
        {
            // Ajustar caso esteja fora do intervalo
            k_decimal = Math.Floor(value / LN2);
            k = (int)k_decimal;
            r = value - k_decimal * LN2;
        }

        // Avaliação do polinômio usando o método de Horner
        //decimal result = ExpCoefficients[0];
        //for (int i = 1; i < ExpCoefficients.Length; i++)
        //{
        //    result = result * r + ExpCoefficients[i];
        //}
        decimal result = 0;
        foreach(var coeff in ExpCoefficients)
        {
            result = result * r + coeff;
        }

        // Aplicar o fator de escala 2^k
        decimal twoPowerK = 1m;
        if (k > 0)
        {
            for (int i = 0; i < k; ++i) twoPowerK *= 2m;
        }
        else if (k < 0)
        {
            for (int i = 0; i < -k; ++i) twoPowerK /= 2m;
        }

        return twoPowerK * result;
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

    private static decimal PowBySquaring(decimal baseValue, long exponent)
    {
        if (exponent < 0)
            throw new ArgumentOutOfRangeException(nameof(exponent), "Expoente para PowBySquaring deve ser não negativo.");
        if (exponent == 0) return 1m;
        if (baseValue == 0m) return 0m;

        decimal result = 1m;
        decimal currentPower = baseValue;

        while (exponent > 0)
        {
            if ((exponent % 2) == 1)
            {
                result *= currentPower;
            }
            if (exponent > 1)
            {
                currentPower *= currentPower;
            }
            exponent /= 2;
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