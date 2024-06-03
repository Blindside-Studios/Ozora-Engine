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
using Ozora_Playgrounds.Pages;

namespace Ozora_Playgrounds
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            ViewPort.NavigateToType(typeof(PhysicsViewPort), null, null);
        }

        private void ViewPort_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            OzoraInterface.Instance.PointerLocation = e.GetCurrentPoint(ViewPort).Position;
        }
    }
}
