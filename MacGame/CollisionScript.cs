using Microsoft.Xna.Framework;

namespace MacGame
{
    /// <summary>
    /// Use this to trigger custom code on a level. If mac intersects this Rectangle we'll do some custom thing. 
    /// Custom code will be in Level.cs (or other places? what do I know).
    /// 
    /// Add a rectangle to the map and give it two properties
    /// 
    /// LoadClass: CollisionScript
    /// Script: UniqueNameHere
    /// 
    /// Then add a case statement in Level.cs to handle the UniqueNameHere script.
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
        public string Script { get; set; } = "";

        /// <summary>
        /// So they don't execute multiple times.
        /// </summary>
        public bool Enabled = true;
    }
}
