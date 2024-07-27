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
            _timer = new Timer(_spawnBird, null, 0, 20000);
        }

        public void StopSpawningBirds()
        {
            _timer = null;
        }

        private void _spawnBird(object state)
        {
            CurrentSimulation.Instance.UIDispatcherQueue.TryEnqueue(() =>
            {
                Bird _bird = new Bird();
                _bird.BirdSprite = new Image();
                var bitmapImage = new BitmapImage(new Uri("ms-appx:///BirdSprites/Flying1.png"));
                _bird.BirdSprite.Source = bitmapImage;
                _bird.BirdSprite.Stretch = Stretch.UniformToFill;
                _bird.BirdSprite.Height = 128;
                _bird.BirdSprite.Width = 128;

                CurrentSimulation.Instance.RootGrid.Children.Add(_bird.BirdSprite);
                _bird.EngageAI();
            });
        }
    }

    public class Bird: INotifyPropertyChanged
    {
        public BirdState State { get; set; }
        public Vector3 Position { get; set; }
        public RestingSpot RestingSpot { get; set; }
        public Image BirdSprite { get; set; }
        
        private static Timer _timer;
        
        public void EngageAI()
        {
            _timer = new Timer(_makeDecision, null, 0, 500);
        }

        private void _makeDecision(object state)
        {
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

    public class CurrentSimulation
    {
        private static CurrentSimulation _instance;
        public static CurrentSimulation Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CurrentSimulation();
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
