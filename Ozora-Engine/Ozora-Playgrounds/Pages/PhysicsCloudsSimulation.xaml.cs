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
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Composition;

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
            loadClouds();
        }

        private void loadClouds()
        {
            CloudsGrid.Children.Clear();

            OzoraInterface.Instance.UIDispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            OzoraInterface.Instance.CloudGrid = CloudsGrid;

            Ozora.Initializer initializer = new();
            Ozora.CloudSettings settings = new()
            {
                AreaWidth = CloudsGrid.ActualWidth,
                AreaHeight = CloudsGrid.ActualHeight,
                ImageWidth = 100,
                ImageHeight = 100,
                DensityModifier = 5
            };
            System.Numerics.Vector3[] _vectorsList = initializer.GenerateCloudPositions(settings);


            Random rnd = new Random();
            foreach (Vector3 position in _vectorsList)
            {
                Microsoft.UI.Xaml.Shapes.Rectangle rect = new();
                rect.Height = 100;
                rect.Width = 100;
                //rect.RadiusX = 50;
                //rect.RadiusY = 50;
                rect.HorizontalAlignment = HorizontalAlignment.Left;
                rect.VerticalAlignment = VerticalAlignment.Top;
                rect.Translation = position;
                rect.CenterPoint = new Vector3(50, 50, 0);
                rect.Rotation = rnd.Next(90);
                rect.Fill = new SolidColorBrush(Microsoft.UI.Colors.White);
                CloudsGrid.Children.Add(rect);
                rect.Opacity = 0.5;
            }
        }

        private void CloudsGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            loadClouds();
        }
    }
}
