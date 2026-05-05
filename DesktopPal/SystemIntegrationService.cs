using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace DesktopPal
{
    /// <summary>
    /// Watches the desktop for new letter files and provides screen-capture / reply-write helpers.
    /// Owns disposable resources (FileSystemWatcher) and must be disposed on shutdown.
    /// </summary>
    public class SystemIntegrationService : IDisposable
    {
        private const string LogSource = "SystemIntegration";

        private FileSystemWatcher? _desktopWatcher;
        private FileSystemEventHandler? _createdHandler;
        private ErrorEventHandler? _errorHandler;
        private bool _disposed;

        public static string DesktopPath => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public void WatchDesktop(Action<string, string> onLetterReceived)
        {
            if (onLetterReceived == null) throw new ArgumentNullException(nameof(onLetterReceived));
            ThrowIfDisposed();

            // Replace any prior watcher cleanly (idempotent re-arm).
            DisposeWatcher();

            try
            {
                var watcher = new FileSystemWatcher(DesktopPath, "*.txt")
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                    IncludeSubdirectories = false,
                };

                _createdHandler = (s, e) =>
                {
                    try
                    {
                        string? content = ReadFileSafe(e.FullPath);
                        if (!string.IsNullOrEmpty(content))
                        {
                            onLetterReceived(e.Name ?? Path.GetFileName(e.FullPath), content);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Error(LogSource, $"Letter handler failed for '{e.FullPath}'.", ex);
                    }
                };

                _errorHandler = (s, e) =>
                {
                    Logging.Error(LogSource, "Desktop watcher reported an internal error.", e.GetException());
                };

                watcher.Created += _createdHandler;
                watcher.Error += _errorHandler;
                watcher.EnableRaisingEvents = true;
                _desktopWatcher = watcher;

                Logging.Info(LogSource, $"Watching desktop path '{DesktopPath}' for *.txt letters.");
            }
            catch (Exception ex)
            {
                Logging.Error(LogSource, "Failed to start desktop watcher.", ex);
                DisposeWatcher();
                throw;
            }
        }

        private string? ReadFileSafe(string path)
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Logging.Warn(LogSource, $"Could not read file '{path}'.", ex);
                return null;
            }
        }

        /// <summary>
        /// Captures the primary screen to a temporary PNG. Returns null if capture is unavailable
        /// (e.g. headless / locked workstation).
        /// </summary>
        public string? CaptureScreen()
        {
            try
            {
                var primary = Screen.PrimaryScreen;
                if (primary == null)
                {
                    Logging.Warn(LogSource, "CaptureScreen skipped: no primary screen reported.");
                    return null;
                }

                using var bmp = new Bitmap(primary.Bounds.Width, primary.Bounds.Height);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                }

                string tempPath = Path.Combine(Path.GetTempPath(), "desktop_pal_vision.png");
                bmp.Save(tempPath, ImageFormat.Png);
                return tempPath;
            }
            catch (Exception ex)
            {
                Logging.Error(LogSource, "Screen capture failed.", ex);
                return null;
            }
        }

        public void WriteLetterBack(string fileName, string content)
        {
            try
            {
                string path = Path.Combine(DesktopPath, "Reply_" + fileName);
                File.WriteAllText(path, content);
                Logging.Info(LogSource, $"Wrote reply '{path}'.");
            }
            catch (Exception ex)
            {
                Logging.Error(LogSource, $"Failed to write reply for '{fileName}'.", ex);
            }
        }

        private void DisposeWatcher()
        {
            var watcher = _desktopWatcher;
            if (watcher == null) return;

            try
            {
                watcher.EnableRaisingEvents = false;
                if (_createdHandler != null) watcher.Created -= _createdHandler;
                if (_errorHandler != null) watcher.Error -= _errorHandler;
                watcher.Dispose();
            }
            catch (Exception ex)
            {
                Logging.Warn(LogSource, "Error while disposing desktop watcher.", ex);
            }
            finally
            {
                _desktopWatcher = null;
                _createdHandler = null;
                _errorHandler = null;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SystemIntegrationService));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            DisposeWatcher();
            Logging.Info(LogSource, "SystemIntegrationService disposed.");
            GC.SuppressFinalize(this);
        }
    }
}
