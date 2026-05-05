using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace DesktopPal
{
    public partial class MainWindow : System.Windows.Window
    {
        public PetControl Pet { get; private set; }
        private System.Windows.Point _velocity = new System.Windows.Point(2, 0);
        private double _gravity = 0.5;
        private bool _isDragging = false;
        private System.Windows.Point _dragOffset;
        private Random _random = new Random();
        private DateTime _lastWanderChange = DateTime.Now;
        private DispatcherTimer _saveTimer;
        private SystemIntegrationService _sys;
        private DispatcherTimer _visionTimer;
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private WorldWindow _world;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int Left, Top, Right, Bottom; }

        public MainWindow()
        {
            InitializeComponent();
            
            _world = new WorldWindow();
            _world.Show();

            Pet = new PetControl();
            MainCanvas.Children.Add(Pet);
            Canvas.SetLeft(Pet, 100);
            Canvas.SetTop(Pet, 100);

            Pet.MouseDown += Pet_MouseDown;
            Pet.MouseMove += Pet_MouseMove;
            Pet.MouseUp += Pet_MouseUp;

            CompositionTarget.Rendering += GameLoop;

            _saveTimer = new DispatcherTimer();
            _saveTimer.Interval = TimeSpan.FromMinutes(1);
            _saveTimer.Tick += (s, e) => Pet.State.Save();
            _saveTimer.Start();

            _sys = new SystemIntegrationService();
            _sys.WatchDesktop(HandleLetterReceived);

            _visionTimer = new DispatcherTimer();
            _visionTimer.Interval = TimeSpan.FromMinutes(5);
            _visionTimer.Tick += (s, e) => HandleVision();
            _visionTimer.Start();

            InitializeTray();
        }

        private void InitializeTray()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.Icon = System.Drawing.SystemIcons.Information;
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "DesktopPal";

            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            contextMenu.Items.Add("Settings", null, (s, e) => ShowSettings());
            contextMenu.Items.Add("Do Not Disturb", null, (s, e) => ToggleDND());
            contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            contextMenu.Items.Add("Exit", null, (s, e) => { _world.Close(); System.Windows.Application.Current.Shutdown(); });

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += (s, e) => 
            {
                this.Show();
                this.WindowState = WindowState.Maximized;
            };
        }

        private void ShowSettings()
        {
            var settings = new SettingsWindow();
            settings.Owner = this;
            settings.ShowDialog();
        }

        private void ToggleDND()
        {
            Pet.Visibility = Pet.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void HandleLetterReceived(string fileName, string content)
        {
            Dispatcher.Invoke(async () => 
            {
                Pet.ShowChat($"*reading {fileName}*");
                
                string reply = await Pet.AIService.ChatAsync($"I received a letter called {fileName}. It says: {content}. I should write a reply.");
                Pet.ShowChat(reply);
                
                _sys.WriteLetterBack(fileName, reply);
            });
        }

        private async void HandleVision()
        {
            if (!Pet.State.IsHatched || !Pet.State.VisionEnabled) return;
            
            IntPtr hwnd = GetForegroundWindow();
            System.Text.StringBuilder sb = new System.Text.StringBuilder(256);
            GetWindowText(hwnd, sb, 256);
            string activeWindow = sb.ToString();

            string prompt = $"I am looking at your screen. You are currently using '{activeWindow}'. ";
            if (activeWindow.Contains("Visual Studio") || activeWindow.Contains("Code"))
                prompt += "It looks like you are coding! I should say something encouraging.";
            else if (activeWindow.Contains("Chrome") || activeWindow.Contains("Edge"))
                prompt += "You are browsing the web. I wonder what you are looking for?";
            else
                prompt += "I wonder what you are up to?";

            string comment = await Pet.AIService.ChatAsync(prompt);
            Pet.ShowChat(comment);
        }

        private void GameLoop(object sender, EventArgs e)
        {
            Pet.State.Tick();
            Pet.UpdateVisuals();

            if (_isDragging) return;

            double x = Canvas.GetLeft(Pet);
            double y = Canvas.GetTop(Pet);

            _velocity.Y += _gravity;
            
            if (DateTime.Now - _lastWanderChange > TimeSpan.FromSeconds(_random.Next(5, 15)))
            {
                _velocity.X = _random.NextDouble() * 2 - 1;
                _lastWanderChange = DateTime.Now;
            }

            x += _velocity.X;
            y += _velocity.Y;

            // Smart Layering: Check if pet is behind active window
            IntPtr foregroundHwnd = GetForegroundWindow();
            if (GetWindowRect(foregroundHwnd, out RECT rect))
            {
                bool isUnderWindow = x > rect.Left && x < rect.Right && y > rect.Top && y < rect.Bottom;
                if (isUnderWindow && _random.Next(0, 100) < 5) // 5% chance to "sneak" behind
                {
                    this.Topmost = false;
                }
                else if (!isUnderWindow)
                {
                    this.Topmost = true;
                }
            }

            // Chance to plant a decoration or poop
            if (Pet.State.IsHatched)
            {
                if (_random.Next(0, 5000) == 0) // Tree/Flower
                {
                    var type = _random.Next(0, 2) == 0 ? "Tree" : "Flower";
                    _world.AddObject(new Decoration(type), x, y + 40);
                }
                else if (_random.Next(0, 8000) == 0) // Poop
                {
                    _world.AddObject(new Decoration("Poop"), x, y + 60);
                    Pet.State.Hygiene = Math.Max(0, Pet.State.Hygiene - 10);
                }
            }

            double floor = SystemParameters.PrimaryScreenHeight - Pet.ActualHeight - 40;
            if (y > floor) { y = floor; _velocity.Y = 0; }
            if (x < 0) { x = 0; _velocity.X *= -1; }
            if (x > SystemParameters.PrimaryScreenWidth - Pet.ActualWidth) { x = SystemParameters.PrimaryScreenWidth - Pet.ActualWidth; _velocity.X *= -1; }

            Canvas.SetLeft(Pet, x);
            Canvas.SetTop(Pet, y);
        }

        private void Pet_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isDragging = true;
                _dragOffset = e.GetPosition(Pet);
                Pet.CaptureMouse();
                this.Topmost = true; // Always bring to front when touched
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                Pet.ToggleStatus();
            }
        }

        private void Pet_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isDragging)
            {
                System.Windows.Point mousePos = e.GetPosition(MainCanvas);
                Canvas.SetLeft(Pet, mousePos.X - _dragOffset.X);
                Canvas.SetTop(Pet, mousePos.Y - _dragOffset.Y);
                _velocity = new System.Windows.Point(0, 0);
            }
        }

        private void Pet_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                Pet.ReleaseMouseCapture();
            }
        }
    }
}
