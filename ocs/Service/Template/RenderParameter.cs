using System.Diagnostics.CodeAnalysis;
using ocs.Lib;

namespace ocs.Service.Template
{
    public class RenderParameter
    {
        [NotNull] public string[] Begin;
        [NotNull] public string[] Main;
        [NotNull] public string[] End;
        [NotNull] public string[] Inline;

        public RenderParameter(IEnumerable<OcsScript> scripts, string[] inline)
        {
            var dict = scripts
                .GroupBy(s => s.Type)
                .ToDictionary(g => g.Key, g => g.Select(s => s.Build()).ToArray());

            Begin = dict.GetValueOrDefault(OcsScript.ScriptType.Begin, Array.Empty<string>());
            Main = dict.GetValueOrDefault(OcsScript.ScriptType.Main, Array.Empty<string>());
            End = dict.GetValueOrDefault(OcsScript.ScriptType.End, Array.Empty<string>());
            Inline = inline;
        }
    }
}
