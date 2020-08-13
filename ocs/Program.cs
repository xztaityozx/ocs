using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;

namespace ocs {
    internal class Program {
        private static async Task Main(string[] args) {

            var opt = Parser.Default.ParseArguments<Options>(args).MapResult(o => o, e =>
                throw new AggregateException(e.Select(s => new Exception(s.ToString())))
            );
            var global = opt.BuildGlobal();
            var builder = new ScriptBuilder();

            var blockParser = new BlockParser();
            try {
                blockParser.Parse(opt.Code);
            }
            catch (FormatException e) {
                Logger.LogError(e.Message);
            }
            catch(Exception e) {
                Logger.LogCritical(e);
            }

            // BEGINブロック
            builder.AddCodeBlock(opt.BeginBlock, ScriptBuilder.BlockType.Begin);
            foreach (var begin in blockParser.BeginBlock) {
                builder.AddCodeBlock(begin, ScriptBuilder.BlockType.Begin);
            }

            // ENDブロック
            builder.AddCodeBlock(opt.EndBlock, ScriptBuilder.BlockType.End);
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

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) => {
                cts.Cancel();
                eventArgs.Cancel = true;
            };

            try {
                var (script, diagnostics) = builder.Build(cts.Token);
                foreach (var diagnostic in diagnostics) {
                    if (diagnostic.WarningLevel == 0) throw new Exception(diagnostic.ToString());
                    Logger.LogWarning(diagnostic.ToString());
                }

                if(opt.ShowGeneratedCode) Logger.LogInformation($"Generated Code\n{builder.GeneratedCode}");

                var state = await script.RunAsync(global, null, cts.Token);
                if (state.Exception != null) throw state.Exception;
            }
            catch (OperationCanceledException e) {
                Logger.LogError(e.Message);
            }
            catch (Exception e) {
                Logger.LogCritical(e.Message);
            }
        }
    }
}
