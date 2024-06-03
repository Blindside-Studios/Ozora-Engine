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
        public PhysicsSunSimulation()
        {
            this.InitializeComponent();
            OzoraInterface.Instance.ObjectWidth = SunObject.ActualWidth;
            OzoraInterface.Instance.ObjectHeight = SunObject.ActualHeight;
            OzoraInterface.Instance.UpdateObjectTranslationRequested += Instance_UpdateObjectTranslationRequested;
        }

        private void Instance_UpdateObjectTranslationRequested(System.Numerics.Vector3 obj)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                this.SunObject.Translation = obj;
            });
        }
    }
}
