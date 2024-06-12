using Microsoft.UI.Composition;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Dispatching;

namespace Ozora
{
    public class OzoraEngine
    {        
        public Physics Physics = new Physics();
        public Initializer Initializer = new Initializer();
        public DefaultValues DefaultValues = new DefaultValues();
    }

    public class OzoraInterface: INotifyPropertyChanged
    {
        public OzoraSettings Settings { get; set; }

        public DispatcherQueue UIDispatcherQueue;


        public Windows.Foundation.Point PointerLocation 
        {
            get => _pointerLocation;
            set
            {
                if (_pointerLocation != value)
                {
                    _pointerLocation = value;
                    PointerLocationChanged(value);
                }
            }
        }
        private Windows.Foundation.Point _pointerLocation;

        internal event EventHandler<PointerPositionUpdatedEvent> PointerPositionUpdated;
        private void PointerLocationChanged(Windows.Foundation.Point newVector) { PointerPositionUpdated?.Invoke(this, new PointerPositionUpdatedEvent(newVector)); }


        public Windows.Foundation.Point AreaDimensions
        {
            get => _areaDimensions;
            set
            {
                if (_areaDimensions != value)
                {
                    _areaDimensions = value;
                    PointerLocationChanged(value);
                }
            }
        }
        private Windows.Foundation.Point _areaDimensions;

        
        public double ObjectWidth
        {
            get
            {
                return _objectWidth;
            }
            set { SetField(ref _objectWidth, value); }
        }
        private double _objectWidth;

        public double ObjectHeight
        {
            get
            {
                return _objectHeight;
            }
            set { SetField(ref _objectHeight, value); }
        }
        private double _objectHeight;

        public Grid CloudGrid { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class OzoraSettings: INotifyPropertyChanged
    {
        Ozora.DefaultValues defaults = new Ozora.DefaultValues();

        public SimulationStyle SimulationStyle
        {
            get => _simulationStyle;
            set
            {
                if (_simulationStyle != value)
                {
                    _simulationStyle = value;
                    SimulationStyleChanged?.Invoke(this, new PropertyChangedEventArgs(nameof (SimulationStyle)));
                }
            }
        }
        private SimulationStyle _simulationStyle;

        public TrailingType TrailingType
        {
            get { return _trailingType; }
            set { SetField(ref _trailingType, value); }
        }
        private TrailingType _trailingType;

        public int FrameRate
        {
            get {
                if (_frameRate == 0) return defaults.FrameRate;
                else return _frameRate; }
            set { SetField(ref _frameRate, value); }
        }
        private int _frameRate;

        public double MaxVectorDeltaPerFrame
        {
            get
            {
                if (_maxVectorDeltaPerFrame == 0) return defaults.MaxVectorDeltaPerFrame;
                else return _maxVectorDeltaPerFrame;
            }
            set { SetField(ref _maxVectorDeltaPerFrame, value); }
        }
        private double _maxVectorDeltaPerFrame;

        public double RubberBandingModifier
        {
            get
            {
                if (_rubberBandingModifier == 0) return defaults.RubberBandingModifier;
                else return _rubberBandingModifier;
            }
            set { SetField(ref _rubberBandingModifier, value); }
        }
        private double _rubberBandingModifier;

        public double TrailingDragCoefficient
        {
            get => _trailingDragCoefficient;
            set { SetField(ref _trailingDragCoefficient, value); }
        }
        private double _trailingDragCoefficient;

        /*public int MaxTrailingSpeed
        {
            get { return _maxTrailingSpeed; }
            set { SetField(ref _maxTrailingSpeed, value); }
        }
        private int _maxTrailingSpeed;*/

        public bool EnableBorderCollision
        {
            get { return _enableBorderCollision; }
            set { SetField(ref _enableBorderCollision, value); }
        }
        private bool _enableBorderCollision;

        public bool EnableBounceOnCollision
        {
            get { return _enableBounceOnCollision; }
            set { SetField(ref _enableBounceOnCollision, value); }
        }
        private bool _enableBounceOnCollision;

        public double BounceMomentumRetention
        {
            get 
            {
                if (_bounceMomentumRetention == 0) return defaults.BounceMomentumRetention;
                else return _bounceMomentumRetention;
            }
            set { SetField(ref _bounceMomentumRetention, value); }
        }
        private double _bounceMomentumRetention;


        // Boilerplate code for INotifyPropertyChanged event
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler SimulationStyleChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public enum TrailingType
    {
        None,
        Vector,
        Classic
    }

    public enum SimulationStyle
    {
        Sun,
        Clouds
    }

    public class PointerPositionUpdatedEvent : EventArgs
    {
        public PointerPositionUpdatedEvent(Windows.Foundation.Point newPosition)
        {
            NewPoint = newPosition;
        }

        public Windows.Foundation.Point NewPoint { get; set; }
    }
}
