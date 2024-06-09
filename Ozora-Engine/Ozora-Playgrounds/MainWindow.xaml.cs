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
using System.ComponentModel;

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
            MouseViewModel.Instance.MousePosition = e.GetCurrentPoint(ViewPort).Position;
        }
    }

    public class MouseViewModel: INotifyPropertyChanged
    {
        private static MouseViewModel _instance;
        public static MouseViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MouseViewModel();
                }
                return _instance;
            }
        }

        public Windows.Foundation.Point MousePosition
        {
            get => _mousePosition;
            set
            {
                if (_mousePosition != value)
                {
                    _mousePosition = value;
                    OnPropertyChanged(nameof(MousePosition));
                }
            }
        }
        private Windows.Foundation.Point _mousePosition;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
