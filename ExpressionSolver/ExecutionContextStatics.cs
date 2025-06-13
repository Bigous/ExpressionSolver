using ExpressionSolver.Operators;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ExpressionSolver;

/// <summary>
/// Provides static methods and a default context for <see cref="ExecutionContext"/>.
/// </summary>
public partial class ExecutionContext
{
    /// <summary>
    /// A default, shared instance of <see cref="ExecutionContext"/> pre-populated with standard operators, functions, and constants.
    /// This instance is thread-safe for read operations (like solving expressions) if not modified after creation.
    /// Modifying this shared instance (e.g., adding custom functions) is not thread-safe and should be done with caution or by creating a new context.
    /// </summary>
    internal static ExecutionContext DefaulContext = CreateStandardContext();

    /// <summary>
    /// Creates a new instance of <see cref="ExecutionContext"/> populated with a standard set of mathematical operators,
    /// functions (like sin, cos, log, sqrt, etc.), and constants (pi, e).
    /// This is an older version kept for compatibility or specific use cases.
    /// Prefer <see cref="CreateStandardContext"/> for the latest standard setup.
    /// </summary>
    /// <returns>A new <see cref="ExecutionContext"/> instance with default settings.</returns>
    [Obsolete("Use CreateStandardContext() instead for the latest standard setup with improved operator/function classes.")]
    public static ExecutionContext CreateStandardContext_old()
    {
        ExecutionContext ret = new ExecutionContext();

        // Constants
        ret.TryAddConstant("pi", new Constant("pi", DecimalMath.PI));
        ret.TryAddConstant("e", new Constant("e", DecimalMath.E));
        ret.TryAddConstant("true", new Constant("true", 1m)); // Represents true as 1
        ret.TryAddConstant("false", new Constant("false", 0m)); // Represents false as 0

        // Default Operators
        ret.TryAddOperatorCreator("+", static operands => new BinaryOperator("+", 11, operands[0], operands[1], static (r, l) => r.Compute() + l.Compute()));
        ret.TryAddOperatorCreator("-", static operands => new BinaryOperator("-", 11, operands[0], operands[1], static (r, l) => r.Compute() - l.Compute()));
        ret.TryAddOperatorCreator("*", static operands => new BinaryOperator("*", 12, operands[0], operands[1], static (r, l) => r.Compute() * l.Compute()));
        ret.TryAddOperatorCreator("/", static operands => new BinaryOperator("/", 12, operands[0], operands[1], static (r, l) =>
        {
            decimal rightVal = l.Compute();
            if (rightVal == 0) throw new DivideByZeroException("Attempt to divide by zero.");
            return r.Compute() / rightVal;
        }));
        ret.TryAddOperatorCreator("%", static operands => new BinaryOperator("%", 12, operands[0], operands[1], static (r, l) => r.Compute() % l.Compute()));
        ret.TryAddOperatorCreator("**", static operands => new BinaryOperator("**", 14, operands[0], operands[1], static (r, l) => DecimalMath.Pow(r.Compute(), l.Compute()))); // Corrected IsRightOperator
        ret.TryAddOperatorCreator("!", static operands => new UnaryOperator("!", false, 14, operands[0], static (operand) => // Factorial is typically left-associative if considered with other ops, but as a postfix, its associativity is less critical. Precedence is high.
        {
            decimal factVal = operand.Compute();
            if (factVal < 0 || factVal != Math.Truncate(factVal)) throw new ArgumentException("Factorial is only defined for non-negative integers.");
            if (factVal == 0m) return 1m;
            if (factVal > 25) throw new OverflowException("Factorial argument too large, causes overflow."); // Practical limit for decimal
            decimal factRet = 1;
            for (int i = Convert.ToInt32(factVal); i > 1; --i) factRet *= i;
            return factRet;
        }));
        ret.TryAddOperatorCreator("||", static operands => new BinaryOperator("||", 3, operands[0], operands[1], static (r, l) => (r.Compute() != 0m || l.Compute() != 0m) ? 1m : 0m));
        ret.TryAddOperatorCreator("&&", static operands => new BinaryOperator("&&", 4, operands[0], operands[1], static (r, l) => (r.Compute() != 0m && l.Compute() != 0m) ? 1m : 0m));
        ret.TryAddOperatorCreator("==", static operands => new BinaryOperator("==", 8, operands[0], operands[1], static (r, l) => (r.Compute() == l.Compute()) ? 1m : 0m));
        ret.TryAddOperatorCreator("!=", static operands => new BinaryOperator("!=", 8, operands[0], operands[1], static (r, l) => (r.Compute() != l.Compute()) ? 1m : 0m));
        ret.TryAddOperatorCreator(">", static operands => new BinaryOperator(">", 9, operands[0], operands[1], static (r, l) => (r.Compute() > l.Compute()) ? 1m : 0m));
        ret.TryAddOperatorCreator("<", static operands => new BinaryOperator("<", 9, operands[0], operands[1], static (r, l) => (r.Compute() < l.Compute()) ? 1m : 0m));
        ret.TryAddOperatorCreator(">=", static operands => new BinaryOperator(">=", 9, operands[0], operands[1], static (r, l) => (r.Compute() >= l.Compute()) ? 1m : 0m));
        ret.TryAddOperatorCreator("<=", static operands => new BinaryOperator("<=", 9, operands[0], operands[1], static (r, l) => (r.Compute() <= l.Compute()) ? 1m : 0m));
        ret.TryAddOperatorCreator("u-", static operands => new UnaryOperator("u-", true, 13, operands[0], op => -op.Compute()));
        ret.TryAddOperatorCreator("u+", static operands => new UnaryOperator("u+", true, 13, operands[0], op => op.Compute()));

        // Functions
        ret.TryAddFunctionCreator("sin", 1, static parameters => new Function("sin", 15, 1, true, parameters, args => DecimalMath.Sin(args[0].Compute())));
        ret.TryAddFunctionCreator("cos", 1, static parameters => new Function("cos", 15, 1, true, parameters, args => DecimalMath.Cos(args[0].Compute())));
        ret.TryAddFunctionCreator("tan", 1, static parameters => new Function("tan", 15, 1, true, parameters, args => DecimalMath.Tan(args[0].Compute())));
        ret.TryAddFunctionCreator("log", 2, static parameters => new Function("log", 15, 2, true, parameters, args => DecimalMath.Log(args[0].Compute(), args[1].Compute())));
        ret.TryAddFunctionCreator("log10", 1, static parameters => new Function("log10", 15, 1, true, parameters, args => DecimalMath.Log10(args[0].Compute())));
        ret.TryAddFunctionCreator("ln", 1, static parameters => new Function("ln", 15, 1, true, parameters, args => DecimalMath.Ln_MinMax(args[0].Compute())));
        ret.TryAddFunctionCreator("exp", 1, static parameters => new Function("exp", 15, 1, true, parameters, args => DecimalMath.Exp_MinMax(args[0].Compute())));
        ret.TryAddFunctionCreator("asin", 1, static parameters => new Function("asin", 15, 1, true, parameters, args => DecimalMath.Asin(args[0].Compute())));
        ret.TryAddFunctionCreator("acos", 1, static parameters => new Function("acos", 15, 1, true, parameters, args => DecimalMath.Acos(args[0].Compute())));
        ret.TryAddFunctionCreator("atan", 1, static parameters => new Function("atan", 15, 1, true, parameters, args => DecimalMath.Atan(args[0].Compute())));
        ret.TryAddFunctionCreator("atan2", 2, static parameters => new Function("atan2", 15, 2, true, parameters, args => DecimalMath.Atan2(args[0].Compute(), args[1].Compute())));
        ret.TryAddFunctionCreator("rad", 1, static parameters => new Function("rad", 15, 1, true, parameters, args => args[0].Compute() * DecimalMath.PI / 180.0m));
        ret.TryAddFunctionCreator("deg", 1, static parameters => new Function("deg", 15, 1, true, parameters, args => args[0].Compute() * 180.0m / DecimalMath.PI));
        ret.TryAddFunctionCreator("sqrt", 1, static parameters => new Function("sqrt", 15, 1, true, parameters, args => DecimalMath.Sqrt(args[0].Compute())));
        ret.TryAddFunctionCreator("max", -1, static parameters => new Function("max", 15, -1, true, parameters, args => { // Variable arity
            if (args.Count == 0) throw new ArgumentException("'max' function requires at least one argument.");
            return args.Max(p => p.Compute());
        }));
        ret.TryAddFunctionCreator("min", -1, static parameters => new Function("min", 15, -1, true, parameters, args => { // Variable arity
            if (args.Count == 0) throw new ArgumentException("'min' function requires at least one argument.");
            return args.Min(p => p.Compute());
        }));
        ret.TryAddFunctionCreator("if", 3, static parameters => new Function("if", 1, 3, false, parameters, args => args[0].Compute() != 0m ? args[1].Compute() : args[2].Compute())); // Low precedence for 'if'
        ret.TryAddFunctionCreator("rand", 0, static parameters => new Function("rand", 15, 0, false, parameters, args => Convert.ToDecimal(Random.Shared.NextDouble())));
        ret.TryAddFunctionCreator("rand_range", 2, static parameters => new Function("rand_range", 15, 2, false, parameters, static args =>
        {
            var min = Convert.ToInt32(args[0].Compute());
            var max = Convert.ToInt32(args[1].Compute());
            if (min >= max) throw new ArgumentException("Minimum value must be less than maximum value for 'rand_range'.");
            return Random.Shared.Next(min, max); // Next(min, max) excludes max, so if inclusive max is desired, use max + 1
        }));
        ret.TryAddFunctionCreator("abs", 1, static parameters => new Function("abs", 15, 1, true, parameters, args => Math.Abs(args[0].Compute())));
        ret.TryAddFunctionCreator("round", 1, static parameters => new Function("round", 15, 1, true, parameters, args => Math.Round(args[0].Compute())));
        ret.TryAddFunctionCreator("floor", 1, static parameters => new Function("floor", 15, 1, true, parameters, args => Math.Floor(args[0].Compute())));
        ret.TryAddFunctionCreator("ceil", 1, static parameters => new Function("ceil", 15, 1, true, parameters, args => Math.Ceiling(args[0].Compute())));

        return ret;
    }

    /// <summary>
    /// Creates a new instance of <see cref="ExecutionContext"/> populated with a standard set of mathematical operators,
    /// functions (like sin, cos, log, sqrt, etc.), and constants (pi, e).
    /// This context uses specific operator and function classes for better type safety and potential performance.
    /// </summary>
    /// <returns>A new <see cref="ExecutionContext"/> instance with default settings.</returns>
    public static ExecutionContext CreateStandardContext()
    {
        ExecutionContext ret = new ExecutionContext();

        // Constants
        ret.TryAddConstant("pi", new Constant("pi", DecimalMath.PI));
        ret.TryAddConstant("e", new Constant("e", DecimalMath.E));
        ret.TryAddConstant("true", new Constant("true", 1m)); // Represents true as 1
        ret.TryAddConstant("false", new Constant("false", 0m)); // Represents false as 0

        // Default Operators using specific classes
        ret.TryAddOperatorCreator("+", static operands => new Add(operands[0], operands[1]));
        ret.TryAddOperatorCreator("-", static operands => new Subtract(operands[0], operands[1]));
        ret.TryAddOperatorCreator("*", static operands => new Multiply(operands[0], operands[1]));
        ret.TryAddOperatorCreator("/", static operands => new Divide(operands[0], operands[1]));
        ret.TryAddOperatorCreator("%", static operands => new Reminder(operands[0], operands[1]));
        ret.TryAddOperatorCreator("**", static operands => new Power(operands[0], operands[1]));
        ret.TryAddOperatorCreator("!", static operands => new Factorial(operands[0]));
        ret.TryAddOperatorCreator("||", static operands => new LogicOr(operands[0], operands[1]));
        ret.TryAddOperatorCreator("&&", static operands => new LogicAnd(operands[0], operands[1]));
        ret.TryAddOperatorCreator("==", static operands => new LogicEquals(operands[0], operands[1]));
        ret.TryAddOperatorCreator("!=", static operands => new LogicDifferent(operands[0], operands[1]));
        ret.TryAddOperatorCreator(">", static operands => new LogicGreaterThan(operands[0], operands[1]));
        ret.TryAddOperatorCreator("<", static operands => new LogicLessThan(operands[0], operands[1]));
        ret.TryAddOperatorCreator(">=", static operands => new LogicGreaterOrEqualsTo(operands[0], operands[1]));
        ret.TryAddOperatorCreator("<=", static operands => new LogicLessOrEqualsTo(operands[0], operands[1]));
        ret.TryAddOperatorCreator("u-", static operands => new UnaryMinus(operands[0]));
        ret.TryAddOperatorCreator("u+", static operands => new UnaryPlus(operands[0]));

        // Functions using specific classes
        ret.TryAddFunctionCreator("sin", 1, static parameters => new FunctionOneArg("sin", parameters[0], static param => DecimalMath.Sin(param.Compute())));
        ret.TryAddFunctionCreator("cos", 1, static parameters => new FunctionOneArg("cos", parameters[0], static param => DecimalMath.Cos(param.Compute())));
        ret.TryAddFunctionCreator("tan", 1, static parameters => new FunctionOneArg("tan", parameters[0], static param => DecimalMath.Tan(param.Compute())));
        ret.TryAddFunctionCreator("log", 2, static parameters => new FunctionTwoArgs("log", parameters[0], parameters[1], static (p1, p2) => DecimalMath.Log(p1.Compute(), p2.Compute())));
        ret.TryAddFunctionCreator("log10", 1, static parameters => new FunctionOneArg("log10", parameters[0], static param => DecimalMath.Log10(param.Compute())));
        ret.TryAddFunctionCreator("ln", 1, static parameters => new FunctionOneArg("ln", parameters[0], static param => DecimalMath.Ln_MinMax(param.Compute())));
        ret.TryAddFunctionCreator("exp", 1, static parameters => new FunctionOneArg("exp", parameters[0], static param => DecimalMath.Exp_MinMax(param.Compute())));
        ret.TryAddFunctionCreator("asin", 1, static parameters => new FunctionOneArg("asin", parameters[0], static param => DecimalMath.Asin(param.Compute())));
        ret.TryAddFunctionCreator("acos", 1, static parameters => new FunctionOneArg("acos", parameters[0], static param => DecimalMath.Acos(param.Compute())));
        ret.TryAddFunctionCreator("atan", 1, static parameters => new FunctionOneArg("atan", parameters[0], static param => DecimalMath.Atan(param.Compute())));
        ret.TryAddFunctionCreator("atan2", 2, static parameters => new FunctionTwoArgs("atan2", parameters[0], parameters[1], static (p1, p2) => DecimalMath.Atan2(p1.Compute(), p2.Compute())));
        ret.TryAddFunctionCreator("rad", 1, static parameters => new FunctionOneArg("rad", parameters[0], static param => param.Compute() * DecimalMath.PI / 180.0m));
        ret.TryAddFunctionCreator("deg", 1, static parameters => new FunctionOneArg("deg", parameters[0], static param => param.Compute() * 180.0m / DecimalMath.PI));
        ret.TryAddFunctionCreator("sqrt", 1, static parameters => new FunctionOneArg("sqrt", parameters[0], static param => DecimalMath.Sqrt(param.Compute())));
        ret.TryAddFunctionCreator("max", -1, static parameters => new FunctionManyArgs("max", parameters, static args => {
            if (args.Count == 0) throw new ArgumentException("'max' function requires at least one argument.");
            decimal maxVal = args[0].Compute();
            for (int i = 1; i < args.Count; i++) { decimal current = args[i].Compute(); if (current > maxVal) maxVal = current; }
            return maxVal;
        }));
        ret.TryAddFunctionCreator("min", -1, static parameters => new FunctionManyArgs("min", parameters, static args => {
            if (args.Count == 0) throw new ArgumentException("'min' function requires at least one argument.");
            decimal minVal = args[0].Compute();
            for (int i = 1; i < args.Count; i++) { decimal current = args[i].Compute(); if (current < minVal) minVal = current; }
            return minVal;
        }));
        ret.TryAddFunctionCreator("if", 3, static parameters => new FunctionThreeArgs("if", parameters[0], parameters[1], parameters[2], static (cond, trueBranch, falseBranch) => cond.Compute() != 0m ? trueBranch.Compute() : falseBranch.Compute()));
        ret.TryAddFunctionCreator("rand", 0, static parameters => new GeneratorNoArg("rand", static () => Convert.ToDecimal(Random.Shared.NextDouble())));
        ret.TryAddFunctionCreator("rand_range", 2, static parameters => new GeneratorTwoArgs("rand_range", parameters[0], parameters[1], static (minExpr, maxExpr) =>
        {
            var min = Convert.ToInt32(minExpr.Compute());
            var max = Convert.ToInt32(maxExpr.Compute());
            if (min >= max) throw new ArgumentException("Minimum value must be less than maximum value for 'rand_range'.");
            return Random.Shared.Next(min, max); // Next(min, max) excludes max. If inclusive max is desired, use max + 1.
        }));
        ret.TryAddFunctionCreator("abs", 1, static parameters => new FunctionOneArg("abs", parameters[0], static param => Math.Abs(param.Compute())));
        ret.TryAddFunctionCreator("round", 1, static parameters => new FunctionOneArg("round", parameters[0], static param => Math.Round(param.Compute())));
        ret.TryAddFunctionCreator("floor", 1, static parameters => new FunctionOneArg("floor", parameters[0], static param => Math.Floor(param.Compute())));
        ret.TryAddFunctionCreator("ceil", 1, static parameters => new FunctionOneArg("ceil", parameters[0], static param => Math.Ceiling(param.Compute())));

        return ret;
    }
}