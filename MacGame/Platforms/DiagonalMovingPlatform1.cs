using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Platforms
{
    /// <summary>
    /// Moves up and to the right.
    /// </summary>
    public class DiagonalMovingPlatform1 : MovingPlatform
    {
        public DiagonalMovingPlatform1(ContentManager content, int cellX, int cellY)
            : base(content, cellX, cellY)
        {
            MoveDirection = new Vector2(1, 1);
        }
    }
}
