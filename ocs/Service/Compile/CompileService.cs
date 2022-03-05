using System.Runtime.Loader;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using ocs.Lib;
using ocs.Lib.Config;

namespace ocs.Service.Compile;

public class CompileService<T>
{
    private readonly ILogger logger;
    private readonly HashSet<string> referenceList;
    private readonly Config config;

    private readonly HashSet<string> usingList = new()
    {
        "ocs.Global",
        "ocs.Lib",
        "System",
        "System.Collections",
        "System.Collections.Generic",
        "System.Text",
        "System.Text.Json"
    };

    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public CompileService(Config config, ILogger<T> logger)
    {
        var objectAssemblyLocation = typeof(object).Assembly.Location;
        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
        if (assemblyPath is null) throw new AssemblyPathIsNullException();
        referenceList = new HashSet<string>
        {
            typeof(IRunner).Assembly.Location,
            typeof(Global.Global).Assembly.Location,
            objectAssemblyLocation,
            typeof(Console).Assembly.Location
        };

        foreach (var item in new[] { "mscorlib", "System.Text.Json", "System.Runtime", "System.Collections" })
        {
            referenceList.Add($"{assemblyPath}/{item}.dll");
        }

        foreach (var item in config.UsingList)
        {
            usingList.Add(item);
        }

        this.logger = logger;
        this.config = config;
    }

    /// <summary>
    /// 与えられた文字列からC#の構文木を生成する
    /// </summary>
    /// <param name="generatedScript"></param>
    /// <returns></returns>
    public SyntaxTree ParseString(string generatedScript)
    {
        var tree = CSharpSyntaxTree.ParseText(generatedScript,
            CSharpParseOptions.Default.WithLanguageVersion(config.LanguageVersion), "Runner.cs");
        var diagnostic = tree.GetDiagnostics().ToArray();
        if (!diagnostic.Any())
        {
            return tree;
        }

        foreach (var d in diagnostic)
        {
            logger.LogError("syntax Error: {message}: {pos}", d.GetMessage(), d.Location.GetLineSpan());
        }

        throw new ParseGeneratedScriptFailedException();
    }

    private SyntaxTree AddUsingDirective(CompilationUnitSyntax unit)
    {
        // usingディレクティブを構文木に追加する
        // CSharpCompilationOptions.WithUsingsで解決できそうだけどこれはスクリプトの文脈でしか機能しない
        // なのでわざわざ構文木をいじっている
        unit = unit.AddUsings(
            usingList.Select(u =>
                    SyntaxFactory.UsingDirective(
                        SyntaxFactory.ParseName(u).WithLeadingTrivia(SyntaxFactory.Space)
                    ).WithLeadingTrivia(SyntaxFactory.CarriageReturnLineFeed)
                )
                .ToArray()
        );
        return CSharpSyntaxTree.Create(unit,
            CSharpParseOptions.Default.WithLanguageVersion(config.LanguageVersion), "Runner.cs");
    }

    public IRunner Compile(CompileParameter parameter)
    {
        foreach (var item in parameter.ReferenceList)
        {
            referenceList.Add(item);
        }

        foreach (var item in parameter.UsingList)
        {
            usingList.Add(item);
        }
        var syntaxTree = AddUsingDirective((CompilationUnitSyntax)ParseString(parameter.RenderedScript).GetRoot());

        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithOptimizationLevel(OptimizationLevel.Release)
            .WithNullableContextOptions(NullableContextOptions.Enable);

        var metadataList = referenceList.Select(r => MetadataReference.CreateFromFile(r));

        var compiler = CSharpCompilation.Create(
            "Runner.dll",
            new[] { syntaxTree },
            metadataList,
            options
        );

        using var stream = new MemoryStream();
        var result = compiler.Emit(stream);

        if (!result.Success)
        {
            throw new CompileFailedException(result.Diagnostics);
        }

        stream.Seek(0, SeekOrigin.Begin);

        var type = AssemblyLoadContext.Default.LoadFromStream(stream).GetType("Runner");
        if (type is not null)
            return (IRunner)Activator.CreateInstance(type, parameter.Global);

        throw new CreateRunnerInstanceFailedException();
    }
}

[Serializable]
public class CreateRunnerInstanceFailedException : Exception
{
    public CreateRunnerInstanceFailedException() : base("failed to create Runner instance") { }

    protected CreateRunnerInstanceFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ParseGeneratedScriptFailedException : Exception
{
    public ParseGeneratedScriptFailedException() : base("failed to parse generated script") { }

    protected ParseGeneratedScriptFailedException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        : base(serializationInfo, streamingContext) { }
}

[Serializable]
public class CompileFailedException : Exception
{

    public CompileFailedException(IEnumerable<Diagnostic> errors) : base("compile was failed")
    {
        Errors = errors;
    }

    public IEnumerable<Diagnostic> Errors { get; init; }

    protected CompileFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class AssemblyPathIsNullException : Exception
{
    public AssemblyPathIsNullException() : base("failed to find typeof(object) assembly path") { }

    protected AssemblyPathIsNullException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        : base(serializationInfo, streamingContext) { }
}
