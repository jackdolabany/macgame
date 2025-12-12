using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using System;

namespace MacGame.Behaviors
{
    /// <summary>
    /// A behavior for NPCs or something where they just walk around and stand still randomly.
    /// </summary>
    public class WalkRandomlyBehavior : Behavior
    {
        public float actionTimer = 0.0f;
        public const float actionTimerLimit = 2.0f;
        private string _idleAnimationName;
        private string _walkAnimationName;
        private Vector2 OriginalPosition;

        const float MaxWalkDistance = Game1.TileSize * 3;
        const float WalkSpeed = 30f;

        public WalkRandomlyBehavior(string idleAnimationName, string walkAnimationName)
        {
            _idleAnimationName = idleAnimationName;
            _walkAnimationName = walkAnimationName;
        }

        public override void Update(GameObject gameObject, GameTime gameTime, float elapsed)
        {

            if (OriginalPosition == default)
            {
                OriginalPosition = gameObject.WorldLocation;
            }

            var animations = (AnimationDisplay)gameObject.DisplayComponent;

            // Randomly walk left and right or go idle.
            actionTimer -= elapsed;
            if (actionTimer <= 0)
            {
                actionTimer = actionTimerLimit;
                gameObject.Velocity = new Vector2(0, gameObject.Velocity.Y);

                int action = Game1.Randy.Next(0, 3);
                if (action == 0 || animations.CurrentAnimationName == _walkAnimationName)
                {
                    animations.Play(_idleAnimationName);
                }
                else /* This happens more often than idle. */
                {
                    animations.Play(_walkAnimationName);

                    gameObject.Velocity = new Vector2(WalkSpeed * (gameObject.Flipped ? -1 : 1), gameObject.Velocity.Y);
                }
            }

            // Turn around if they're walking off the edge.
            if (animations.CurrentAnimationName == _walkAnimationName)
            {
                var shouldFlip = false;

               // Get the distance from the gameObject and it's original location in the x space only
               var distance = Math.Abs(gameObject.WorldLocation.X - OriginalPosition.X);

                if (gameObject.Flipped && gameObject.OnLeftWall)
                {
                    shouldFlip = true;
                }
                else if (!gameObject.Flipped && gameObject.OnRightWall)
                {
                    shouldFlip = true;
                }
                else if (Math.Abs(gameObject.WorldLocation.X - OriginalPosition.X) > MaxWalkDistance)
                {
                    // You walked too far.
                    shouldFlip = true;
                }
                else
                {
                    // About to walk off the edge.
                    var pointDownInFront = new Vector2(gameObject.WorldLocation.X.ToInt() + (4 * (gameObject.Flipped ? -1 : 1)), gameObject.WorldLocation.Y.ToInt() + 4);
                    var mapSquare = Game1.CurrentLevel.Map.GetMapSquareAtPixel(pointDownInFront);
                    if (mapSquare != null && (mapSquare.Passable && !mapSquare.IsPlatform))
                    {
                        shouldFlip = true;
                    }
                }
                
                if (shouldFlip)
                {
                    gameObject.Flipped = !gameObject.Flipped;
                    gameObject.Velocity = new Vector2(WalkSpeed * (gameObject.Flipped ? -1 : 1), gameObject.Velocity.Y);
                }
            }

        }
    }
}
