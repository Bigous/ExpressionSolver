using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
        ret.TryAddFunctionCreator("sin", 1, static parameters => new Function("sin", 15, 1, true, parameters, parameters => Convert.ToDecimal(Math.Sin(Convert.ToDouble(parameters[0].Compute())))));
        ret.TryAddFunctionCreator("cos", 1, static parameters => new Function("cos", 15, 1, true, parameters, parameters => Convert.ToDecimal(Math.Cos(Convert.ToDouble(parameters[0].Compute())))));
        ret.TryAddFunctionCreator("sqrt", 1, static parameters => new Function("sqrt", 15, 1, true, parameters, parameters => Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(parameters[0].Compute())))));
        ret.TryAddFunctionCreator("max", 2, static parameters => new Function("max", 15, 2, true, parameters, parameters =>
        {
            var v1 = parameters[0].Compute();
            var v2 = parameters[1].Compute();
            return v1 > v2 ? v1 : v2;
        }));
        ret.TryAddFunctionCreator("min", 2, static parameters => new Function("min", 15, 2, true, parameters, parameters =>
        {
            var v1 = parameters[0].Compute();
            var v2 = parameters[1].Compute();
            return v1 < v2 ? v1 : v2;
        }));
        ret.TryAddFunctionCreator("if", 3, static parameters =>
            new Function("if", 1, 3, false, // Precedência baixa, não associativo à direita para funções não é tão relevante
                         parameters,
                         args => args[0].Compute() != 0m ? args[1].Compute() : args[2].Compute()));
        ret.TryAddFunctionCreator("rand", 0, static parameters => new Function("rand", 15, 0, false, parameters, parameters => Convert.ToDecimal(Random.Shared.NextDouble())));

        // Constants
        ret.TryAddConstant("pi", new Constant("pi", 3.1415926535897932384626433832795m));
        ret.TryAddConstant("e", new Constant("e", 2.7182818284590452353602874713527m));
        ret.TryAddConstant("true", new Constant("true", 1m)); // Representa verdadeiro como 1
        ret.TryAddConstant("false", new Constant("false", 0m)); // Representa falso como 0


        return ret;
    }
}