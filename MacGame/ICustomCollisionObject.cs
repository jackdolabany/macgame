using Microsoft.Xna.Framework;

namespace MacGame
{
    public interface ICustomCollisionObject
    {
        Rectangle CollisionRectangle { get; set; }
        Vector2 WorldLocation { get; set; }
        bool DoesCollideWithObject(GameObject obj);
    }
}
