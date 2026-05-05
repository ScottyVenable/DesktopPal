using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DesktopPal
{
    public class Decoration : System.Windows.Controls.UserControl
    {
        public Decoration(string type)
        {
            Width = 40;
            Height = 40;
            
            var grid = new Grid();
            if (type == "Tree")
            {
                var trunk = new System.Windows.Shapes.Rectangle { Fill = System.Windows.Media.Brushes.SaddleBrown, Width = 10, Height = 20, VerticalAlignment = VerticalAlignment.Bottom };
                var leaves = new Ellipse { Fill = System.Windows.Media.Brushes.ForestGreen, Width = 30, Height = 30, VerticalAlignment = VerticalAlignment.Top };
                grid.Children.Add(trunk);
                grid.Children.Add(leaves);
            }
            else if (type == "Flower")
            {
                var stem = new System.Windows.Shapes.Rectangle { Fill = System.Windows.Media.Brushes.Green, Width = 2, Height = 15, VerticalAlignment = VerticalAlignment.Bottom };
                var petal = new Ellipse { Fill = System.Windows.Media.Brushes.Pink, Width = 15, Height = 15, VerticalAlignment = VerticalAlignment.Top };
                grid.Children.Add(stem);
                grid.Children.Add(petal);
            }
            
            Content = grid;
        }
    }
}
