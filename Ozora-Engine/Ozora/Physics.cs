using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
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
    public class Physics
    {
        public event EventHandler<ObjectPositionUpdatedEvent> ObjectPositionCalculated;
        private void VectorUpdated(Vector3 newVector) { ObjectPositionCalculated?.Invoke(this, new ObjectPositionUpdatedEvent(newVector)); }

        public void InterruptSimulation()
        {
            AnimateActivity = false;
        }

        public Windows.Foundation.Point CursorPosition
        {
            get => _cursorPosition;
            set
            {
                _cursorPosition = value;
                AnimateActivity = true;
            }
        }
        private Windows.Foundation.Point _cursorPosition;

        public OzoraInterface Interface { get; set; }

        public void StartSimulation()
        {
            if (Interface != null) Interface.PointerPositionUpdated += Interface_PointerPositionUpdated;
        }

        public bool MouseCursorEngaged = true;

        private void Interface_PointerPositionUpdated(object sender, PointerPositionUpdatedEvent e)
        {
            CursorPosition = Interface.PointerLocation;
        }

        private static Timer _timer;
        private int _lastFrameRate;

        private VectorState _vectorState = new VectorState() { RateOfChange = new Vector3(0, 0, 0) };


        private bool AnimateActivity
        {
            set
            {
                /// This may be continuously set on each pointer update, therefore check for
                /// !_animateActivity, to not restart the same timer over and over again.
                /// Also, this is only necessary if the mouse cursor is engaged - however,
                /// this will still be run if the mouse is disengaged but a new PointerPosition
                /// is recognized, which would be pointless. Therefore, we are not doing it.
                if (value == true && MouseCursorEngaged && !_animateActivity && Interface != null)
                {
                    _animateActivity = true;
                    _lastFrameRate = Interface.Settings.FrameRate;
                    switch (Interface.Settings.SimulationStyle)
                    {
                        case SimulationStyle.Sun:
                            _timer = new Timer(AnimateSunObject, null, 0, 1000 / Interface.Settings.FrameRate);
                            break;
                        case SimulationStyle.Clouds:
                            _timer = new Timer(AnimateCloudsObject, null, 0, 1000 / Interface.Settings.FrameRate);
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
            Vector2 cursorPosition = new Vector2((float)CursorPosition.X, (float)CursorPosition.Y);
            Vector2 elementPosition = new Vector2(
                _vectorState.LastTranslation.X + Interface.ObjectWidth / 2,
                _vectorState.LastTranslation.Y + Interface.ObjectHeight / 2);

            Vector2 direction;

            if (MouseCursorEngaged)
            {
                // Object chases the mouse cursor
                direction = cursorPosition - elementPosition;
                direction.X = direction.X * Interface.Settings.RubberBandingModifier;
                direction.Y = direction.Y * Interface.Settings.RubberBandingModifier;

                Vector2 _deltaVector = new Vector2(direction.X - _vectorState.RateOfChange.X, direction.Y - _vectorState.RateOfChange.Y);

                if (_deltaVector.Length() > Interface.Settings.MaxVectorDeltaPerFrame)
                {
                    _deltaVector = Vector2.Normalize(_deltaVector) * Interface.Settings.MaxVectorDeltaPerFrame;
                }

                direction = new Vector2(
                    _vectorState.RateOfChange.X + _deltaVector.X,
                    _vectorState.RateOfChange.Y + _deltaVector.Y);
            }
            else
            {
                // Object uses its own kinetic energy for trailing
                direction = new Vector2(_vectorState.RateOfChange.X, _vectorState.RateOfChange.Y);
                if (Interface.Settings.TrailingDragCoefficient > 0)
                {
                    direction.X = direction.X * (1 - Interface.Settings.TrailingDragCoefficient);
                    direction.Y = direction.Y * (1 - Interface.Settings.TrailingDragCoefficient);
                }
            }

            Vector3 _finalTranslation = new Vector3(
                elementPosition.X + direction.X - Interface.ObjectWidth / 2,
                elementPosition.Y + direction.Y - Interface.ObjectHeight / 2, 0);


            // Collision detection
            if (Interface.Settings.EnableBorderCollision)
            {
                if (_finalTranslation.X < 0)
                {
                    // Left side collision
                    _finalTranslation.X = 0;
                    if (!Interface.Settings.EnableBounceOnCollision) direction.X = 0;
                    else direction.X = (-direction.X) * Interface.Settings.BounceMomentumRetention;
                }
                else if (_finalTranslation.X > (Interface.AreaDimensions.X - Interface.ObjectWidth))
                {
                    // Right side collision
                    _finalTranslation.X = (float)Interface.AreaDimensions.X - Interface.ObjectWidth;
                    if (!Interface.Settings.EnableBounceOnCollision) direction.X = 0;
                    else direction.X = (-direction.X) * Interface.Settings.BounceMomentumRetention;
                }
                if (_finalTranslation.Y < 0)
                {
                    // Top side collision
                    _finalTranslation.Y = 0;
                    if (!Interface.Settings.EnableBounceOnCollision) direction.Y = 0;
                    else direction.Y = (-direction.Y) * Interface.Settings.BounceMomentumRetention;
                }
                else if (_finalTranslation.Y > (Interface.AreaDimensions.Y - Interface.ObjectHeight))
                {
                    // Bottom side collision
                    _finalTranslation.Y = (float)Interface.AreaDimensions.Y - Interface.ObjectHeight;
                    if (!Interface.Settings.EnableBounceOnCollision) direction.Y = 0;
                    else direction.Y = (-direction.Y) * Interface.Settings.BounceMomentumRetention;
                }
            }

            // check if target FrameRate was updated
            if (Interface.Settings.FrameRate != _lastFrameRate) { AnimateActivity = false; }


            // check movement delta to cancel animation if it got too small
            Vector3 _movedDistance = _finalTranslation - _vectorState.LastTranslation;
            if (_movedDistance.Length() < 0.0001) { AnimateActivity = false; Debug.WriteLine("Animation cancelled"); }

            _vectorState.RateOfChange = new Vector3(direction, 0);
            _vectorState.LastTranslation = _finalTranslation;
            VectorUpdated(_finalTranslation);
        }

        private void AnimateCloudsObject(object state)
        {
            if (Interface.CloudGrid != null && Interface.UIDispatcherQueue != null)
            {
                Interface.UIDispatcherQueue.TryEnqueue(() =>
                {
                    foreach (UIElement element in Interface.CloudGrid.Children)
                    {
                        Vector2 _cloudCoordinate = new Vector2(
                            element.Translation.X + (float)Interface.ObjectWidth,
                            element.Translation.Y + (float)Interface.ObjectHeight);
                        Vector2 _cursorPosition = Interface.PointerLocation.ToVector2();

                        Vector2 _distanceVector = _cloudCoordinate - _cursorPosition;

                        // cloud diameter: 200
                        double _length = _distanceVector.Length();
                        double _opacityCaluclation = _length / 250;
                        if (_opacityCaluclation > 1) _opacityCaluclation = 1;
                        
                        element.Opacity = 1 - _opacityCaluclation;
                    }
                    
                    // if cursor position remains unchanged, stop updating
                    if (_vectorState.PointerPosition == CursorPosition) { AnimateActivity = false; Debug.WriteLine("Animation cancelled"); }
                    _vectorState.PointerPosition = CursorPosition;
                });
            }
        }
    }

    internal class VectorState
    {
        public Vector3 RateOfChange { get; set; }
        public Vector3 LastTranslation { get; set; }
        public Windows.Foundation.Point PointerPosition { get; set; }
    }

    public class ObjectPositionUpdatedEvent : EventArgs
    {
        public ObjectPositionUpdatedEvent(Vector3 newVector)
        {
            NewTranslationVector = newVector;
        }

        public Vector3 NewTranslationVector { get; set; }
    }
}
