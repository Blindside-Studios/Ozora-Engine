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
using System.Diagnostics;

namespace Ozora_Playgrounds.Pages
{
    public sealed partial class PhysicsCloudsSimulation : Page
    {
        OzoraEngine ozoraEngine;

        public PhysicsCloudsSimulation()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            this.Unloaded += PhysicsCloudsSimulation_Unloaded;

            // make sure to wait for the CloudsGrid to be loaded so it has an actual width that can be used to compute the cloud distribution
            CloudsGrid.Loaded += CloudsGrid_Loaded;
            MouseViewModel.Instance.PropertyChanged += Instance_PropertyChanged;
        }

        private void PhysicsCloudsSimulation_Unloaded(object sender, RoutedEventArgs e)
        {
            ozoraEngine.Physics.MouseCursorEngaged = false;
            ozoraEngine.Physics.InterruptSimulation();
            ozoraEngine = null;
            Debug.WriteLine("Unloaded Ozora Cloud Simulation Model");
        }

        private void CloudsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            loadClouds();
        }

        private void loadClouds()
        {
            CloudsGrid.Children.Clear();

            // the initializer is used to generate clouds, it doesn't need to be passed into the actual engine
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


            ozoraEngine = new OzoraEngine();

            OzoraSettings ozoraSettings = new OzoraSettings()
            {
                SimulationStyle = SimulationStyle.Clouds,
                FrameRate = 60,
            };

            ozoraEngine.Physics.Interface = new OzoraInterface()
            {
                AreaDimensions = new Windows.Foundation.Point(CloudsGrid.ActualWidth, CloudsGrid.ActualHeight),
                CloudGrid = CloudsGrid,
                UIDispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread(),
                Settings = ozoraSettings
            };

            /// Very important to set this property, otherwise, the physics simulation will not engage!
            /// Equally important to call the method below. This allows the simulation to disengage while it 
            /// is still active to avoid having to restart, though this is mainly used for the sun simulation.
            ozoraEngine.Physics.MouseCursorEngaged = true;
            ozoraEngine.Physics.StartSimulation();
        }

        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ozoraEngine != null) ozoraEngine.Physics.Interface.PointerLocation = MouseViewModel.Instance.MousePosition;
        }

        private void CloudsGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            loadClouds();
        }
    }
}
