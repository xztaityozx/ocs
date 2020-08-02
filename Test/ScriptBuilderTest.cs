using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ocs;
using Xunit;

namespace Test {
    public class ScriptBuilderTest {
        [Fact]
        public void AddImportsTest() {
            var sb = new ScriptBuilder();
            sb.AddImports("System.Threading");
            sb.AddImports("System.Text", "System.IO");

            var (_, diagnostics) = sb.Build(CancellationToken.None);

            Assert.Empty(diagnostics);
        }

        [Fact]
        public void AddAssembliesTest() {
            var sb = new ScriptBuilder();
            sb.AddAssemblies("System.Core");
            sb.AddImports("System.Linq");

            var (_, diagnostic) = sb.Build(CancellationToken.None);
            Assert.Empty(diagnostic);
        }

        [Fact]
        public void AddCodeBlockTest() {
            var sb = new ScriptBuilder();
            sb.AddCodeBlock("print(\"This is MainBlock\")");
            sb.AddCodeBlock("print(\"This is MainBlock 2\")");
            sb.AddCodeBlock("print(\"This is MainBlock 3\")");

            sb.AddCodeBlock("print(\"This is BeginBlock\")", ScriptBuilder.BlockType.Begin);
            sb.AddCodeBlock("print(\"This is BeginBlock 2\")", ScriptBuilder.BlockType.Begin);
            sb.AddCodeBlock("print(\"This is BeginBlock 3\")", ScriptBuilder.BlockType.Begin);

            sb.AddCodeBlock("print(\"This is EndBlock\")", ScriptBuilder.BlockType.End);
            sb.AddCodeBlock("print(\"This is EndBlock 2\")", ScriptBuilder.BlockType.End);
            sb.AddCodeBlock("print(\"This is EndBlock 3\")", ScriptBuilder.BlockType.End);

            var (script, diagnostics) = sb.Build(CancellationToken.None);
            Assert.Empty(diagnostics);

            var expect = new List<string> {
                "print(\"This is BeginBlock\");",
                "print(\"This is BeginBlock 2\");",
                "print(\"This is BeginBlock 3\");",
                "using(Reader) while(Reader.Peek() > 0) {",
                "F0 = Reader.ReadLine();",
                "print(\"This is MainBlock\");",
                "print(\"This is MainBlock 2\");",
                "print(\"This is MainBlock 3\");",
                "}",
                "print(\"This is EndBlock\");",
                "print(\"This is EndBlock 2\");",
                "print(\"This is EndBlock 3\");"
            };

            Assert.Equal(string.Join(Environment.NewLine, expect) + Environment.NewLine, script.Code);
        }
        [Fact]
        public async Task ReaderTest() {
            var sb = new ScriptBuilder();
            sb.AddCodeBlock("println(F0)");
            sb.AddCodeBlock("println(F[1])");
            sb.AddCodeBlock("println(F[2])");
            var (script, _) = sb.Build(CancellationToken.None);

            using var iop = new IoProxy();

            await script.RunAsync(new Global {
                Reader = new StringReader(string.Join(Environment.NewLine, new[] {
                    "1 a",
                    "a 1",
                    "2 b",
                    "b 2",
                    "3 c",
                    "c 3"
                })),
                Separator = new Regex(@"\s")
            });

            var expects = new[] {
                "1 a", "1", "a",
                "a 1", "a", "1",
                "2 b", "2", "b",
                "b 2", "b", "2",
                "3 c", "3", "c",
                "c 3", "c", "3"
            };

            foreach (var (expect, actual) in expects.Zip(iop.ReadLineFromStdOut())) {
                Assert.Equal(expect, actual);
            }
        }
    }
}
