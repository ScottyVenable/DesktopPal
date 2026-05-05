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
        public PetState State { get; private set; }
        public AIService AIService { get; private set; }
        private DispatcherTimer _chatTimer;

        public PetControl()
        {
            InitializeComponent();
            State = PetState.Load();
            AIService = new AIService(State);
            
            _chatTimer = new DispatcherTimer();
            _chatTimer.Interval = TimeSpan.FromSeconds(8);
            _chatTimer.Tick += (s, e) => { ChatBubble.Visibility = Visibility.Collapsed; _chatTimer.Stop(); };

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
            else
            {
                SetEmote(false);
            }
            
            PetBody.Fill = State.IsHatched ? System.Windows.Media.Brushes.LightGreen : System.Windows.Media.Brushes.White;
        }

        public void ToggleStatus()
        {
            StatusBubble.Visibility = StatusBubble.Visibility == Visibility.Visible 
                ? Visibility.Collapsed 
                : Visibility.Visible;
        }

        private async void ProcessChat()
        {
            string input = ChatInput.Text;
            if (string.IsNullOrWhiteSpace(input)) return;
            
            ChatInput.Text = "";
            ShowChat("...");

            string reply = await AIService.ChatAsync(input);
            ShowChat(reply);
            
            State.Experience += 2;
            State.Happiness = Math.Min(100, State.Happiness + 5);
        }

        private void Say_Click(object sender, RoutedEventArgs e) => ProcessChat();

        private void ChatInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter) ProcessChat();
        }

        private void Feed_Click(object sender, RoutedEventArgs e)
        {
            State.Hunger = Math.Min(100, State.Hunger + 20);
            State.Experience += 5;
            UpdateVisuals();
        }

        private void Clean_Click(object sender, RoutedEventArgs e)
        {
            State.Hygiene = 100;
            State.Experience += 2;
            UpdateVisuals();
        }

        public void SetColor(System.Windows.Media.Brush color) => PetBody.Fill = color;
    }
}
