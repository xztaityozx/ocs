using ocs.Lib;
using Xunit;

namespace Test
{
    public class OcsScriptTest
    {
        [Fact]
        public void Test_Type()
        {
            var testCase = new[]
            {
                (pattern: "BEGIN", expect: OcsScript.ScriptType.Begin),
                (pattern: "END", expect: OcsScript.ScriptType.End),
                (pattern: "", expect: OcsScript.ScriptType.Main),
                (pattern: "F0==\"abc\"", expect: OcsScript.ScriptType.Main)
            };

            foreach (var (pattern, expect) in testCase)
            {
                var actual = new OcsScript(pattern);
                Assert.Equal(expect, actual.Type);
            }
        }

        [Fact]
        public void Test_Build()
        {
            var testCase = new[]
            {
                (pattern: "BEGIN", action: "println(F[1])", expect: "println(F[1]);"),
                (pattern: "END", action: "println(F[2])", expect: "println(F[2]);"),
                (pattern: "BEGIN", action: null, expect: "println(F0);"),
                (pattern: "END", action: null, expect: "println(F0);"),
                (pattern: "Pattern", action: null, expect: "if(Pattern){println(F0);}"),
                (pattern: "Pattern", action: "println(F[3])", expect: "if(Pattern){println(F[3]);}"),
                (pattern: "", action: "println(F[4])", expect: "println(F[4]);"),
            };

            foreach (var (pattern, action, expect) in testCase)
            {
                var actual = action is not null ? new OcsScript(pattern, action) : new OcsScript(pattern);
                Assert.Equal(pattern, actual.Pattern);
                Assert.Equal(action ?? "println(F0)", actual.Action);
                Assert.Equal(expect, actual.Build());
            }

            {
                var throws = new OcsScript("Pattern", "");
                Assert.Throws<InvalidSyntaxException>(() => throws.Build());
            }
        }
    }
}
