using Microsoft.Xna.Framework;

namespace MacGame
{
    /// <summary>
    /// Use this to trigger custom code on a level. If mac intersects this Rectangle we'll do some custom thing. 
    /// Custome code will be in Level.cs (or other places? what do I know).
    /// </summary>
    public class CollisionScript
    {
        /// <summary>
        /// A rectangle to check if the player collides with this.
        /// </summary>
        public Rectangle CollisionRectangle { get; set; }

        /// <summary>
        /// Give it a unique name.
        /// </summary>
        public string Name { get; set; }
    }
}
