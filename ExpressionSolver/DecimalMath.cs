using System;

namespace ExpressionSolver;

public static class DecimalMath
{
    public const decimal PI = 3.14159265358979323846264338327950288419716939937510m;
    public const decimal E = 2.71828182845904523536028747135266249775724709369995m;

    public static decimal Sin(decimal value)
    {
        return (decimal)Math.Sin((double)value);
    }

    public static decimal Cos(decimal value)
    {
        return (decimal)Math.Cos((double)value);
    }

    public static decimal Tan(decimal value)
    {
        return (decimal)Math.Tan((double)value);
    }

    public static decimal Asin(decimal value)
    {
        return (decimal)Math.Asin((double)value);
    }

    public static decimal Acos(decimal value)
    {
        return (decimal)Math.Acos((double)value);
    }

    public static decimal Atan(decimal value)
    {
        return (decimal)Math.Atan((double)value);
    }

    public static decimal Atan2(decimal y, decimal x)
    {
        return (decimal)Math.Atan2((double)y, (double)x);
    }

    public static decimal Log(decimal value)
    {
        return (decimal)Math.Log((double)value);
    }

    public static decimal Log10(decimal value)
    {
        return (decimal)Math.Log10((double)value);
    }

    public static decimal Exp(decimal value)
    {
        return (decimal)Math.Exp((double)value);
    }

    public static decimal Sqrt(decimal value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Cannot calculate square root of a negative number.");
        return (decimal)Math.Sqrt((double)value);
    }
}