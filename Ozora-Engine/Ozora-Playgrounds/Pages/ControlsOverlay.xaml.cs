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
using System.ComponentModel;
using System.Diagnostics;

namespace Ozora_Playgrounds.Pages
{
    public sealed partial class ControlsOverlay : Page
    {
        public ControlsOverlay()
        {
            this.InitializeComponent();

            Ozora.DefaultValues defaults = new Ozora.DefaultValues();
            FrameRateSlider.Value = defaults.FrameRate;
            MaxVectorDeltaSlider.Value = defaults.MaxVectorDeltaPerFrame;
            RubberBandingSlider.Value = defaults.RubberBandingModifier;
            BounceMomentumRetentionSlider.Value = defaults.BounceMomentumRetention;
        }

        private void FrameRateSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            ControlPanelViewModel.Instance.FrameRate = (int)e.NewValue;
        }

        private void MaxVectorDeltaSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            ControlPanelViewModel.Instance.MaxVectorDelta = (double)e.NewValue;
        }

        private void RubberBandingSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            ControlPanelViewModel.Instance.RubberBandingModifier = (double)e.NewValue;
        }

        private void SimulationPickerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex == 2)
            {

            }
            else
            {

            }
            
            /*ComboBox _comboBox = (ComboBox)sender;
            switch (_comboBox.SelectedIndex)
            {
                case 0:
                    OzoraSettings.Instance.SimulationStyle = SimulationStyle.Sun;
                    break;
                case 1:
                    OzoraSettings.Instance.SimulationStyle = SimulationStyle.Clouds;
                    break;
                default:
                    break;
            }
            OzoraInterface.Instance.LaunchNewActivity();*/
        }

        private void EnableBorderCollisionToggle_Toggled(object sender, RoutedEventArgs e)
        {
            ControlPanelViewModel.Instance.EnableBorderCollisions = EnableBorderCollisionToggle.IsOn;
        }

        private void EnableCollisionBounceToggle_Toggled(object sender, RoutedEventArgs e)
        {
            ControlPanelViewModel.Instance.EnableBounceOnCollision = EnableCollisionBounceToggle.IsOn;
        }

        private void BounceMomentumRetentionSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            ControlPanelViewModel.Instance.BounceMomentumRetention = (double)e.NewValue;
        }

        private void TrailingDragCoefficientSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            ControlPanelViewModel.Instance.TrailingDragCoefficient = TrailingDragCoefficientSlider.Value;
        }
    }

    public class ControlPanelViewModel: INotifyPropertyChanged
    {
        private static ControlPanelViewModel _instance;
        public static ControlPanelViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ControlPanelViewModel();
                }
                return _instance;
            }
        }


        public int FrameRate
        {
            get => _frameRate;
            set
            {
                if (value != _frameRate)
                {
                    _frameRate = value;
                    OnPropertyChanged(nameof(FrameRate));
                }
            }
        }
        private int _frameRate;

        public double MaxVectorDelta
        {
            get => _maxVectorDelta;
            set
            {
                if (value != _maxVectorDelta)
                {
                    _maxVectorDelta = value;
                    OnPropertyChanged(nameof(MaxVectorDelta));
                }
            }
        }
        private double _maxVectorDelta;

        public double RubberBandingModifier
        {
            get => _rubberBandingModifier;
            set
            {
                if (value != _rubberBandingModifier)
                {
                    _rubberBandingModifier = value;
                    OnPropertyChanged(nameof(RubberBandingModifier));
                }
            }
        }
        private double _rubberBandingModifier;

        public bool EnableBorderCollisions
        {
            get => _enableBorderCollisions;
            set
            {
                if (value != _enableBorderCollisions)
                {
                    _enableBorderCollisions = value;
                    OnPropertyChanged(nameof(EnableBorderCollisions));
                }
            }
        }
        private bool _enableBorderCollisions;

        public bool EnableBounceOnCollision
        {
            get => _enableBounceOnCollision;
            set
            {
                if (value != _enableBounceOnCollision)
                {
                    _enableBounceOnCollision = value;
                    OnPropertyChanged(nameof(EnableBorderCollisions));
                }
            }
        }
        private bool _enableBounceOnCollision;

        public double BounceMomentumRetention
        {
            get => _bounceMomentumRetention;
            set
            {
                if (value != _bounceMomentumRetention)
                {
                    _bounceMomentumRetention = value;
                    OnPropertyChanged(nameof(BounceMomentumRetention));
                }
            }
        }
        private double _bounceMomentumRetention;

        public double TrailingDragCoefficient
        {
            get => _trailingDragCoefficient;
            set
            {
                if (value != _trailingDragCoefficient)
                {
                    _trailingDragCoefficient = value;
                    OnPropertyChanged(nameof(TrailingDragCoefficient));
                }
            }
        }
        private double _trailingDragCoefficient;


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
