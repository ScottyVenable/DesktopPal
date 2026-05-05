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
        private PetControl _pet;
        private System.Windows.Point _velocity = new System.Windows.Point(2, 0);
        private double _gravity = 0.5;
        private bool _isDragging = false;
        private System.Windows.Point _dragOffset;
        private Random _random = new Random();
        private DateTime _lastWanderChange = DateTime.Now;
        private DispatcherTimer _saveTimer;
        private SystemIntegrationService _sys;
        private DispatcherTimer _visionTimer;

        public MainWindow()
        {
            InitializeComponent();
            
            _pet = new PetControl();
            MainCanvas.Children.Add(_pet);
            Canvas.SetLeft(_pet, 100);
            Canvas.SetTop(_pet, 100);

            _pet.MouseDown += Pet_MouseDown;
            _pet.MouseMove += Pet_MouseMove;
            _pet.MouseUp += Pet_MouseUp;

            CompositionTarget.Rendering += GameLoop;

            _saveTimer = new DispatcherTimer();
            _saveTimer.Interval = TimeSpan.FromMinutes(1);
            _saveTimer.Tick += (s, e) => _pet.State.Save();
            _saveTimer.Start();

            _sys = new SystemIntegrationService();
            _sys.WatchDesktop(HandleLetterReceived);

            _visionTimer = new DispatcherTimer();
            _visionTimer.Interval = TimeSpan.FromMinutes(5);
            _visionTimer.Tick += (s, e) => HandleVision();
            _visionTimer.Start();
        }

        private async void HandleLetterReceived(string fileName, string content)
        {
            Dispatcher.Invoke(async () => 
            {
                _pet.ShowChat($"*reading {fileName}*");
                
                string reply = await _pet.AIService.ChatAsync($"I received a letter called {fileName}. It says: {content}. I should write a reply.");
                _pet.ShowChat(reply);
                
                _sys.WriteLetterBack(fileName, reply);
            });
        }

        private async void HandleVision()
        {
            if (!_pet.State.IsHatched) return;
            
            string comment = await _pet.AIService.ChatAsync("I'm just sitting here watching you work. What are you up to?");
            _pet.ShowChat(comment);
        }

        private void GameLoop(object sender, EventArgs e)
        {
            _pet.State.Tick();
            _pet.UpdateVisuals();

            if (_isDragging) return;

            double x = Canvas.GetLeft(_pet);
            double y = Canvas.GetTop(_pet);

            _velocity.Y += _gravity;
            
            if (DateTime.Now - _lastWanderChange > TimeSpan.FromSeconds(_random.Next(5, 15)))
            {
                _velocity.X = _random.NextDouble() * 2 - 1;
                _lastWanderChange = DateTime.Now;
            }

            x += _velocity.X;
            y += _velocity.Y;

            // Chance to plant a decoration
            if (_pet.State.IsHatched && _random.Next(0, 5000) == 0)
            {
                var type = _random.Next(0, 2) == 0 ? "Tree" : "Flower";
                var deco = new Decoration(type);
                MainCanvas.Children.Add(deco);
                Canvas.SetLeft(deco, x);
                Canvas.SetTop(deco, y + 40);
                System.Windows.Controls.Panel.SetZIndex(deco, -1);
            }

            double floor = SystemParameters.PrimaryScreenHeight - _pet.ActualHeight - 40;
            if (y > floor) { y = floor; _velocity.Y = 0; }
            if (x < 0) { x = 0; _velocity.X *= -1; }
            if (x > SystemParameters.PrimaryScreenWidth - _pet.ActualWidth) { x = SystemParameters.PrimaryScreenWidth - _pet.ActualWidth; _velocity.X *= -1; }

            Canvas.SetLeft(_pet, x);
            Canvas.SetTop(_pet, y);
        }

        private void Pet_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isDragging = true;
                _dragOffset = e.GetPosition(_pet);
                _pet.CaptureMouse();
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                _pet.ToggleStatus();
            }
        }

        private void Pet_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isDragging)
            {
                System.Windows.Point mousePos = e.GetPosition(MainCanvas);
                Canvas.SetLeft(_pet, mousePos.X - _dragOffset.X);
                Canvas.SetTop(_pet, mousePos.Y - _dragOffset.Y);
                _velocity = new System.Windows.Point(0, 0);
            }
        }

        private void Pet_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _pet.ReleaseMouseCapture();
            }
        }
    }
}
