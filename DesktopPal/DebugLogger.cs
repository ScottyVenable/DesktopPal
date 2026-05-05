using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace DesktopPal
{
    /// <summary>
    /// Lightweight, thread-safe debug logger that writes to both the Debug output
    /// window and a rolling log file located next to the application executable.
    /// </summary>
    public static class DebugLogger
    {
        // ── Configuration ────────────────────────────────────────────────────────
        public static bool IsEnabled { get; set; } = true;
        public static LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

        private static readonly string LogFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "desktoppal.log");

        private static readonly object _lock = new object();

        // ── Public API ───────────────────────────────────────────────────────────
        public static void Debug(
            string message,
            [CallerMemberName] string caller = "",
            [CallerFilePath] string filePath = "")
            => Write(LogLevel.Debug, message, caller, filePath);

        public static void Info(
            string message,
            [CallerMemberName] string caller = "",
            [CallerFilePath] string filePath = "")
            => Write(LogLevel.Info, message, caller, filePath);

        public static void Warning(
            string message,
            [CallerMemberName] string caller = "",
            [CallerFilePath] string filePath = "")
            => Write(LogLevel.Warning, message, caller, filePath);

        public static void Error(
            string message,
            Exception? exception = null,
            [CallerMemberName] string caller = "",
            [CallerFilePath] string filePath = "")
        {
            string full = exception is null
                ? message
                : $"{message} | Exception: {exception.GetType().Name}: {exception.Message}";

            if (exception?.InnerException is not null)
                full += $" | Inner: {exception.InnerException.Message}";

            Write(LogLevel.Error, full, caller, filePath);
        }

        // ── Implementation ───────────────────────────────────────────────────────
        private static void Write(LogLevel level, string message, string caller, string filePath)
        {
            if (!IsEnabled || level < MinimumLevel) return;

            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string line = $"[{timestamp}] [{level.ToString().ToUpper(),7}] [{fileName}.{caller}] {message}";

            lock (_lock)
            {
                System.Diagnostics.Debug.WriteLine(line);

                try
                {
                    // Keep the log file from growing unbounded (trim to last 5 000 lines).
                    AppendToLog(line);
                }
                catch
                {
                    // Logging must never crash the application.
                }
            }
        }

        private static void AppendToLog(string line)
        {
            File.AppendAllText(LogFilePath, line + Environment.NewLine);

            // Rolling trim: if the file exceeds ~500 KB, drop the oldest half.
            var info = new FileInfo(LogFilePath);
            if (info.Length > 512_000)
            {
                string[] lines = File.ReadAllLines(LogFilePath);
                int half = lines.Length / 2;
                File.WriteAllLines(LogFilePath, lines[half..]);
            }
        }
    }

    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }
}
