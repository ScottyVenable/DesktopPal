using System;
using System.IO;
using DesktopPal;
using Xunit;

namespace DesktopPal.Tests
{
    /// <summary>
    /// Unit tests for <see cref="DebugLogger"/>.
    /// </summary>
    public class DebugLoggerTests : IDisposable
    {
        private readonly string _tempDir;

        public DebugLoggerTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);

            // Redirect log output to the temp directory via reflection.
            SetLogPath(Path.Combine(_tempDir, "test.log"));
        }

        public void Dispose()
        {
            // Restore default path (best effort).
            SetLogPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "desktoppal.log"));
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }

        private static void SetLogPath(string path)
        {
            var field = typeof(DebugLogger).GetField(
                "LogFilePath",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            field?.SetValue(null, path);
        }

        private string ReadLog()
        {
            string logPath = Path.Combine(_tempDir, "test.log");
            return File.Exists(logPath) ? File.ReadAllText(logPath) : string.Empty;
        }

        // ── Tests ────────────────────────────────────────────────────────────────

        [Fact]
        public void Info_WritesToLog()
        {
            DebugLogger.Info("info-message-xyz");
            Assert.Contains("info-message-xyz", ReadLog());
        }

        [Fact]
        public void Warning_WritesToLog()
        {
            DebugLogger.Warning("warn-message-abc");
            Assert.Contains("warn-message-abc", ReadLog());
        }

        [Fact]
        public void Error_WithException_IncludesExceptionType()
        {
            DebugLogger.Error("error-occurred", new InvalidOperationException("boom"));
            string log = ReadLog();
            Assert.Contains("error-occurred", log);
            Assert.Contains("InvalidOperationException", log);
        }

        [Fact]
        public void Debug_BelowMinimumLevel_NotWritten()
        {
            DebugLogger.MinimumLevel = LogLevel.Warning;
            DebugLogger.Debug("should-not-appear");
            DebugLogger.MinimumLevel = LogLevel.Debug; // restore
            Assert.DoesNotContain("should-not-appear", ReadLog());
        }

        [Fact]
        public void Disabled_DoesNotWrite()
        {
            DebugLogger.IsEnabled = false;
            DebugLogger.Info("disabled-message");
            DebugLogger.IsEnabled = true; // restore
            Assert.DoesNotContain("disabled-message", ReadLog());
        }

        [Fact]
        public void Error_NoException_DoesNotThrow()
        {
            var ex = Record.Exception(() => DebugLogger.Error("no-exception-test"));
            Assert.Null(ex);
        }
    }
}
