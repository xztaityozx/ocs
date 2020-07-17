using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;

namespace ocs {
    public class ScriptBuilder {
        public enum BlockType {
            Begin,
            End,
            Main
        }

        private readonly Dictionary<BlockType, StringBuilder> blockDictionary =
            new Dictionary<BlockType, StringBuilder> {
                [BlockType.Main] = new StringBuilder(),
                [BlockType.Begin] = new StringBuilder(),
                [BlockType.End] = new StringBuilder()
            };

        private readonly List<string> importList= new List<string>{"System"};

        private readonly ILogger logger = LoggerFactory.Create(
            builder => builder.AddConsole(config => {
                config.DisableColors = false;
                config.LogToStandardErrorThreshold = LogLevel.Warning;
                config.TimestampFormat = "[HH:mm:ss]";
            })
        ).CreateLogger<ScriptBuilder>();

        /// <summary>
        /// Add import assemblies
        /// </summary>
        /// <param name="imports"></param>
        public void AddImports(params string[] imports) => importList.AddRange(imports);


        /// <summary>
        /// Add code to specified block
        /// </summary>
        /// <param name="code"></param>
        /// <param name="blockType"></param>
        public void AddCodeBlock(string code, BlockType blockType = BlockType.Main) {
            blockDictionary[blockType] ??= new StringBuilder();
            blockDictionary[blockType].AppendLine(code);
        }


        public string this[BlockType index] {
            get {
                var code = blockDictionary[index].ToString();

                return code switch {
                    var s when string.IsNullOrEmpty(s) => s,
                    var s when s.Last() == ';' => s + ";",
                    _ => code
                };
            }
        }

        private string BuildCode() {
            var sb = new StringBuilder();
            sb.AppendLine(this[BlockType.Begin]);
            sb.AppendLine("using(Stream) while(Stream.Peek() > 0) {");
            sb.AppendLine("F0 = Stream.ReadLine();");
            sb.AppendLine(this[BlockType.Main]);
            sb.AppendLine("}");
            sb.AppendLine(this[BlockType.End]);

            return sb.ToString();
        }


        public Script Build(CancellationToken token) {
            var opt = ScriptOptions.Default
                .AddImports(importList);

            var script = CSharpScript.Create(BuildCode(), opt);

            var compileResult = script.Compile(token);
            
            if (!compileResult.IsEmpty) {
                foreach (var diagnostic in compileResult) {
                    logger.Log((LogLevel) diagnostic.WarningLevel, diagnostic.Descriptor.Description.ToString());
                }
            }

            return script;
        }
    }
}
