using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ocs;
using Xunit;

namespace Test {
    public class InnerFunctionTest {
        [Fact]
        public void PrintlnTest() {
            var g = new Global();
            var data = new object[] {
                1, 1M, 10.55, "string"
            };

            foreach (var obj in data) {
                using var iop = new IoProxy();
                g.println(obj);
                Assert.Equal(obj + Environment.NewLine, iop.ReadAllFromStdOut());
            }

            {
                using var iop =new IoProxy();
                var tuple = (1, 2, 3, "xxx");
                g.println(tuple);
                Assert.Equal(string.Join(" ", tuple) + Environment.NewLine, iop.ReadAllFromStdOut());
            }

            {
                using var iop = new IoProxy();
                var tuple = (1, 2, 3, "xxx");
                g.Env.Add("OFS", ":");
                g.println(tuple);
                Assert.Equal(string.Join(":", tuple) + Environment.NewLine, iop.ReadAllFromStdOut());
            }
        }

        [Fact]
        public void PrintTest() {
            var gv = new Global();

            var data = new object[] {
                1, 1M, 10.55
            };

            foreach (var obj in data) {
                using var iop = new IoProxy();
                gv.print(obj);
                Assert.Equal(obj.ToString(), iop.ReadAllFromStdOut());
            }
        }

        [Fact]
        public void StringPrintTest() {
            var g = new Global();
            using var iop = new IoProxy();
            g.print("abc");
            Assert.Equal("abc", iop.ReadAllFromStdOut());
        }

        [Fact]
        public void PrintCollectionTest() {
            var gv = new Global();
            var data = new[] {"a", "b", "c"};
            var expect = string.Join(Environment.NewLine, data) + Environment.NewLine;

            using var iop = new IoProxy();
            gv.print(data);

            Assert.Equal(expect, iop.ReadAllFromStdOut());
        }

        [Fact]
        public void TuplePrintTest() {
            var gv = new Global();
            var data = (1, 3, "xx");
            var expect = string.Join(" ", data);

            {
                using var iop = new IoProxy();
                gv.print(data);

                Assert.Equal(expect, iop.ReadAllFromStdOut());
            }
            {
                using var iop = new IoProxy();

                gv.Env.Add("OFS", ":");
                gv.print(data);
                expect = string.Join(":", data);
                Assert.Equal(expect, iop.ReadAllFromStdOut());
            }
        }

        [Fact]
        public void ParseIntTest() {
            var gv = new Global();
            Assert.Equal(1, gv.i("1"));
            Assert.Equal(10, gv.i("10.11"));
            Assert.Throws<FormatException>(() => gv.i("str"));
        }

        [Fact]
        public void ParseDecimalTest() {
            var gv = new Global();
            Assert.Equal(1M, gv.d("1"));
            Assert.Equal(1.234M, gv.d("1.234"));
            Assert.Equal(1E9M, gv.d("1E9"));
            Assert.Equal(1E9M, gv.d("1e9"));
            Assert.Throws<FormatException>(() => gv.d("str"));
        }
    }
}