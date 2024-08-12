using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Ozora;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Ozora_Playgrounds.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BirdsSimulation : Page
    {
        ParulAI parulAI;
        public BirdsSimulation()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            this.Loaded += BirdsSimulation_Loaded;
            this.Unloaded += BirdsSimulation_Unloaded;
        }

        private void BirdsSimulation_Unloaded(object sender, RoutedEventArgs e)
        {
            parulAI.StopSpawningBirds();
            parulAI = null;
            RootGrid.Children.Clear();

            // important cleanup to prevent ghost birds from being cached and flickering when reloading the page
            CurrentBirdSimulation.Instance.CleanUp();

            Debug.WriteLine("Unloaded ParulAI Model");
        }

        private void BirdsSimulation_Loaded(object sender, RoutedEventArgs e)
        {
            parulAI = new ParulAI();

            CurrentBirdSimulation.Instance.RestingSpots = new RestingSpot[]
            {
                new RestingSpot() { Position = new System.Numerics.Vector3(500, 500, 0), IsOccupied = false },
                new RestingSpot() { Position = new System.Numerics.Vector3(960,60,0), IsOccupied = false },
                new RestingSpot() { Position = new System.Numerics.Vector3(400,80,0), IsOccupied = false },
                new RestingSpot() { Position = new System.Numerics.Vector3(1000, 400, 0), IsOccupied = false }
            };

            /*CurrentBirdSimulation.Instance.RestingSpots.Append(new RestingSpot() { Position = new System.Numerics.Vector3(500, 500, 0), IsOccupied = false });
            CurrentBirdSimulation.Instance.RestingSpots.Append(new RestingSpot() { Position = new System.Numerics.Vector3(960,60,0), IsOccupied = false });
            CurrentBirdSimulation.Instance.RestingSpots.Append(new RestingSpot() { Position = new System.Numerics.Vector3(400,80,0), IsOccupied = false });
            CurrentBirdSimulation.Instance.RestingSpots.Append(new RestingSpot() { Position = new System.Numerics.Vector3(1000, 400, 0), IsOccupied = false });*/

            /*foreach (Microsoft.UI.Xaml.Shapes.Rectangle rect in RootGrid.Children.OfType<Microsoft.UI.Xaml.Shapes.Rectangle>())
            {
                CurrentBirdSimulation.Instance.RestingSpots.Append(new RestingSpot()
                {
                    Position = rect.Translation,
                    IsOccupied = false
                });
            }*/

            CurrentBirdSimulation.Instance.RootGrid = RootGrid;
            CurrentBirdSimulation.Instance.UIDispatcherQueue = DispatcherQueue;

            parulAI.StartSpawningBirds();
        }
    }
}
