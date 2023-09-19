using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using Xunit.Abstractions;

namespace Delobytes.NetCore.EntityReportGeneration.Tests;
public class BenchmarkTests
{
    private readonly ITestOutputHelper _output;

    public BenchmarkTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Run_Benchmarks()
    {
        AccumulationLogger logger = new AccumulationLogger();
        ManualConfig config = ManualConfig.Create(DefaultConfig.Instance)
            .AddLogger(logger)
            .WithOptions(ConfigOptions.DisableOptimizationsValidator);

        BenchmarkRunner.Run<GeneratorBechmarkTests>(config);

        _output.WriteLine(logger.GetLog());
    }

}
