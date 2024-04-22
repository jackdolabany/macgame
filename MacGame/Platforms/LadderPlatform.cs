using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Platforms
{
    /// <summary>
    /// An invisible platform at the top of ladders. If you climb to the top you are standing on this ladder.
    /// </summary>
    public class LadderPlatform : Platform
    {
        public LadderPlatform(ContentManager content, int cellX, int cellY)
            : base(content, cellX, cellY)
        {
            this.DisplayComponent = new NoDisplay();
            this.CollisionRectangle = new Rectangle(-12, -32, 24, 12);
        }
    }
}
