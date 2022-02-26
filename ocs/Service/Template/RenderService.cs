using ocs.Lib;
using ocs.Lib.Config;

namespace ocs.Service.Template
{
    public class RenderService
    {
        private readonly Config config;
        public RenderService(Config config) => this.config = config;
        public string Render(IEnumerable<OcsScript> scripts)
        {
            return Scriban.Template.Parse(Template).Render(new RenderParameter(scripts, config.InlineCode));
        }

        private const string Template = @"
public class Runner : IRunner {
    private Global global;

    public Runner(Global global) => this.global = global;

    private string Ofs => global.Ofs;
    private int NR => global.NR;
    private int NF => global.NF;
    private List<string> F => global.F;
    private string F0 => global.F0;

    private void print(params object[] o) => global.Print(Global.PrintOption.None,o);
    private void println(params object[] o) => global.Print(Global.PrintOption.Line, o);
    private int i(string s) => global.i(s); 
    private decimal d(string s) => global.d(s); 

    {{ for code in inline }}
    {{ code }}
    {{ end }}

    // Entry point
    public void Run() {
        {{ for script in begin }}
        {{ script }}
        {{ end }}
        while(global.NextLine()) {
            {{ for script in main }}
            {{ script }}
            {{ end }}
        }
        {{ for script in end }}
        {{ script }}
        {{ end }}
    }
}
";
    }
}
