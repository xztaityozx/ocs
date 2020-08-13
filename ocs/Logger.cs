using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ocs {
    public class Logger {
        public enum LogLevel {
            Debug,
            Information,
            Warning,
            Critical
        }

        private static void Print(LogLevel logLevel ,ConsoleColor color, object obj) {
            Console.Write($"[ {DateTime.Now:T} | ");
            Console.ForegroundColor = color;
            Console.Write($"{logLevel}");
            Console.ResetColor();
            Console.Write(" ] ");
            Console.WriteLine(obj);
        }

        private static readonly Dictionary<LogLevel, Action<object>> DelegateTable = new Dictionary<LogLevel, Action<object>> {
            [LogLevel.Debug] = o => Print(LogLevel.Debug,ConsoleColor.Green, o),
            [LogLevel.Information] = o => Print(LogLevel.Information,ConsoleColor.Cyan, o),
            [LogLevel.Warning] = o => Print(LogLevel.Warning,ConsoleColor.Yellow, o),
            [LogLevel.Critical] = o => Print(LogLevel.Critical,ConsoleColor.Red, o)
        };

        public static void Log(LogLevel logLevel, object obj) {
            DelegateTable[logLevel](obj);
        }

        public static void LogDebug(object obj) => DelegateTable[LogLevel.Debug](obj);

        public static void LogInformation(object obj) => DelegateTable[LogLevel.Information](obj);

        public static void LogWarning(object obj) => DelegateTable[LogLevel.Warning](obj);

        public static void LogCritical(object obj) => DelegateTable[LogLevel.Critical](obj);
    }
}
