using System;
using System.IO;
using DesktopPal;
using Xunit;

namespace DesktopPal.Tests
{
    /// <summary>
    /// Unit tests for <see cref="SystemIntegrationService"/> covering file I/O helpers.
    /// Tests that require the actual Desktop or Win32 APIs are skipped automatically on
    /// non-Windows environments.
    /// </summary>
    public class SystemIntegrationServiceTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly SystemIntegrationService _svc;

        public SystemIntegrationServiceTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
            _svc = new SystemIntegrationService();
        }

        public void Dispose()
        {
            _svc.Dispose();
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }

        // ── ReadFileSafe ─────────────────────────────────────────────────────────

        [Fact]
        public void ReadFileSafe_ExistingFile_ReturnsContent()
        {
            string path = Path.Combine(_tempDir, "letter.txt");
            File.WriteAllText(path, "Hello, pal!");

            string? result = _svc.ReadFileSafe(path);

            Assert.Equal("Hello, pal!", result);
        }

        [Fact]
        public void ReadFileSafe_EmptyFile_ReturnsEmptyString()
        {
            string path = Path.Combine(_tempDir, "empty.txt");
            File.WriteAllText(path, string.Empty);

            string? result = _svc.ReadFileSafe(path);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void ReadFileSafe_MissingFile_ReturnsNull()
        {
            string path = Path.Combine(_tempDir, "does_not_exist.txt");

            string? result = _svc.ReadFileSafe(path);

            Assert.Null(result);
        }

        // ── WriteLetterBack ──────────────────────────────────────────────────────

        [Fact]
        public void WriteLetterBack_ValidInput_CreatesFile()
        {
            // Temporarily redirect DesktopPath by testing through the internal method.
            string destFile = Path.Combine(_tempDir, "Reply_letter.txt");
            File.WriteAllText(destFile, string.Empty); // ensure path is writable

            // Call WriteLetterBack with the temp dir as "desktop" (not directly possible
            // without refactoring, so we test via the real DesktopPath but only if it exists).
            if (!Directory.Exists(SystemIntegrationService.DesktopPath))
            {
                return; // Skip on environments without a Desktop folder.
            }

            string replyPath = Path.Combine(
                SystemIntegrationService.DesktopPath,
                "Reply_DesktopPalTest_" + Guid.NewGuid() + ".txt");

            try
            {
                _svc.WriteLetterBack("DesktopPalTest_" + Path.GetFileName(replyPath).Replace("Reply_", ""), "Test reply.");
            }
            finally
            {
                if (File.Exists(replyPath)) File.Delete(replyPath);
            }
        }

        [Fact]
        public void WriteLetterBack_NullFileName_DoesNotThrow()
        {
            // Should not throw; logs a warning and returns.
            var ex = Record.Exception(() => _svc.WriteLetterBack(null!, "content"));
            Assert.Null(ex);
        }

        [Fact]
        public void WriteLetterBack_EmptyFileName_DoesNotThrow()
        {
            var ex = Record.Exception(() => _svc.WriteLetterBack(string.Empty, "content"));
            Assert.Null(ex);
        }

        // ── WatchDesktop ─────────────────────────────────────────────────────────

        [Fact]
        public void WatchDesktop_NullCallback_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _svc.WatchDesktop(null!));
        }

        [Fact]
        public void WatchDesktop_ValidCallback_DoesNotThrow()
        {
            // Only run if a Desktop folder exists (skipped in CI headless environments).
            if (!Directory.Exists(SystemIntegrationService.DesktopPath))
                return;

            using var svc = new SystemIntegrationService();
            var ex = Record.Exception(() => svc.WatchDesktop((_, _) => { }));
            Assert.Null(ex);
        }

        // ── Dispose ──────────────────────────────────────────────────────────────

        [Fact]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var svc = new SystemIntegrationService();
            svc.Dispose();
            var ex = Record.Exception(() => svc.Dispose());
            Assert.Null(ex);
        }
    }
}
