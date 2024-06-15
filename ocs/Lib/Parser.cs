using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ocs.Lib
{
    public class Parser
    {
        /// <summary>
        /// 文字列リテラルの終端がどこまでかを返す。文字列リテラルの始点は削ってある必要がある
        /// </summary>
        /// <param name="line"></param>
        /// <returns>終端のインデックス</returns>
        /// <exception cref="InvalidSyntaxException">文字列リテラルが閉じられなかったときに投げられる</exception>
        private static int ParseStringLiteral(string line)
        {
            var currentPosition = 0;
            var isBeforeEscape = false;
            while (true)
            {
                if (currentPosition >= line.Length) throw new InvalidSyntaxException("文字列リテラルが終了していません");
                if (line[currentPosition] != '"')
                {
                    isBeforeEscape = line[currentPosition++] == '\\';
                    continue;
                }

                if (!isBeforeEscape) return currentPosition + 1;
                currentPosition += 1;
                isBeforeEscape = false;
            }
        }

        /// <summary>
        /// 文字リテラルの終点がどこまでかを返す。文字リテラルの始点は削ってある必要がある
        /// </summary>
        /// <param name="line"></param>
        /// <returns>終端のインデックス</returns>
        /// <exception cref="InvalidSyntaxException">文字リテラルが閉じられなかったときに投げられる</exception>
        private static int ParseCharLiteral(string line) => line switch
        {
            var s when s[0] == '\\' && s[2] == '\'' => 2,
            var s when s[1] == '\'' => 1,
            _ => throw new InvalidSyntaxException("文字リテラルが終了していません")
        };

        public enum Bracket
        {
            A = '{', B = '(', C = '['
        }
        private static bool IsBracket(char c) => c is (char)Bracket.A or (char)Bracket.B or (char)Bracket.C;

        /// <summary>
        /// カッコの終端がどこまでかを返す。カッコの始点は削ってある必要がある
        /// </summary>
        /// <param name="line"></param>
        /// <param name="type"></param>
        /// <returns>終端のインデックス</returns>
        /// <exception cref="IndexOutOfRangeException">カッコの始点が予期せぬものなときに投げられる</exception>
        /// <exception cref="InvalidSyntaxException">カッコが閉じられなかったときに投げられる</exception>
        private static int ParseBracketsPair(string line, Bracket type)
        {
            var end = type switch
            {
                Bracket.A => '}',
                Bracket.B => ')',
                Bracket.C => ']',
                _ => throw new IndexOutOfRangeException($"{type}は対応していないカッコの始点です")
            };

            var currentPosition = 0;
            while (true)
            {
                if (currentPosition >= line.Length) throw new InvalidSyntaxException($"{type}が閉じていません");
                if (line[currentPosition] == end) return currentPosition;

                currentPosition += line[currentPosition] switch
                {
                    '"' => ParseStringLiteral(line[(currentPosition + 1)..]),
                    '\'' => ParseCharLiteral(line[(currentPosition + 1)..]),
                    var c when IsBracket(c) => ParseBracketsPair(line[(currentPosition + 1)..], (Bracket)c),
                    _ => 0
                } + 1;
            }
        }

        /// <summary>
        /// 文字列をocsのスクリプトとしてパースし、いくつかのOcsScriptとして返す
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static IEnumerable<OcsScript> Parse([NotNull] string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new InvalidSyntaxException("input script must not be null or empty");

            var pattern = new StringBuilder();
            var script = input;

            while (script.Length > 0)
            {
                var top = script[0];
                var remain = script[1..];
                switch (top)
                {
                    case ';':
                        yield return new OcsScript(pattern.ToString());
                        pattern = new StringBuilder();
                        script = remain;
                        continue;
                    case '{':
                        {
                            var blockEnd = ParseBracketsPair(remain, Bracket.A);
                            yield return new OcsScript(pattern.ToString(), remain[..blockEnd]);
                            script = remain[(blockEnd + 1)..];
                            pattern = new StringBuilder();
                            continue;
                        }
                }

                var next = top switch
                {
                    '"' => ParseStringLiteral(remain),
                    '\'' => ParseCharLiteral(remain),
                    (char)Bracket.B => ParseBracketsPair(remain, Bracket.B),
                    (char)Bracket.C => ParseBracketsPair(remain, Bracket.C),
                    _ => 0
                };

                pattern.Append(top + remain[..next]);
                script = remain[next..];
            }

            if (pattern.Length != 0)
            {
                yield return new OcsScript(pattern.ToString());
            }
        }
    }

    public record OcsScript([NotNull] string Pattern, [NotNull] string Action = "println(F0)")
    {
        public ScriptType Type => Pattern switch
        {
            "BEGIN" => ScriptType.Begin,
            "END" => ScriptType.End,
            _ => ScriptType.Main,
        };

        public enum ScriptType
        {
            Begin,
            End,
            Main
        }

        /// <summary>
        /// C#として理解可能なコードに組み立てて返す
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <exception cref="InvalidSyntaxException"></exception>
        public string Build()
        {
            if (string.IsNullOrEmpty(Action))
                throw new InvalidSyntaxException("Action must not be null");

            var rt = Type switch
            {
                ScriptType.Begin or ScriptType.End => $"{Action};",
                ScriptType.Main =>
                    string.IsNullOrEmpty(Pattern)
                        ? $"{Action};"
                        : $"if({Pattern}){{{Action};}}",
                _ => throw new IndexOutOfRangeException($"{Type} is unknown ScriptType")
            };

            return rt;
        }
    }

    public sealed class InvalidSyntaxException(string message) : Exception(message)
    {
    }
}
