using BenchmarkDotNet.Attributes;
using ExpressionSolver;
using StringMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionSolverBench;

[MemoryDiagnoser]
public class ExpressionSolverVSStringMath
{
    private const string ShortExpression = "1 + 2";
    private const string MidExpressionSolver = "1 + 2 * 3 - 4 / 2 + sqrt(16) - 3 ** 2";
    private const string LongExpressionSolver = "1 + 2 * 3 - 4 / 2 + sqrt(16) - 3 ** 2 + max(5, 10) - min(3, 7) + abs(-5) + log(100,2)";
    private const string MidExpressionMath = "1 + 2 * 3 - 4 / 2 + (sqrt 16) - 3 ^ 2";
    private const string LongExpressionMath = "1 + 2 * 3 - 4 / 2 + (sqrt 16) - 3 ^ 2 + (5 max 10) - (3 min 7) + (abs -5) + (100 log 2)";

    private ExpressionSolver.ExecutionContext _context = ExpressionSolver.ExecutionContext.CreateStandardContext();

    [Benchmark]
    public decimal ExpressionSolver_ShortExpression() => ShortExpression.SolveExpression();

    [Benchmark]
    public decimal ExpressionSolver_MidExpression() => MidExpressionSolver.SolveExpression();

    [Benchmark]
    public decimal ExpressionSolver_LongExpression() => LongExpressionSolver.SolveExpression();

    [Benchmark]
    public IExpression ExpressionSolver_Compile_Short() => ShortExpression.CompileExpression();

    [Benchmark]
    public IExpression ExpressionSolver_Compile_Mid() => MidExpressionSolver.CompileExpression();

    [Benchmark]
    public IExpression ExpressionSolver_Compile_Long() => LongExpressionSolver.CompileExpression();

    [Benchmark]
    public IExpression ExpressionSolver_Optimize_Short() => ShortExpression.OptimizeExpression();

    [Benchmark]
    public IExpression ExpressionSolver_Optimize_Mid() => MidExpressionSolver.OptimizeExpression();

    [Benchmark]
    public IExpression ExpressionSolver_Optimize_Long() => LongExpressionSolver.OptimizeExpression();

    [Benchmark]
    public double StringMath_ShortExpression() => ShortExpression.Eval();

    [Benchmark]
    public double StringMath_MidExpression() => MidExpressionMath.Eval();

    [Benchmark]
    public double StringMath_LongExpression() => LongExpressionMath.Eval();
}
