using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;

namespace MacGame.Behaviors
{
    /// <summary>
    /// A behavior for an NPC to just move to a waypoint. This class assumes the NPC can
    /// walk, climb, jump, and be idle. And should have animations with those names.
    ///
    /// This class expects the gameObject to move to the bottom center of the waypoint.
    ///
    /// We should be able to safely assume the NPC won't be effected by gravity.
    /// </summary>
    public class MoveToLocation : Behavior
    {
        private Vector2 _previousLocation;
        private Vector2 _targetLocation;
        private Queue<Vector2> _locationQueue;

        public Vector2 TargetLocation => _targetLocation;

        private GameObject _gameObject { get; set; }

        public float MoveSpeed { get; set; }

        private string _idleAnimationName;
        private string _walkAnimationName;
        private string _jumpAnimationName;
        private string _climbAnimationName;

        public enum WaypointMovement
        {
            Walking,
            Idle,
            ClimbingLadder,
            Jumping,
            Falling
        }

        private bool _isAtFinalLocation = true;
        public bool IsAtFinalLocation => _isAtFinalLocation;


        private WaypointMovement _waypointMovement;

        private float playClimbSoundTimer = 0f;

        /// <summary>
        /// Create a new MoveToLocation behavior for NPCs to follow waypoints or just move to a location. They'll have a half hearted
        /// attempt to avoid some obstacles or climb ladders.
        /// </summary>
        public MoveToLocation(GameObject gameObject, float moveSpeed, string idleAnimationName, string walkAnimationName, string jumpAnimationName, string climbAnimationName)
        {
            _gameObject = gameObject;
            MoveSpeed = moveSpeed;
            _idleAnimationName = idleAnimationName;
            _walkAnimationName = walkAnimationName;
            _jumpAnimationName = jumpAnimationName;
            _climbAnimationName = climbAnimationName;
            _locationQueue = new Queue<Vector2>();
        }

        public override void Update(GameObject _, GameTime gameTime, float elapsed)
        {
            // Check if we've reached the current location from the previous frame
            var isAtTargetLocation = Vector2.Distance(_gameObject.WorldLocation, _targetLocation) < 4f;

            // Edge case, you aren't done climbing a ladder until you are fully at the top of the ladder.
            // Otherwise the character might try to walk prematurely and get stuck on an edge.
            if (_waypointMovement == WaypointMovement.ClimbingLadder 
                && _gameObject.Velocity.Y < 0
                && _gameObject.WorldLocation.Y >= _targetLocation.Y)
            {
                isAtTargetLocation = false;
            }

            if (isAtTargetLocation && _locationQueue.Count > 0)
            {
                // Move to the next location in the queue
                SetTargetLocation(_locationQueue.Dequeue());
            }
            else if (isAtTargetLocation && _locationQueue.Count == 0 && _targetLocation != Vector2.Zero)
            {
                // We've reached the final location, stop and set to idle
                SetTargetLocation(Vector2.Zero);
                _waypointMovement = WaypointMovement.Idle;
                _gameObject.Velocity = Vector2.Zero;
                _isAtFinalLocation = true;
            }

            var normalToWaypoint = _targetLocation - _gameObject.WorldLocation;
            normalToWaypoint.Normalize();

            switch (_waypointMovement)
            {
                case WaypointMovement.Walking:
                    _gameObject.Velocity = new Vector2((_targetLocation.X > _gameObject.WorldLocation.X ? 1 : -1) * MoveSpeed, _gameObject.Velocity.Y);
                    break;
                case WaypointMovement.Idle:
                    _gameObject.Velocity = new Vector2(0, _gameObject.Velocity.Y);
                    break;
                case WaypointMovement.Jumping:

                    // Jumping
                    _gameObject.Velocity = normalToWaypoint * MoveSpeed;

                    break;
                case WaypointMovement.Falling:
                    // Falling
                    _gameObject.Velocity = normalToWaypoint * MoveSpeed;
                    break;
                case WaypointMovement.ClimbingLadder:

                    // Move in the x direction full speed until you are within 1 pixel of the target x.
                    var distanceToCenter = Math.Abs(_targetLocation.X - _gameObject.WorldLocation.X);
                    if (distanceToCenter < 4)
                    {
                        // Lock on the x coordinate if it's close enough.
                        _gameObject.WorldLocation = new Vector2(_targetLocation.X, _gameObject.WorldLocation.Y);
                    }

                    // Half speed on the ladder.
                    _gameObject.Velocity = normalToWaypoint * MoveSpeed / 2;

                    playClimbSoundTimer -= elapsed;
                    if (playClimbSoundTimer <= 0f)
                    {
                        SoundManager.PlaySound("Climb", 1f, 0.3f);
                        playClimbSoundTimer += 0.15f;
                    }
                    break;
                default:
                    throw new NotImplementedException("Unknown WaypointMovement type");
            }
        }

        public void SetTargetLocation(Vector2 location)
        {
            if (_targetLocation != Vector2.Zero)
            {
                _previousLocation = _targetLocation;
            }
            else
            {
                _previousLocation = _gameObject.WorldLocation;
            }
            _targetLocation = location;
            
            _isAtFinalLocation = false;

            var priorMovement = _waypointMovement;

            _waypointMovement = DetermineMovementToNextTarget();

            if (_waypointMovement == WaypointMovement.Jumping && priorMovement != WaypointMovement.Jumping)
            {
                SoundManager.PlaySound("Jump");
            }

            if (_targetLocation != Vector2.Zero && _previousLocation.X != _targetLocation.X)
            {
                _gameObject.Flipped = _targetLocation.X < _previousLocation.X;
            }

            var animations = (AnimationDisplay)_gameObject.DisplayComponent;

            switch (_waypointMovement)
            {
                case WaypointMovement.Walking:
                    animations.PlayIfNotAlreadyPlaying(_walkAnimationName);
                    break;
                case WaypointMovement.Idle:
                    animations.PlayIfNotAlreadyPlaying(_idleAnimationName);
                    break;
                case WaypointMovement.Jumping:
                case WaypointMovement.Falling:
                    animations.PlayIfNotAlreadyPlaying(_jumpAnimationName);
                    break;
                case WaypointMovement.ClimbingLadder:
                    animations.PlayIfNotAlreadyPlaying(_climbAnimationName);
                    break;
            }

            _gameObject.IsAffectedByGravity = _waypointMovement == WaypointMovement.Walking || _waypointMovement == WaypointMovement.Idle;
        }

        /// <summary>
        /// Set multiple target locations for the NPC to follow in sequence.
        /// The NPC will move through each location in order and stop at the final one.
        /// </summary>
        public void SetTargetLocations(IEnumerable<Vector2> locations)
        {
            _locationQueue.Clear();

            foreach (var location in locations)
            {
                _locationQueue.Enqueue(location);
            }

            if (_locationQueue.Any())
            {
                var location = _locationQueue.Dequeue();
                SetTargetLocation(location);
            }
        }

        private WaypointMovement DetermineMovementToNextTarget()
        {

            if (_targetLocation == Vector2.Zero)
            {
                return WaypointMovement.Idle;
            }

            var isWalkingToWaypoint = Math.Abs(_targetLocation.Y - _gameObject.WorldLocation.Y) < Game1.TileSize / 2;
            if (isWalkingToWaypoint)
            {
                return WaypointMovement.Walking;
            }
            else if (_gameObject.WorldLocation.Y > _targetLocation.Y)
            {
                // Can only climb a ladder if the next waypoint is directly above
                var isTargetDirectlyAbove = Math.Abs(_targetLocation.X - _gameObject.WorldLocation.X) < Game1.TileSize / 2;
                var tileAtCenter = Game1.CurrentMap?.GetMapSquareAtPixel(_gameObject.WorldCenter);
                var isOverLadder = (tileAtCenter != null && tileAtCenter.IsLadder);
                if (isTargetDirectlyAbove && isOverLadder)
                {
                    return WaypointMovement.ClimbingLadder;
                }
                else
                {
                    return WaypointMovement.Jumping;
                }
            }
            else if (_gameObject.WorldLocation.Y < _targetLocation.Y)
            {
                var isTargetDirectlyBelow = Math.Abs(_targetLocation.X - _gameObject.WorldLocation.X) < Game1.TileSize / 2;
                var tileBelow = Game1.CurrentMap?.GetMapSquareAtPixel(_gameObject.WorldLocation - new Vector2(0, 16));
                var isOverLadder = (tileBelow != null && tileBelow.IsLadder);
                if (isTargetDirectlyBelow && isOverLadder)
                {
                    return WaypointMovement.ClimbingLadder;
                }
                else
                {
                    return WaypointMovement.Falling;
                }
            }
            else
            {
                return WaypointMovement.Idle;
            }
        }
    }
}
