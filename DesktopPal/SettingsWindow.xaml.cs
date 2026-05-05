using System.Windows;

namespace DesktopPal
{
    public partial class SettingsWindow : Window
    {
        private PetState _state;

        public SettingsWindow()
        {
            InitializeComponent();
            _state = ((MainWindow)System.Windows.Application.Current.MainWindow).Pet.State;
            
            PetNameBox.Text = _state.Name;
            ModelBox.Text = _state.ModelName;
            VisionEnabledCheck.IsChecked = _state.VisionEnabled;
            HotkeyBox.Text = ((char)_state.HotkeyCode).ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _state.Name = PetNameBox.Text;
            _state.ModelName = ModelBox.Text;
            _state.VisionEnabled = VisionEnabledCheck.IsChecked ?? true;
            
            if (!string.IsNullOrEmpty(HotkeyBox.Text))
            {
                _state.HotkeyCode = (int)HotkeyBox.Text.ToUpper()[0];
            }

            _state.Save();
            
            // Notify MainWindow to re-register hotkey
            ((MainWindow)System.Windows.Application.Current.MainWindow).RegisterGlobalHotkey();

            DialogResult = true;
            Close();
        }
    }
}
