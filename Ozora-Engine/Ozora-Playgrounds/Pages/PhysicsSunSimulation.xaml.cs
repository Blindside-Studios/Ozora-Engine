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
            this.Loaded += PhysicsSunSimulation_Loaded;
        }

        private void PhysicsSunSimulation_Loaded(object sender, RoutedEventArgs e)
        {
            OzoraSettings SunSettings = new OzoraSettings()
            {
                SimulationStyle = SimulationStyle.Sun,
                FrameRate = 60,
                MaxVectorDeltaPerFrame = 1,
                RubberBandingModifier = 0.05f,
                EnableBorderCollision = false,
                EnableBounceOnCollision = false,
                BounceMomentumRetention = 0.5f
            };

            Ozora.Physics.Interface = new OzoraInterface()
            {
                ObjectWidth = (float)SunObject.ActualWidth,
                ObjectHeight = (float)SunObject.ActualHeight,
                Settings = SunSettings,
                AreaDimensions = new Windows.Foundation.Point(SunGrid.ActualWidth, SunGrid.ActualHeight)
            };

            Ozora.Physics.ObjectPositionCalculated += Physics_ObjectPositionCalculated;
            MouseViewModel.Instance.PropertyChanged += MouseViewModel_PropertyChanged;

            Ozora.Physics.StartSimulation();

            ControlPanelViewModel.Instance.PropertyChanged += ControlPanelViewModel_PropertyChanged1;
        }

        private void ControlPanelViewModel_PropertyChanged1(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Ozora.Physics.Interface.Settings.FrameRate = ControlPanelViewModel.Instance.FrameRate;
            Ozora.Physics.Interface.Settings.MaxVectorDeltaPerFrame = (float)ControlPanelViewModel.Instance.MaxVectorDelta;
            Ozora.Physics.Interface.Settings.RubberBandingModifier = (float)ControlPanelViewModel.Instance.RubberBandingModifier;
            Ozora.Physics.Interface.Settings.EnableBorderCollision = ControlPanelViewModel.Instance.EnableBorderCollisions;
            Ozora.Physics.Interface.Settings.EnableBounceOnCollision = ControlPanelViewModel.Instance.EnableBounceOnCollision;
            Ozora.Physics.Interface.Settings.BounceMomentumRetention = (float)ControlPanelViewModel.Instance.BounceMomentumRetention;
            Ozora.Physics.Interface.Settings.TrailingDragCoefficient = (float)ControlPanelViewModel.Instance.TrailingDragCoefficient;
        }

        private void MouseViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Ozora.Physics.Interface.PointerLocation = MouseViewModel.Instance.MousePosition;
            Ozora.Physics.MouseCursorEngaged = MouseViewModel.Instance.EngageMouse;
        }

        private void Physics_ObjectPositionCalculated(object sender, ObjectPositionUpdatedEvent e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                this.SunObject.Translation = e.NewTranslationVector;
            });
        }

        private void SunGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            /// Null check as this event is fired when the page loads, 
            /// briefly before the code in the PageLoaded event is executed, 
            /// which initializes the Interface
            if (Ozora.Physics.Interface != null)
            {
                Ozora.Physics.Interface.AreaDimensions = 
                    new Windows.Foundation.Point(SunGrid.ActualWidth, SunGrid.ActualHeight);
            }
        }
    }
}
