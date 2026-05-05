using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace DesktopPal
{
    public class SystemIntegrationService
    {
        public static string DesktopPath => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public void WatchDesktop(Action<string, string> onLetterReceived)
        {
            FileSystemWatcher watcher = new FileSystemWatcher(DesktopPath, "*.txt");
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            
            watcher.Created += (s, e) => 
            {
                string content = ReadFileSafe(e.FullPath);
                if (!string.IsNullOrEmpty(content))
                    onLetterReceived(e.Name, content);
            };
            
            watcher.EnableRaisingEvents = true;
        }

        private string ReadFileSafe(string path)
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch { return null; }
        }

        public string CaptureScreen()
        {
            try
            {
                var bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                }
                
                string tempPath = Path.Combine(Path.GetTempPath(), "desktop_pal_vision.png");
                bmp.Save(tempPath, ImageFormat.Png);
                return tempPath;
            }
            catch { return null; }
        }

        public void WriteLetterBack(string fileName, string content)
        {
            try
            {
                string path = Path.Combine(DesktopPath, "Reply_" + fileName);
                File.WriteAllText(path, content);
            }
            catch { }
        }
    }
}
