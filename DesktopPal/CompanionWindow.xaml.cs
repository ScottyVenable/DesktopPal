using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace DesktopPal
{
    public partial class CompanionWindow : Window
    {
        private const string PendingReplyText = "...";

        private readonly MainWindow _mainWindow;
        private readonly PetControl _pet;
        private readonly DispatcherTimer _refreshTimer;
        private bool _allowClose;

        public CompanionWindow(MainWindow mainWindow, PetControl pet)
        {
            InitializeComponent();

            _mainWindow = mainWindow;
            _pet = pet;
            Owner = mainWindow;

            _pet.MessageAdded += HandleMessageAdded;
            _pet.AIService.StatusChanged += HandleAIStatusChanged;

            _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _refreshTimer.Tick += (s, e) => RefreshFromState();
            _refreshTimer.Start();

            AddSystemNote($"Press {FormatHotkey()} to open this panel any time.");
            UpdateAIStatusIndicator(_pet.AIService.Status, _pet.AIService.LastErrorMessage);
            RefreshFromState();
        }

        public void ShowPanel()
        {
            PositionWindow();

            if (!IsVisible)
            {
                Show();
            }

            RefreshFromState();
            Activate();
            ChatInput.Focus();
            ChatInput.CaretIndex = ChatInput.Text.Length;
        }

        public void HidePanel()
        {
            Hide();
        }

        public void PrepareForExit()
        {
            _allowClose = true;
        }

        public void RefreshFromState()
        {
            if (!IsInitialized)
            {
                return;
            }

            var state = _pet.State;
            PetNameLabel.Text = state.Name;
            PetLevelLabel.Text = $"Level {state.Level}";
            PetAgeLabel.Text = $"Age {FormatAge()} · Hotkey {FormatHotkey()}";

            HungerBar.Value = state.Hunger;
            HappyBar.Value = state.Happiness;
            HygieneBar.Value = state.Hygiene;
            EnergyBar.Value = state.Energy;

            HungerVal.Text = $"{state.Hunger:F0}%";
            HappyVal.Text = $"{state.Happiness:F0}%";
            HygieneVal.Text = $"{state.Hygiene:F0}%";
            EnergyVal.Text = $"{state.Energy:F0}%";
        }

        private void PositionWindow()
        {
            var workArea = SystemParameters.WorkArea;
            var width = Width > 0 ? Width : 320;
            var height = Height > 0 ? Height : 540;

            Left = workArea.Right - width - 24;
            Top = Math.Max(workArea.Top + 24, workArea.Bottom - height - 24);
        }

        private string FormatHotkey()
        {
            return $"Ctrl+Alt+{char.ToUpperInvariant((char)_pet.State.HotkeyCode)}";
        }

        private string FormatAge()
        {
            var age = DateTime.Now - _pet.State.BirthTime;

            if (age.TotalDays >= 1)
            {
                return $"{Math.Floor(age.TotalDays)}d";
            }

            if (age.TotalHours >= 1)
            {
                return $"{Math.Floor(age.TotalHours)}h";
            }

            return $"{Math.Max(1, Math.Floor(age.TotalMinutes))}m";
        }

        private void HandleMessageAdded(string message, bool fromPet)
        {
            Dispatcher.Invoke(() =>
            {
                if (fromPet && message == PendingReplyText)
                {
                    return;
                }

                AddMessageBubble(message, fromPet);
            });
        }

        private void AddSystemNote(string message)
        {
            var text = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 116, 139)),
                FontSize = 10.5,
                Margin = new Thickness(8, 4, 8, 10),
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI")
            };

            ChatStack.Children.Add(text);
        }

        private void AddMessageBubble(string message, bool fromPet)
        {
            var bubble = new Border
            {
                Background = new SolidColorBrush(fromPet
                    ? System.Windows.Media.Color.FromRgb(235, 244, 255)
                    : System.Windows.Media.Color.FromRgb(255, 255, 255)),
                BorderBrush = new SolidColorBrush(fromPet
                    ? System.Windows.Media.Color.FromRgb(190, 227, 248)
                    : System.Windows.Media.Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(10, 8, 10, 8),
                Margin = new Thickness(fromPet ? 0 : 28, 0, fromPet ? 28 : 0, 8),
                HorizontalAlignment = fromPet ? System.Windows.HorizontalAlignment.Left : System.Windows.HorizontalAlignment.Right,
                MaxWidth = 224
            };

            bubble.Child = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 41, 59)),
                FontSize = 11.5,
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI")
            };

            ChatStack.Children.Add(bubble);
            ChatScroll.ScrollToEnd();
        }

        private void SendCurrentMessage()
        {
            var text = ChatInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            ChatInput.Clear();
            _pet.ProcessChat(text);
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            HidePanel();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            SendCurrentMessage();
        }

        private void ChatInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            e.Handled = true;
            SendCurrentMessage();
        }

        private void Feed_Click(object sender, RoutedEventArgs e)
        {
            _pet.Feed();
            RefreshFromState();
        }

        private void Clean_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CleanAll();
            RefreshFromState();
        }

        private void Call_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.CallPetToMouse();
        }

        private void Plant_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.PlantSeed();
            RefreshFromState();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ShowSettings();
            RefreshFromState();
        }

        private void HandleAIStatusChanged(AIServiceStatus status)
        {
            string? detail = _pet.AIService.LastErrorMessage;
            Dispatcher.Invoke(() => UpdateAIStatusIndicator(status, detail));
        }

        private void UpdateAIStatusIndicator(AIServiceStatus status, string? detail)
        {
            if (!IsInitialized) return;

            System.Windows.Media.Color dot;
            string label;
            string tooltip;

            switch (status)
            {
                case AIServiceStatus.Available:
                    dot = System.Windows.Media.Color.FromRgb(72, 187, 120);   // green
                    label = "AI: online";
                    tooltip = "LM Studio reachable.";
                    break;
                case AIServiceStatus.Unavailable:
                    dot = System.Windows.Media.Color.FromRgb(237, 137, 54);   // amber
                    label = "AI: offline (degraded)";
                    tooltip = detail ?? "LM Studio unreachable. Using offline phrases.";
                    break;
                case AIServiceStatus.Error:
                    dot = System.Windows.Media.Color.FromRgb(229, 62, 62);    // red
                    label = "AI: error";
                    tooltip = detail ?? "LM Studio returned an error. Using offline phrases.";
                    break;
                default:
                    dot = System.Windows.Media.Color.FromRgb(160, 174, 192);  // grey
                    label = "AI: checking";
                    tooltip = "LM Studio status unknown.";
                    break;
            }

            AIStatusDot.Fill = new SolidColorBrush(dot);
            AIStatusDot.ToolTip = tooltip;
            AIStatusLabel.Text = label;
            AIStatusLabel.ToolTip = tooltip;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_allowClose)
            {
                e.Cancel = true;
                Hide();
                return;
            }

            _refreshTimer.Stop();
            _pet.MessageAdded -= HandleMessageAdded;
            _pet.AIService.StatusChanged -= HandleAIStatusChanged;
            base.OnClosing(e);
        }
    }
}
