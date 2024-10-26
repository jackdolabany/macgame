using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
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

        private string _idleAnimationName;
        private string _walkAnimationName;
        private string _jumpAnimationName;
        private string _climbAnimationName;

        private bool _isAtLocation;

        public MoveToLocation(Vector2 targetLocation, float moveSpeed, string idleAnimationName, string walkAnimationName, string jumpAnimationName, string climbAnimationName)
        {
            TargetLocation = targetLocation;
            MoveSpeed = moveSpeed;
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
                gameObject.Flipped = false;
            }
            else if (TargetLocation.X <= gameObject.CollisionRectangle.Left)
            {
                gameObject.Velocity = new Vector2(-MoveSpeed, gameObject.Velocity.Y);
                if (gameObject.OnGround && animations.CurrentAnimationName != _walkAnimationName)
                {
                    animations.Play(_walkAnimationName);
                }
                gameObject.Flipped = true;
            }
            else
            {
                gameObject.Velocity = new Vector2(0, gameObject.Velocity.Y);
            }

            // Jump before you walk into a wall.
            if (tileInFront != null && !tileInFront.Passable && gameObject.OnGround && gameObject.Velocity.X != 0)
            {
                gameObject.Velocity -= new Vector2(0, 600);
                animations.Play(_jumpAnimationName);
            }

            // Jump before a cliff, unless the waypoint is below you.
            if (tileAtFrontBelow != null && tileAtFrontBelow.Passable && gameObject.OnGround && TargetLocation.Y < gameObject.CollisionRectangle.Bottom)
            {
                gameObject.Velocity -= new Vector2(0, 600);
                animations.Play(_jumpAnimationName);
            }

            // Ladder climbing.
            var tileAtHead = Game1.CurrentMap?.GetMapSquareAtPixel(gameObject.WorldCenter.X.ToInt(), gameObject.CollisionRectangle.Top);
            var tileAtFeet = Game1.CurrentMap?.GetMapSquareAtPixel(gameObject.WorldLocation);
            var onLadder = (tileAtHead != null && tileAtHead.IsLadder) || (tileAtFeet != null && tileAtFeet.IsLadder);
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

            _isAtLocation = gameObject.CollisionRectangle.Contains(TargetLocation.ToPoint());
        }

        public bool IsAtLocation()
        {
            return _isAtLocation;
        }
    }
}
