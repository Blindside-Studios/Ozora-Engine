using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Devices;
using Windows.System;
using Windows.UI.WebUI;

namespace Ozora
{
    internal class Physics
    {
        private static Timer _timer;
        private int interval = 1000 / Ozora.OzoraSettings.Instance.FrameRate;

        private VectorState _vectorState = new VectorState() { RateOfChange = new Vector3(0, 0, 0) };

        public static Physics Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Physics();
                }
                return _instance;
            }
        }
        private static Physics _instance;

        public bool AnimateActivity
        {
            set
            {
                if (value == true && _animateActivity == false)
                {
                    _animateActivity = true;
                    switch (OzoraSettings.Instance.SimulationStyle)
                    {
                        case SimulationStyle.Sun:
                            _timer = new Timer(AnimateSunObject, null, 0, interval);
                            break;
                        case SimulationStyle.Clouds:
                            _timer = new Timer(AnimateCloudsObject, null, 0, interval);
                            break;
                    }
                }
                else if (value == false && _animateActivity == true)
                {
                    _animateActivity = false;
                    _timer.Dispose();
                }
            }
        }
        private bool _animateActivity;


        private void AnimateSunObject(object state)
        {
            Vector2 cursorPosition = new Vector2((float)OzoraInterface.Instance.PointerLocation.X, (float)OzoraInterface.Instance.PointerLocation.Y);
            Vector2 elementPosition = new Vector2(
                (float)(OzoraInterface.Instance.ObjectTranslation.X + OzoraInterface.Instance.ObjectWidth / 2),
                (float)(OzoraInterface.Instance.ObjectTranslation.Y + OzoraInterface.Instance.ObjectHeight / 2));

            Vector2 direction = cursorPosition - elementPosition;

            direction.X = direction.X * (float)OzoraSettings.Instance.RubberBandingModifier;
            direction.Y = direction.Y * (float)OzoraSettings.Instance.RubberBandingModifier;

            Vector2 _deltaVector = new Vector2(direction.X - _vectorState.RateOfChange.X, direction.Y - _vectorState.RateOfChange.Y);

            if (_deltaVector.Length() > OzoraSettings.Instance.MaxVectorDeltaPerFrame)
            {
                _deltaVector = Vector2.Normalize(_deltaVector) * (float)OzoraSettings.Instance.MaxVectorDeltaPerFrame;
            }

            Vector3 _deltaVector3 = new Vector3(_deltaVector, 0);
            direction = new Vector2(
                _vectorState.RateOfChange.X + _deltaVector.X,
                _vectorState.RateOfChange.Y + _deltaVector.Y);

            _vectorState.RateOfChange = new Vector3(direction, 0);

            Vector3 _finalTranslation = new Vector3(
                (float)(elementPosition.X + direction.X - OzoraInterface.Instance.ObjectWidth / 2),
                (float)(elementPosition.Y + direction.Y - OzoraInterface.Instance.ObjectHeight / 2), 0);

            OzoraInterface.Instance.ObjectTranslation = _finalTranslation;

            // check may be removed as this becomes a non-variable
            if (1000 / Ozora.OzoraSettings.Instance.FrameRate != interval) { AnimateActivity = false; interval = 1000 / Ozora.OzoraSettings.Instance.FrameRate; }
            if (direction.Length() < 0.0001) { AnimateActivity = false; Debug.WriteLine("Animation cancelled"); }
        }

        private void AnimateCloudsObject(object state)
        {
            if (OzoraInterface.Instance.CloudGrid != null && OzoraInterface.Instance.UIDispatcherQueue != null)
            {
                OzoraInterface.Instance.UIDispatcherQueue.TryEnqueue(() =>
                {
                    foreach (UIElement element in OzoraInterface.Instance.CloudGrid.Children)
                    {
                        Vector2 _cloudCoordinate = new Vector2(
                            element.Translation.X + (float)OzoraInterface.Instance.ObjectWidth,
                            element.Translation.Y + (float)OzoraInterface.Instance.ObjectHeight);
                        Vector2 _cursorPosition = OzoraInterface.Instance.PointerLocation.ToVector2();

                        Vector2 _distanceVector = _cloudCoordinate - _cursorPosition;

                        // cloud diameter: 200
                        double _length = _distanceVector.Length();
                        double _opacityCaluclation = _length / 250;
                        if (_opacityCaluclation > 1) _opacityCaluclation = 1;
                        
                        element.Opacity = 1 - _opacityCaluclation;
                    }
                    
                    if (1000 / Ozora.OzoraSettings.Instance.FrameRate != interval) { AnimateActivity = false; interval = 1000 / Ozora.OzoraSettings.Instance.FrameRate; }

                    // if cursor position remains unchanged, stop updating
                    if (_vectorState.PointerPosition == OzoraInterface.Instance.PointerLocation) { AnimateActivity = false; Debug.WriteLine("Animation cancelled"); }
                    _vectorState.PointerPosition = OzoraInterface.Instance.PointerLocation;
                });
            }
        }
    }

    internal class VectorState
    {
        public Vector3 RateOfChange { get; set; }
        public Windows.Foundation.Point PointerPosition { get; set; }
    }
}
