using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ozora
{
    /// <summary>
    /// ParulAI is the internal name for the algorithm that drives the birds' decisionmaking.
    /// Its name is derived from the Tropical Parula, which is a bird.
    /// </summary>
    public class ParulAI
    {
        private Timer _timer;

        public void StartSpawningBirds()
        {
            CurrentBirdSimulation.Instance.Birds = new Bird[] { };
            _timer = new Timer(_spawnBird, null, 0, 20000);
        }

        public void StopSpawningBirds()
        {
            _timer = null;
        }

        private void _spawnBird(object state)
        {
            CurrentBirdSimulation.Instance.UIDispatcherQueue.TryEnqueue(() =>
            {
                Bird _bird = new Bird();
                _bird.BirdSprite = new Image();
                var bitmapImage = new BitmapImage(new Uri("ms-appx:///BirdSprites/Flying1.png"));
                _bird.BirdSprite.Source = bitmapImage;
                _bird.BirdSprite.Stretch = Stretch.UniformToFill;
                _bird.BirdSprite.Height = 128;
                _bird.BirdSprite.Width = 128;
                _bird.BirdSprite.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left;
                _bird.BirdSprite.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Top;

                _bird.State = BirdState.Flying1;
                // TODO: No longer hardcode this value
                _bird.Position = new Vector3(-128, 200, 0);

                CurrentBirdSimulation.Instance.RootGrid.Children.Add(_bird.BirdSprite);
                CurrentBirdSimulation.Instance.Birds.Append(_bird);
                _bird.EngageAI();
            });
        }
    }

    public class Bird: INotifyPropertyChanged
    {
        public BirdState State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged(nameof(State));
                    CurrentBirdSimulation.Instance.UIDispatcherQueue.TryEnqueue(() =>
                    {
                        var bitmapImage = new BitmapImage(new Uri($"ms-appx:///BirdSprites/{value.ToString()}.png"));
                        BirdSprite.Source = bitmapImage;
                    });
                }
            }
        }
        private BirdState _state;
        public Vector3 Position
        {
            get => _position;
            set
            {
                if (value != _position)
                {
                    _position = value;
                    OnPropertyChanged(nameof(Position));
                    CurrentBirdSimulation.Instance.UIDispatcherQueue.TryEnqueue(() =>
                    {
                        BirdSprite.Translation = value;
                    });
                }
            }
        }
        private Vector3 _position;

        public RestingSpot RestingSpot { get; set; }
        public Image BirdSprite { get; set; }
        
        private static Timer _timer;
        
        public void EngageAI()
        {
            _timer = new Timer(_makeDecision, null, 0, 500);
        }

        private void _makeDecision(object state)
        {
            Position = new Vector3(250,250,0);
            State = BirdState.Singing3;
            switch (State)
            {

            }
        }

        public void SnapToRestingSpot()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RestingSpot
    {
        public Vector3 Position { get; set; }
        public RestingSpot SpotToTheLeft { get; set; }
        public RestingSpot SpotToTheRight { get; set; }
        public bool IsOccupied { get; set; }
    }

    public class CurrentBirdSimulation
    {
        private static CurrentBirdSimulation _instance;
        public static CurrentBirdSimulation Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CurrentBirdSimulation();
                }
                return _instance;
            }
        }

        public DispatcherQueue UIDispatcherQueue;
        public Grid RootGrid { get; set; }
        public RestingSpot[] RestingSpots { get; set; }
        public Bird[] Birds { get; set; }
    }

    public enum BirdState
    {
        Flying1,
        Flying2,
        Flying3,
        Flying4,
        Sitting,
        LookingLeft,
        ChattingLeft,
        LookingRight,
        ChattingRight,
        StretchingWings,
        LookingAtWing1,
        LookingAtWing2,
        LookingAtWing3,
        Singing1,
        Singing2,
        Singing3,
        Singing4
    }
}
