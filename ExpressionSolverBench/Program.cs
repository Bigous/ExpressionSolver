using BenchmarkDotNet.Running;
using ExpressionSolverBench;

BenchmarkRunner.Run<ExpressionSolverVSStringMath>();
//BenchmarkRunner.Run<TokenizationOptimizations>();
//BenchmarkRunner.Run<BuildExpressionTreeOptimizations>();