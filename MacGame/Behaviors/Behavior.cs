using Microsoft.Xna.Framework;

namespace MacGame.Behaviors
{
    /// <summary>
    /// A class to share update logic using a component system.
    /// </summary>
    public abstract class Behavior
    {
        public abstract void Update(GameObject gameObject, GameTime gameTime, float elapsed);
    }
}
