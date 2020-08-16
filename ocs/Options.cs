﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine;

namespace ocs {
    public class Options {
        [Option('I', "imports", HelpText = "using Assembles")]
        public string Imports { get; set; }

        [Option('F', "field", Default = "", HelpText = "Field separator")]
        public string FieldSeparator { get; set; }

        [Option('f', "file", HelpText = "target file")]
        public string File { get; set; }

        [Option("env", Default = false, HelpText = "load global environments")]
        public bool LoadEnvironments { get; set; }

        [Option("show", Default = false, HelpText = "show generated code")]
        public bool ShowGeneratedCode { get; set; }

        [Value(0, MetaName = "code", HelpText = "CODE block")]
        public string Code { get; set; }

        public Global BuildGlobal() => new Global(new GlobalVariableOptions {
            LoadGlobalEnvironments = LoadEnvironments,
        }) {
            Reader = string.IsNullOrEmpty(File)
                ? new StreamReader(Console.OpenStandardInput())
                : new StreamReader(File),
            Separator = string.IsNullOrEmpty(FieldSeparator) ? new Regex(@"\s") : new Regex(FieldSeparator)
        };
    }

    public class OptionException : Exception {
        public readonly IReadOnlyList<string> Errors;
        public OptionException(IEnumerable<string> errors) {
            Errors = errors.ToList();
        }
    }
}
