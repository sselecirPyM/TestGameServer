using BenchmarkDotNet.Running;
using System;
namespace Benchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<MyBenchmark>();
        }
    }
}
