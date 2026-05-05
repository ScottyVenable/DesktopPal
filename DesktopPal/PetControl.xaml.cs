using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace DesktopPal
{
    public partial class PetControl : System.Windows.Controls.UserControl
    {
        // ── Properties ───────────────────────────────────────────────────────────
        public PetState State { get; private set; }
        public AIService AIService { get; private set; }

        // ── Fields ───────────────────────────────────────────────────────────────
        private readonly DispatcherTimer _chatTimer;

        // ── Constructor ──────────────────────────────────────────────────────────
        public PetControl()
        {
            InitializeComponent();

            State = PetState.Load();
            AIService = new AIService(State);

            _chatTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(8)
            };
            _chatTimer.Tick += (_, _) =>
            {
                ChatBubble.Visibility = Visibility.Collapsed;
                _chatTimer.Stop();
            };

            UpdateVisuals();
            DebugLogger.Info($"PetControl initialised. Pet: '{State.Name}', Level {State.Level}.");
        }

        // ── Chat bubble ──────────────────────────────────────────────────────────
        public void ShowChat(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            ChatText.Text = text;
            ChatBubble.Visibility = Visibility.Visible;
            _chatTimer.Stop();
            _chatTimer.Start();
            DebugLogger.Debug($"Chat bubble shown: \"{text}\"");
        }

        // ── Emote / visuals ──────────────────────────────────────────────────────
        public void SetEmote(bool active)
        {
            if (active)
            {
                LeftEye.Visibility = Visibility.Collapsed;
                RightEye.Visibility = Visibility.Collapsed;
                LeftEmote.Visibility = Visibility.Visible;
                RightEmote.Visibility = Visibility.Visible;
            }
            else
            {
                LeftEye.Visibility = Visibility.Visible;
                RightEye.Visibility = Visibility.Visible;
                LeftEmote.Visibility = Visibility.Collapsed;
                RightEmote.Visibility = Visibility.Collapsed;
            }
        }

        public void UpdateVisuals()
        {
            HungerBar.Value = State.Hunger;

            if (State.Hunger < 30)
            {
                StatusBubble.Visibility = Visibility.Visible;
                StatusText.Text = "I'm hungry!";
                SetEmote(true);
            }
            else if (State.Happiness < 30)
            {
                StatusBubble.Visibility = Visibility.Visible;
                StatusText.Text = "I'm sad...";
                SetEmote(true);
            }
            else
            {
                SetEmote(false);
            }

            PetBody.Fill = State.IsHatched
                ? System.Windows.Media.Brushes.LightGreen
                : System.Windows.Media.Brushes.White;
        }

        public void ToggleStatus()
        {
            StatusBubble.Visibility = StatusBubble.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        // ── Chat input ───────────────────────────────────────────────────────────
        private async void ProcessChat()
        {
            string input = ChatInput.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(input)) return;

            ChatInput.Text = string.Empty;
            ShowChat("...");
            DebugLogger.Debug($"User sent chat: \"{input}\"");

            try
            {
                string reply = await AIService.ChatAsync(input);
                ShowChat(reply);

                State.AddExperience(2);
                State.Happiness = PetState.Clamp(State.Happiness + 5);
            }
            catch (Exception ex)
            {
                DebugLogger.Error("Error processing chat input.", ex);
                ShowChat("*confused* (Something went wrong!)");
            }
        }

        private void Say_Click(object sender, RoutedEventArgs e) => ProcessChat();

        private void ChatInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter) ProcessChat();
        }

        // ── Interaction buttons ──────────────────────────────────────────────────
        private void Feed_Click(object sender, RoutedEventArgs e)
        {
            State.Hunger = PetState.Clamp(State.Hunger + 20);
            State.AddExperience(5);
            UpdateVisuals();
            DebugLogger.Info($"Pet fed. Hunger now {State.Hunger:F1}%.");
        }

        private void Clean_Click(object sender, RoutedEventArgs e)
        {
            State.Hygiene = PetState.StatMax;
            State.AddExperience(2);
            UpdateVisuals();
            DebugLogger.Info("Pet cleaned. Hygiene restored to 100%.");
        }

        // ── Helpers ──────────────────────────────────────────────────────────────
        public void SetColor(System.Windows.Media.Brush color)
        {
            PetBody.Fill = color;
            DebugLogger.Debug("Pet body color updated.");
        }
    }
}

