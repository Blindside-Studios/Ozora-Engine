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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Ozora_Playgrounds.Pages
{
    public sealed partial class PhysicsViewPort : Page
    {
        public PhysicsViewPort()
        {
            this.InitializeComponent();
            BodyViewPort.NavigateToType(typeof(BirdsSimulation), null, null);
            //StateOverlay.NavigateToType(typeof(ControlsOverlay), null, null);
            StateOverlay.Visibility = Visibility.Collapsed;

            //OzoraSettings.Instance.SimulationStyleChanged += Instance_SimulationStyleChanged;
        }

        /*private void Instance_SimulationStyleChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (OzoraSettings.Instance.SimulationStyle)
            {
                case SimulationStyle.Sun:
                    BodyViewPort.NavigateToType(typeof(PhysicsSunSimulation), null, null);
                    break;
                case SimulationStyle.Clouds:
                    BodyViewPort.NavigateToType(typeof(PhysicsCloudsSimulation), null, null);
                    break;
            }
        }*/
    }
}
