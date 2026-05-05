using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace DesktopPal
{
    public enum PetEmote { Normal, Happy, Hungry, Sleeping, Excited, Love }

    // Animation note (issue #2 — groundwork only):
    // The idle bob and blink below are stepping stones, not the final
    // animation system. The intended direction is a small state-machine
    // (Idle / Wander / Sleep / React) that drives either:
    //   (a) procedural WPF storyboards composed per state, or
    //   (b) a sprite atlas / Lottie pipeline once art is authored.
    // Treat these storyboards as scaffolding to be replaced when the
    // sprite/anim format is decided. Do not extend this file into the
    // full animation system; that work belongs in a dedicated
    // AnimationController class.
    public partial class PetControl : System.Windows.Controls.UserControl
    {
        public PetState  State      { get; private set; }
        public AIService AIService  { get; private set; }

        // Fires whenever the pet says something (or user sends a message via ProcessChat)
        public event Action<string, bool>? MessageAdded;

        private DispatcherTimer _chatTimer;
        private DispatcherTimer _emoteTimer;
        private DispatcherTimer _blinkTimer;
        private DispatcherTimer _blinkRestoreTimer;
        private Storyboard?     _idleBobStoryboard;
        private bool            _idleBobPaused;
        private Canvas?         _blinkHiddenFace;
        private PetEmote        _currentEmote = PetEmote.Sleeping;
        private readonly Random _random = new Random();

        // Idle-bob tuning constants (named per house rules).
        private const double IdleBobAmplitudePx        = 3.0;
        private const double IdleBobPeriodSeconds      = 1.6;
        private const double BlinkDurationMs           = 120.0;
        private const double BlinkIntervalMinSeconds   = 3.0;
        private const double BlinkIntervalMaxSeconds   = 6.0;

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

            _blinkTimer = new DispatcherTimer();
            _blinkTimer.Tick += BlinkTimer_Tick;

            _blinkRestoreTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(BlinkDurationMs)
            };
            _blinkRestoreTimer.Tick += BlinkRestoreTimer_Tick;

            Loaded += (_, __) =>
            {
                StartIdleBob();
                ScheduleNextBlink();
            };
            Unloaded += (_, __) =>
            {
                StopIdleBob();
                _blinkTimer.Stop();
                _blinkRestoreTimer.Stop();
            };

            UpdateVisuals();
        }

        // ── Idle bob (issue #2 groundwork) ────────────────────────────────────

        private void StartIdleBob()
        {
            if (_idleBobStoryboard != null) return;

            var anim = new DoubleAnimation
            {
                From           = -IdleBobAmplitudePx,
                To             =  IdleBobAmplitudePx,
                Duration       = new Duration(TimeSpan.FromSeconds(IdleBobPeriodSeconds / 2.0)),
                AutoReverse    = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(anim, IdleBob);
            Storyboard.SetTargetProperty(anim, new PropertyPath(TranslateTransform.YProperty));

            _idleBobStoryboard = new Storyboard();
            _idleBobStoryboard.Children.Add(anim);
            _idleBobStoryboard.Begin(this, true);
        }

        private void StopIdleBob()
        {
            if (_idleBobStoryboard == null) return;
            try { _idleBobStoryboard.Stop(this); } catch { /* ignore */ }
            _idleBobStoryboard = null;
            IdleBob.Y = 0;
        }

        /// <summary>Pause the idle-bob loop (e.g. while user is dragging the pet).</summary>
        public void PauseIdleBob()
        {
            if (_idleBobStoryboard == null || _idleBobPaused) return;
            try { _idleBobStoryboard.Pause(this); } catch { /* ignore */ }
            _idleBobPaused = true;
        }

        /// <summary>Resume the idle-bob loop after a drag completes.</summary>
        public void ResumeIdleBob()
        {
            if (_idleBobStoryboard == null || !_idleBobPaused) return;
            try { _idleBobStoryboard.Resume(this); } catch { /* ignore */ }
            _idleBobPaused = false;
        }

        // ── Blink (issue #2 groundwork) ───────────────────────────────────────

        private void ScheduleNextBlink()
        {
            double seconds = BlinkIntervalMinSeconds
                + _random.NextDouble() * (BlinkIntervalMaxSeconds - BlinkIntervalMinSeconds);
            _blinkTimer.Interval = TimeSpan.FromSeconds(seconds);
            _blinkTimer.Start();
        }

        private void BlinkTimer_Tick(object? sender, EventArgs e)
        {
            _blinkTimer.Stop();

            // Don't blink mid-sleep — closed eyes already.
            if (_currentEmote == PetEmote.Sleeping)
            {
                ScheduleNextBlink();
                return;
            }

            Canvas? face = GetActiveFaceCanvas();
            if (face != null && face.Visibility == Visibility.Visible)
            {
                _blinkHiddenFace = face;
                face.Visibility = Visibility.Hidden;
                _blinkRestoreTimer.Start();
            }
            else
            {
                ScheduleNextBlink();
            }
        }

        private void BlinkRestoreTimer_Tick(object? sender, EventArgs e)
        {
            _blinkRestoreTimer.Stop();
            if (_blinkHiddenFace != null)
            {
                _blinkHiddenFace.Visibility = Visibility.Visible;
                _blinkHiddenFace = null;
            }
            ScheduleNextBlink();
        }

        private Canvas? GetActiveFaceCanvas() => _currentEmote switch
        {
            PetEmote.Normal   => NormalFace,
            PetEmote.Happy    => HappyFace,
            PetEmote.Hungry   => HungryFace,
            PetEmote.Sleeping => SleepingFace,
            PetEmote.Excited  => ExcitedFace,
            PetEmote.Love     => LoveFace,
            _ => null
        };

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
