using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ocs;
using Xunit;

namespace Test {
    public class InnerFunctionTest {
        [Fact]
        public void PrintTest() {
            var gv = new Global();

            var data = new object[] {
                1, "string", 1M, 10.55
            };

            foreach (var obj in data) {
                using var iop = new IoProxy();
                gv.print(obj);
                Assert.Equal(obj + Environment.NewLine, iop.ReadAllFromStdOut());
            }
        }

        [Fact]
        public void PrintCollectionTest() {
            var gv = new Global();
            var data = new IEnumerable<object>[] {
                new object[] {1,2,3,4,5},
                new object[] {"a","b","c","d"}, 
                new object[] {1M,2M,3M},
            };

            foreach (var obj in data) {
                using var iop = new IoProxy();
                var array = obj.ToArray();
                gv.print(array);
                Assert.Equal(string.Join(Environment.NewLine, array) + Environment.NewLine, iop.ReadAllFromStdOut());
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