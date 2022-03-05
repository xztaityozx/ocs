
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp;

namespace ocs.Service.Compile;

public record CompileParameter(
    [NotNull] Global.Global Global,
    [NotNull] IEnumerable<string> UsingList,
    [NotNull] IEnumerable<string> ReferenceList,
    LanguageVersion LanguageVersion,
    [NotNull] string RenderedScript
)
{
    public override string ToString()
    {
        return JsonSerializer.Serialize(new
        {
            UsingList,
            LanguageVersion,
            ReferenceList,
            RenderedScript
        });
    }
}
