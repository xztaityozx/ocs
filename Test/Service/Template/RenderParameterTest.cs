using ocs.Lib;
using ocs.Service.Template;
using System;
using System.Collections.Generic;
using Xunit;

namespace Test.Service.Template
{
    public class RenderParameterTest
    {
        [Fact]
        public void Test_New()
        {
            var testCase = new[]
            {
                (
                    scripts: new[] { new OcsScript("BEGIN", "print(F)"), new OcsScript("END") },
                    inline: Array.Empty<string>(),
                    expect: new Dictionary<OcsScript.ScriptType, string[]>
                    {
                        [OcsScript.ScriptType.Begin] = new[] { "print(F);" },
                        [OcsScript.ScriptType.End] = new[] { "println(F0);" },
                    }
                ),
                (
                    scripts: new[] { new OcsScript("END") },
                    inline: new [] {"THIS_IS_INLINE"},
                    expect: new Dictionary<OcsScript.ScriptType, string[]>
                    {
                        [OcsScript.ScriptType.End] = new[] { "println(F0);" }
                    }
                ),
                (
                    scripts: new[] { new OcsScript("END") },
                    inline: Array.Empty<string>(),
                    expect: new Dictionary<OcsScript.ScriptType, string[]>
                    {
                        [OcsScript.ScriptType.End] = new[] { "println(F0);" }
                    }
                ),
                (
                    scripts: new[] { new OcsScript("BEGIN") },
                    inline: Array.Empty<string>(),
                    expect: new Dictionary<OcsScript.ScriptType, string[]>
                    {
                        [OcsScript.ScriptType.Begin] = new[] { "println(F0);" }
                    }
                ),
                (
                    scripts: new[]
                    {
                        new OcsScript("BEGIN"),
                        new OcsScript("BEGIN", "print(F[2])")
                    },
                    inline: Array.Empty<string>(),
                    expect: new Dictionary<OcsScript.ScriptType, string[]>
                    {
                        [OcsScript.ScriptType.Begin] = new[] { "println(F0);", "print(F[2]);" }
                    }
                ),
                (
                    scripts: new[]
                    {
                        new OcsScript("BEGIN"),
                        new OcsScript("BEGIN", "print(F[2])"),
                        new OcsScript("NR%2==0"),
                        new OcsScript("NR%2==1", "print(F[1])"),
                        new OcsScript("END"),
                        new OcsScript("END", "print(F[2])"),
                    },
                    inline: Array.Empty<string>(),
                    expect: new Dictionary<OcsScript.ScriptType, string[]>
                    {
                        [OcsScript.ScriptType.Begin] = new[] { "println(F0);", "print(F[2]);" },
                        [OcsScript.ScriptType.End] = new[] { "println(F0);", "print(F[2]);" },
                        [OcsScript.ScriptType.Main] = new[]
                        {
                            "if(NR%2==0){println(F0);}",
                            "if(NR%2==1){print(F[1]);}"
                        }
                    }
                ),
            };

            foreach (var (scripts, inline, expect) in testCase)
            {
                var actual = new RenderParameter(scripts, inline);
                foreach (var (key, value) in expect)
                {
                    switch (key)
                    {
                        case OcsScript.ScriptType.Begin:
                            Assert.Equal(value, actual.Begin);
                            break;
                        case OcsScript.ScriptType.End:
                            Assert.Equal(value, actual.End);
                            break;
                        case OcsScript.ScriptType.Main:
                            Assert.Equal(value, actual.Main);
                            break;
                        default:
                            throw new IndexOutOfRangeException();
                    }
                }
                Assert.Equal(inline, actual.Inline);
            }
        }
    }
}
