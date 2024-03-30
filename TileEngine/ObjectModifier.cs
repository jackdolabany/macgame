using Microsoft.Xna.Framework;

namespace TileEngine
{
    /// <summary>
    /// Maps to objects in Tiled. These modifiers are rectangles with custom properties we can analyze to 
    /// add one time, non-reusable modifiers to things on the map.
    /// </summary>
    public class ObjectModifier 
    {
        public string Name;
        public Rectangle Rectangle;
        public Dictionary<string, string> Properties = new Dictionary<string, string>();
    }
}
