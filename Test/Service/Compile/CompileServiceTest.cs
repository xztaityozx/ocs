using System;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using ocs.Global;
using ocs.Lib.Config;
using ocs.Service.Compile;
using Xunit;

namespace Test.Service.Compile;

public class ConfigFixture : IDisposable
{
    public Config? Config { get; private set; }
    public ConfigFixture()
    {
        Config = new Config
        {
            UsingList = new[] { "System.Collections.Generic", "System.Runtime.Loader" },
            ReferenceList = Array.Empty<string>(),
            InlineCode = new[] { "int Inline1() => 1;", "string Inline2() => \"abc\";" },
            LanguageVersion = LanguageVersion.CSharp10,
            LogLevel = LogLevel.Trace
        };
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Config = null;
        }
    }
}

public class CompileServiceTest : IClassFixture<ConfigFixture>
{
    private readonly ConfigFixture fixture;

    public CompileServiceTest(ConfigFixture fixture) => this.fixture = fixture;

    [Fact]
    public void Test_New()
    {
        var logger = new Logger<CompileServiceTest>(new LoggerFactory());

        var actual = new CompileService<CompileServiceTest>(fixture.Config, logger);

        Assert.NotNull(actual);
    }

    [Fact]
    public void Test_ParseString()
    {
        const string script = @"public class TestClass {}";
        var logger = new Logger<CompileServiceTest>(new LoggerFactory());
        var service = new CompileService<CompileServiceTest>(fixture.Config, logger);

        var actual = service.ParseString(script);

        Assert.Equal(script, actual.ToString());
    }

    [Fact]
    public void Test_ParseStringThrows()
    {
        const string script = @"public class TestClass {";
        var logger = new Logger<CompileServiceTest>(new LoggerFactory());
        var service = new CompileService<CompileServiceTest>(fixture.Config, logger);

        Assert.Throws<ParseGeneratedScriptFailedException>(() => service.ParseString(script));
    }

    [Fact]
    public void Test_Compile()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Line1");
        builder.AppendLine("Line2");
        builder.AppendLine("Line3");
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(builder.ToString()));

        var param = new CompileParameter(
            new Global(new GlobalVariableOption
            {
                InputStream = memoryStream
            }),
            Array.Empty<string>(),
            Array.Empty<string>(),
            LanguageVersion.CSharp10,
            "public class Runner : IRunner { public Runner(Global global) {} public void Run() { return; }}"
        );

        var service =
            new CompileService<CompileServiceTest>(
                fixture.Config,
                new Logger<CompileServiceTest>(new LoggerFactory())
            );

        var runner = service.Compile(param);

        Assert.NotNull(runner);
        runner.Run();
    }
}