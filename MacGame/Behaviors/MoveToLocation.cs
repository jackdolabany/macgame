using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using System;
using System.Xml.Linq;

namespace MacGame.Behaviors
{
    /// <summary>
    /// A behavior for an NPC to just move to a waypoint. This class assumes the NPC can 
    /// walk, climb, jump, and be idle. And should have animations with those names.
    /// 
    /// This is not for NPCs that don't collide with the level.
    /// </summary>
    public class MoveToLocation : Behavior
    {
        private Vector2 _targetLocation;
        public Vector2 TargetLocation 
        { 
            get
            {
                return _targetLocation;
            }
            set
            {
                if (_targetLocation != value)
                {
                    _targetLocation = value;
                    _locationBeforeMove = _gameObject.WorldLocation;
                }
            }
        }

        private GameObject _gameObject { get; set; }

        private Vector2 _locationBeforeMove;

        public float MoveSpeed { get; set; }

        /// <summary>
        /// The x direction speed when jumping in case it needs to be different from regular.
        /// </summary>
        public float JumpMoveSpeed { get; set; }

        private string _idleAnimationName;
        private string _walkAnimationName;
        private string _jumpAnimationName;
        private string _climbAnimationName;

        private bool _isAtLocation;

        private bool isJumping;
        private float jumpXVelocity;

        // Keep track if we are on a ladder. Once you're on a ladder you'll move all the way up or down it.
        private bool _isClimbingLadder;
        private float _ladderYVelocity;

        /// <summary>
        /// Create a new MoveToLocation behavior for NPCs to follow waypoints or just move to a location. They'll have a half hearted
        /// attemp to avoid some obstacles or climb ladders.
        /// </summary>
        /// <param name="targetLocation"></param>
        /// <param name="moveSpeed"></param>
        /// <param name="jumpMoveSpeed">Pass something in here if you want to move verticaly at a different speed when jumping. This can
        /// be useful if you have a character that moves at different speeds but you want to jump gaps consistently</param>
        /// <param name="idleAnimationName"></param>
        /// <param name="walkAnimationName"></param>
        /// <param name="jumpAnimationName"></param>
        /// <param name="climbAnimationName"></param>
        public MoveToLocation(GameObject gameObject, float moveSpeed, float jumpMoveSpeed, string idleAnimationName, string walkAnimationName, string jumpAnimationName, string climbAnimationName)
        {
            _gameObject = gameObject;
            MoveSpeed = moveSpeed;
            JumpMoveSpeed = jumpMoveSpeed;
            _idleAnimationName = idleAnimationName;
            _walkAnimationName = walkAnimationName;
            _jumpAnimationName = jumpAnimationName;
            _climbAnimationName = climbAnimationName;
        }

        public override void Update(GameObject _, GameTime gameTime, float elapsed)
        {
            var animations = (AnimationDisplay)_gameObject.DisplayComponent;

            // Go to the next waypoint.
            Vector2 inFrontOfCenter = new Vector2(_gameObject.CollisionRectangle.Width / 2 + Game1.TileSize, -16);
            Vector2 inFrontBelow = new Vector2(8, 8);
            if (_gameObject.Flipped)
            {
                inFrontOfCenter.X *= -1;
                inFrontBelow.X *= -1;
            }

            var tileInFront = Game1.CurrentMap?.GetMapSquareAtPixel(_gameObject.WorldLocation + inFrontOfCenter);
            var tileAtFrontBelow = Game1.CurrentMap?.GetMapSquareAtPixel(_gameObject.WorldLocation + inFrontBelow);

            // Walk towards the target
            if (_gameObject.OnGround)
            {
                if (_gameObject.WorldLocation.X < TargetLocation.X - 4)
                {
                    _gameObject.Velocity = new Vector2(MoveSpeed, _gameObject.Velocity.Y);

                    if (_gameObject.OnGround && animations.CurrentAnimationName != _walkAnimationName)
                    {
                        animations.Play(_walkAnimationName);
                    }

                    _gameObject.Flipped = false;
                }
                else if (_gameObject.WorldLocation.X > TargetLocation.X + 4)
                {
                    _gameObject.Velocity = new Vector2(-MoveSpeed, _gameObject.Velocity.Y);
                    if (_gameObject.OnGround && animations.CurrentAnimationName != _walkAnimationName)
                    {
                        animations.Play(_walkAnimationName);
                    }

                    _gameObject.Flipped = true;
                }
                else
                {
                    // we're there in the x direction.
                    _gameObject.Velocity = new Vector2(0, _gameObject.Velocity.Y);
                }
            }

            // Once in a jump, x velocity doesn't change until you hit the ground.
            // This helps if you nick the corner of a tile or something.
            if (isJumping)
            {
                _gameObject.Velocity = new Vector2(jumpXVelocity, _gameObject.Velocity.Y);

                if ((jumpXVelocity > 0 && _gameObject.WorldLocation.X > TargetLocation.X) ||
                    (jumpXVelocity < 0 && _gameObject.WorldLocation.X < TargetLocation.X))
                {
                    // overshot the jump, just stop, we tried our best to time it right.
                    _gameObject.Velocity = new Vector2(0, _gameObject.Velocity.Y);
                    jumpXVelocity = 0f;
                }
            }

            var tileAtHead = Game1.CurrentMap?.GetMapSquareAtPixel(_gameObject.WorldLocation.X.ToInt(), _gameObject.CollisionRectangle.Top + 1);
            var tileAtFeet = Game1.CurrentMap?.GetMapSquareAtPixel(_gameObject.WorldLocation.X.ToInt(), _gameObject.CollisionRectangle.Bottom);
            var isOverLadder = (tileAtHead != null && tileAtHead.IsLadder) || (tileAtFeet != null && tileAtFeet.IsLadder);

            if (isOverLadder || _gameObject.OnGround)
            {
                isJumping = false;
            }

            if (!isOverLadder)
            {
                _isClimbingLadder = false;
            }

            // Jump before you walk into a wall.
            var isWalkingIntoAWall = !isOverLadder && tileInFront != null && !tileInFront.Passable && _gameObject.OnGround && _gameObject.Velocity.X != 0;
            var isWalkingOffACliff = !_isClimbingLadder && tileAtFrontBelow != null && tileAtFrontBelow.Passable && _gameObject.OnGround && TargetLocation.Y <= _gameObject.CollisionRectangle.Bottom;

            // Jump before a cliff, unless the waypoint is below you.
            if (isWalkingIntoAWall || isWalkingOffACliff)
            {

                // figure out how high you have to jump to hit the target location
                var heightToTarget = Math.Abs(TargetLocation.Y - _gameObject.WorldLocation.Y);
                var distanceToTarget = Math.Abs(TargetLocation.X - _gameObject.WorldLocation.X);

                // at this speed, how long would it take to reach the target
                var timeToImpact = distanceToTarget / JumpMoveSpeed;

                var upwardSpeed  = -((0.55f * _gameObject.Gravity.Y * timeToImpact) + (heightToTarget / timeToImpact));
                
                jumpXVelocity = _gameObject.Flipped ? -JumpMoveSpeed : JumpMoveSpeed;

                _gameObject.Velocity = new Vector2(jumpXVelocity, upwardSpeed);
                jumpXVelocity = _gameObject.Flipped ? -JumpMoveSpeed : JumpMoveSpeed;

                _gameObject.Velocity = new Vector2(jumpXVelocity, upwardSpeed);
                animations.Play(_jumpAnimationName);
                SoundManager.PlaySound("Jump");
                isJumping = true;
            }

            if (_gameObject.OnGround && isOverLadder && _gameObject.WorldLocation.Y > TargetLocation.Y)
            {
                // Climb up the ladder
                _isClimbingLadder = true;
                _ladderYVelocity = -MoveSpeed;
            }
            else if (isOverLadder && _gameObject.WorldLocation.Y < TargetLocation.Y)
            {
                // Climb down the ladder
                _isClimbingLadder = true;
                _ladderYVelocity = MoveSpeed;
            }

            // Ladder climbing.
            if (_isClimbingLadder)
            {
                if (animations.CurrentAnimationName != _climbAnimationName)
                {
                    animations.Play(_climbAnimationName);
                }
                _gameObject.IsAffectedByGravity = false;

                // Move towards the center of the ladder
                var targetX = ((int)_gameObject.WorldLocation.X / Game1.TileSize * Game1.TileSize) + (Game1.TileSize / 2);

                if (targetX > _gameObject.WorldLocation.X - 2)
                {
                    _gameObject.Velocity = new Vector2(20, _gameObject.Velocity.Y);
                }
                else if (targetX <= _gameObject.WorldLocation.X + 2)
                {
                    _gameObject.Velocity = new Vector2(-20, _gameObject.Velocity.Y);
                }
                else
                {
                    // Lock on the x coordinate.
                    _gameObject.WorldLocation = new Vector2(targetX, _gameObject.WorldLocation.Y);
                }

                _gameObject.Velocity = new Vector2(_gameObject.Velocity.X, _ladderYVelocity);
            }
            else
            {
                _gameObject.IsAffectedByGravity = true;
            }

            if (_gameObject.OnGround && _gameObject.Velocity.X == 0 && animations.CurrentAnimationName != _idleAnimationName)
            {
                animations.Play(_idleAnimationName);
            }

            _isAtLocation = IsAtLocation(_gameObject.WorldLocation, TargetLocation);
        }

        public bool IsAtLocation()
        {
            return _isAtLocation;
        }

        public static bool IsAtLocation(Vector2 gameObjectLocation, Vector2 targetLocation)
        {
            return Vector2.Distance(gameObjectLocation, targetLocation) < 10;
        }
    }
}
