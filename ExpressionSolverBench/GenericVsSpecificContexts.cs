using BenchmarkDotNet.Attributes;
using ExpressionSolver;

using ExecutionContext = ExpressionSolver.ExecutionContext;

namespace ExpressionSolverBench;

[MemoryDiagnoser]
public class GenericVsSpecificContexts
{
    private static ExecutionContext _genericContext = ExecutionContext.CreateStandardContext_old();
    private static ExecutionContext _specificContext = ExecutionContext.CreateStandardContext();

    const string Expression = "1 + 2 * 3 - 4 / 2 + sqrt(16) - 3 ** 2 + max(5, 10) - min(3, 7) + abs(-5) + log(100,2)";

    [Benchmark(Baseline = true)]
    public decimal GenericContext() => _genericContext.Solve(Expression);

    [Benchmark]
    public decimal SpecificContext() => _specificContext.Solve(Expression);
}
