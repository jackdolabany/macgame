using Microsoft.Xna.Framework;

namespace MacGame
{
    /// <summary>
    /// Use this class to nudge the camera in a direction on the level for certain sections.
    /// 
    /// Add an ObjectModifier to the map, no name.
    /// Give it a property called "CameraOffset" with a value like "0,-150"
    /// </summary>
    public class CameraOffsetZone
    {
        public Rectangle CollisionRectangle { get; set; }
        public Vector2 Offset { get; set; }
    }
}
