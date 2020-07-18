using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.Logging;

namespace ocs {
    internal class Program {
        private static async Task Main(string[] args) {
            var opt = Parser.Default.ParseArguments<Options>(args).MapResult(o => o, e => 
                throw new AggregateException(e.Select(s=> new Exception(s.ToString())))
            );
            var global = opt.BuildGlobal();
            var builder = new ScriptBuilder();
            builder.AddCodeBlock(opt.BeginBlock, ScriptBuilder.BlockType.Begin);
            builder.AddCodeBlock(opt.EndBlock, ScriptBuilder.BlockType.End);
            builder.AddCodeBlock(opt.Code);

            if(!string.IsNullOrEmpty(opt.Imports)){
                var imports = opt.Imports.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if(imports.Any()) builder.AddImports(imports);
            }

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) => {
                cts.Cancel();
                eventArgs.Cancel = true;
            };

            var logger = LoggerFactory.Create(config => {
                config.AddConsole(cc => {
                    cc.DisableColors = false;
                    cc.TimestampFormat = "[HH:mm:ss]";
                    cc.LogToStandardErrorThreshold = LogLevel.Warning;
                });
            }).CreateLogger<Program>();

            try {

                var (script, diagnostics) = builder.Build(cts.Token);

                foreach (var diagnostic in diagnostics) {
                    logger.Log((LogLevel) diagnostic.WarningLevel, diagnostic.Descriptor.Description.ToString());
                }

                await script.RunAsync(global, null, cts.Token);
            }
            catch (OperationCanceledException e) {
                logger.LogWarning(e.ToString());
            }
            catch (Exception e) {
                logger.LogCritical(e.ToString());
            }
        }
    }
}
