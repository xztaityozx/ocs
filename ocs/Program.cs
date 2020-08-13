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
            builder.AddCodeBlock(opt.BeginBlock, ScriptBuilder.BlockType.Begin);
            builder.AddCodeBlock(opt.EndBlock, ScriptBuilder.BlockType.End);
            builder.AddCodeBlock(opt.Code);

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

                var state = await script.RunAsync(global, null, cts.Token);
                if (state.Exception != null) throw state.Exception;
            }
            catch (OperationCanceledException e) {
                Logger.LogCritical(e.Message);
            }
            catch (Exception e) {
                Logger.LogCritical(e.Message);
            }
        }
    }
}
