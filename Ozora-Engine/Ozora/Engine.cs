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

namespace Ozora
{
    public class OzoraEngine: INotifyPropertyChanged
    {
        public static OzoraEngine Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new OzoraEngine();
                }
                return _instance;
            }
        }
        private static OzoraEngine _instance;

        public OzoraSettings Settings
        {
            get { return _settings; }
            set { SetField(ref _settings, value); }
        }
        private OzoraSettings _settings;

        // Boilerplate code for INotifyPropertyChanged event
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

    public class OzoraInterface: INotifyPropertyChanged
    {
        public static OzoraInterface Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new OzoraInterface();
                }
                return _instance;
            }
        }
        private static OzoraInterface _instance;

        public Windows.Foundation.Point PointerLocation 
        {
            get => _pointerLocation;
            set
            {
                if (_pointerLocation != value)
                {
                    _pointerLocation = value;
                    OnPropertyChanged(nameof(PointerLocation));
                    Ozora.Physics.Instance.AnimateActivity = true;
                }
            }
        }
        private Windows.Foundation.Point _pointerLocation;

        public double ObjectWidth { get; set; }
        public double ObjectHeight { get; set; }

        public event Action<Vector3> UpdateObjectTranslationRequested;
        public Vector3 ObjectTranslation
        {
            get => _objectTranslation;
            set
            {
                _objectTranslation = value;
                UpdateObjectTranslationRequested?.Invoke(value);
            }
        }
        private Vector3 _objectTranslation;

        public double ObjectRotation
        {
            get => _objectRotation;
            set
            {
                _objectRotation = value;
                OnPropertyChanged(nameof(ObjectRotation));
            }
        }
        private double _objectRotation;


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class OzoraSettings: INotifyPropertyChanged
    {
        Ozora.DefaultValues defaults = new Ozora.DefaultValues();

        public static OzoraSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new OzoraSettings();
                }
                return _instance;
            }
        }
        private static OzoraSettings _instance;

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

        public int MaxTrailingSpeed
        {
            get { return _maxTrailingSpeed; }
            set { SetField(ref _maxTrailingSpeed, value); }
        }
        private int _maxTrailingSpeed;


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
}
