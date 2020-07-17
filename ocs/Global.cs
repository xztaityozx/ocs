using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ocs {
    public class Global {
        public Dictionary<string, string> Env { get; }
        private List<string> f;
        private string f0;
        public Regex Separator;
        public StreamReader Reader { get; set; }

        public string F0 {
            get => f0;
            set {
                f0 = value;
                f = null;
                NR++;
            }
        }

        public List<string> F => f ??= new List<string>{F0}
            .Concat(
                Separator.Split(F0).Where(s => !string.IsNullOrEmpty(s))
            ).ToList();

        /// <summary>
        /// Number of fields
        /// </summary>
        public int NF => F.Count;

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

        public void print(object obj) => Console.WriteLine(obj);

        public int i(string s) => int.TryParse(s, out var res) ? res : (int) d(s);

        public decimal d(string s) => decimal.Parse(s, NumberStyles.Float);

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
