using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;

namespace ocs {
    internal class Program {
        private static async Task Main(string[] args) {
            try {
                var opt = Parser.Default.ParseArguments<Options>(args).MapResult(o => o, e =>
                    throw new OptionException(e.Select(s => s.ToString()))
                );

                using var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (sender, eventArgs) => {
                    cts.Cancel();
                    eventArgs.Cancel = true;
                };

                await Application.Run(opt, cts.Token);
            }
            catch (FormatException e) {
                Logger.LogError(e.Message);
                Environment.Exit(1);
            }
            catch (OperationCanceledException e) {
                Logger.LogError(e.Message);
                Environment.Exit(1);
            }
            catch (OptionException e) {
                foreach (var err in e.Errors) {
                    if(err != nameof(HelpRequestedError) && err != nameof(VersionRequestedError))
                        Logger.LogError(err);
                    else 
                        Logger.LogWarning("finished ocs");
                    return;
                }
            }
            catch (Exception e) {
                Logger.LogCritical(e.Message);
                Environment.Exit(1);
            }
        }
    }

    public static class Application {
        public static async Task Run(Options opt, CancellationToken token) {
            var global = opt.BuildGlobal();
            var builder = new ScriptBuilder();
            var blockParser = new BlockParser();
            blockParser.Parse(opt.Code);

            // BEGINブロック
            foreach (var begin in blockParser.BeginBlock) {
                builder.AddCodeBlock(begin, ScriptBuilder.BlockType.Begin);
            }

            // ENDブロック
            foreach (var end in blockParser.EndBlock) {
                builder.AddCodeBlock(end, ScriptBuilder.BlockType.End);
            }

            // Mainブロック
            foreach (var main in blockParser.MainBlock) {
                builder.AddCodeBlock(main);
            }


            if (!string.IsNullOrEmpty(opt.Imports)) {
                var imports = opt.Imports.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (imports.Any()) builder.AddImports(imports);
            }

            var (script, diagnostics) = builder.Build(token);
            if (opt.ShowGeneratedCode) Logger.LogInformation($"Generated Code\n{builder.GeneratedCode}");

            foreach (var diagnostic in diagnostics) {
                if (diagnostic.WarningLevel == 0) throw new Exception(diagnostic.ToString());
                Logger.LogWarning(diagnostic.ToString());
            }

            var state = await script.RunAsync(global, null, token);
            if (state.Exception != null) throw state.Exception;
        }
    }
}
