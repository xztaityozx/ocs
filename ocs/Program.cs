using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace ocs {
    internal class Program {
        private static async Task Main(string[] args) {
            var opt = Parser.Default.ParseArguments<Options>(args).MapResult(o => o, e => 
                throw new AggregateException(e.Select(s=> new Exception(s.ToString())))
            );
            var gv = new Global(
                new GlobalVariableOptions {
                    LoadGlobalEnvironments = opt.LoadEnvironments
                }
            ) {
                Reader = string.IsNullOrEmpty(opt.File)
                    ? new StreamReader(Console.OpenStandardInput())
                    : new StreamReader(opt.File)
            };

            if (!string.IsNullOrEmpty(opt.FieldSeparator)) gv.Separator = new Regex(opt.FieldSeparator);

            static string AppendSemiColon(string s) => s switch {
                var str when string.IsNullOrEmpty(str) => "",
                var str when str.Last() == ';' => str + ";",
                _ => s
            };

            var code = new StringBuilder();
            code.AppendLine(AppendSemiColon(opt.BeginBlock));
            code.AppendLine("using(Reader) while(Reader.Peek() > 0) {");
            code.AppendLine("F0 = Reader.ReadLine();");
            code.AppendLine(AppendSemiColon(opt.Code));
            code.AppendLine("}");
            code.AppendLine(AppendSemiColon(opt.EndBlock));


            // load assembles
            var scriptOpt = ScriptOptions.Default.AddImports("System");
            if (!string.IsNullOrEmpty(opt.Imports)) scriptOpt.AddImports(opt.Imports.Split(','));

            
            using (var cts = new CancellationTokenSource()) {
                var script = CSharpScript.Create(
                    code.ToString(),
                    globalsType: typeof(Global),
                    options: scriptOpt
                );

                var compileResult = script.Compile(cts.Token);
                //if (!compileResult.IsEmpty) {
                //    foreach (var diagnostic in compileResult) {
                //        Console.Error.WriteLine(diagnostic.Descriptor.Category);
                //        Console.Error.WriteLine(diagnostic.Descriptor.Description);
                //    }
                //}

                var result = await script.RunAsync(gv, cts.Token);
            }


            //// BEGIN
            //var script = await CSharpScript.RunAsync(
            //    opt.BeginBlock+";",
            //    globalsType: typeof(Global),
            //    globals: gv,
            //    options: scriptOpt
            //);

            //// CODE
            //using (var sr = new StreamReader(Console.OpenStandardInput())) {
            //    while (sr.Peek() > 0) {
            //        gv.F0 = sr.ReadLine();
            //        await script.ContinueWithAsync(opt.Code);
            //    }
            //}

            //// END
            //await script.ContinueWithAsync(opt.EndBlock);
        }
    }

    public class Options {
        [Option('b', "begin", Default = "", HelpText = "begin codes. this code runs at begin")]
        public string BeginBlock { get; set; }
        [Option('e', "end", Default = "", HelpText = "end codes. this code runs at after all")]
        public string EndBlock { get; set; }
        [Option('I', "imports", HelpText = "using Assembles")]
        public string Imports { get; set; }
        [Option('F', "field", Default = "", HelpText = "Field separator")]
        public string FieldSeparator { get; set; }
        [Option('f', "file", HelpText = "target file")]
        public string File { get; set; }
        [Option("env", HelpText = "load global environments")]
        public bool LoadEnvironments { get; set; }
        [Value(0, MetaName = "code", HelpText = "CODE block")]
        public string Code { get; set; }

        public Global BuildGlobal() => new Global(new GlobalVariableOptions {
            LoadGlobalEnvironments = LoadEnvironments,
        }) {
            Reader = string.IsNullOrEmpty(File)
                ? new StreamReader(Console.OpenStandardInput())
                : new StreamReader(File)
        };
    }
}
