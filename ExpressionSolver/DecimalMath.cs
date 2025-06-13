using System;

namespace ExpressionSolver;

/// <summary>
/// Provides mathematical functions and constants for <see cref="decimal"/> types.
/// This class aims to offer higher precision alternatives or decimal-specific implementations
/// for common math operations.
/// </summary>
public static class DecimalMath
{
    /// <summary>
    /// Represents the mathematical constant Pi (π).
    /// </summary>
    public const decimal PI = 3.1415926535897932384626433832m;

    /// <summary>
    /// Represents Pi (π) divided by 2.
    /// </summary>
    public const decimal PI_OVER_2 = PI / 2m;

    /// <summary>
    /// Represents three times Pi (3π) divided by 2.
    /// </summary>
    public const decimal THREE_PI_OVER_2 = (3m * PI) / 2m;

    /// <summary>
    /// Represents two times Pi (2π).
    /// </summary>
    public const decimal TWO_PI = 2m * PI;

    /// <summary>
    /// Represents the mathematical constant e (Euler's number).
    /// </summary>
    public const decimal E = 2.7182818284590452353602874713m;

    /// <summary>
    /// Represents the natural logarithm of 2 (ln(2)).
    /// </summary>
    public const decimal LN2 = 0.6931471805599453094172321214m;

    /// <summary>
    /// Represents 1 divided by the natural logarithm of 10 (1 / ln(10)), used for converting natural log to base-10 log.
    /// This is equivalent to log_10(e).
    /// </summary>
    public const decimal INV_LOG10 = 0.4342944819032518276511289189m;

    /// <summary>
    /// Maximum number of iterations for iterative algorithms like Sqrt.
    /// </summary>
    public const int MaxIterations = 100;

    /// <summary>
    /// Maximum number of terms for series expansions (e.g., in Sin, Cos, Atan Taylor series).
    /// </summary>
    public const int MaxTerms = 50;

    // Polynomial coefficients for Ln approximation.
    // Degree: 20, Approximation interval for x_poly: (0.5, 1.0], Target function: ln(x_poly)
    // Max observed error: 3.78e-17
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

    // Polynomial coefficients for Exp approximation.
    // Degree: 18, Approximation interval for x_poly: (-0.34657..., 0.34657...], Target function: exp(x_poly)
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

    // Polynomial coefficients for Sin approximation.
    // Degree: 20, Approximation interval for x_poly: (0, PI/2], Target function: sin(x_poly)
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

    // Polynomial coefficients for Cos approximation.
    // Degree: 20, Approximation interval for x_poly: (0, PI/2], Target function: cos(x_poly)
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

    /// <summary>
    /// Computes the sine of the specified angle.
    /// Angle is assumed to be in radians.
    /// Uses polynomial approximation for values in the range [0, PI/2] after argument reduction.
    /// </summary>
    /// <param name="value">An angle, measured in radians.</param>
    /// <returns>The sine of <paramref name="value"/>.</returns>
    public static decimal Sin(decimal value)
    {
        // Normalize to the interval [0, 2*PI)
        decimal x = value % TWO_PI;
        if (x < 0m) x += TWO_PI;

        // Argument reduction to [0, PI/2] for better convergence
        decimal sign = 1m;
        if (x > PI_OVER_2 && x <= PI) // Quadrant II
        {
            x = PI - x;
        }
        else if (x > PI && x <= THREE_PI_OVER_2) // Quadrant III
        {
            x = x - PI;
            sign = -1m;
        }
        else if (x > THREE_PI_OVER_2 && x < TWO_PI) // Quadrant IV
        {
            x = TWO_PI - x;
            sign = -1m;
        }

        // Polynomial evaluation using Horner's method
        decimal result = 0;
        foreach (var coeff in SinCoefficients)
        {
            result = result * x + coeff;
        }

        return sign * result;
    }

    /// <summary>
    /// Computes the cosine of the specified angle.
    /// Angle is assumed to be in radians.
    /// Uses polynomial approximation for values in the range [0, PI/2] after argument reduction.
    /// </summary>
    /// <param name="value">An angle, measured in radians.</param>
    /// <returns>The cosine of <paramref name="value"/>.</returns>
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

        // Polynomial evaluation using Horner's method
        decimal result = 0;
        foreach (var coeff in CosCoefficients)
        {
            result = result * x + coeff;
        }

        return sign * result;
    }

    /// <summary>
    /// Computes the tangent of the specified angle.
    /// Angle is assumed to be in radians.
    /// </summary>
    /// <param name="value">An angle, measured in radians.</param>
    /// <returns>The tangent of <paramref name="value"/>.</returns>
    /// <exception cref="DivideByZeroException">Thrown if the cosine of <paramref name="value"/> is zero.</exception>
    public static decimal Tan(decimal value)
    {
        decimal cosVal = Cos(value);
        if (cosVal == 0m)
            throw new DivideByZeroException("Tangent undefined: cosine of the argument is zero.");
        return Sin(value) / cosVal;
    }

    /// <summary>
    /// Computes the angle whose tangent is the specified number (arctangent).
    /// Uses Taylor series expansion for |value| &lt;= 1, and identities for |value| &gt; 1.
    /// </summary>
    /// <param name="value">A number representing a tangent.</param>
    /// <returns>An angle, θ, measured in radians, such that -π/2 ≤ θ ≤ π/2.
    /// Returns <see cref="decimal.Zero"/> if <paramref name="value"/> is 0.
    /// </returns>
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

    /// <summary>
    /// Computes the angle whose sine is the specified number (arcsine).
    /// </summary>
    /// <param name="value">A number representing a sine, where -1 ≤ value ≤ 1.</param>
    /// <returns>An angle, θ, measured in radians, such that -π/2 ≤ θ ≤ π/2.
    /// Returns NaN if <paramref name="value"/> &lt; -1 or <paramref name="value"/> &gt; 1.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is less than -1 or greater than 1.</exception>
    public static decimal Asin(decimal value)
    {
        if (value < -1m || value > 1m)
            throw new ArgumentOutOfRangeException(nameof(value), "Argument for Asin must be between -1 and 1.");
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

    /// <summary>
    /// Computes the angle whose cosine is the specified number (arccosine).
    /// </summary>
    /// <param name="value">A number representing a cosine, where -1 ≤ value ≤ 1.</param>
    /// <returns>An angle, θ, measured in radians, such that 0 ≤ θ ≤ π.
    /// Returns NaN if <paramref name="value"/> &lt; -1 or <paramref name="value"/> &gt; 1.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is less than -1 or greater than 1.</exception>
    public static decimal Acos(decimal value)
    {
        if (value < -1m || value > 1m)
            throw new ArgumentOutOfRangeException(nameof(value), "Argument for Acos must be between -1 and 1.");
        if (value == 1m) return 0m;
        if (value == -1m) return PI;
        if (value == 0m) return PI_OVER_2;

        return PI_OVER_2 - Asin(value);
    }

    /// <summary>
    /// Computes the angle whose tangent is the quotient of two specified numbers (y/x).
    /// This method correctly determines the quadrant of the angle.
    /// </summary>
    /// <param name="y">The y-coordinate of a point.</param>
    /// <param name="x">The x-coordinate of a point.</param>
    /// <returns>An angle, θ, measured in radians, such that -π &lt; θ ≤ π, and tan(θ) = <paramref name="y"/>/<paramref name="x"/>
    /// where (<paramref name="x"/>, <paramref name="y"/>) is a point in the Cartesian plane.
    /// Returns 0 if (x,y) is (0,0).
    /// </returns>
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

    /// <summary>
    /// Computes the natural (base e) logarithm of a specified number using Minimax polynomial approximation.
    /// </summary>
    /// <param name="value">A positive number.</param>
    /// <returns>The natural logarithm of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is negative or zero.</exception>
    public static decimal Ln_MinMax(decimal value)
    {
        if (value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(value), "Argument for Log (Ln) must be positive.");
        if (value == 1m) return 0m;
        if (value == E) return 1m; // Optimization for Ln(E) = 1

        decimal s = value;
        int p = 0;

        while (s >= 2m) { s /= 2m; p++; }
        while (s < 1m) { s *= 2m; p--; }

        if (s == 1m) return p * LN2;

        decimal x_poly = 1m / s;

        decimal poly_eval_ln_one_over_s = LnCoefficients[0];
        for (int i = 1; i < LnCoefficients.Length; i++)
        {
            poly_eval_ln_one_over_s = poly_eval_ln_one_over_s * x_poly + LnCoefficients[i];
        }

        decimal ln_s = -poly_eval_ln_one_over_s;
        return p * LN2 + ln_s;
    }

    /// <summary>
    /// Computes the natural (base e) logarithm of a specified number using Taylor series expansion.
    /// </summary>
    /// <param name="value">A positive number.</param>
    /// <returns>The natural logarithm of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is negative or zero.</exception>
    public static decimal Ln_tylor(decimal value)
    {
        if (value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(value), "Argument for Log (Ln) must be positive.");
        if (value == 1m) return 0m;

        decimal s = value;
        int p = 0;

        while (s >= 2m) { s /= 2m; p++; }
        while (s < 1m) { s *= 2m; p--; }

        decimal y = (s - 1m) / (s + 1m);
        decimal ySquared = y * y;

        decimal termComponent = y;
        decimal sum_logs_s = y;

        var nTerms = DecimalMath.MaxTerms + 10;

        for (int n = 1; n < nTerms; n++)
        {
            termComponent *= ySquared;
            decimal termToAdd = termComponent / (2 * n + 1);

            if (sum_logs_s + termToAdd == sum_logs_s)
                break;
            sum_logs_s += termToAdd;
        }
        return 2m * sum_logs_s + p * DecimalMath.LN2;
    }

    /// <summary>
    /// Computes e (Euler's number) raised to the specified power using Taylor series expansion.
    /// </summary>
    /// <param name="value">A number specifying a power.</param>
    /// <returns>The number e raised to the power <paramref name="value"/>.</returns>
    /// <exception cref="OverflowException">Thrown if <paramref name="value"/> is too large or too small, causing overflow/underflow.</exception>
    public static decimal Exp_tylor(decimal value)
    {
        if (value == 0m) return 1m;

        if (value < -65m) return 0m;
        if (value > 66m) throw new OverflowException("Argument too large for Exp, would result in overflow.");

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
            for (int i = 0; i < -k; i++) twoPowerK /= 2m;
        }
        return twoPowerK * sum_er;
    }

    /// <summary>
    /// Computes the base-10 logarithm of a specified number.
    /// </summary>
    /// <param name="value">A positive number.</param>
    /// <returns>The base-10 logarithm of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is negative or zero.</exception>
    public static decimal Log10(decimal value)
    {
        return Ln_MinMax(value) * INV_LOG10;
    }

    /// <summary>
    /// Computes the logarithm of a specified number in a specified base.
    /// </summary>
    /// <param name="value">A positive number (the argument of the logarithm).</param>
    /// <param name="baseValue">A positive number specifying the base of the logarithm (must not be 1).</param>
    /// <returns>The logarithm of <paramref name="value"/> in base <paramref name="baseValue"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is negative or zero,
    /// or if <paramref name="baseValue"/> is negative, zero, or one.</exception>
    public static decimal Log(decimal value, decimal baseValue)
    {
        if (baseValue <= 0m || baseValue == 1m)
            throw new ArgumentOutOfRangeException(nameof(baseValue), "Base of the logarithm must be positive and not equal to 1.");
        if (value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(value), "Argument of the logarithm must be positive.");
        return Ln_MinMax(value) / Ln_MinMax(baseValue);
    }

    /// <summary>
    /// Computes e (Euler's number) raised to the specified power using Minimax polynomial approximation.
    /// </summary>
    /// <param name="value">A number specifying a power.</param>
    /// <returns>The number e raised to the power <paramref name="value"/>.</returns>
    /// <exception cref="OverflowException">Thrown if <paramref name="value"/> is too large or too small, causing overflow.</exception>
    public static decimal Exp_MinMax(decimal value)
    {
        if (value == 0m) return 1m;
        if (value < -65m) return 0m;
        if (value > 66m) throw new OverflowException("Argument too large for Exp, would result in overflow.");

        decimal k_decimal = Math.Round(value / LN2);
        int k = (int)k_decimal;
        decimal r = value - k_decimal * LN2;

        const decimal validRange = 0.34657359027997265470861606072908828403775006718013m;
        if (r < -validRange || r > validRange)
        {
            k_decimal = Math.Floor(value / LN2);
            k = (int)k_decimal;
            r = value - k_decimal * LN2;
        }

        decimal result = 0;
        foreach(var coeff in ExpCoefficients)
        {
            result = result * r + coeff;
        }

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

    /// <summary>
    /// Computes the square root of a specified number using Newton's method.
    /// </summary>
    /// <param name="value">A non-negative number.</param>
    /// <returns>The square root of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is negative.</exception>
    public static decimal Sqrt(decimal value)
    {
        if (value < 0m) throw new ArgumentOutOfRangeException(nameof(value), "Cannot calculate the square root of a negative number.");
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

    /// <summary>
    /// Computes a base raised to an integer exponent using exponentiation by squaring.
    /// Internal helper for Pow.
    /// </summary>
    /// <param name="baseValue">The base value.</param>
    /// <param name="exponent">A non-negative integer exponent.</param>
    /// <returns>The result of <paramref name="baseValue"/> raised to the power of <paramref name="exponent"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if exponent is negative.</exception>
    internal static decimal PowBySquaring(decimal baseValue, long exponent)
    {
        if (exponent < 0)
            throw new ArgumentOutOfRangeException(nameof(exponent), "Exponent for PowBySquaring must be non-negative.");
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

    /// <summary>
    /// Computes a specified number raised to a specified power.
    /// Handles integer and non-integer exponents.
    /// </summary>
    /// <param name="value">A decimal number to be raised to a power (the base).</param>
    /// <param name="exponent">A decimal number that specifies the power.</param>
    /// <returns>The number <paramref name="value"/> raised to the power <paramref name="exponent"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown for invalid operations like 0 to a negative power, or negative base to a non-integer power.</exception>
    /// <exception cref="OverflowException">Thrown if the exponent is too large or the result overflows.</exception>
    public static decimal Pow(decimal value, decimal exponent)
    {
        if (exponent == 0m) return 1m;
        if (value == 0m)
        {
            if (exponent < 0m)
                throw new ArgumentOutOfRangeException(nameof(exponent), "Cannot raise 0 to a negative power.");
            return 0m;
        }

        if (exponent == Math.Truncate(exponent))
        {
            long intExponent = (long)exponent;
            const long practicalExponentLimit = 100000;

            if (Math.Abs(intExponent) > practicalExponentLimit)
                throw new OverflowException($"Integer exponent ({intExponent}) exceeds practical limit for Pow.");

            if (intExponent < 0)
            {
                return 1m / PowBySquaring(value, -intExponent);
            }
            else
            {
                return PowBySquaring(value, intExponent);
            }
        }

        if (value < 0m)
            throw new ArgumentOutOfRangeException(nameof(exponent), "Cannot calculate the power of a negative number with a non-integer exponent.");

        var pow = Exp_tylor(exponent * Ln_tylor(value));
        var nDigits = GetDecimalDigit(pow);

        if (nDigits > 20)
            return Math.Round(pow, nDigits - 1); // para corrigit casos como Pow(100,0.5) = 10.000000000000000000000000000001

        return pow;
    }

    /// <summary>
    /// Gets the number of decimal digits (scale) of a decimal value.
    /// The scale is the number of digits to the right of the decimal point.
    /// </summary>
    /// <param name="value">The decimal value.</param>
    /// <returns>The number of decimal digits.</returns>
    public static int GetDecimalDigit(decimal value) => (decimal.GetBits(value)[3] >> 16) & 0xFF;
}