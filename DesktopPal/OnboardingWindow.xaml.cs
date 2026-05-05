using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DesktopPal
{
    /// <summary>
    /// First-run onboarding for DesktopPal (issue #20).
    ///
    /// Three soft, friendly steps: welcome → hotkey → tray + care.
    /// Shown only when <see cref="PetState.HasCompletedOnboarding"/> is false.
    /// On Done, the flag is set to true and persisted via PetState.Save().
    ///
    /// NOTE: Copy below is placeholder in the blue-bear voice (cute, warm,
    /// short). Final copy lives in docs/content/onboarding.md (Vex is
    /// authoring it in parallel) and can be ported here later — keep the
    /// step structure stable so a swap is mechanical.
    /// </summary>
    public partial class OnboardingWindow : System.Windows.Window
    {
        private readonly PetState _state;
        private int _stepIndex;
        private const int TotalSteps = 3;

        private static readonly (string Title, string Body)[] _steps =
        {
            ( "Hi there, I'm your buddy.",
              "I'll hang out on your desktop and keep you company. " +
              "Feed me, talk to me, give me a little pet now and then — " +
              "we're going to get along just fine." ),

            ( "Need me? Just call.",
              "Press your hotkey any time to open my companion panel. " +
              "That's where you can chat with me, see how I'm doing, " +
              "and tweak settings. You can change the keys later." ),

            ( "I'm always around.",
              "If you can't see me, look in your system tray — that little " +
              "icon brings me back. From the panel you can feed me when " +
              "I'm hungry and tidy me up when I'm grubby. That's it. " +
              "Welcome aboard." )
        };

        public OnboardingWindow(PetState state)
        {
            InitializeComponent();
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _stepIndex = 0;
            RenderStep();

            // Allow click-drag on the chrome-less window so it feels like
            // the rest of the app's frameless surfaces.
            MouseLeftButtonDown += (_, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed) DragMove();
            };
        }

        private void RenderStep()
        {
            var (title, body) = _steps[_stepIndex];
            StepTitle.Text = title;
            StepBody.Text  = body;

            Dot1.Fill = StepDotBrush(0);
            Dot2.Fill = StepDotBrush(1);
            Dot3.Fill = StepDotBrush(2);

            // Hotkey card visible only on step 2 (index 1).
            if (_stepIndex == 1)
            {
                HotkeyCard.Visibility = Visibility.Visible;
                HotkeyText.Text       = "Your hotkey: " + DescribeHotkey(_state);
            }
            else
            {
                HotkeyCard.Visibility = Visibility.Collapsed;
            }

            BackButton.IsEnabled = _stepIndex > 0;
            NextButton.Content   = (_stepIndex == TotalSteps - 1) ? "Done" : "Next";
        }

        private System.Windows.Media.Brush StepDotBrush(int index)
        {
            // Filled (blue) for current/past steps; muted for upcoming.
            return index <= _stepIndex
                ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x42, 0x99, 0xE1))
                : new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xCB, 0xD5, 0xE0));
        }

        private static string DescribeHotkey(PetState state)
        {
            // Bit flags match WinAPI MOD_* used elsewhere:
            //   MOD_ALT = 0x1, MOD_CONTROL = 0x2, MOD_SHIFT = 0x4, MOD_WIN = 0x8
            var parts = new StringBuilder();
            int mods = state.HotkeyModifier;
            if ((mods & 0x2) != 0) parts.Append("Ctrl + ");
            if ((mods & 0x1) != 0) parts.Append("Alt + ");
            if ((mods & 0x4) != 0) parts.Append("Shift + ");
            if ((mods & 0x8) != 0) parts.Append("Win + ");

            char key = (char)state.HotkeyCode;
            if (key < 32 || key > 126) key = '?';
            parts.Append(key);
            return parts.ToString();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (_stepIndex > 0)
            {
                _stepIndex--;
                RenderStep();
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (_stepIndex < TotalSteps - 1)
            {
                _stepIndex++;
                RenderStep();
                return;
            }

            // Final step: mark completed and persist before closing.
            _state.HasCompletedOnboarding = true;
            try { _state.Save(); }
            catch (Exception ex)
            {
                Logging.Error("OnboardingWindow", "Failed to persist onboarding flag.", ex);
            }
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            // Closing the window via the OS (Alt+F4, etc.) also counts as
            // dismissal — don't pester the user on next launch.
            if (!_state.HasCompletedOnboarding)
            {
                _state.HasCompletedOnboarding = true;
                try { _state.Save(); } catch { /* ignore */ }
            }
            base.OnClosed(e);
        }
    }
}
