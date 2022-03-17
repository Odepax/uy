using BenchmarkDotNet.Running;
using Uy.Benchmarks;

var summary = BenchmarkRunner.Run<StructVsClassEventsBenchmarks>();
