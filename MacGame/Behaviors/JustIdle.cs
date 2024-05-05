using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;

namespace MacGame.Behaviors
{
    public class JustIdle : Behavior
    {
        private string _idleAnimationName;

        public JustIdle(string idleAnimationName)
        {
            _idleAnimationName = idleAnimationName;
        }

        public override void Update(GameObject gameObject, GameTime gameTime, float elapsed)
        {
            var animations = (AnimationDisplay)gameObject.DisplayComponent;
            if (animations.CurrentAnimationName != _idleAnimationName)
            {
                animations.Play(_idleAnimationName);
            }
        }
    }
}
