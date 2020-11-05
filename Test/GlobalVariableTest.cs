using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ocs;
using Xunit;

namespace Test {
    public class GlobalVariableTest {
        [Fact]
        public void FieldTest() {
            var data = new[] {
                new {f0 = "a b c d", sep = new Regex(@"\s")},
                new {f0 = "a\tb\tc\td", sep = new Regex(@"\s")},
                new {f0 = "", sep = new Regex(@" ")}
            };

            foreach (var item in data) {
                var gv = new Global {F0 = item.f0, Separator = item.sep};
                foreach (var (e,a) in new List<string>{item.f0}.Concat(item.sep.Split(item.f0)).Zip(gv.F)) {
                    Assert.Equal(e, a);
                }
                Assert.Equal(item.sep.Split(item.f0).Count(s => !string.IsNullOrEmpty(s)), gv.NF);
            }
        }

        [Fact]
        public void SetEnvTest() {
            var gv = new Global();
            Assert.Empty(gv.Env);

            gv.SetEnv("XXX", "YYY");
            Assert.Single(gv.Env);
            Assert.True(gv.Env.ContainsKey("XXX"));
            Assert.Equal("YYY", gv.Env["XXX"]);

            gv.SetEnv("AAA", "BBB");
            Assert.True(gv.Env.ContainsKey("AAA"));
            Assert.Equal("BBB", gv.Env["AAA"]);
            Assert.True(gv.Env.ContainsKey("XXX"));
            Assert.Equal("YYY", gv.Env["XXX"]);

            gv.SetEnv("AAA", "CCC");
            Assert.True(gv.Env.ContainsKey("AAA"));
            Assert.Equal("CCC", gv.Env["AAA"]);
            Assert.True(gv.Env.ContainsKey("XXX"));
            Assert.Equal("YYY", gv.Env["XXX"]);
        }

        [Fact]
        public void SetEnvWithGlobalTest() {
            var gv = new Global(new GlobalVariableOptions {LoadGlobalEnvironments = true});
            var env = Environment.GetEnvironmentVariables();
            Assert.NotEmpty(gv.Env);
            foreach (var key in gv.Env.Keys) {
                Assert.Equal(env[key], gv.Env[key]);
            }
            gv.SetEnv("XXX", "YYY");
            gv.SetEnv("AAA", "BBB");
            Assert.True(gv.Env.ContainsKey("AAA"));
            Assert.Equal("BBB", gv.Env["AAA"]);
            Assert.True(gv.Env.ContainsKey("XXX"));
            Assert.Equal("YYY", gv.Env["XXX"]);
        }
    }
}
