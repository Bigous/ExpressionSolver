using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ExpressionSolver;

public partial class ExecutionContext // Adicionado partial para o caso de Token.cs ser separado
{
    internal static ExecutionContext DefaulContext = CreateStandardContext();

    public static ExecutionContext CreateStandardContext()
    {
        ExecutionContext ret = new ExecutionContext();

        // Constants


        // default Operators

        // Atribuição: Precedence 1 - not used in expressions for now : = += -= *= /= %=

        // Conditional (ternary ? :): Precedence 2

        // Basic + - * /
        ret.TryAddOperatorCreator("+", static operands => new BinaryOperator("+", 11, operands[0], operands[1], static (r, l) => r.Compute() + l.Compute()));
        ret.TryAddOperatorCreator("-", static operands => new BinaryOperator("-", 11, operands[0], operands[1], static (r, l) => r.Compute() - l.Compute()));
        ret.TryAddOperatorCreator("*", static operands => new BinaryOperator("*", 12, operands[0], operands[1], static (r, l) => r.Compute() * l.Compute()));
        ret.TryAddOperatorCreator("/", static operands => new BinaryOperator("/", 12, operands[0], operands[1], static (r, l) =>
        {
            decimal rightVal = l.Compute();
            if (rightVal == 0) throw new DivideByZeroException("Tentativa de divisão por zero.");
            return r.Compute() / rightVal;
        }));
        ret.TryAddOperatorCreator("%", static operands => new BinaryOperator("%", 12, operands[0], operands[1], static (r, l) => r.Compute() % l.Compute()));

        // Power ^
        ret.TryAddOperatorCreator("**", static operands => new BinaryOperator("**", 14, operands[0], operands[1], static (r, l) => Convert.ToDecimal(Math.Pow(Convert.ToDouble(r.Compute()), Convert.ToDouble(l.Compute())))));

        // Factorial !
        ret.TryAddOperatorCreator("!", static operands => new UnaryOperator("!", true, 14, operands[0], static (operand) =>
        {
            decimal fact = operand.Compute();
            if (fact == 0m) return 1m;
            decimal factRet = 1;
            for (int i = Convert.ToInt32(fact); i > 1; --i) factRet *= i;
            return factRet;
        }));

        // Logic || (precedence 4) && (precedence 5)
        ret.TryAddOperatorCreator("||", static operands => new BinaryOperator("||", 3, operands[0], operands[1], static (r, l) => (r.Compute() != 0m || l.Compute() != 0m) ? 1m : 0m));
        ret.TryAddOperatorCreator("&&", static operands => new BinaryOperator("&&", 4, operands[0], operands[1], static (r, l) => (r.Compute() != 0m && l.Compute() != 0m) ? 1m : 0m));

        // Equality == !=
        ret.TryAddOperatorCreator("==", static operands => new BinaryOperator("==", 8, operands[0], operands[1], static (r, l) => (r.Compute() == l.Compute()) ? 1m : 0m));
        ret.TryAddOperatorCreator("!=", static operands => new BinaryOperator("!=", 8, operands[0], operands[1], static (r, l) => (r.Compute() != l.Compute()) ? 1m : 0m));

        // Relationals > < >= <=
        ret.TryAddOperatorCreator(">", static operands => new BinaryOperator(">", 9, operands[0], operands[1], static (r, l) => (r.Compute() > l.Compute()) ? 1m : 0m));
        ret.TryAddOperatorCreator("<", static operands => new BinaryOperator("<", 9, operands[0], operands[1], static (r, l) => (r.Compute() < l.Compute()) ? 1m : 0m));
        ret.TryAddOperatorCreator(">=", static operands => new BinaryOperator(">=", 9, operands[0], operands[1], static (r, l) => (r.Compute() >= l.Compute()) ? 1m : 0m));
        ret.TryAddOperatorCreator("<=", static operands => new BinaryOperator("<=", 9, operands[0], operands[1], static (r, l) => (r.Compute() <= l.Compute()) ? 1m : 0m));

        // Unary operators - and +
        ret.TryAddOperatorCreator("u-", static operands => new UnaryOperator("u-", true, 13, operands[0], op => -op.Compute()));
        ret.TryAddOperatorCreator("u+", static operands => new UnaryOperator("u+", true, 13, operands[0], op => op.Compute())); // u+ geralmente é no-op

        // Functions - Chamadas TryAddFunctionCreator atualizadas para incluir a aridade
        ret.TryAddFunctionCreator("sin", 1, static parameters => new Function("sin", 15, 1, true, parameters, parameters => DecimalMath.Sin(parameters[0].Compute())));
        ret.TryAddFunctionCreator("cos", 1, static parameters => new Function("cos", 15, 1, true, parameters, parameters => DecimalMath.Cos(parameters[0].Compute())));
        ret.TryAddFunctionCreator("tan", 1, static parameters => new Function("tan", 15, 1, true, parameters, parameters => DecimalMath.Tan(parameters[0].Compute())));
        ret.TryAddFunctionCreator("log", 1, static parameters => new Function("log", 15, 2, true, parameters, parameters => DecimalMath.Log(parameters[0].Compute(), parameters[1].Compute())));
        ret.TryAddFunctionCreator("log10", 1, static parameters => new Function("log10", 15, 1, true, parameters, parameters => DecimalMath.Log10(parameters[0].Compute())));
        ret.TryAddFunctionCreator("ln", 1, static parameters => new Function("ln", 15, 1, true, parameters, parameters => DecimalMath.Ln(parameters[0].Compute())));
        ret.TryAddFunctionCreator("exp", 1, static parameters => new Function("exp", 15, 1, true, parameters, parameters => DecimalMath.Exp(parameters[0].Compute())));
        ret.TryAddFunctionCreator("asin", 1, static parameters => new Function("asin", 15, 1, true, parameters, parameters => DecimalMath.Asin(parameters[0].Compute())));
        ret.TryAddFunctionCreator("acos", 1, static parameters => new Function("acos", 15, 1, true, parameters, parameters => DecimalMath.Acos(parameters[0].Compute())));
        ret.TryAddFunctionCreator("atan", 1, static parameters => new Function("atan", 15, 1, true, parameters, parameters => DecimalMath.Atan(parameters[0].Compute())));
        ret.TryAddFunctionCreator("atan2", 2, static parameters => new Function("atan2", 15, 2, true, parameters, parameters =>
        {
            var y = parameters[0].Compute();
            var x = parameters[1].Compute();
            return DecimalMath.Atan2(y, x);
        }));
        ret.TryAddFunctionCreator("rad", 1, static parameters => new Function("rad", 15, 1, true, parameters, parameters => parameters[0].Compute() * DecimalMath.PI / 180.0m));
        ret.TryAddFunctionCreator("deg", 1, static parameters => new Function("deg", 15, 1, true, parameters, parameters => parameters[0].Compute() * 180.0m / DecimalMath.PI));
        ret.TryAddFunctionCreator("sqrt", 1, static parameters => new Function("sqrt", 15, 1, true, parameters, parameters => DecimalMath.Sqrt(parameters[0].Compute())));
        ret.TryAddFunctionCreator("max", 2, static parameters => new Function("max", 15, 2, true, parameters, parameters =>
        {
            var v1 = parameters[0].Compute();
            var v2 = parameters[1].Compute();
            return v1 > v2 ? v1 : v2;
        }));
        ret.TryAddFunctionCreator("max3", 3, static parameters => new Function("max3", 15, 3, true, parameters, parameters =>
        {
            var v1 = parameters[0].Compute();
            var v2 = parameters[1].Compute();
            var v3 = parameters[3].Compute();
            return v1 > v2 ? (v1 > v3 ? v1 : v3) : (v2 > v3 ? v2 : v3);
        }));
        ret.TryAddFunctionCreator("min", 2, static parameters => new Function("min", 15, 2, true, parameters, parameters =>
        {
            var v1 = parameters[0].Compute();
            var v2 = parameters[1].Compute();
            return v1 < v2 ? v1 : v2;
        }));
        ret.TryAddFunctionCreator("min3", 3, static parameters => new Function("min3", 15, 3, true, parameters, parameters =>
        {
            var v1 = parameters[0].Compute();
            var v2 = parameters[1].Compute();
            var v3 = parameters[3].Compute();
            return v1 < v2 ? (v1 < v3 ? v1 : v3) : (v2 < v3 ? v2 : v3);
        }));
        ret.TryAddFunctionCreator("if", 3, static parameters =>
            new Function("if", 1, 15, false,
                         parameters,
                         args => args[0].Compute() != 0m ? args[1].Compute() : args[2].Compute()));
        ret.TryAddFunctionCreator("rand", 0, static parameters => new Function("rand", 15, 0, false, parameters, parameters => Convert.ToDecimal(Random.Shared.NextDouble())));
        ret.TryAddFunctionCreator("rand_range", 2, static parameters => new Function("rand_range", 15, 2, false, parameters, args =>
        {
            var min = Convert.ToInt32(args[0].Compute());
            var max = Convert.ToInt32(args[1].Compute());
            if (min >= max) throw new ArgumentException("O valor mínimo deve ser menor que o valor máximo.");
            return Random.Shared.Next(min, max + 1);
        }));
        ret.TryAddFunctionCreator("abs", 1, static parameters => new Function("abs", 15, 1, true, parameters, parameters => Math.Abs(parameters[0].Compute())));
        ret.TryAddFunctionCreator("round", 1, static parameters => new Function("round", 15, 1, true, parameters, parameters => Math.Round(parameters[0].Compute())));
        ret.TryAddFunctionCreator("floor", 1, static parameters => new Function("floor", 15, 1, true, parameters, parameters => Math.Floor(parameters[0].Compute())));
        ret.TryAddFunctionCreator("ceil", 1, static parameters => new Function("ceil", 15, 1, true, parameters, parameters => Math.Ceiling(parameters[0].Compute())));


        // Constants
        ret.TryAddConstant("pi", new Constant("pi", DecimalMath.PI));
        ret.TryAddConstant("e", new Constant("e", DecimalMath.E));
        ret.TryAddConstant("true", new Constant("true", 1m)); // Representa verdadeiro como 1
        ret.TryAddConstant("false", new Constant("false", 0m)); // Representa falso como 0


        return ret;
    }
}