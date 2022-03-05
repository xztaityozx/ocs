using ocs.Lib;
using ocs.Lib.Config;
using ocs.Service.Template;
using Xunit;

namespace Test.Service.Template
{
    public class RenderServiceTest
    {
        [Fact]
        public void Test_Render()
        {
            var testCase = new[]
            {
                (config: new Config
                {
                    InlineCode = new []{"ABC"}
                }, scripts: new[]
                {
                    new OcsScript("BEGIN"),
                    new OcsScript("END"),
                    new OcsScript("true")
                })
            };

            foreach (var (config, param) in testCase)
            {
                var actual = new RenderService(config).Render(param);
                Assert.NotNull(actual);
            }
        }
    }
}
