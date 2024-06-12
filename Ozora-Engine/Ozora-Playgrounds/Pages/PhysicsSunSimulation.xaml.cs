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

namespace Ozora_Playgrounds.Pages
{
    public sealed partial class PhysicsSunSimulation : Page
    {
        OzoraEngine Ozora = new OzoraEngine();

        public PhysicsSunSimulation()
        {
            this.InitializeComponent();

            OzoraSettings SunSettings = new OzoraSettings()
            {
                SimulationStyle = SimulationStyle.Sun,
                FrameRate = 60,
                MaxVectorDeltaPerFrame = 1,
                RubberBandingModifier = 0.05
            };

            Ozora.Physics.Interface = new OzoraInterface()
            {
                ObjectWidth = SunObject.ActualWidth,
                ObjectHeight = SunObject.ActualHeight,
                Settings = SunSettings
            };
            Ozora.Physics.ObjectPositionCalculated += Physics_ObjectPositionCalculated;
            MouseViewModel.Instance.PropertyChanged += Instance_PropertyChanged;

            Ozora.Physics.StartSimulation();

            ControlPanelViewModel.Instance.PropertyChanged += Instance_PropertyChanged1;
        }

        private void Instance_PropertyChanged1(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Ozora.Physics.Interface.Settings.FrameRate = ControlPanelViewModel.Instance.FrameRate;
            Ozora.Physics.Interface.Settings.MaxVectorDeltaPerFrame = ControlPanelViewModel.Instance.MaxVectorDelta;
            Ozora.Physics.Interface.Settings.RubberBandingModifier = ControlPanelViewModel.Instance.RubberBandingModifier;
        }

        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Ozora.Physics.Interface.PointerLocation = MouseViewModel.Instance.MousePosition;
        }

        private void Physics_ObjectPositionCalculated(object sender, ObjectPositionUpdatedEvent e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                this.SunObject.Translation = e.NewTranslationVector;
            });
        }
    }
}
