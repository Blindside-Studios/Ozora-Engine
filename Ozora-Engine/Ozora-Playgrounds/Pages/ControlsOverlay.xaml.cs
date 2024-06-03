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
    public sealed partial class ControlsOverlay : Page
    {
        public ControlsOverlay()
        {
            this.InitializeComponent();
            Ozora.DefaultValues defaults = new Ozora.DefaultValues();
            FrameRateSlider.Value = defaults.FrameRate;
            MaxVectorDeltaSlider.Value = defaults.MaxVectorDeltaPerFrame;
        }

        private void FrameRateSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            OzoraSettings.Instance.FrameRate = (int)e.NewValue;
        }

        private void MaxVectorDeltaSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            OzoraSettings.Instance.MaxVectorDeltaPerFrame = (double)e.NewValue;
        }
    }
}
