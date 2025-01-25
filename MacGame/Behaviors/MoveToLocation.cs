using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using System;
using System.Xml.Linq;

namespace MacGame.Behaviors
{
    /// <summary>
    /// A behavior for an NPC to just move to a waypoint. This class assumes the NPC can 
    /// walk, climb, jump, and be idle. And should have animations with those names.
    /// </summary>
    public class MoveToLocation : Behavior
    {
        public Vector2 TargetLocation { get; set; }
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
        public MoveToLocation(Vector2 targetLocation, float moveSpeed, float jumpMoveSpeed, string idleAnimationName, string walkAnimationName, string jumpAnimationName, string climbAnimationName)
        {
            TargetLocation = targetLocation;
            MoveSpeed = moveSpeed;
            JumpMoveSpeed = jumpMoveSpeed;
            _idleAnimationName = idleAnimationName;
            _walkAnimationName = walkAnimationName;
            _jumpAnimationName = jumpAnimationName;
            _climbAnimationName = climbAnimationName;
        }

        public override void Update(GameObject gameObject, GameTime gameTime, float elapsed)
        {
            var animations = (AnimationDisplay)gameObject.DisplayComponent;

            // Go to the next waypoint.
            Vector2 inFrontOfCenter = new Vector2(Game1.TileSize, 0);
            Vector2 inFrontBelow = new Vector2(8, 8);
            if (gameObject.Flipped)
            {
                inFrontOfCenter.X *= -1;
                inFrontBelow.X *= -1;
            }

            var tileInFront = Game1.CurrentMap?.GetMapSquareAtPixel(gameObject.WorldCenter + inFrontOfCenter);
            var tileAtFrontBelow = Game1.CurrentMap?.GetMapSquareAtPixel(gameObject.WorldLocation + inFrontBelow);

            if (TargetLocation.X >= gameObject.CollisionRectangle.Right)
            {
                gameObject.Velocity = new Vector2(MoveSpeed, gameObject.Velocity.Y);

                if (gameObject.OnGround && animations.CurrentAnimationName != _walkAnimationName)
                {
                    animations.Play(_walkAnimationName);
                }

                if (isJumping)
                {
                    gameObject.Velocity = new Vector2(JumpMoveSpeed, gameObject.Velocity.Y);
                }

                gameObject.Flipped = false;
            }
            else if (TargetLocation.X <= gameObject.CollisionRectangle.Left)
            {
                gameObject.Velocity = new Vector2(-MoveSpeed, gameObject.Velocity.Y);
                if (gameObject.OnGround && animations.CurrentAnimationName != _walkAnimationName)
                {
                    animations.Play(_walkAnimationName);
                }

                if (isJumping)
                {
                    gameObject.Velocity = new Vector2(-JumpMoveSpeed, gameObject.Velocity.Y);
                }

                gameObject.Flipped = true;
            }
            else
            {
                gameObject.Velocity = new Vector2(0, gameObject.Velocity.Y);
            }

            var tileAtHead = Game1.CurrentMap?.GetMapSquareAtPixel(gameObject.WorldCenter.X.ToInt(), gameObject.CollisionRectangle.Top);
            var tileAtFeet = Game1.CurrentMap?.GetMapSquareAtPixel(gameObject.WorldLocation);
            var onLadder = (tileAtHead != null && tileAtHead.IsLadder) || (tileAtFeet != null && tileAtFeet.IsLadder);

            if (onLadder || gameObject.OnGround)
            {
                isJumping = false;
            }

            // Jump before you walk into a wall.
            if (!onLadder && tileInFront != null && !tileInFront.Passable && gameObject.OnGround && gameObject.Velocity.X != 0)
            {
                gameObject.Velocity -= new Vector2(0, 600);
                animations.Play(_jumpAnimationName);
                SoundManager.PlaySound("Jump");
                isJumping = true;
            }

            // Jump before a cliff, unless the waypoint is below you.
            if (!onLadder && tileAtFrontBelow != null && tileAtFrontBelow.Passable && gameObject.OnGround && TargetLocation.Y < gameObject.CollisionRectangle.Bottom)
            {

                // figure out how high you have to jump to hit the target location
                var heightToTarget = TargetLocation.Y - gameObject.CollisionRectangle.Bottom;
                var distanceToTarget = Math.Abs(TargetLocation.X - gameObject.CollisionCenter.X);

                // at this speed, how long would it take to reach the target
                var timeToImpact = distanceToTarget / JumpMoveSpeed;

                var fallSpeedBasedOnGravity  = 0.5f * Game1.Gravity.Y * timeToImpact * timeToImpact;

                var upwardSpeed = (heightToTarget - fallSpeedBasedOnGravity) / timeToImpact;

                gameObject.Velocity += new Vector2(0, upwardSpeed);
                animations.Play(_jumpAnimationName);
                SoundManager.PlaySound("Jump");
                isJumping = true;
            }

            // Ladder climbing.
            if (onLadder)
            {
                if (animations.CurrentAnimationName != _climbAnimationName)
                {
                    animations.Play(_climbAnimationName);
                }
                gameObject.IsAffectedByGravity = false;

                // Move towards the center of the ladder
                var targetX = ((int)gameObject.WorldLocation.X / Game1.TileSize * Game1.TileSize) + (Game1.TileSize / 2);

                if (targetX > gameObject.WorldLocation.X)
                {
                    gameObject.Velocity = new Vector2(20, gameObject.Velocity.Y);
                }
                else if (targetX <= gameObject.WorldLocation.X)
                {
                    gameObject.Velocity = new Vector2(-20, gameObject.Velocity.Y);
                }

                // Climb up or down.
                if (TargetLocation.Y > gameObject.CollisionCenter.Y)
                {
                    gameObject.Velocity = new Vector2(gameObject.Velocity.X, MoveSpeed);
                }
                else if (TargetLocation.Y < gameObject.CollisionCenter.Y)
                {
                    gameObject.Velocity = new Vector2(gameObject.Velocity.X, -MoveSpeed);
                }
            }
            else
            {
                gameObject.IsAffectedByGravity = true;
            }

            if (gameObject.OnGround && gameObject.Velocity.X == 0 && animations.CurrentAnimationName != _idleAnimationName)
            {
                animations.Play(_idleAnimationName);
            }

            _isAtLocation = Vector2.Distance(gameObject.CollisionCenter, TargetLocation) < 10;
        }

        public bool IsAtLocation()
        {
            return _isAtLocation;
        }
    }
}
