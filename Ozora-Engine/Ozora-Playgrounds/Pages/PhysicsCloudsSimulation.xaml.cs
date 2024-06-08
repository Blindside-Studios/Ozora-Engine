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
using Microsoft.UI.Xaml.Media.Imaging;

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
                DensityModifier = 10
            };
            System.Numerics.Vector3[] _vectorsList = initializer.GenerateCloudPositions(settings);


            Random rnd = new Random();
            foreach (Vector3 position in _vectorsList)
            {
                Image cloud = new();
                cloud.Height = 100;
                cloud.Width = 100;
                cloud.HorizontalAlignment = HorizontalAlignment.Left;
                cloud.VerticalAlignment = VerticalAlignment.Top;
                cloud.Translation = position;
                cloud.CenterPoint = new Vector3(50, 50, 0);
                
                var bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/CloudSprites/cloud - " + rnd.Next(0,9).ToString() + ".png"));
                cloud.Source = bitmapImage;
                cloud.Stretch = Stretch.UniformToFill;

                CloudsGrid.Children.Add(cloud);
                cloud.Opacity = 0.5;
            }
        }

        private void CloudsGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            loadClouds();
        }
    }
}
