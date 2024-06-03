using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ozora
{
    internal class Physics
    {
        private static Timer _timer;
        private int interval = 1000 / Ozora.OzoraSettings.Instance.FrameRate;

        private VectorState _vectorState;

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

        public bool AnimateActivity { 
            set
            {
                if (value == true && _animateActivity == false)
                {
                    _animateActivity = true;
                    _timer = new Timer(AnimateObject, null, 0, interval);
                }
                else if (value == false && _animateActivity == true)
                {
                    _animateActivity = false;
                }
            } 
        }
        private bool _animateActivity;


        private void AnimateObject(object state)
        {
            Vector2 cursorPosition = new Vector2((float)OzoraInterface.Instance.PointerLocation.X, (float)OzoraInterface.Instance.PointerLocation.Y);
            Vector2 elementPosition = new Vector2(
                (float)(OzoraInterface.Instance.ObjectTranslation.X + OzoraInterface.Instance.ObjectWidth / 2),
                (float)(OzoraInterface.Instance.ObjectTranslation.Y + OzoraInterface.Instance.ObjectHeight / 2));

            Vector2 direction = cursorPosition - elementPosition;

            direction.X = direction.X / 20;
            direction.Y = direction.Y / 20;

            Vector3 _finalTranslation = new Vector3(
                (float)(elementPosition.X + direction.X - OzoraInterface.Instance.ObjectWidth / 2), 
                (float)(elementPosition.Y + direction.Y - OzoraInterface.Instance.ObjectHeight / 2), 0);

            OzoraInterface.Instance.ObjectTranslation = _finalTranslation;

            // check may be removed as this becomes a non-variable
            if (1000 / Ozora.OzoraSettings.Instance.FrameRate != interval) _timer = new Timer(AnimateObject, null, 0, interval);
        }
    }

    internal class VectorState
    {
        public Vector3 RateOfChange { get; set; }
        public double VectorAngle { get; set; }
        public double VectorLength { get; set; }
    }
}
