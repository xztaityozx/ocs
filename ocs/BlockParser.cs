using System;
using System.Collections.Generic;
using System.Linq;

namespace ocs {
    public class BlockParser {

        public IReadOnlyList<string> MainBlock { get; private set; }
        public IReadOnlyList<string> BeginBlock { get; private set; }
        public IReadOnlyList<string> EndBlock { get; private set; }

        public void Parse(string line) {
            var depth = new Dictionary<char, int> {
                ['"'] = 0,
                ['\''] = 0,
                ['('] = 0,
                ['['] = 0
            };

            var main = new List<string>();
            var end = new List<string>();
            var begin = new List<string>();

            // CONDITION { ACTION } をパースしたい
            // CONDITIONは空になりうる
            // CONDITION には普通のC#の式が入るので単純に { でスプリットするとえらい目にあう
            var action = "";
            var condition = "";
            var isCondition = true;
            var beforeIsEscape = false;

            foreach (var c in line) {
                if (isCondition) {
                    // ブロックの開始記号だけど、条件式の一部かもしれない
                    if (c == '{') {
                        // 全部の深さが0ならACTIONの開始記号
                        // ここまでがCONDITIONの部分になる
                        isCondition = depth.Any(kv => kv.Value != 0);
                    }
                    // 文字列/文字リテラルの開始/終端記号
                    // エスケープされてることがあるので、直前がエスケープ文字かどうかを見ないといけない
                    // これが閉じられるまでに登場する { はブロックの開始記号ではない
                    // ネストしない
                    else {
                        if ((c == '"' || c == '\'') && !beforeIsEscape) {
                            depth[c] = depth[c] == 0 ? 1 : 0;
                        }
                        // メソッドとかの開始記号
                        // ネストしうる
                        else
                            switch (c) {
                                // メソッドとかの終端記号
                                // ( の個数と一致するはずなので引き算をする感じで
                                case '(' when (depth['"'] == 0 || depth['\''] == 0):
                                    // 文字/文字列リテラル中の ( は無視する
                                    depth['(']++;
                                    break;
                                case ')' when (depth['"'] == 0 || depth['\''] == 0):
                                    // 文字/文字列リテラル中の ) は無視する
                                    depth['(']--;
                                    break;
                                case '[' when depth['"'] == 0 || depth['\''] == 0:
                                    depth['[']++;
                                    break;
                                case ']' when depth['"'] == 0 || depth['\''] == 0:
                                    depth['[']--;
                                    break;
                            }

                        beforeIsEscape = c == '\\';
                    }

                    if (isCondition) condition += c;
                }
                else {
                    // アクションブロック
                    // 深さが変わるルールはCONDITIONの時と同じ
                    if (c == '}') {
                        if (depth.Any(kv => kv.Value != 0)) continue;

                        // ここまできたら ACTION ブロックも終了、次は終端か次のCONDITION
                        isCondition = true;
                        // もしactionの最後にセミコロンがなければ、付け足すおせっかい
                        action = action.TrimStart(' ').TrimEnd(' ');
                        condition = condition.TrimStart(' ').TrimEnd(' ');
                        if (action.Last() != ';') action += ";";

                        // BEGINブロック
                        if (condition == "BEGIN") begin.Add(action);
                        // ENDブロックだった
                        else if (condition == "END") end.Add(action);
                        // conditionがBEGINやEND以外のとき
                        else main.Add(string.IsNullOrEmpty(condition) ? action : $"if({condition}){{{action}}}");

                        condition = "";
                        action = "";
                    }
                    else {
                        if ((c == '"' || c == '\'') && !beforeIsEscape) {
                            depth[c] = depth[c] == 0 ? 1 : 0;
                        }
                        // メソッドとかの開始記号
                        // ネストしうる
                        else
                            switch (c) {
                                // メソッドとかの終端記号
                                // ( の個数と一致するはずなので引き算をする感じで
                                case '(' when (depth['"'] == 0 || depth['\''] == 0):
                                    // 文字/文字列リテラル中の ( は無視する
                                    depth['(']++;
                                    break;
                                case ')' when (depth['"'] == 0 || depth['\''] == 0):
                                    // 文字/文字列リテラル中の ) は無視する
                                    depth['(']--;
                                    break;
                                case '[' when depth['"'] == 0 || depth['\''] == 0:
                                    depth['[']++;
                                    break;
                                case ']' when depth['"'] == 0 || depth['\''] == 0:
                                    depth['[']--;
                                    break;
                            }

                        action += c;
                        beforeIsEscape = c == '\\';
                    }
                }
            }

            // ここにきて condition や action, depthが残っていたらParse失敗なので例外を投げる
            foreach (var statement in new[]{condition, action}) {
                if (!string.IsNullOrEmpty(statement))
                    throw new FormatException(
                        $"ocs failed to parse script.\n{statement}\n{string.Join("", Enumerable.Repeat(" ", statement.Length))}\x2191");

            }

            if (depth['"'] != 0) throw new FormatException("ocs detect unclosed string literal: \"");
            if (depth['\''] != 0) throw new FormatException("ocs detect unclosed character literal: '");
            if (depth['('] != 0) throw new FormatException("ocs detect unclosed brackets: ()");
            if (depth['('] != 0) throw new FormatException("ocs detect unclosed brackets: []");
            
            (MainBlock, BeginBlock, EndBlock) = (main, begin, end);
        }
    }
}