using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace DesktopPal
{
    public enum PetEmote { Normal, Happy, Hungry, Sleeping, Excited, Love }

    public partial class PetControl : System.Windows.Controls.UserControl
    {
        public PetState  State      { get; private set; }
        public AIService AIService  { get; private set; }

        // Fires whenever the pet says something (or user sends a message via ProcessChat)
        public event Action<string, bool>? MessageAdded;

        private DispatcherTimer _chatTimer;
        private DispatcherTimer _emoteTimer;
        private PetEmote        _currentEmote = PetEmote.Sleeping;
        private readonly Random _random = new Random();

        // Body color pairs (top, bottom) per emote index
        private static readonly (System.Windows.Media.Color top, System.Windows.Media.Color bottom)[] _bodyColors =
        {
            (Color(122, 173, 234), Color( 78, 133, 200)), // Normal  — blue
            (Color(100, 220, 160), Color( 50, 180, 110)), // Happy   — green-teal
            (Color(255, 210, 100), Color(220, 160,  40)), // Hungry  — amber
            (Color(190, 175, 240), Color(145, 120, 210)), // Sleeping — lavender
            (Color( 90, 200, 240), Color( 40, 150, 210)), // Excited — cyan
            (Color(255, 180, 210), Color(220, 120, 170)), // Love    — pink
        };

        private static System.Windows.Media.Color Color(byte r, byte g, byte b)
            => System.Windows.Media.Color.FromRgb(r, g, b);

        public PetControl()
        {
            InitializeComponent();
            State    = PetState.Load();
            AIService = new AIService(State);

            _chatTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(9) };
            _chatTimer.Tick += (s, e) =>
            {
                ChatBubble.Visibility = Visibility.Collapsed;
                ChatTail.Visibility   = Visibility.Collapsed;
                _chatTimer.Stop();
            };

            _emoteTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _emoteTimer.Tick += (s, e) => { SetEmoteState(PetEmote.Normal); _emoteTimer.Stop(); };

            UpdateVisuals();
        }

        // ── Emote system ──────────────────────────────────────────────────────

        public void SetEmoteState(PetEmote emote, double durationSeconds = 0)
        {
            _currentEmote = emote;

            NormalFace.Visibility   = emote == PetEmote.Normal   ? Visibility.Visible : Visibility.Collapsed;
            HappyFace.Visibility    = emote == PetEmote.Happy    ? Visibility.Visible : Visibility.Collapsed;
            HungryFace.Visibility   = emote == PetEmote.Hungry   ? Visibility.Visible : Visibility.Collapsed;
            SleepingFace.Visibility = emote == PetEmote.Sleeping ? Visibility.Visible : Visibility.Collapsed;
            ExcitedFace.Visibility  = emote == PetEmote.Excited  ? Visibility.Visible : Visibility.Collapsed;
            LoveFace.Visibility     = emote == PetEmote.Love     ? Visibility.Visible : Visibility.Collapsed;

            int idx = (int)emote;
            var brush = new LinearGradientBrush(
                _bodyColors[idx].top, _bodyColors[idx].bottom,
                new System.Windows.Point(0.25, 0), new System.Windows.Point(0.75, 1));

            if (!State.IsHatched)
                brush = new LinearGradientBrush(
                    System.Windows.Media.Color.FromRgb(230, 230, 230),
                    System.Windows.Media.Color.FromRgb(200, 200, 200),
                    new System.Windows.Point(0.25, 0), new System.Windows.Point(0.75, 1));

            BodyPath.Fill = brush;

            if (durationSeconds > 0)
            {
                _emoteTimer.Stop();
                _emoteTimer.Interval = TimeSpan.FromSeconds(durationSeconds);
                _emoteTimer.Start();
            }
        }

        // ── Chat bubble ───────────────────────────────────────────────────────

        public void ShowChat(string text)
        {
            ChatText.Text             = text;
            ChatBubble.Visibility     = Visibility.Visible;
            ChatTail.Visibility       = Visibility.Visible;
            _chatTimer.Stop();
            _chatTimer.Start();
            MessageAdded?.Invoke(text, true);
        }

        // ── Visual update (called each game tick) ─────────────────────────────

        public void UpdateVisuals()
        {
            if (!State.IsHatched)
            {
                if (_currentEmote != PetEmote.Sleeping) SetEmoteState(PetEmote.Sleeping);
                return;
            }

            if (State.Energy < 15 && _currentEmote != PetEmote.Sleeping)
                SetEmoteState(PetEmote.Sleeping);
            else if (State.Hunger < 20 && _currentEmote == PetEmote.Normal)
                SetEmoteState(PetEmote.Hungry);
            else if (State.Hunger >= 20 && _currentEmote == PetEmote.Hungry)
                SetEmoteState(PetEmote.Normal);
        }

        // ── Direction / depth ─────────────────────────────────────────────────

        public void SetFacing(bool faceLeft) => FacingScale.ScaleX = faceLeft ? -1 : 1;

        public void SetDepthScale(double scale)
        {
            DepthScale.ScaleX = scale;
            DepthScale.ScaleY = scale;
        }

        // ── Interactions ──────────────────────────────────────────────────────

        public void Feed()
        {
            State.Hunger     = Math.Min(100, State.Hunger + 20);
            State.Experience += 5;
            SetEmoteState(PetEmote.Happy, 3);
            ShowChat("Yum! Thank you!");
        }

        public void PetMe()
        {
            State.Happiness  = Math.Min(100, State.Happiness + 10);
            State.Experience += 1;
            bool love = _random.NextDouble() < 0.25;
            SetEmoteState(love ? PetEmote.Love : PetEmote.Happy, 4);
            ShowChat(OfflineBrain.GetRandomPhrase("Petting"));
        }

        public async void ProcessChat(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;
            MessageAdded?.Invoke(input, false);
            SetEmoteState(PetEmote.Excited, 1);
            ShowChat("...");
            string reply = await AIService.ChatAsync(input);
            ShowChat(reply);
            State.Experience += 2;
            State.Happiness   = Math.Min(100, State.Happiness + 5);
            SetEmoteState(PetEmote.Happy, 3);
        }
    }
}
