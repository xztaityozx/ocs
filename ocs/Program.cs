using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;

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

            {
                var imports = opt.Imports.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if(imports.Any()) builder.AddImports(imports);
            }

            using var cts = new CancellationTokenSource();
            var script = builder.Build(cts.Token);
            
            await script.RunAsync(global, null, cts.Token);
        }
    }
}
