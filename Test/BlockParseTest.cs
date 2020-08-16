using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ocs;
using Xunit;

namespace Test {
    public class BlockParseTest {
        [Fact]
        public void MainBlockParseTest() {
            var testCaseTuples = new[] {
                (new[]{"action1;"}, "{action1}"),
                (new[]{"if(condition1){action1;}"}, "condition1{action1}"),
                (new[]{"if(condition2){action2;}"}, "condition2 { action2 }"),
                (new[]{"if(condition3){action3;}"}, "condition3{action3;}"),
                (new[]{"if(condition4){action4;}"}, "condition4 { action4; }"),
                (new[]{"if(cond5){act5;}", "act6;"}, "cond5{act5}{act6}"),
                (new[]{"if(cond6){act6;}", "act7;", "if(cond8){act8;}"}, "cond6{act6}{act7}cond8{act8;}"),
                (new[]{"if(cond9.Select(k=>k+\"{\").All()){act9;}"}, "cond9.Select(k=>k+\"{\").All(){act9}"),
                (new[]{"if(cond10.Select(k=>k+'{').All()){act10;}"}, "cond10.Select(k=>k+'{').All(){act10}"),
                (new[]{"if((new int[\"xyz\".Where(c => {return c == 'x'}).Count()]).Length == 1){act11;}"},
                    "(new int[\"xyz\".Where(c => {return c == 'x'}).Count()]).Length == 1{act11}"),
                (new[]{"cond.Select(item => item + '{');"}, "{cond.Select(item => item + '{');}"),
                (new[]{"println($\"{F[0]}, {F[1]}\");"}, "{println($\"{F[0]}, {F[1]}\")}"),
            };

            var blockParser = new BlockParser();

            foreach (var (expects, input) in testCaseTuples) {
                blockParser.Parse(input);
                Assert.Equal(blockParser.MainBlock.Count, expects.Length);
                Assert.Empty(blockParser.BeginBlock);
                Assert.Empty(blockParser.EndBlock);
                foreach (var (e, a) in expects.Zip(blockParser.MainBlock, (e, a) => (e,a))) {
                    Assert.Equal(e, a);
                }
            }
        }

        [Fact]
        public void BeginBlockTest() {
            var p = new BlockParser();
            var data = new[] {
                ("",""),
                ("BEGIN{action1}", "action1;"),
                ("BEGIN{action2}BEGIN{action3}", "action2;action3;"),
                ("BEGIN{action4}{action5}condition{action6}BEGIN{action7}","action4;action7;"),
                ("BEGIN{action4}{action5}condition{action6}END{action7}","action4;")
            };

            foreach (var (input, expect) in data) {
                p.Parse(input);
                Assert.Equal(expect, string.Join("", p.BeginBlock));
            }
        }


        [Fact]
        public void EndBlockTest() {
            var p = new BlockParser();
            var data = new[] {
                ("",""),
                ("END{action1}", "action1;"),
                ("END{action2}END{action3}", "action2;action3;"),
                ("END{action4}{action5}condition{action6}END{action7}","action4;action7;"),
                ("END{action4}{action5}condition{action6}BEGIN{action7}","action4;")
            };

            foreach (var (input, expect) in data) {
                p.Parse(input);
                Assert.Equal(expect, string.Join("", p.EndBlock));
            }
        }
    }
}
