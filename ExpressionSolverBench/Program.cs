using BenchmarkDotNet.Running;
using ExpressionSolverBench;

//BenchmarkRunner.Run<DecimalMathBenchs>();
//BenchmarkRunner.Run<ExpressionSolverVSStringMath>();
//BenchmarkRunner.Run<TokenizationOptimizations>();
//BenchmarkRunner.Run<BuildExpressionTreeOptimizations>();
BenchmarkRunner.Run<GenericVsSpecificContexts>();