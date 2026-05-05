using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace DesktopPal
{
    public partial class PetControl : System.Windows.Controls.UserControl
    {
        public PetState State { get; private set; }
        public AIService AIService { get; private set; }
        private DispatcherTimer _chatTimer;
        private DispatcherTimer _emoteTimer;

        public PetControl()
        {
            InitializeComponent();
            State = PetState.Load();
            AIService = new AIService(State);
            
            _chatTimer = new DispatcherTimer();
            _chatTimer.Interval = TimeSpan.FromSeconds(8);
            _chatTimer.Tick += (s, e) => { ChatBubble.Visibility = Visibility.Collapsed; _chatTimer.Stop(); };

            _emoteTimer = new DispatcherTimer();
            _emoteTimer.Interval = TimeSpan.FromSeconds(2);
            _emoteTimer.Tick += (s, e) => { SetEmote(false); _emoteTimer.Stop(); };

            UpdateVisuals();
        }

        public void ShowChat(string text)
        {
            ChatText.Text = text;
            ChatBubble.Visibility = Visibility.Visible;
            _chatTimer.Stop();
            _chatTimer.Start();
        }

        public void SetEmote(bool active)
        {
            if (active)
            {
                LeftEye.Visibility = Visibility.Collapsed;
                RightEye.Visibility = Visibility.Collapsed;
                LeftEyeWhite.Visibility = Visibility.Collapsed;
                RightEyeWhite.Visibility = Visibility.Collapsed;
                LeftEmote.Visibility = Visibility.Visible;
                RightEmote.Visibility = Visibility.Visible;
            }
            else
            {
                LeftEye.Visibility = Visibility.Visible;
                RightEye.Visibility = Visibility.Visible;
                LeftEyeWhite.Visibility = Visibility.Visible;
                RightEyeWhite.Visibility = Visibility.Visible;
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
            
            BodyPath.Fill = State.IsHatched ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(178, 255, 178)) : System.Windows.Media.Brushes.White;
        }

        public void SetFacing(bool faceLeft)
        {
            PetVisualRoot.RenderTransform = new System.Windows.Media.ScaleTransform(faceLeft ? -1 : 1, 1, 50, 50);
        }

        public void SetDepthScale(double scale)
        {
            DepthScale.ScaleX = scale;
            DepthScale.ScaleY = scale;
        }

        public void ToggleStatus()
        {
            StatusBubble.Visibility = StatusBubble.Visibility == Visibility.Visible 
                ? Visibility.Collapsed 
                : Visibility.Visible;
        }

        public async void ProcessChat(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;
            
            ChatInput.Text = "";
            ShowChat("...");

            string reply = await AIService.ChatAsync(input);
            ShowChat(reply);
            
            State.Experience += 2;
            State.Happiness = Math.Min(100, State.Happiness + 5);
        }

        public void Feed()
        {
            State.Hunger = Math.Min(100, State.Hunger + 20);
            State.Experience += 5;
            UpdateVisuals();
            ShowChat("Yum! Thank you!");
        }

        public void PetMe()
        {
            State.Happiness = Math.Min(100, State.Happiness + 10);
            State.Experience += 1;
            SetEmote(true);
            _emoteTimer.Start();
            ShowChat(OfflineBrain.GetRandomPhrase("Petting"));
        }

        private void Say_Click(object sender, RoutedEventArgs e) => ProcessChat(ChatInput.Text);

        private void ChatInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter) ProcessChat(ChatInput.Text);
        }

        private void Feed_Click(object sender, RoutedEventArgs e) => Feed();

        private void Clean_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Window.GetWindow(this)).CleanAll();
        }
    }
}
