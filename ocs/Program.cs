using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            var gv = new GlobalVariable(
                new GlobalVariableOptions {
                    LoadGlobalEnvironments = opt.LoadEnvironments
                }
            );

            if (!string.IsNullOrEmpty(opt.FieldSeparator)) gv.Separator = new Regex(opt.FieldSeparator);

            // load assembles
            var scriptOpt = ScriptOptions.Default.AddImports("System");
            if (!string.IsNullOrEmpty(opt.Imports)) scriptOpt.AddImports(opt.Imports.Split(','));
            
            // BEGIN
            var script = await CSharpScript.RunAsync(
                opt.BeginBlock+";",
                globalsType: typeof(GlobalVariable),
                globals: gv,
                options: scriptOpt
            );

            // CODE
            using (var sr = new StreamReader(Console.OpenStandardInput())) {
                while (sr.Peek() > 0) {
                    gv.F0 = sr.ReadLine();
                    await script.ContinueWithAsync(opt.Code);
                }
            }

            // END
            await script.ContinueWithAsync(opt.EndBlock);
        }
    }

    public class Options {
        [Option('b', "begin", Default = "", HelpText = "begin codes. this code runs at begin")]
        public string BeginBlock { get; set; }
        [Option('e', "end", Default = "", HelpText = "end codes. this code runs at after all")]
        public string EndBlock { get; set; }
        [Option('M', "imports", HelpText = "using Assembles")]
        public string Imports { get; set; }
        [Option('F', "field", Default = "", HelpText = "Field separator")]
        public string FieldSeparator { get; set; }
        [Option('f', "file", HelpText = "target file")]
        public string File { get; set; }
        [Option("env", HelpText = "load global environments")]
        public bool LoadEnvironments { get; set; }
        [Value(0, MetaName = "code", HelpText = "CODE block")]
        public string Code { get; set; }
    }
}
