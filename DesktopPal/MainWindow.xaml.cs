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
        // ── Fields ───────────────────────────────────────────────────────────────
        public PetControl Pet { get; private set; }
        private System.Windows.Point _velocity = new System.Windows.Point(2, 0);
        private readonly double _gravity = 0.5;
        private bool _isDragging = false;
        private System.Windows.Point _dragOffset;
        private readonly Random _random = new Random();
        private DateTime _lastWanderChange = DateTime.Now;
        private readonly DispatcherTimer _saveTimer;
        private readonly SystemIntegrationService _sys;
        private readonly DispatcherTimer _visionTimer;
        private System.Windows.Forms.NotifyIcon? _notifyIcon;
        private readonly WorldWindow _world;

        // ── Win32 imports ─────────────────────────────────────────────────────────
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int Left, Top, Right, Bottom; }

        // ── Constructor ──────────────────────────────────────────────────────────
        public MainWindow()
        {
            InitializeComponent();
            DebugLogger.Info("MainWindow initialising.");

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

            _saveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            _saveTimer.Tick += (_, _) => Pet.State.Save();
            _saveTimer.Start();

            _sys = new SystemIntegrationService();
            _sys.WatchDesktop(HandleLetterReceived);

            _visionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(5)
            };
            _visionTimer.Tick += async (_, _) => await HandleVisionAsync();
            _visionTimer.Start();

            InitializeTray();
            DebugLogger.Info("MainWindow ready.");
        }

        // ── Tray icon ────────────────────────────────────────────────────────────
        private void InitializeTray()
        {
            try
            {
                _notifyIcon = new System.Windows.Forms.NotifyIcon
                {
                    Icon = System.Drawing.SystemIcons.Information,
                    Visible = true,
                    Text = "DesktopPal"
                };

                var contextMenu = new System.Windows.Forms.ContextMenuStrip();
                contextMenu.Items.Add("Settings", null, (_, _) => ShowSettings());
                contextMenu.Items.Add("Do Not Disturb", null, (_, _) => ToggleDND());
                contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
                contextMenu.Items.Add("Exit", null, (_, _) =>
                {
                    DebugLogger.Info("Exit requested from tray.");
                    _world.Close();
                    System.Windows.Application.Current.Shutdown();
                });

                _notifyIcon.ContextMenuStrip = contextMenu;
                _notifyIcon.DoubleClick += (_, _) =>
                {
                    this.Show();
                    this.WindowState = WindowState.Maximized;
                };
            }
            catch (Exception ex)
            {
                DebugLogger.Error("Failed to initialise system tray icon.", ex);
            }
        }

        private void ShowSettings()
        {
            try
            {
                var settings = new SettingsWindow { Owner = this };
                settings.ShowDialog();
            }
            catch (Exception ex)
            {
                DebugLogger.Error("Failed to open Settings window.", ex);
            }
        }

        private void ToggleDND()
        {
            Pet.Visibility = Pet.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
            DebugLogger.Info($"Do-Not-Disturb toggled. Pet visible: {Pet.Visibility == Visibility.Visible}");
        }

        // ── Letter system ────────────────────────────────────────────────────────
        private async void HandleLetterReceived(string fileName, string content)
        {
            try
            {
                await Dispatcher.InvokeAsync(async () =>
                {
                    DebugLogger.Info($"Processing letter: {fileName}");
                    Pet.ShowChat($"*reading {fileName}*");

                    string reply = await Pet.AIService.ChatAsync(
                        $"I received a letter called '{fileName}'. It says: {content}. Write a short, friendly reply.");

                    Pet.ShowChat(reply);
                    _sys.WriteLetterBack(fileName, reply);
                });
            }
            catch (Exception ex)
            {
                DebugLogger.Error("Error handling received letter.", ex);
            }
        }

        // ── Vision system ─────────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task HandleVisionAsync()
        {
            if (!Pet.State.IsHatched || !Pet.State.VisionEnabled)
            {
                DebugLogger.Debug("Vision skipped – pet not hatched or vision disabled.");
                return;
            }

            try
            {
                IntPtr hwnd = GetForegroundWindow();
                var sb = new System.Text.StringBuilder(256);
                GetWindowText(hwnd, sb, 256);
                string activeWindow = sb.ToString();

                DebugLogger.Debug($"Vision check – foreground window: '{activeWindow}'");

                string prompt = $"I am looking at your screen. You are currently using '{activeWindow}'. ";
                if (activeWindow.Contains("Visual Studio", StringComparison.OrdinalIgnoreCase) ||
                    activeWindow.Contains("Code", StringComparison.OrdinalIgnoreCase))
                    prompt += "It looks like you are coding! Say something encouraging.";
                else if (activeWindow.Contains("Chrome", StringComparison.OrdinalIgnoreCase) ||
                         activeWindow.Contains("Edge", StringComparison.OrdinalIgnoreCase) ||
                         activeWindow.Contains("Firefox", StringComparison.OrdinalIgnoreCase))
                    prompt += "You are browsing the web. I wonder what you are looking for?";
                else
                    prompt += "I wonder what you are up to?";

                string comment = await Pet.AIService.ChatAsync(prompt);
                Pet.ShowChat(comment);
            }
            catch (Exception ex)
            {
                DebugLogger.Error("Error in vision handler.", ex);
            }
        }

        // ── Game loop ─────────────────────────────────────────────────────────────
        private void GameLoop(object? sender, EventArgs e)
        {
            try
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

                // Smart layering: occasionally let pet "sneak" behind the foreground window.
                IntPtr foregroundHwnd = GetForegroundWindow();
                if (GetWindowRect(foregroundHwnd, out RECT rect))
                {
                    bool isUnderWindow = x > rect.Left && x < rect.Right &&
                                         y > rect.Top && y < rect.Bottom;
                    if (isUnderWindow && _random.Next(0, 100) < 5)
                        this.Topmost = false;
                    else if (!isUnderWindow)
                        this.Topmost = true;
                }

                // Chance to plant a decoration or poop.
                if (Pet.State.IsHatched)
                {
                    if (_random.Next(0, 5000) == 0)
                    {
                        string type = _random.Next(0, 2) == 0 ? "Tree" : "Flower";
                        _world.AddObject(new Decoration(type), x, y + 40);
                        DebugLogger.Debug($"Pet planted a {type}.");
                    }
                    else if (_random.Next(0, 8000) == 0)
                    {
                        _world.AddObject(new Decoration("Poop"), x, y + 60);
                        Pet.State.Hygiene = PetState.Clamp(Pet.State.Hygiene - 10);
                        DebugLogger.Debug("Pet pooped.");
                    }
                }

                double floor = SystemParameters.PrimaryScreenHeight - Pet.ActualHeight - 40;
                if (y > floor) { y = floor; _velocity.Y = 0; }
                if (x < 0) { x = 0; _velocity.X *= -1; }
                if (x > SystemParameters.PrimaryScreenWidth - Pet.ActualWidth)
                {
                    x = SystemParameters.PrimaryScreenWidth - Pet.ActualWidth;
                    _velocity.X *= -1;
                }

                Canvas.SetLeft(Pet, x);
                Canvas.SetTop(Pet, y);
            }
            catch (Exception ex)
            {
                DebugLogger.Error("Exception in game loop.", ex);
            }
        }

        // ── Mouse events ──────────────────────────────────────────────────────────
        private void Pet_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isDragging = true;
                _dragOffset = e.GetPosition(Pet);
                Pet.CaptureMouse();
                this.Topmost = true;
                DebugLogger.Debug("Pet drag started.");
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
                DebugLogger.Debug("Pet drag ended.");
            }
        }

        // ── Window closing ────────────────────────────────────────────────────────
        protected override void OnClosed(EventArgs e)
        {
            DebugLogger.Info("MainWindow closing – saving state and disposing resources.");
            Pet.State.Save();
            _saveTimer.Stop();
            _visionTimer.Stop();
            _sys.Dispose();
            _notifyIcon?.Dispose();
            CompositionTarget.Rendering -= GameLoop;
            base.OnClosed(e);
        }
    }
}

