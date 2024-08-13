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
using Windows.Media.Playback;

namespace Ozora
{
    /// <summary>
    /// ParulAI is the internal name for the algorithm that drives the birds' decisionmaking.
    /// Its name is derived from the Tropical Parula, which is a bird.
    /// </summary>
    
    public class ParulAI
    {
        private Timer _timer;
        private int _numberOfBirds = 0;

        public void StartSpawningBirds()
        {
            CurrentBirdSimulation.Instance.RefreshCachedSprites();
            CurrentBirdSimulation.Instance.Birds = new Bird[] { };
            _timer = new Timer(_spawnBird, null, 0, 10000);
        }

        public void StopSpawningBirds()
        {
            _timer = null;
        }

        private void _spawnBird(object state)
        {
            CurrentBirdSimulation.Instance.UIDispatcherQueue.TryEnqueue(() =>
            {
                Random rnd = new Random();
                
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
                _bird.Position = new Vector3(-128, rnd.Next(0,400), 0);

                CurrentBirdSimulation.Instance.RootGrid.Children.Add(_bird.BirdSprite);
                CurrentBirdSimulation.Instance.Birds.Append(_bird);
                _bird.BirdID = _numberOfBirds;
                _bird.Personality = new BirdPersonality()
                {
                    Cleanliness = rnd.Next(2, 10),
                    Energy = rnd.Next(2, 10),
                    Hectic = rnd.Next(2, 10),
                    Musical = rnd.Next(2, 10),
                    Social = rnd.Next(2, 10)
                };
                _bird.EngageAI();
                _numberOfBirds++;
            });
        }
    }

    public class Bird: INotifyPropertyChanged
    {
        public int BirdID { get; set; }
        
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
        public BirdPersonality Personality { get; set; }
        public Vector3 TargetPosition { get; set; }

        public RestingSpot RestingSpot { get; set; }
        public RestingSpot TargetedRestingSpot { get; set; }
        public bool IsTargetingLocation { get; set; }
        public bool IsTargetedLocationRestingSpot { get; set; }
        public Image BirdSprite { get; set; }
        private bool _overriddenBehavior;
        private int _sittingCooldown = 20;
        private Timer _birdDecisionTimer;

        public void EngageAI()
        {
            _birdDecisionTimer = new Timer(_makeDecision, null, 0, 500);
        }

        private void _makeDecision(object state)
        {
            if (!_overriddenBehavior)
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
                        List<RestingSpot> _freeRestingSpots = CurrentBirdSimulation.Instance.RestingSpots.Where(x => !x.IsOccupied).ToList<RestingSpot>();
                        if (_freeRestingSpots.Count() > 0)
                        {
                            int index = rnd.Next(0, _freeRestingSpots.Count() - 1);
                            RestingSpot _spot = _freeRestingSpots[index];
                            TargetPosition = _spot.Position;
                            TargetedRestingSpot = _spot;
                            _spot.IsOccupied = true;
                            IsTargetedLocationRestingSpot = true;
                        }
                        else
                        {
                            // 10000 is the expected maximum width, may update as needed. Curerntly, this is overkill, if it performs badly, this can be reduced, obviously.
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
                            RestingSpot.OccupyingBird = this;
                            this._interactWithNeighbor();

                            // if targeted position wasn't a resting spot but the bird went off the screen, dispose of the bird
                            if (!IsTargetedLocationRestingSpot)
                            {
                                this.Kill();
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
                    else
                    {
                        _sittingCooldown--; // count down sitting cooldown to enable flying for bird again after it has been sitting down
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
                            int baseRestingLikelihood = 40; // Base likelihood for resting
                            int totalWeight = baseRestingLikelihood + Personality.Musical + Personality.Social + Personality.Hectic + Personality.Cleanliness;
                            if (_sittingCooldown < 1) totalWeight += this.Personality.Energy; // only allow bird to fly from sitting position after cooldown
                            int randomValue = rnd.Next(0, totalWeight);
                            if (randomValue < baseRestingLikelihood)
                            {
                                State = BirdState.Sitting;
                            }
                            else if (randomValue < baseRestingLikelihood + Personality.Musical) // Singing
                            {
                                State = BirdState.Singing1;
                            }
                            else if (randomValue < baseRestingLikelihood + Personality.Musical + Personality.Social) // Social
                            {
                                if (rnd.NextDouble() < 0.5) State = BirdState.ChattingLeft;
                                else State = BirdState.ChattingRight;
                            }
                            else if (randomValue < baseRestingLikelihood + Personality.Musical + Personality.Social + Personality.Hectic) // Looking around / Hectic
                            {
                                if (rnd.NextDouble() < 0.5) State = BirdState.LookingLeft;
                                else State = BirdState.LookingRight;
                            }
                            else if (randomValue < baseRestingLikelihood + Personality.Musical + Personality.Social + Personality.Hectic + Personality.Cleanliness) // Cleanliness
                            {
                                State = BirdState.LookingAtWing1;
                            }
                            else // Energy
                            {
                                State = BirdState.Flying1;
                                TargetPosition = new Vector3(5000, 500, 0);
                                IsTargetedLocationRestingSpot = false;
                                this.RestingSpot.OccupyingBird = null;
                                this.RestingSpot.IsOccupied = false;
                                this.RestingSpot = null;
                            }
                            break;
                        case BirdState.LookingLeft:
                            int baseLookingLeftLikelihood = 40; // Base likelihood for resting
                            int totalLookingLeftWeight = baseLookingLeftLikelihood + Personality.Social + Personality.Hectic + Personality.Cleanliness + Personality.Energy;
                            int randomLookingLeftValue = rnd.Next(0, totalLookingLeftWeight);
                            if (randomLookingLeftValue < baseLookingLeftLikelihood)
                            {
                                State = BirdState.LookingLeft;
                            }
                            else if (randomLookingLeftValue < baseLookingLeftLikelihood + Personality.Social) // Social
                            {
                                State = BirdState.ChattingLeft;
                            }
                            else if (randomLookingLeftValue < baseLookingLeftLikelihood + Personality.Social + Personality.Hectic) // Looking around / Hectic
                            {
                                State = BirdState.LookingRight;
                            }
                            else if (randomLookingLeftValue < baseLookingLeftLikelihood + Personality.Social + Personality.Hectic + Personality.Cleanliness) // Cleanliness
                            {
                                State = BirdState.LookingAtWing1;
                            }
                            else // When high energy, more likely to look forward again
                            {
                                State = BirdState.Sitting;
                            }
                            break;
                        case BirdState.LookingRight:
                            int baseLookingRightLikelihood = 40; // Base likelihood for resting
                            int totalLookingRightWeight = baseLookingRightLikelihood + Personality.Social + Personality.Hectic + Personality.Musical + Personality.Energy;
                            int randomLookingRightValue = rnd.Next(0, totalLookingRightWeight);
                            if (randomLookingRightValue < baseLookingRightLikelihood)
                            {
                                State = BirdState.LookingRight;
                            }
                            else if (randomLookingRightValue < baseLookingRightLikelihood + Personality.Social) // Social
                            {
                                State = BirdState.ChattingRight;
                            }
                            else if (randomLookingRightValue < baseLookingRightLikelihood + Personality.Social + Personality.Hectic) // Looking around / Hectic
                            {
                                State = BirdState.LookingLeft;
                            }
                            else if (randomLookingRightValue < baseLookingRightLikelihood + Personality.Social + Personality.Hectic + Personality.Musical) // Musical
                            {
                                State = BirdState.Singing1;
                            }
                            else // When high energy, more likely to look forward again
                            {
                                State = BirdState.Sitting;
                            }
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
                            if (_nextBehaviorStretchingWings * this.Personality.Cleanliness > 4) State = BirdState.LookingAtWing1;
                            // just have it fly off the screen in this case
                            else if (_nextBehaviorStretchingWings * this.Personality.Energy > 4)
                            {
                                /// If the bird arrives here, it has stretched its wings before and likely also looked at its wings.
                                /// Therefore, it is not necessary to add a cooldown check here. Let's just say stretching wings magically
                                /// refills a bird's will to fly!
                                State = BirdState.Flying1;
                                TargetPosition = new Vector3(5000, 500, 0);
                                IsTargetedLocationRestingSpot = false;
                                this.RestingSpot.OccupyingBird = null;
                                this.RestingSpot.IsOccupied = false;
                                this.RestingSpot = null;
                            }
                            else State = BirdState.Sitting;
                            break;
                        case BirdState.LookingAtWing1:
                            var _nextBehaviorLookingAtWing1 = rnd.NextDouble();
                            if (_nextBehaviorLookingAtWing1 * this.Personality.Hectic > 4) State = BirdState.LookingLeft;
                            else State = BirdState.LookingAtWing2;
                            break;
                        case BirdState.LookingAtWing2:
                            var _nextBehaviorLookingAtWing2 = rnd.NextDouble();
                            if (_nextBehaviorLookingAtWing2 * this.Personality.Hectic > 6) State = BirdState.LookingLeft;
                            else
                            {
                                if (_nextBehaviorLookingAtWing2 * this.Personality.Energy < 5) State = BirdState.LookingAtWing3;
                                else State = BirdState.LookingAtWing1; // perform action again if a lot of energy is present
                            }
                            break;
                        case BirdState.LookingAtWing3:
                            var _totalBehaviorSumWingCheck3 = this.Personality.Hectic + this.Personality.Social + this.Personality.Energy;
                            var _behaviorIndexWingCheck3 = rnd.NextDouble() * _totalBehaviorSumWingCheck3;
                            if (_behaviorIndexWingCheck3 < this.Personality.Hectic) State = BirdState.LookingRight;
                            else if (_behaviorIndexWingCheck3 < this.Personality.Hectic + this.Personality.Social) State = BirdState.ChattingRight;
                            else
                            {
                                if (rnd.NextDouble() < 0.5) State = BirdState.StretchingWings;
                                else State = BirdState.Sitting;
                            }
                            break;
                        case BirdState.Singing1:
                            State = BirdState.Singing2;
                            break;
                        case BirdState.Singing2:
                            State = BirdState.Singing3;
                            break;
                        case BirdState.Singing3:
                            State = BirdState.Singing4;
                            break;
                        case BirdState.Singing4:
                            var _totalBehaviorSumSing4 = 10 + this.Personality.Hectic + Personality.Social;
                            var _behaviorIndexSinging4 = rnd.Next() * _totalBehaviorSumSing4;
                            if (_behaviorIndexSinging4 < 10) State = BirdState.Sitting;
                            else if (_behaviorIndexSinging4 < 10 + this.Personality.Hectic) State = BirdState.LookingRight;
                            else State = BirdState.ChattingRight;
                            break;
                    }
                }
            }
        }

        public void Kill()
        {
            this.RestingSpot.OccupyingBird = null;
            this.RestingSpot.IsOccupied = false;
            this.RestingSpot = null;
            CurrentBirdSimulation.Instance.UIDispatcherQueue.TryEnqueue(() =>
            {
                CurrentBirdSimulation.Instance.RootGrid.Children.Remove(BirdSprite);
            });
            var list = CurrentBirdSimulation.Instance.Birds.ToList();
            list.Remove(this);
            CurrentBirdSimulation.Instance.Birds = list.ToArray<Bird>();
        }

        private async void _interactWithNeighbor()
        {
            Random rnd = new Random();
            // Checking for neighbors and making an appropriate program
            bool hasNeighborLeft = false;
            bool hasNeighborRight = false;
            NeighborhoodType neighborhoodType = NeighborhoodType.None;

            /// NOTE: Do not use the IsOccupied variable here as it is set to true when a bird approaches!
            /// NOTE: This means there is not necesarily a bird there. 
            /// NOTE: This would be an edge case but let's rule it out!
            /// NOTE: Check if _overriddenBehavior is not true, rather than if it's false: null is to be treated as false.
            if (this.RestingSpot.SpotToTheRight != null &&
                this.RestingSpot.SpotToTheRight.OccupyingBird != null &&
                this.RestingSpot.SpotToTheRight.OccupyingBird._overriddenBehavior != true) hasNeighborRight = true;
            if (this.RestingSpot.SpotToTheLeft != null &&
                this.RestingSpot.SpotToTheLeft.OccupyingBird != null &&
                this.RestingSpot.SpotToTheLeft.OccupyingBird._overriddenBehavior != true) hasNeighborLeft = true;

            if      ( hasNeighborLeft && !hasNeighborRight) neighborhoodType = NeighborhoodType.OnlyLeft;
            else if (!hasNeighborLeft &&  hasNeighborRight) neighborhoodType = NeighborhoodType.OnlyRight;
            else if ( hasNeighborLeft &&  hasNeighborRight) neighborhoodType = NeighborhoodType.LeftAndRight;

            switch (neighborhoodType)
            {
                case NeighborhoodType.None:
                    // no neighbors, do nothing
                    break;


                case NeighborhoodType.OnlyLeft:
                    this._overriddenBehavior = true;
                    this.RestingSpot.SpotToTheLeft.OccupyingBird._overriddenBehavior = true;
                    var _leftBird = this.RestingSpot.SpotToTheLeft.OccupyingBird;
                    // create interactions between both birds here
                    switch (rnd.Next(0, 2))
                    {
                        case 0:
                            this.State = BirdState.Sitting;
                            _leftBird.State = BirdState.Sitting;
                            await Task.Delay(500);
                            _leftBird.State = BirdState.Singing1;
                            await Task.Delay(500);
                            _leftBird.State = BirdState.Singing2;
                            this.State = BirdState.LookingLeft;
                            await Task.Delay(500);
                            _leftBird.State = BirdState.Singing3;
                            await Task.Delay(500);
                            _leftBird.State = BirdState.Singing4;
                            await Task.Delay(500);
                            _leftBird.State = BirdState.LookingRight;
                            this.State = BirdState.ChattingLeft;
                            await Task.Delay(500);
                            _leftBird.State = BirdState.ChattingRight;
                            this.State = BirdState.LookingAtWing1; // frame to be interpreted as "pointing with left wing"
                            await Task.Delay(500);
                            _leftBird.State = BirdState.StretchingWings;
                            this.State = BirdState.ChattingLeft;
                            await Task.Delay(500);
                            break;

                        case 1:
                            _leftBird.State = BirdState.Sitting;
                            this.State = BirdState.Sitting;
                            await Task.Delay(500);
                            if (rnd.NextDouble() < 0.5)
                            {
                                // 50% chance: start the animation with left bird instead of main bird
                                this.State = BirdState.LookingLeft;
                                await Task.Delay(500);
                                this.State = BirdState.ChattingLeft;
                            }
                            else
                            {
                                _leftBird.State = BirdState.LookingRight;
                                await Task.Delay(500);
                                _leftBird.State = BirdState.ChattingRight;
                            }
                            await Task.Delay(500);
                            _leftBird.State = BirdState.LookingRight;
                            this.State = BirdState.LookingLeft;

                            for (int i = 0; i < rnd.Next(2, 6); i++)
                            {
                                var leftBirdBehavior = rnd.Next(0, 2);
                                switch (leftBirdBehavior)
                                {
                                    case 0:
                                        _leftBird.State = BirdState.LookingRight;
                                        break;
                                    case 1:
                                        _leftBird.State = BirdState.ChattingRight;
                                        break;
                                    case 2:
                                        _leftBird.State = BirdState.Singing1;
                                        break;
                                }
                                var rightBirdBehavior = rnd.Next(0, 2);
                                switch (rightBirdBehavior)
                                {
                                    case 0:
                                        this.State = BirdState.LookingLeft;
                                        break;
                                    case 1:
                                        this.State = BirdState.ChattingLeft;
                                        break;
                                    case 2:
                                        this.State = BirdState.LookingAtWing1;
                                        break;
                                }
                                await Task.Delay(500);
                            }
                            _leftBird.State = BirdState.LookingRight;
                            this.State = BirdState.LookingLeft;
                            break;

                        case 2:
                            this.State = BirdState.ChattingLeft;
                            await Task.Delay(500);
                            this.State = BirdState.LookingAtWing1;
                            _leftBird.State = BirdState.ChattingRight;
                            await Task.Delay(500);
                            for (int i = 0; i < rnd.Next(2, 6); i++)
                            {
                                _leftBird.State = BirdState.StretchingWings;
                                this.State = BirdState.StretchingWings;
                                await Task.Delay(250);
                                _leftBird.State = BirdState.Sitting;
                                this.State = BirdState.Sitting;
                                await Task.Delay(250);
                            }
                            break;
                    }
                    this._overriddenBehavior = false;
                    this.RestingSpot.SpotToTheLeft.OccupyingBird._overriddenBehavior = false;
                    break;
                
                
                case NeighborhoodType.OnlyRight:
                    this._overriddenBehavior = true;
                    this.RestingSpot.SpotToTheRight.OccupyingBird._overriddenBehavior = true;
                    var _rightBird = this.RestingSpot.SpotToTheRight.OccupyingBird;
                    // create interactions between both birds here
                    switch (rnd.Next(0, 2))
                    {
                        case 0:
                            _rightBird.State = BirdState.Sitting;
                            this.State = BirdState.Sitting;
                            await Task.Delay(500);
                            this.State = BirdState.Singing1;
                            await Task.Delay(500);
                            this.State = BirdState.Singing2;
                            _rightBird.State = BirdState.LookingLeft;
                            await Task.Delay(500);
                            this.State = BirdState.Singing3;
                            await Task.Delay(500);
                            this.State = BirdState.Singing4;
                            await Task.Delay(500);
                            this.State = BirdState.LookingRight;
                            _rightBird.State = BirdState.ChattingLeft;
                            await Task.Delay(500);
                            this.State = BirdState.ChattingRight;
                            _rightBird.State = BirdState.LookingAtWing1; // frame to be interpreted as "pointing with left wing"
                            await Task.Delay(500);
                            this.State = BirdState.StretchingWings;
                            _rightBird.State = BirdState.ChattingLeft;
                            await Task.Delay(500);
                            break;

                        case 1:
                            this.State = BirdState.Sitting;
                            _rightBird.State = BirdState.Sitting;
                            await Task.Delay(500);
                            if (rnd.NextDouble() < 0.5)
                            {
                                // 50% chance: start the animation with right bird instead of main bird
                                _rightBird.State = BirdState.LookingLeft;
                                await Task.Delay(500);
                                _rightBird.State = BirdState.ChattingLeft;
                            }
                            else
                            {
                                this.State = BirdState.LookingRight;
                                await Task.Delay(500);
                                this.State = BirdState.ChattingRight;
                            }
                            await Task.Delay(500);
                            this.State = BirdState.LookingRight;
                            _rightBird.State = BirdState.LookingLeft;

                            for (int i = 0; i < rnd.Next(2, 6); i++)
                            {
                                var leftBirdBehavior = rnd.Next(0, 2);
                                switch (leftBirdBehavior)
                                {
                                    case 0:
                                        this.State = BirdState.LookingRight;
                                        break;
                                    case 1: 
                                        this.State = BirdState.ChattingRight;
                                        break;
                                    case 2:
                                        this.State = BirdState.Singing1;
                                        break;
                                }
                                var rightBirdBehavior = rnd.Next(0, 2);
                                switch (rightBirdBehavior)
                                {
                                    case 0:
                                        _rightBird.State = BirdState.LookingLeft;
                                        break;
                                    case 1:
                                        _rightBird.State = BirdState.ChattingLeft;
                                        break;
                                    case 2:
                                        _rightBird.State = BirdState.LookingAtWing1;
                                        break;
                                }
                                await Task.Delay(500);
                            }
                            this.State = BirdState.LookingRight;
                            _rightBird.State = BirdState.LookingLeft;
                            break;
                        
                        case 2:
                            _rightBird.State = BirdState.ChattingLeft;
                            await Task.Delay(500);
                            _rightBird.State = BirdState.LookingAtWing1;
                            this.State = BirdState.ChattingRight;
                            await Task.Delay(500);
                            for (int i = 0; i < rnd.Next(2, 6); i++)
                            {
                                this.State = BirdState.StretchingWings;
                                _rightBird.State = BirdState.StretchingWings;
                                await Task.Delay(250);
                                this.State = BirdState.Sitting;
                                _rightBird.State = BirdState.Sitting;
                                await Task.Delay(250);
                            }
                            break;
                    }
                    this._overriddenBehavior = false;
                    this.RestingSpot.SpotToTheRight.OccupyingBird._overriddenBehavior = false;
                    break;


                case NeighborhoodType.LeftAndRight:
                    this._overriddenBehavior = true;
                    this.RestingSpot.SpotToTheLeft.OccupyingBird._overriddenBehavior = true;
                    this.RestingSpot.SpotToTheRight.OccupyingBird._overriddenBehavior = true;
                    var _leftBirdOf3 = this.RestingSpot.SpotToTheLeft.OccupyingBird;
                    var _rightBirdOf3 = this.RestingSpot.SpotToTheRight.OccupyingBird;
                    // create interactions between the three birds here
                    switch (rnd.Next(0, 1))
                    {
                        case 0:
                            this.State = BirdState.Sitting;
                            _leftBirdOf3.State = BirdState.Sitting;
                            _rightBirdOf3.State = BirdState.Sitting;
                            await Task.Delay(500);
                            this.State = BirdState.ChattingLeft;
                            _leftBirdOf3.State = BirdState.LookingRight;
                            _rightBirdOf3.State = BirdState.Sitting;
                            await Task.Delay(500);
                            this.State = BirdState.ChattingRight;
                            _leftBirdOf3.State = BirdState.LookingRight;
                            _rightBirdOf3.State = BirdState.LookingLeft;
                            await Task.Delay(500);
                            this.State = BirdState.StretchingWings;
                            _leftBirdOf3.State = BirdState.LookingRight;
                            _rightBirdOf3.State = BirdState.LookingLeft;
                            await Task.Delay(500);
                            this.State = BirdState.StretchingWings;
                            _leftBirdOf3.State = BirdState.StretchingWings;
                            _rightBirdOf3.State = BirdState.StretchingWings;
                            await Task.Delay(500);
                            this.State = BirdState.Sitting;
                            _leftBirdOf3.State = BirdState.StretchingWings;
                            _rightBirdOf3.State = BirdState.StretchingWings;
                            await Task.Delay(500);
                            this.State = BirdState.Sitting;
                            _leftBirdOf3.State = BirdState.Sitting;
                            _rightBirdOf3.State = BirdState.Sitting;
                            await Task.Delay(500);
                            break;
                        case 1:
                            this.State = BirdState.Sitting;
                            _leftBirdOf3.State = BirdState.Sitting;
                            _rightBirdOf3.State = BirdState.Sitting;
                            await Task.Delay(500);
                            this.State = BirdState.ChattingLeft;
                            _leftBirdOf3.State = BirdState.LookingRight;
                            _rightBirdOf3.State = BirdState.Sitting;
                            await Task.Delay(500);
                            this.State = BirdState.ChattingRight;
                            _leftBirdOf3.State = BirdState.LookingRight;
                            _rightBirdOf3.State = BirdState.LookingLeft;
                            await Task.Delay(500);
                            this.State = BirdState.StretchingWings;
                            _leftBirdOf3.State = BirdState.ChattingRight;
                            _rightBirdOf3.State = BirdState.ChattingLeft;
                            await Task.Delay(500);
                            this.State = BirdState.Singing1;
                            _leftBirdOf3.State = BirdState.Singing1;
                            _rightBirdOf3.State = BirdState.Singing1;
                            await Task.Delay(500);
                            this.State = BirdState.Singing2;
                            _leftBirdOf3.State = BirdState.Singing2;
                            _rightBirdOf3.State = BirdState.Singing2;
                            await Task.Delay(500);
                            this.State = BirdState.Singing3;
                            _leftBirdOf3.State = BirdState.Singing3;
                            _rightBirdOf3.State = BirdState.Singing3;
                            await Task.Delay(500);
                            this.State = BirdState.Singing4;
                            _leftBirdOf3.State = BirdState.Singing4;
                            _rightBirdOf3.State = BirdState.Singing4;
                            await Task.Delay(500);
                            this.State = BirdState.StretchingWings;
                            _leftBirdOf3.State = BirdState.ChattingRight;
                            _rightBirdOf3.State = BirdState.ChattingLeft;
                            this.State = BirdState.Sitting;
                            _leftBirdOf3.State = BirdState.LookingRight;
                            _rightBirdOf3.State = BirdState.LookingLeft;
                            break;
                    }
                    this._overriddenBehavior = true;
                    this.RestingSpot.SpotToTheLeft.OccupyingBird._overriddenBehavior = false;
                    this.RestingSpot.SpotToTheRight.OccupyingBird._overriddenBehavior = false;
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Assigns some values to the bird's personality that are supposed to impact is behavior.
    /// Each of these values is to be assigned an integer value from 2 to 10.
    /// Do not assign less than 1, since that can make the bird more likely to get stuck in a loop.
    /// </summary>
    public class BirdPersonality
    {
        /// <summary>
        /// Impacts the likelihood of the bird entering the singing event
        /// </summary>
        public int Musical { get; set; }
        /// <summary>
        /// Impacts how often a bird will engage in social activities.
        /// This includes chatting, but also interaction with other birds.
        /// </summary>
        public int Social { get; set; }
        /// <summary>
        /// This impacts how often a bird looks around and how paranoid it appears
        /// while doing so. A higher value means the bird will look around more often.
        /// </summary>
        public int Hectic { get; set; }
        /// <summary>
        /// This impacts how often a bird will check and stretch its wings.
        /// </summary>
        public int Cleanliness { get; set; }
        /// <summary>
        /// This impacts how likely a bird is to take off from its resting spot.
        /// It also impacts how quickly a bird may perform an action and how thoroughly it's done.
        /// </summary>
        public int Energy { get; set; }
    }

    public class RestingSpot
    {
        public string Identifier { get; set; }
        public Vector3 Position { get; set; }
        public RestingSpot SpotToTheLeft { get; set; }
        public RestingSpot SpotToTheRight { get; set; }

        public void SetNeighborRestingSpots (RestingSpot left, RestingSpot right)
        {
            this.SpotToTheLeft = left;
            this.SpotToTheRight = right;
            // set this spot as neighboring spot of the other spots too
            if (left != null) { this.SpotToTheLeft.SpotToTheRight = this; }
            if (right != null) { this.SpotToTheRight.SpotToTheLeft = this; }
        }

        /// <summary>
        /// IsOccupied is also true when a bird is currenly approaching 
        /// to prevent another bird from targeting this spot.
        /// </summary>
        public bool IsOccupied { get; set; }
        /// <summary>
        /// OccupyingBird refers to the bird that is actually sitting in the spot. 
        /// This is used for multi-bird animations.
        /// </summary>
        public Bird OccupyingBird { get; set; }
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

    internal enum NeighborhoodType
    {
        None,
        OnlyLeft,
        OnlyRight,
        LeftAndRight
    }
}
