using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;

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

                    gameObject.Velocity = new Vector2(30, gameObject.Velocity.Y);
                    gameObject.Flipped = false;
                    if (gameObject.WorldLocation.X > OriginalPosition.X)
                    {
                        gameObject.Flipped = true;
                        gameObject.Velocity = new Vector2(-gameObject.Velocity.X, gameObject.Velocity.Y);
                    }
                }
            }

        }
    }
}
