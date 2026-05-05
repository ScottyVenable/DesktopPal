using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DesktopPal
{
    public class Decoration : System.Windows.Controls.UserControl
    {
        public string DecorationType { get; private set; }

        public Decoration(string type)
        {
            DecorationType = type;
            Width = 40;
            Height = 40;
            
            var grid = new Grid();
            grid.Background = System.Windows.Media.Brushes.Transparent; // Make it clickable

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
            else if (type == "Poop")
            {
                var poop = new Ellipse { Fill = System.Windows.Media.Brushes.SaddleBrown, Width = 20, Height = 15, VerticalAlignment = VerticalAlignment.Center };
                grid.Children.Add(poop);
                this.Cursor = System.Windows.Input.Cursors.Hand;
                this.MouseDown += (s, e) => {
                    ((WorldWindow)Window.GetWindow(this)).RemoveObject(this);
                    ((MainWindow)System.Windows.Application.Current.MainWindow).Pet.State.Hygiene = Math.Min(100, ((MainWindow)System.Windows.Application.Current.MainWindow).Pet.State.Hygiene + 5);
                };
            }
            
            Content = grid;
        }
    }

    /// <summary>
    /// Garden plot lifecycle (issue #3 — first-playable gardening loop).
    /// A plot advances through Empty -> Seeded -> Sprout -> Bloom over real
    /// time. Reaching Bloom makes the plot harvestable for a small reward.
    /// Persisted via <see cref="GardenPlotData"/> on <see cref="WorldState"/>.
    /// </summary>
    public enum GardenPlotState
    {
        Empty,
        Seeded,
        Sprout,
        Bloom
    }

    /// <summary>
    /// Tunables for the gardening MVP. Intervals are intentionally short so
    /// the loop is observable in a single play session. Values will be
    /// rebalanced once the broader progression doc lands.
    /// </summary>
    public static class GardenConstants
    {
        // Seeded -> Sprout transition delay.
        public static readonly TimeSpan SeededToSproutDelay = TimeSpan.FromSeconds(30);
        // Sprout -> Bloom transition delay.
        public static readonly TimeSpan SproutToBloomDelay = TimeSpan.FromMinutes(2);
        // Reward applied on harvesting a Bloom plot.
        public const double HarvestHappinessReward = 10.0;
        public const double HarvestExperienceReward = 5.0;
        // Visual footprint of a plot (matches Decoration footprint).
        public const double PlotWidth = 40.0;
        public const double PlotHeight = 40.0;
    }

    /// <summary>
    /// Visual control for a single garden plot. The control is a thin
    /// projection of <see cref="GardenPlotData"/>; mutate the data and call
    /// <see cref="Refresh"/> to update what the user sees.
    ///
    /// The harvest action is surfaced as a click handler that fires
    /// <see cref="HarvestRequested"/>; the simulation owner (MainWindow)
    /// applies the reward and resets the plot to Empty.
    /// </summary>
    public class GardenPlot : System.Windows.Controls.UserControl
    {
        public GardenPlotData Data { get; }

        /// <summary>
        /// Fired when the user clicks a Bloom plot. The handler is expected
        /// to apply the reward and reset the plot's state on the data side,
        /// then call <see cref="Refresh"/> on this control.
        /// </summary>
        public event Action<GardenPlot>? HarvestRequested;

        public GardenPlot(GardenPlotData data)
        {
            Data = data;
            Width = GardenConstants.PlotWidth;
            Height = GardenConstants.PlotHeight;
            Cursor = System.Windows.Input.Cursors.Hand;
            MouseDown += (s, e) =>
            {
                if (Data.State == GardenPlotState.Bloom)
                {
                    HarvestRequested?.Invoke(this);
                }
            };
            Refresh();
        }

        /// <summary>
        /// Rebuilds the visual to match the current <see cref="GardenPlotData.State"/>.
        /// Cheap to call; uses a handful of WPF primitives per state.
        /// </summary>
        public void Refresh()
        {
            var grid = new Grid { Background = System.Windows.Media.Brushes.Transparent };

            // Soil mound — present in every state so the plot reads as a
            // single tended location even when Empty.
            var soil = new Ellipse
            {
                Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(120, 78, 48)),
                Width = 28,
                Height = 10,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };
            grid.Children.Add(soil);

            switch (Data.State)
            {
                case GardenPlotState.Empty:
                    // Soil only.
                    break;

                case GardenPlotState.Seeded:
                    var seed = new Ellipse
                    {
                        Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(60, 40, 24)),
                        Width = 5,
                        Height = 4,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 6)
                    };
                    grid.Children.Add(seed);
                    break;

                case GardenPlotState.Sprout:
                    var sproutStem = new System.Windows.Shapes.Rectangle
                    {
                        Fill = System.Windows.Media.Brushes.YellowGreen,
                        Width = 2,
                        Height = 12,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 8)
                    };
                    var leaf = new Ellipse
                    {
                        Fill = System.Windows.Media.Brushes.YellowGreen,
                        Width = 8,
                        Height = 5,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        Margin = new Thickness(8, 0, 0, 14)
                    };
                    grid.Children.Add(sproutStem);
                    grid.Children.Add(leaf);
                    break;

                case GardenPlotState.Bloom:
                    var bloomStem = new System.Windows.Shapes.Rectangle
                    {
                        Fill = System.Windows.Media.Brushes.ForestGreen,
                        Width = 2,
                        Height = 18,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 8)
                    };
                    var petal = new Ellipse
                    {
                        Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 170, 200)),
                        Width = 16,
                        Height = 16,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        Margin = new Thickness(0, 2, 0, 0)
                    };
                    var center = new Ellipse
                    {
                        Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 215, 90)),
                        Width = 6,
                        Height = 6,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        Margin = new Thickness(0, 7, 0, 0)
                    };
                    grid.Children.Add(bloomStem);
                    grid.Children.Add(petal);
                    grid.Children.Add(center);
                    ToolTip = "Click to harvest";
                    break;
            }

            Content = grid;
        }
    }
}

