﻿using Microsoft.UI.Dispatching;
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
        public Vector3 TargetPosition { get; set; }

        public RestingSpot RestingSpot { get; set; }
        public RestingSpot TargetedRestingSpot { get; set; }
        public bool IsTargetingLocation { get; set; }
        public bool IsTargetedLocationRestingSpot { get; set; }
        public Image BirdSprite { get; set; }
        
        private static Timer _timer;
        
        public void EngageAI()
        {
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
                Debug.WriteLine(CurrentBirdSimulation.Instance.RestingSpots.Count());
                if (CurrentBirdSimulation.Instance.RestingSpots.Count() > 0)
                {
                    foreach (RestingSpot spot in CurrentBirdSimulation.Instance.RestingSpots) { Debug.WriteLine(spot.Position); }

                    int _restingIndex = rnd.Next(0, CurrentBirdSimulation.Instance.RestingSpots.Count() - 1);
                    RestingSpot _spot = CurrentBirdSimulation.Instance.RestingSpots[_restingIndex];
                    if (_spot.IsOccupied != true) { TargetPosition = _spot.Position; TargetedRestingSpot = _spot; _spot.IsOccupied = true; IsTargetedLocationRestingSpot = true; }
                    // TODO: What the fuck is this hardcoded number, fix this, oh my goooood!
                    else
                    {
                        TargetPosition = new Vector3(500, 500, 0);
                        IsTargetedLocationRestingSpot = false;
                    }
                }
                
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
                }
            }
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
