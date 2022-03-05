using ocs.Global;
using Xunit;

namespace Test
{
    public class GlobalTest
    {
        [Fact]
        public void Test_F()
        {
            var expect = new[] { "a b c", "a", "b", "c" };
            using var global = new Global(new GlobalVariableOption())
            {
                F0 = "a b c"
            };


            Assert.Equal(expect, global.F);
        }
        [Fact]
        public void Test_NF()
        {
            const int expect = 3;
            using var global = new Global(new GlobalVariableOption())
            {
                F0 = "a b c"
            };

            Assert.Equal(expect, global.NF);
        }
        [Fact]
        public void Test_F0()
        {
            const string expect = "a b c";
            using var global = new Global(new GlobalVariableOption())
            {
                F0 = expect
            };

            Assert.Equal(expect, global.F0);
        }
        [Fact]
        public void Test_NR()
        {
            const int expect = 1;
            using var global = new Global(new GlobalVariableOption())
            {
                F0 = "a b c"
            };

            Assert.Equal(expect, global.NR);
        }

        [Fact]
        public void Test_d()
        {
            var testCase = new[] { ("1", 1M), ("123", 123M), ("1.1234", 1.1234M) };
            using var global = new Global(new GlobalVariableOption());

            foreach (var (input, expect) in testCase)
            {
                Assert.Equal(expect, global.d(input));
            }
        }
        [Fact]
        public void Test_i()
        {
            var testCase = new[] { ("1", 1), ("123", 123), ("1.1234", 1) };
            using var global = new Global(new GlobalVariableOption());

            foreach (var (input, expect) in testCase)
            {
                Assert.Equal(expect, global.i(input));
            }
        }
    }
}
