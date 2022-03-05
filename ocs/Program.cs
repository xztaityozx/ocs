using System.Collections.Immutable;
using CommandLine;
using CommandLine.Text;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ocs;
using ocs.Cli;
using ocs.Global;
using ocs.Lib;
using ocs.Lib.Config;
using ocs.Service.Compile;
using ocs.Service.Template;
using Parser = CommandLine.Parser;

var config = ConfigFactory.Create();
var collection = new ServiceCollection();
collection.AddLogging(builder =>
    {
        builder.AddConsole(configure =>
        {
            configure.LogToStandardErrorThreshold = config.LogLevel;
        });
        builder.AddFilter(level => level >= config.LogLevel);
    })
    .AddSingleton(config)
    .AddSingleton<RenderService>()
    .AddSingleton<CompileService<Program>>();

using var provider = collection.BuildServiceProvider();

var parser = new Parser(with => with.HelpWriter = null);
var result = parser.ParseArguments<Options>(args);


return result.MapResult(Run, PrintParseError);

int PrintParseError(IEnumerable<Error> errors)
{
    HelpText help;
    if (errors.IsVersion())
    {
        help = HelpText.AutoBuild(result);
    }
    else
    {
        help = HelpText.AutoBuild(result, h =>
        {
            h.AdditionalNewLineAfterOption = false;
            h.Copyright = "Copyright (c) 2022 xztaityozx";
            h.AddPostOptionsLine("------- loaded config -------" + Environment.NewLine + config);
            return HelpText.DefaultParsingErrorsHandler(result, h);
        }, e => e);
    }

    Console.WriteLine(help);
    return 1;
}

int Run(Options options)
{
    var logger = provider.GetService<ILogger<Program>>();
    try
    {
        using var global = new Global(new GlobalVariableOption
        {
            RemoveEmpty = options.RemoveEmpty,
            InputSeparator = options.InputDelimiter,
            UseRegex = options.UseRegexp,
            OutputSeparator = options.OutputDelimiter,
            InputStream = options.File is null
                ? Console.OpenStandardInput()
                : new StreamReader(options.File.FullName).BaseStream
        });

        var ocsScripts = ocs.Lib.Parser.Parse(options.Script);
        var renderedClass =
            (provider.GetService<RenderService>() ?? throw new GetServiceFailedException(typeof(RenderService)))
            .Render(ocsScripts);

        logger?.LogDebug("Runner class generated: {class}", renderedClass);

        var compileService = provider.GetService<CompileService<Program>>() ??
                             throw new GetServiceFailedException(typeof(CompileService<Program>));

        if (options.PrintGenerated)
        {
            Console.WriteLine(compileService.ParseString(renderedClass).GetRoot().NormalizeWhitespace().ToFullString());
            return (int)ExitCode.Success;
        }

        var param = new CompileParameter(global, options.UsingList ?? ImmutableArray<string>.Empty,
            options.ReferenceList ?? ImmutableArray<string>.Empty,
            options.LanguageVersion ?? config.LanguageVersion, renderedClass);

        logger?.LogDebug("Compile parameter: {param}", param);

        var runner = compileService.Compile(param);
        runner?.Run();

        return (int)ExitCode.Success;
    }
    catch (FileNotFoundException e)
    {
        logger?.LogError("file not found: {file}", e.FileName);
        return (int)ExitCode.FileNotFound;
    }
    catch (GetServiceFailedException e)
    {
        logger?.LogError("{msg}", e.Message);
        return (int)ExitCode.Failure;
    }
    catch (CompileFailedException e)
    {
        foreach (var diagnostic in e.Errors)
        {
            logger?.LogError("{msg}", diagnostic.GetMessage());
        }

        return (int)ExitCode.Failure;
    }
    catch (Exception e) when (e is AssemblyPathIsNullException or InvalidSyntaxException
                                  or CreateRunnerInstanceFailedException or ParseGeneratedScriptFailedException)
    {
        logger?.LogError("{msg}", e.Message);

        return (int)ExitCode.Failure;
    }
}
