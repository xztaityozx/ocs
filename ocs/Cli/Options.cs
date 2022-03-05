using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using CommandLine;
using Microsoft.CodeAnalysis.CSharp;

namespace ocs.Cli
{
    public record Options
    {
        [Option('g', "use-regexp", Default = false, HelpText = "HelpTextUseRegexp", ResourceType = typeof(Resource))]
        public bool UseRegexp { get; init; } = false;

        [Option('d', "input-delimiter", Default = " ", HelpText = "HelpTextInputDelimiter")]
        public string InputDelimiter { get; init; } = " ";

        [Option('D', "output-delimiter", Default = " ", HelpText = "HelpTextOutputDelimiter")]
        public string OutputDelimiter { get; init; } = " ";

        [Option('U', "using-list", Default = null, HelpText = "HelpTextUsingList", Separator = ',')]
        public ImmutableArray<string>? UsingList { get; init; } = null;

        [Option('R', "reference-list", Default = null, HelpText = "HelpTextReferenceList",
            Separator = ',')]
        public ImmutableArray<string>? ReferenceList { get; init; } = null;

        [Option("language-version", Default = null, HelpText = "HelpTextLanguageVersion")]
        public LanguageVersion? LanguageVersion { get; init; } = null;

        [Option('r', "remove-empty", Default = false, HelpText = "HelpTextRemoveEmpty")]
        public bool RemoveEmpty { get; init; } = false;

        [Option('f', "file", Default = null, HelpText = "HelpTextFile",
            ResourceType = typeof(Resource))]
        [AllowNull]
        public FileInfo File { get; init; } = null;

        [Option("print-generated", Default = false, HelpText = "HelpTextPrintGenerated")]
        public bool PrintGenerated { get; init; } = false;

        [Value(0, Required = true, HelpText = "HelpTextValue0")]
        public string Script { get; init; }
    }
}
