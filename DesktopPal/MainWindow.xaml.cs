using System;
using System.ComponentModel;
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
        private bool _isDragging = false;
        private System.Windows.Point _dragOffset;
        private readonly Random _random = new Random();
        private DateTime _lastWanderChange = DateTime.Now;
        private DispatcherTimer _saveTimer;
        private SystemIntegrationService _sys;
        private DispatcherTimer _visionTimer;
        private System.Windows.Forms.NotifyIcon? _notifyIcon;
        private readonly WorldWindow _world;
        private readonly CompanionWindow _companionWindow;
        private System.Windows.Point? _targetPosition = null;

        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int Left, Top, Right, Bottom; }

        public MainWindow()
        {
            InitializeComponent();

            _world = new WorldWindow();
            _world.Show();
            _world.MouseDown += World_MouseDown;

            Pet = new PetControl();
            MainCanvas.Children.Add(Pet);
            // Multi-monitor note: clamp to PRIMARY screen working area only.
            // SystemParameters.WorkArea excludes the taskbar so the pet
            // never spawns under it. Multi-monitor support is intentionally
            // deferred — see issue #12.
            var workArea = SystemParameters.WorkArea;
            Canvas.SetLeft(Pet, workArea.Left + workArea.Width  / 2 - 85);
            Canvas.SetTop (Pet, workArea.Top  + workArea.Height / 2 - 140);

            Pet.MouseDown += Pet_MouseDown;
            Pet.MouseMove += Pet_MouseMove;
            Pet.MouseUp   += Pet_MouseUp;

            CompositionTarget.Rendering += GameLoop;

            _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
            _saveTimer.Tick += (s, e) => Pet.State.Save();
            _saveTimer.Start();

            _sys = new SystemIntegrationService();
            try
            {
                _sys.WatchDesktop(HandleLetterReceived);
            }
            catch (Exception ex)
            {
                Logging.Error("MainWindow", "Desktop watcher failed to start; letter feature disabled.", ex);
            }

            _visionTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
            _visionTimer.Tick += (s, e) => HandleVision();
            _visionTimer.Start();

            _companionWindow = new CompanionWindow(this, Pet);
            InitializeTray();

            // First-run onboarding (issue #20). Show after the main window
            // has rendered so users see the pet appear, then receive the
            // welcome card. Gated by PetState.HasCompletedOnboarding.
            ContentRendered += MainWindow_ShowOnboardingIfNeeded;

            Logging.Info("MainWindow", "Startup complete.");
        }

        private void MainWindow_ShowOnboardingIfNeeded(object? sender, EventArgs e)
        {
            ContentRendered -= MainWindow_ShowOnboardingIfNeeded;
            if (Pet?.State == null || Pet.State.HasCompletedOnboarding) return;

            try
            {
                var onboarding = new OnboardingWindow(Pet.State) { Owner = this };
                onboarding.Show();
            }
            catch (Exception ex)
            {
                Logging.Error("MainWindow", "Failed to show onboarding window.", ex);
                // Fail-soft: mark complete so we don't loop on this on every launch.
                Pet.State.HasCompletedOnboarding = true;
                try { Pet.State.Save(); } catch { /* ignore */ }
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            RegisterGlobalHotkey();
            var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(HwndHook);
        }

        public void RegisterGlobalHotkey()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            UnregisterHotKey(hwnd, HOTKEY_ID);
            RegisterHotKey(hwnd, HOTKEY_ID, (uint)Pet.State.HotkeyModifier, (uint)Pet.State.HotkeyCode);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0312 && wParam.ToInt32() == HOTKEY_ID)
            {
                ToggleCompanionPanel();
                handled = true;
            }
            return IntPtr.Zero;
        }

        // ── Tray ─────────────────────────────────────────────────────────────

        private void InitializeTray()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon    = CreatePawIcon(),
                Visible = true,
                Text    = $"DesktopPal — {Pet.State.Name}"
            };

            var menu = new System.Windows.Forms.ContextMenuStrip();
            menu.Items.Add("Open Companion", null, (s, e) => Dispatcher.Invoke(OpenCompanionPanel));
            menu.Items.Add("Show or Hide Pet", null, (s, e) => Dispatcher.Invoke(TogglePetVisibility));
            menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            menu.Items.Add("Exit", null, (s, e) => Dispatcher.Invoke(() =>
            {
                System.Windows.Application.Current.Shutdown();
            }));

            _notifyIcon.ContextMenuStrip = menu;
            _notifyIcon.DoubleClick += (s, e) => Dispatcher.Invoke(OpenCompanionPanel);
        }

        private System.Drawing.Icon CreatePawIcon()
        {
            var bmp = new System.Drawing.Bitmap(32, 32);
            using (var g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(System.Drawing.Color.Transparent);
                var green = System.Drawing.Color.FromArgb(160, 235, 180);
                var outline = System.Drawing.Color.FromArgb(74, 124, 89);
                using var fill = new System.Drawing.SolidBrush(green);
                using var pen  = new System.Drawing.Pen(outline, 1f);
                // Toes
                g.FillEllipse(fill, 2,  3, 9, 9);
                g.FillEllipse(fill, 12, 1, 9, 9);
                g.FillEllipse(fill, 22, 3, 9, 9);
                // Palm
                g.FillEllipse(fill, 3, 12, 26, 20);
                g.DrawEllipse(pen,  3, 12, 25, 19);
            }
            return System.Drawing.Icon.FromHandle(bmp.GetHicon());
        }

        public void ShowSettings()
        {
            Window owner = _companionWindow.IsVisible ? _companionWindow : this;
            var w = new SettingsWindow { Owner = owner };
            w.ShowDialog();
            RefreshSurfaceState();
        }

        public void OpenCompanionPanel()
        {
            _companionWindow.ShowPanel();
            RefreshSurfaceState();
        }

        public void ToggleCompanionPanel()
        {
            if (_companionWindow.IsVisible)
            {
                _companionWindow.HidePanel();
                return;
            }

            OpenCompanionPanel();
        }

        private void TogglePetVisibility()
        {
            Pet.Visibility = Pet.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void RefreshSurfaceState()
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Text = $"DesktopPal — {Pet.State.Name}";
            }

            _companionWindow.RefreshFromState();
        }

        public void CleanAll()
        {
            _world.WorldCanvas.Children.Clear();
            Pet.State.Hygiene = 100;
            Pet.ShowChat("All clean! Thank you!");
            Pet.SetEmoteState(PetEmote.Happy, 3);
        }

        public void CallPetToMouse()
        {
            // System.Windows.Forms gives physical screen pixels; convert to WPF DIPs via PointFromScreen
            var screenPx = System.Windows.Forms.Control.MousePosition;
            var wpfPt    = this.PointFromScreen(new System.Windows.Point(screenPx.X, screenPx.Y));

            // Center the pet's sprite (120 px wide, sprite sits at y+160 within the 285-tall control)
            _targetPosition = new System.Windows.Point(
                wpfPt.X - Pet.ActualWidth  / 2,
                wpfPt.Y - Pet.ActualHeight * 0.75);

            // Zero velocity so wander doesn't fight the target
            _velocity = new System.Windows.Point(0, 0);
            _lastWanderChange = DateTime.Now;

            Pet.SetEmoteState(PetEmote.Excited, 1.5);
            Pet.ShowChat(OfflineBrain.GetRandomPhrase("Calling"));
        }

        // ── Letter / Vision ───────────────────────────────────────────────────

        private async void HandleLetterReceived(string fileName, string content)
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                Pet.SetEmoteState(PetEmote.Excited, 2);
                Pet.ShowChat($"*reading {fileName}*");
                string reply = await Pet.AIService.ChatAsync(
                    $"I received a letter called {fileName}. It says: {content}. Write a short, cute reply.");
                Pet.ShowChat(reply);
                _sys.WriteLetterBack(fileName, reply);
            });
        }

        private async void HandleVision()
        {
            if (!Pet.State.IsHatched || !Pet.State.VisionEnabled) return;

            var sb = new System.Text.StringBuilder(256);
            GetWindowText(GetForegroundWindow(), sb, 256);
            string active = sb.ToString();

            string prompt = $"You are looking at the user's screen. Active window: '{active}'. ";
            if (active.Contains("Visual Studio") || active.Contains("Code"))
                prompt += "They are coding! Say something encouraging in one sentence.";
            else if (active.Contains("Chrome") || active.Contains("Edge") || active.Contains("Firefox"))
                prompt += "They are browsing the web. Make a curious one-sentence comment.";
            else
                prompt += "Make a short, cute observation in one sentence.";

            string comment = await Pet.AIService.ChatAsync(prompt);
            Dispatcher.Invoke(() => Pet.ShowChat(comment));
        }

        // ── Game Loop ─────────────────────────────────────────────────────────

        private void GameLoop(object? sender, EventArgs e)
        {
            Pet.State.Tick();
            Pet.UpdateVisuals();

            if (_isDragging) return;

            double x = Canvas.GetLeft(Pet);
            double y = Canvas.GetTop(Pet);

            double baseScale = Pet.State.IsHatched ? 1.0 : 0.5;
            Pet.SetDepthScale(baseScale);

            if (_targetPosition.HasValue)
            {
                double dx   = _targetPosition.Value.X - x;
                double dy   = _targetPosition.Value.Y - y;
                double dist = Math.Sqrt(dx * dx + dy * dy);

                if (dist < 6)
                {
                    _targetPosition   = null;
                    _velocity         = new System.Windows.Point(0, 0);
                    _lastWanderChange = DateTime.Now; // pause wander after arriving
                }
                else
                {
                    double speed = Math.Min(7, dist * 0.12); // eases in near target
                    _velocity.X = (dx / dist) * speed;
                    _velocity.Y = (dy / dist) * speed;
                }
            }
            else if (DateTime.Now - _lastWanderChange > TimeSpan.FromSeconds(_random.Next(3, 9)))
            {
                _velocity.X       = _random.NextDouble() * 4 - 2;
                _velocity.Y       = _random.NextDouble() * 3 - 1.5;
                _lastWanderChange = DateTime.Now;
            }

            x += _velocity.X;
            y += _velocity.Y;

            if (_velocity.X != 0) Pet.SetFacing(_velocity.X < 0);

            // Smart layering — duck under foreground windows occasionally
            if (GetWindowRect(GetForegroundWindow(), out RECT rect))
            {
                bool under = x > rect.Left && x < rect.Right && y > rect.Top && y < rect.Bottom;
                if (under && _random.Next(0, 100) < 5) this.Topmost = false;
                else if (!under)                        this.Topmost = true;
            }

            // Poop — spawn just below the pet sprite, but never under the taskbar.
            // The pet visual sits roughly at y + Pet.ActualHeight * 0.95 (sprite bottom).
            // Decoration height is 40 px (see Decoration.cs).
            if (Pet.State.IsHatched && _random.Next(0, 8000) == 0)
            {
                const double DecorationHeight = 40.0;
                double poopY = y + Pet.ActualHeight * 0.95;
                double maxPoopY = SystemParameters.WorkArea.Bottom - DecorationHeight;
                if (poopY > maxPoopY) poopY = maxPoopY;
                _world.AddObject(new Decoration("Poop"), x + 30, poopY);
                Pet.State.Hygiene = Math.Max(0, Pet.State.Hygiene - 10);
            }

            // Boundary — clamp to the working area on the PRIMARY screen so the
            // pet stays out from under the taskbar (top, bottom, or side-docked).
            // Multi-monitor: deferred. We intentionally do not span monitors yet;
            // the pet stays inside the primary display's WorkArea. Tracked in #12.
            var area = SystemParameters.WorkArea;
            double minX = area.Left;
            double minY = area.Top;
            double maxX = area.Right  - Pet.ActualWidth;
            // Keep the full sprite (Viewbox bottom ≈ y + ActualHeight * 0.95)
            // above the taskbar.
            double maxY = area.Bottom - Pet.ActualHeight * 0.95;
            if (y < minY) { y = minY; _velocity.Y *= -1; }
            if (y > maxY) { y = maxY; _velocity.Y *= -1; }
            if (x < minX) { x = minX; _velocity.X *= -1; }
            if (x > maxX) { x = maxX; _velocity.X *= -1; }

            Canvas.SetLeft(Pet, x);
            Canvas.SetTop(Pet,  y);
        }

        // ── Mouse handlers ────────────────────────────────────────────────────

        private void World_MouseDown(object sender, MouseButtonEventArgs e) { }

        private void Pet_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isDragging = true;
                _dragOffset = e.GetPosition(Pet);
                Pet.CaptureMouse();
                Pet.PauseIdleBob();
                this.Topmost = true;
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                ToggleCompanionPanel();
            }
        }

        private void Pet_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_isDragging) return;
            var pos = e.GetPosition(MainCanvas);
            Canvas.SetLeft(Pet, pos.X - _dragOffset.X);
            Canvas.SetTop(Pet,  pos.Y - _dragOffset.Y);
            _velocity = new System.Windows.Point(0, 0);
        }

        private void Pet_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                Pet.ReleaseMouseCapture();
                Pet.ResumeIdleBob();
                Pet.PetMe();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                if (hwnd != IntPtr.Zero) UnregisterHotKey(hwnd, HOTKEY_ID);
            }
            catch (Exception ex)
            {
                Logging.Warn("MainWindow", "Failed to unregister global hotkey.", ex);
            }

            CompositionTarget.Rendering -= GameLoop;

            try { _saveTimer?.Stop(); } catch { /* shutdown */ }
            try { _visionTimer?.Stop(); } catch { /* shutdown */ }

            try { Pet.State.Save(); }
            catch (Exception ex) { Logging.Error("MainWindow", "Final state save failed.", ex); }

            if (_notifyIcon != null)
            {
                try
                {
                    _notifyIcon.Visible = false;
                    _notifyIcon.Dispose();
                }
                catch (Exception ex) { Logging.Warn("MainWindow", "Tray icon dispose failed.", ex); }
                _notifyIcon = null;
            }

            try { _sys?.Dispose(); }
            catch (Exception ex) { Logging.Warn("MainWindow", "SystemIntegrationService dispose failed.", ex); }

            try
            {
                _companionWindow.PrepareForExit();
                _companionWindow.Close();
            }
            catch (Exception ex) { Logging.Warn("MainWindow", "Companion window close failed.", ex); }

            try { _world.Close(); }
            catch (Exception ex) { Logging.Warn("MainWindow", "World window close failed.", ex); }

            Logging.Info("MainWindow", "Shutdown complete.");
            base.OnClosing(e);
        }
    }
}
