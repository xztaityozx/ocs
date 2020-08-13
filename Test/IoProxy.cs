using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Test {
    public class IoProxy : IDisposable {
        private readonly TextReader defaultStdIn = Console.In;
        private readonly TextWriter defaultStdOut = Console.Out;

        private readonly StringWriter stdOut;

        public IoProxy() {
            stdOut = new StringWriter();
            Console.SetOut(stdOut);
        }


        public string ReadAllFromStdOut() => stdOut == null ? "" : stdOut.ToString();

        public IEnumerable<string> ReadLineFromStdOut() => ReadAllFromStdOut().Split(Environment.NewLine);

        public void Dispose() {
            stdOut?.Dispose();
            Console.SetOut(defaultStdOut);
            Console.SetIn(defaultStdIn);
        }
    }
}
