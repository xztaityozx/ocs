using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace ocs {
    public class ScriptBuilder {
        public enum BlockType {
            Begin,
            End,
            Main,
        }

        private readonly Dictionary<BlockType, List<string>> blockDictionary =
            new Dictionary<BlockType, List<string>> {
                [BlockType.Main] =  new List<string>(),
                [BlockType.Begin] = new List<string>(),
                [BlockType.End] =   new List<string>()
            };

        private readonly List<string> importList = new List<string> {"System"};

        private readonly List<string> assemblyList = new List<string>();

        public string GeneratedCode { get; private set; }

        /// <summary>
        /// Add imports
        /// </summary>
        /// <param name="imports"></param>
        public void AddImports(params string[] imports) => importList.AddRange(imports);

        /// <summary>
        /// Add assembly
        /// </summary>
        /// <param name="assemblies"></param>
        public void AddAssemblies(params string[] assemblies) => assemblyList.AddRange(assemblies);

        /// <summary>
        /// Add code to specified block
        /// </summary>
        /// <param name="code"></param>
        /// <param name="blockType"></param>
        public void AddCodeBlock(string code, BlockType blockType = BlockType.Main) =>
            blockDictionary[blockType].Add(code switch
            {
                var s when string.IsNullOrEmpty(s) => s,
                var s when s.Last() != ';' => s + ";",
                _ => code
            });
        

        public string this[BlockType index] => string.Join(Environment.NewLine, blockDictionary[index]);

        private string BuildCode() {
            var sb = new StringBuilder();
            sb.AppendLine(this[BlockType.Begin]);
            sb.AppendLine("using(Reader) while(Reader.Peek() > 0) {");
            sb.AppendLine("F0 = Reader.ReadLine();");
            sb.AppendLine(this[BlockType.Main]);
            sb.AppendLine("}");
            sb.AppendLine(this[BlockType.End]);

            return sb.ToString();
        }

        public (Script, ImmutableArray<Diagnostic>) Build(CancellationToken token) {
            var opt = ScriptOptions.Default
                .AddReferences(
                    typeof(System.Console).Assembly, typeof(System.Type).Assembly,
                    typeof(IEnumerable<>).Assembly, typeof(ScriptBuilder).Assembly)
                .AddReferences(assemblyList)
                .AddImports("System", "System.Linq", "System.Collections", "System.Collections.Generic")
                .AddImports(importList);

            GeneratedCode = BuildCode();

            var script = CSharpScript.Create(GeneratedCode, opt, typeof(Global));

            var compileResult = script.Compile(token);

            return (script, compileResult);
        }
    }
}
