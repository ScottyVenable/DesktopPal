using System;
using System.IO;
using System.Text;

namespace DesktopPal
{
    /// <summary>
    /// Lightweight file logger that writes to %LOCALAPPDATA%\DesktopPal\logs\.
    /// Failures inside the logger never throw — logging must never crash the app.
    /// </summary>
    public static class Logging
    {
        private const long MaxLogBytes = 1_000_000; // 1 MB rotation threshold
        private static readonly object _gate = new object();
        private static readonly string _logDirectory;
        private static readonly string _logFile;
        private static bool _initialized;

        static Logging()
        {
            try
            {
                string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                _logDirectory = Path.Combine(baseDir, "DesktopPal", "logs");
                _logFile = Path.Combine(_logDirectory, "desktoppal.log");
            }
            catch
            {
                _logDirectory = string.Empty;
                _logFile = string.Empty;
            }
        }

        public static string LogFilePath => _logFile;

        public static void Info(string source, string message) => Write("INFO ", source, message, null);

        public static void Warn(string source, string message, Exception? ex = null) => Write("WARN ", source, message, ex);

        public static void Error(string source, string message, Exception? ex = null) => Write("ERROR", source, message, ex);

        private static void Write(string level, string source, string message, Exception? ex)
        {
            try
            {
                EnsureReady();
                if (string.IsNullOrEmpty(_logFile)) return;

                var sb = new StringBuilder(256);
                sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                sb.Append(' ').Append(level);
                sb.Append(" [").Append(source).Append("] ");
                sb.Append(message);
                if (ex != null)
                {
                    sb.Append(" :: ").Append(ex.GetType().Name).Append(": ").Append(ex.Message);
                }
                sb.AppendLine();

                lock (_gate)
                {
                    RotateIfNeeded();
                    File.AppendAllText(_logFile, sb.ToString(), Encoding.UTF8);
                }
            }
            catch
            {
                // Logger must never throw.
            }
        }

        private static void EnsureReady()
        {
            if (_initialized) return;
            if (string.IsNullOrEmpty(_logDirectory)) return;
            try
            {
                Directory.CreateDirectory(_logDirectory);
                _initialized = true;
            }
            catch
            {
                // Leave _initialized false; subsequent writes will retry.
            }
        }

        private static void RotateIfNeeded()
        {
            try
            {
                var info = new FileInfo(_logFile);
                if (!info.Exists || info.Length < MaxLogBytes) return;

                string archive = _logFile + ".1";
                if (File.Exists(archive)) File.Delete(archive);
                File.Move(_logFile, archive);
            }
            catch
            {
                // Rotation is best-effort.
            }
        }
    }
}
