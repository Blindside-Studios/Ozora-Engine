using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
            CurrentBirdSimulation.Instance.RefreshCachedSprites();
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
                    if (CurrentBirdSimulation.Instance.UIDispatcherQueue != null)
                    {
                        CurrentBirdSimulation.Instance.UIDispatcherQueue.TryEnqueue(async () =>
                        {
                            /// Clone properties from the original image control in order to mask the flickering that happens on switch.
                            /// But only do this when the bird isn't flying, because the flying animation moves so much it looks weird.
                            bool isBirdStationary = State != BirdState.Flying1 && State != BirdState.Flying2 && State != BirdState.Flying3 && State != BirdState.Flying4;

                            Image _clone = new Image
                            {
                                Source = BirdSprite.Source,
                                Width = BirdSprite.Width,
                                Height = BirdSprite.Height,
                                Stretch = BirdSprite.Stretch,
                                Translation = BirdSprite.Translation,
                                HorizontalAlignment = BirdSprite.HorizontalAlignment,
                                VerticalAlignment = BirdSprite.VerticalAlignment,
                            };

                            if (isBirdStationary)
                            {
                                CurrentBirdSimulation.Instance.RootGrid.Children.Add(_clone);
                                await Task.Delay(1);
                            }

                            /// Load sprites from cache. This should improve performance and reduce flickering on sprite swap.
                            /// Turns out, it still fucking flickers... fuck!
                            BirdSprite.Source = CurrentBirdSimulation.Instance._imageCache[value];

                            // remove cloned image again to only show new image after it has correctly loaded
                            if (isBirdStationary)
                            {
                                await Task.Delay(1);
                                CurrentBirdSimulation.Instance.RootGrid.Children.Remove(_clone);
                            }
                        });
                    }
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
                    if (CurrentBirdSimulation.Instance.UIDispatcherQueue != null)
                    {
                        CurrentBirdSimulation.Instance.UIDispatcherQueue.TryEnqueue(() =>
                        {
                            BirdSprite.Translation = value;
                        });
                    }
                }
            }
        }
        private Vector3 _position;
        public Vector3 TargetPosition { get; set; }

        public RestingSpot RestingSpot { get; set; }
        public RestingSpot TargetedRestingSpot { get; set; }
        public bool IsTargetingLocation { get; set; }
        public bool IsTargetedLocationRestingSpot { get; set; }
        public Image BirdSprite { get; set; }
        
        private static Timer _timer;

        public void EngageAI()
        {
            /// Technically, this would be loading all the sprites for each bird individually.
            /// Not ideal but not the end of the world.
            /// Can optimize later if necessary.
            _timer = new Timer(_makeDecision, null, 0, 500);
        }

        private void _makeDecision(object state)
        {
            if (IsTargetingLocation != true && IsTargetedLocationRestingSpot != true && TargetedRestingSpot == null)
            {
                /// You may ask why this has so many conditions, well, let me explain:
                /// IsTargetingLocation != null: This code should only run when the bird is not currently targeting anything
                /// IsTargetedLocationRestingSpot != null: If the bird isn't currently targeting something but this is set to true, it means that the bird is sitting
                /// RestingSpot != null: Additional check that this code should only run while the bird isn't sitting.
                /// Note that null states also allow this to be run.
                /// In summary, this code will only run once to find a suitable first target.
                Random rnd = new Random();
                if (CurrentBirdSimulation.Instance.RestingSpots.Count() > 0)
                {
                    int _restingIndex = rnd.Next(0, CurrentBirdSimulation.Instance.RestingSpots.Count() - 1);
                    RestingSpot _spot = CurrentBirdSimulation.Instance.RestingSpots[_restingIndex];
                    if (_spot.IsOccupied != true) { TargetPosition = _spot.Position; TargetedRestingSpot = _spot; _spot.IsOccupied = true; IsTargetedLocationRestingSpot = true; }
                    // 10000 is the expected maximum width, may update as needed. Curerntly, this is overkill, if it performs badly, this can be reduced, obviously.
                    else
                    {
                        TargetPosition = new Vector3(10000, (float)rnd.Next(20, 200), 0);
                        IsTargetingLocation = true;
                        IsTargetedLocationRestingSpot = false;
                    }
                }
                else this.Kill();
                
            }
            else
            {
                if (State == BirdState.Flying1 || State == BirdState.Flying2 || State == BirdState.Flying3 || State == BirdState.Flying4)
                {
                    if (Vector3.Distance(Position, TargetPosition) < 2f)
                    {
                        // if close enough, snap to resting spot
                        Position = TargetPosition;
                        RestingSpot = TargetedRestingSpot;
                        State = BirdState.Sitting;
                        
                        // if targeted position wasn't a resting spot but the bird went off the screen, dispose of the bird
                        if (!IsTargetedLocationRestingSpot)
                        {
                            CurrentBirdSimulation.Instance.UIDispatcherQueue.TryEnqueue(() =>
                            {
                                CurrentBirdSimulation.Instance.RootGrid.Children.Remove(BirdSprite);
                            });
                            var list = CurrentBirdSimulation.Instance.Birds.ToList();
                            list.Remove(this);
                            CurrentBirdSimulation.Instance.Birds = list.ToArray<Bird>();
                        }
                        return;
                    }
                    else
                    {
                        // if not close to destination, continue moving
                        Vector3 _movement = new Vector3(TargetPosition.X - Position.X, TargetPosition.Y - Position.Y, 0);
                        if (_movement.Length() > 40) _movement = Vector3.Normalize(_movement) * 40;
                        Position = Position + _movement;
                    }
                }

                Random rnd = new Random();
                switch (State)
                {
                    case BirdState.Flying1:
                        State = BirdState.Flying2;
                        break;
                    case BirdState.Flying2:
                        State = BirdState.Flying3;
                        break;
                    case BirdState.Flying3:
                        State = BirdState.Flying4;
                        break;
                    case BirdState.Flying4:
                        State = BirdState.Flying1;
                        break;
                    case BirdState.Sitting:
                        int nextAnimationIndex = rnd.Next(0, 50);
                        if (nextAnimationIndex < 41) State = BirdState.Sitting;
                        else if (nextAnimationIndex < 43) State = BirdState.LookingLeft;
                        else if (nextAnimationIndex < 45) State = BirdState.LookingRight;
                        else if (nextAnimationIndex < 47) State = BirdState.ChattingLeft;
                        else if (nextAnimationIndex < 48) State = BirdState.ChattingRight;
                        else if (nextAnimationIndex < 49) State = BirdState.Singing1;
                        else if (nextAnimationIndex < 50) State = BirdState.LookingAtWing1;
                        // just have it fly off the screen in this case
                        else if (nextAnimationIndex == 50) { State = BirdState.Flying1; TargetPosition = new Vector3(5000, 500, 0); IsTargetedLocationRestingSpot = false; }
                        break;
                    case BirdState.LookingLeft:
                        var _nextBehaviorLookingLeft = rnd.NextDouble();
                        if (_nextBehaviorLookingLeft < 0.75) State = BirdState.LookingLeft;
                        else if (_nextBehaviorLookingLeft < 0.80) State = BirdState.LookingAtWing1; // in both sprites, bird is looking to left
                        else if (_nextBehaviorLookingLeft < 0.85) State = BirdState.LookingRight;
                        else if (_nextBehaviorLookingLeft < 0.95) State = BirdState.ChattingLeft;
                        else State = BirdState.Sitting;
                        break;
                    case BirdState.LookingRight:
                        var _nextBehaviorLookingRight = rnd.NextDouble();
                        if (_nextBehaviorLookingRight < 0.75) State = BirdState.LookingRight;
                        else if (_nextBehaviorLookingRight < 0.80) State = BirdState.Singing1; // in both sprites, bird is looking to right
                        else if (_nextBehaviorLookingRight < 0.85) State = BirdState.LookingLeft;
                        else if (_nextBehaviorLookingRight < 0.95) State = BirdState.ChattingRight;
                        else State = BirdState.Sitting;
                        break;
                    case BirdState.ChattingLeft:
                        var _nextBehaviorChattingLeft = rnd.NextDouble();
                        if (_nextBehaviorChattingLeft < 0.9) State = BirdState.LookingLeft;
                        else State = BirdState.Sitting;
                        break;
                    case BirdState.ChattingRight:
                        var _nextBehaviorChattingRight = rnd.NextDouble();
                        if (_nextBehaviorChattingRight < 0.9) State = BirdState.LookingRight;
                        else State = BirdState.Sitting;
                        break;
                    case BirdState.StretchingWings:
                        var _nextBehaviorStretchingWings = rnd.NextDouble();
                        if (_nextBehaviorStretchingWings < 0.3) State = BirdState.LookingAtWing1;
                        // just have it fly off the screen in this case
                        else if (_nextBehaviorStretchingWings < 0.4) { State = BirdState.Flying1; TargetPosition = new Vector3(5000, 500, 0); IsTargetedLocationRestingSpot = false; }
                        else State = BirdState.Sitting;
                        break;
                    case BirdState.LookingAtWing1:
                        var _nextBehaviorLookingAtWing1 = rnd.NextDouble();
                        if (_nextBehaviorLookingAtWing1 < 0.2) State = BirdState.LookingLeft;
                        else State = BirdState.LookingAtWing2;
                        break;
                    case BirdState.LookingAtWing2:
                        var _nextBehaviorLookingAtWing2 = rnd.NextDouble();
                        if (_nextBehaviorLookingAtWing2 < 0.2) State = BirdState.LookingAtWing2;
                        else if (_nextBehaviorLookingAtWing2 < 0.6) State = BirdState.LookingAtWing1;
                        else if (_nextBehaviorLookingAtWing2 < 0.75) State = BirdState.LookingLeft;
                        else State = BirdState.LookingAtWing3;
                        break;
                    case BirdState.LookingAtWing3:
                        var _nextBehaviorLookingAtWing3 = rnd.NextDouble();
                        if (_nextBehaviorLookingAtWing3 < 0.7) State = BirdState.LookingRight;
                        else if (_nextBehaviorLookingAtWing3 < 9) State = BirdState.ChattingRight;
                        else if (_nextBehaviorLookingAtWing3 < 0.9) State = BirdState.StretchingWings;
                        else State = BirdState.Sitting;
                        break;
                    case BirdState.Singing1:
                        State = BirdState.Singing2;
                        break;
                    case BirdState.Singing2:
                        State = BirdState.Singing3;
                        break;
                    case BirdState.Singing3:
                        var _nextBehaviorSinging3 = rnd.NextDouble();
                        if (_nextBehaviorSinging3 < 0.4) State = BirdState.Singing3;
                        else State = BirdState.Singing4;
                        break;
                    case BirdState.Singing4:
                        var _nextBehaviorSinging4 = rnd.NextDouble();
                        if (_nextBehaviorSinging4 < 0.4) State = BirdState.LookingRight;
                        if (_nextBehaviorSinging4 < 0.6) State = BirdState.ChattingRight;
                        else State = BirdState.Sitting;
                        break;
                }
            }
        }

        public void Kill()
        {
            CurrentBirdSimulation.Instance.UIDispatcherQueue.TryEnqueue(() =>
            {
                CurrentBirdSimulation.Instance.RootGrid.Children.Remove(BirdSprite);
            });
            var list = CurrentBirdSimulation.Instance.Birds.ToList();
            list.Remove(this);
            CurrentBirdSimulation.Instance.Birds = list.ToArray<Bird>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RestingSpot
    {
        public string Identifier { get; set; }
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

        public void CleanUp()
        {
            CurrentBirdSimulation.Instance.RootGrid = null;
            CurrentBirdSimulation.Instance.UIDispatcherQueue = null;
            CurrentBirdSimulation.Instance.Birds = null;
            CurrentBirdSimulation.Instance.RestingSpots = null;
        }

        public DispatcherQueue UIDispatcherQueue;
        public Grid RootGrid { get; set; }
        public RestingSpot[] RestingSpots { get; set; }
        public Bird[] Birds { get; set; }

        internal Dictionary<BirdState, BitmapImage> _imageCache = new Dictionary<BirdState, BitmapImage>();
        public void RefreshCachedSprites()
        {
            foreach (BirdState state in Enum.GetValues(typeof(BirdState)))
            {
                var bitmapImage = new BitmapImage(new Uri($"ms-appx:///BirdSprites/{state.ToString()}.png"));
                _imageCache[state] = bitmapImage;
            }
        }
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
