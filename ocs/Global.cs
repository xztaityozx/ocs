using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ocs {
    /// <summary>
    /// スクリプト内で自由に使えるメソッド、変数を定義するクラス
    /// </summary>
    public class Global {
        /// <summary>
        /// Environments
        /// </summary>
        public Dictionary<string, string> Env { get; }
        private List<string> f;
        private string f0;
        /// <summary>
        /// Field separator
        /// </summary>
        public Regex Separator;

        /// <summary>
        /// Input stream
        /// </summary>
        public TextReader Reader { get; set; }

        /// <summary>
        /// Current line
        /// </summary>
        public string F0 {
            get => f0;
            set {
                f0 = value;
                f = null;
                NR++;
            }
        }

        /// <summary>
        /// Split fields
        /// </summary>
        public List<string> F => f ??= new List<string>{F0}
            .Concat(
                Separator.Split(F0).Where(s => !string.IsNullOrEmpty(s))
            ).ToList();

        /// <summary>
        /// Number of fields
        /// </summary>
        public int NF => Math.Max(0, F.Count - 1);

        /// <summary>
        /// Current line number
        /// </summary>
        public int NR;

        #region exports functions

        public void print<T>(IEnumerable<T> l) {
            foreach (var item in l) {
                Console.WriteLine(item);
            }
        }

        public void print(string str) => Console.Write(str);

        public void print(object obj) {
            switch (obj) {
                case ValueTuple t:
                    Console.Write(string.Join(Env.ContainsKey("OFS") ? Env["OFS"] : " ", t));
                    break;
                default:
                    Console.Write(obj);
                    break;
            }
        }

        public void println(object obj) {
            switch (obj) {
                case ValueTuple t:
                    Console.WriteLine(string.Join(Env.ContainsKey("OFS") ? Env["OFS"] : " ", t));
                    break;
                default:
                    Console.WriteLine(obj);
                    break;
            }
        }

        public int i(string s) => int.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture.NumberFormat, out var res) ? res : (int) d(s);

        public decimal d(string s) => decimal.Parse(s, NumberStyles.Float, CultureInfo.CurrentCulture.NumberFormat);

        #endregion

        /// <summary>
        /// Set Environments
        /// </summary>
        /// <param name="key">key of environment</param>
        /// <param name="value">value of environment</param>
        public void SetEnv(string key, string value) {
            if (Env.ContainsKey(key)) Env[key] = value;
            else Env.Add(key, value);
        }

        public Global(GlobalVariableOptions options = null) {
            Env = new Dictionary<string, string>();

            options ??= new GlobalVariableOptions();

            // set global environment
            if (!options.LoadGlobalEnvironments) return;

            foreach (DictionaryEntry de in Environment.GetEnvironmentVariables()) {
                SetEnv($"{de.Key}", $"{de.Value}");
            }
        }
    }

    public class GlobalVariableOptions {
        public bool LoadGlobalEnvironments { get; set; } = false;
    }
}
