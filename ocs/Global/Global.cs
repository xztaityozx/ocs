using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using ocs.Extensions;

namespace ocs.Global
{
    /// <summary>
    /// ocsスクリプト内で使えるメソッドとか変数を定義してるところです
    /// </summary>
    public class Global : IDisposable
    {

        private Regex InputDelimiterRegex { get; }

        private bool UseRegex { get; }

        private string InputDelimiterString { get; }

        private bool RemoveEmpty { get; }

        private StreamWriter OutputStreamWriter { get; }
        private StreamReader InputStreamReader { get; }

        private List<string> f;
        private string f0;

        public string F0
        {
            get => f0;
            set
            {
                f0 = value;
                f = null;
                NR++;
            }
        }

        public int NR { get; private set; }

        public List<string> F
        {
            get
            {
                if (f != null) return f;

                if (UseRegex.Not())
                {
                    f = new[] { f0 }.Concat(f0.Split(InputDelimiterString,
                        RemoveEmpty
                            ? StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                            : StringSplitOptions.None)).ToList();

                    return f;
                }

                IEnumerable<string> a = new[] { f0 }.Concat(InputDelimiterRegex.Split(f0));
                if (RemoveEmpty) a = a.Where(s => string.IsNullOrEmpty(s).Not());

                f = a.ToList();

                return f;
            }
        }

        public int NF => Math.Max(0, F.Count - 1);

        public string Ofs { get; }

        public enum PrintOption
        {
            None, Line
        }

        private string Serialize(IEnumerable<object> items) => string.Join(Ofs, items.Select(o => o switch
        {
            ValueTuple vt => $"({string.Join(',', vt)})",
            string s => s,
            _ => JsonSerializer.Serialize(o)
        }));


        public void Print(PrintOption option, params object[] objects)
        {
            var str = Serialize(objects);
            switch (option)
            {
                case PrintOption.None:
                    OutputStreamWriter.Write(str);
                    break;
                case PrintOption.Line:
                    OutputStreamWriter.WriteLine(str);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(option), option, null);
            }
        }

        public int i(string s) => int.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture.NumberFormat, out var res) ? res : (int)d(s);

        public decimal d(string s) => decimal.Parse(s, NumberStyles.Float, CultureInfo.CurrentCulture.NumberFormat);

        public bool NextLine()
        {
            if (InputStreamReader.EndOfStream) return false;
            F0 = InputStreamReader.ReadLine();
            return true;
        }

        public Global(GlobalVariableOption options)
        {
            UseRegex = options.UseRegex;
            RemoveEmpty = options.RemoveEmpty;

            if (UseRegex) InputDelimiterRegex = new Regex(options.InputSeparator);
            else InputDelimiterString = options.InputSeparator;

            OutputStreamWriter = new StreamWriter(Console.OpenStandardOutput())
            {
                AutoFlush = false
            };
            OutputStreamWriter.AutoFlush = true;
            InputStreamReader = new StreamReader(options.InputStream);

            Ofs = options.OutputSeparator;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            OutputStreamWriter?.Dispose();
            InputStreamReader?.Dispose();
        }
    }

    public class GlobalVariableOption
    {
        public string InputSeparator { get; init; } = " ";
        public string OutputSeparator { get; init; } = " ";
        public bool UseRegex { get; init; } = false;
        public bool RemoveEmpty { get; init; } = false;
        [NotNull] public Stream InputStream { get; init; } = Console.OpenStandardInput();
    }
}
