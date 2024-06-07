using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Ozora;
using System.Numerics;
using System.Drawing;

namespace Ozora_Playgrounds.Pages
{
    public sealed partial class PhysicsCloudsSimulation : Page
    {
        public PhysicsCloudsSimulation()
        {
            this.InitializeComponent();

            // make sure to wait for the CloudsGrid to be loaded so it has an actual width that can be used to compute the cloud distribution
            CloudsGrid.Loaded += CloudsGrid_Loaded;
        }

        private void CloudsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Ozora.Initializer initializer = new();
            Ozora.CloudSettings settings = new()
            {
                AreaWidth = CloudsGrid.ActualWidth,
                AreaHeight = CloudsGrid.ActualHeight,
                ImageWidth = 50,
                ImageHeight = 50,
                DensityModifier = 4
            };
            System.Numerics.Vector3[] _vectorsList = initializer.GenerateCloudPositions(settings);

            foreach (Vector3 position in _vectorsList)
            {
                Microsoft.UI.Xaml.Shapes.Rectangle rect = new();
                rect.Height = 50;
                rect.Width = 50;
                rect.RadiusX = 25;
                rect.RadiusY = 25;
                rect.HorizontalAlignment = HorizontalAlignment.Left;
                rect.VerticalAlignment = VerticalAlignment.Top;
                rect.Translation = position;
                rect.Fill = new SolidColorBrush(Microsoft.UI.Colors.White);
                CloudsGrid.Children.Add(rect);
                rect.Opacity = 0.5;
            }
        }
    }
}
