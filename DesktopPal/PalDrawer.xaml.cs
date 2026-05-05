using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace DesktopPal
{
    public partial class PalDrawer : System.Windows.Controls.UserControl
    {
        private bool _isExpanded = false;
        private MainWindow _parent;

        public PalDrawer()
        {
            InitializeComponent();
        }

        private void EnsureParent()
        {
            if (_parent == null)
            {
                _parent = (MainWindow)Window.GetWindow(this);
            }
        }

        private void DrawerToggle_Click(object sender, RoutedEventArgs e)
        {
            EnsureParent();
            if (_isExpanded)
            {
                ((Storyboard)Resources["CollapseDrawer"]).Begin();
                _isExpanded = false;
            }
            else
            {
                ((Storyboard)Resources["ExpandDrawer"]).Begin();
                _isExpanded = true;
            }
        }

        private void Stats_Click(object sender, RoutedEventArgs e)
        {
            EnsureParent();
            System.Windows.MessageBox.Show($"Level: {_parent.Pet.State.Level}\nHunger: {_parent.Pet.State.Hunger:F1}%\nHappiness: {_parent.Pet.State.Happiness:F1}%", "Pet Stats");
        }

        private void Feed_Click(object sender, RoutedEventArgs e)
        {
            EnsureParent();
            _parent.Pet.Feed();
        }

        private void Clean_Click(object sender, RoutedEventArgs e)
        {
            EnsureParent();
            _parent.CleanAll();
        }

        private void Call_Click(object sender, RoutedEventArgs e)
        {
            EnsureParent();
            _parent.CallPetToMouse();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            EnsureParent();
            _parent.ShowSettings();
        }
    }
}
