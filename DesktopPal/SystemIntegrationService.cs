using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace DesktopPal
{
    /// <summary>
    /// Provides OS-level integration: desktop file watching ("letter" system)
    /// and screen capture for future vision features.
    /// </summary>
    public class SystemIntegrationService : IDisposable
    {
        // ── Fields ───────────────────────────────────────────────────────────────
        private FileSystemWatcher? _watcher;
        private bool _disposed;

        // ── Properties ───────────────────────────────────────────────────────────
        public static string DesktopPath =>
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        // ── Letter system ────────────────────────────────────────────────────────

        /// <summary>
        /// Starts watching the user's Desktop for new <c>.txt</c> files.
        /// When a file is created, reads its content and invokes
        /// <paramref name="onLetterReceived"/> with the file name and content.
        /// </summary>
        /// <param name="onLetterReceived">
        /// Callback of the form <c>(fileName, content)</c>; invoked on a thread-pool thread.
        /// </param>
        public void WatchDesktop(Action<string, string> onLetterReceived)
        {
            if (onLetterReceived is null) throw new ArgumentNullException(nameof(onLetterReceived));

            if (!Directory.Exists(DesktopPath))
            {
                DebugLogger.Warning($"Desktop path '{DesktopPath}' does not exist – letter watcher not started.");
                return;
            }

            _watcher = new FileSystemWatcher(DesktopPath, "*.txt")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                EnableRaisingEvents = false
            };

            _watcher.Created += (_, e) =>
            {
                DebugLogger.Info($"New desktop letter detected: {e.Name}");
                string? content = ReadFileSafe(e.FullPath);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    try
                    {
                        onLetterReceived(e.Name!, content);
                    }
                    catch (Exception ex)
                    {
                        DebugLogger.Error("Error in onLetterReceived callback.", ex);
                    }
                }
                else
                {
                    DebugLogger.Warning($"Letter '{e.Name}' was empty or could not be read – skipping.");
                }
            };

            _watcher.Error += (_, e) =>
            {
                DebugLogger.Error("FileSystemWatcher encountered an error.", e.GetException());
            };

            _watcher.EnableRaisingEvents = true;
            DebugLogger.Info($"Desktop letter watcher started on '{DesktopPath}'.");
        }

        // ── Screen capture ───────────────────────────────────────────────────────

        /// <summary>
        /// Captures a screenshot of the primary monitor and saves it to a temp file.
        /// </summary>
        /// <returns>
        /// The path to the saved PNG file, or <c>null</c> if the capture failed.
        /// </returns>
        public string? CaptureScreen()
        {
            try
            {
                var bounds = Screen.PrimaryScreen?.Bounds;
                if (bounds is null)
                {
                    DebugLogger.Warning("Primary screen bounds could not be determined.");
                    return null;
                }

                using var bmp = new Bitmap(bounds.Value.Width, bounds.Value.Height);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                }

                string tempPath = Path.Combine(Path.GetTempPath(), "desktop_pal_vision.png");
                bmp.Save(tempPath, ImageFormat.Png);
                DebugLogger.Debug($"Screen captured → {tempPath}");
                return tempPath;
            }
            catch (Exception ex)
            {
                DebugLogger.Error("Screen capture failed.", ex);
                return null;
            }
        }

        // ── Letter writing ───────────────────────────────────────────────────────

        /// <summary>
        /// Writes the AI reply as a <c>Reply_&lt;originalFileName&gt;</c> file on the Desktop.
        /// </summary>
        public void WriteLetterBack(string fileName, string content)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                DebugLogger.Warning("WriteLetterBack called with a null or empty file name.");
                return;
            }

            try
            {
                string path = Path.Combine(DesktopPath, "Reply_" + fileName);
                File.WriteAllText(path, content ?? string.Empty);
                DebugLogger.Info($"Reply written to '{path}'.");
            }
            catch (UnauthorizedAccessException ex)
            {
                DebugLogger.Error("Insufficient permissions to write reply to Desktop.", ex);
            }
            catch (IOException ex)
            {
                DebugLogger.Error("IO error while writing reply.", ex);
            }
            catch (Exception ex)
            {
                DebugLogger.Error("Unexpected error in WriteLetterBack.", ex);
            }
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        /// <summary>Reads a file with a shared read/write lock, retrying once on failure.</summary>
        internal string? ReadFileSafe(string path)
        {
            for (int attempt = 1; attempt <= 2; attempt++)
            {
                try
                {
                    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(stream);
                    return reader.ReadToEnd();
                }
                catch (IOException ex) when (attempt == 1)
                {
                    // File may still be locked by the creating process – wait briefly before retry.
                    DebugLogger.Warning($"Could not read '{path}' on attempt {attempt}, retrying. ({ex.Message})");
                    // Use a brief synchronous wait; ReadFileSafe is intentionally synchronous
                    // because it is invoked from a FileSystemWatcher callback (non-async context).
                    System.Threading.Thread.Sleep(200);
                }
                catch (Exception ex)
                {
                    DebugLogger.Error($"Failed to read file '{path}'.", ex);
                    return null;
                }
            }
            return null;
        }

        // ── IDisposable ──────────────────────────────────────────────────────────

        public void Dispose()
        {
            if (_disposed) return;
            _watcher?.Dispose();
            _disposed = true;
            DebugLogger.Debug("SystemIntegrationService disposed.");
        }
    }
}

