using ocs.Lib;
using System.Linq;
using Xunit;

namespace Test
{
    public class ParserTest
    {
        [Fact]
        public void Test_Parse()
        {
            var testCase = new[]
            {
                (input: "ABC{DEF}", expects: new[] { new OcsScript("ABC", "DEF") }),
                (input: "ABC", expects: new[] { new OcsScript("ABC") }),
                (input: "ABC;DEF;", expects: new[] { new OcsScript("ABC"), new OcsScript("DEF") }),
                (input: "{DEF}", expects: new[] { new OcsScript("", "DEF") }),
                (input: "{ABC}{DEF}", expects: new[] { new OcsScript("", "ABC"), new OcsScript("", "DEF") }),
                (input: "{ABC}F0==\"abc\"{DEF}",
                    expects: new[] { new OcsScript("", "ABC"), new OcsScript("F0==\"abc\"", "DEF") }),
                (input: "BEGIN{println(F[1])}END{println(F[2])}{ABC}F0==\"abc\"{DEF}", expects: new[]
                {
                    new OcsScript("BEGIN", "println(F[1])"),
                    new OcsScript("END", "println(F[2])"),
                    new OcsScript("", "ABC"), new OcsScript("F0==\"abc\"", "DEF"),
                }),
                (input: "F.Any(f => f==\"100\"){println(F)}",
                    expects: new[] { new OcsScript("F.Any(f => f==\"100\")", "println(F)") })
            };

            foreach (var (input, expects) in testCase)
            {
                Assert.Equal(expects, Parser.Parse(input).ToArray());
            }
        }
    }
}