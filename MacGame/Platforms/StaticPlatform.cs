using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Platforms
{
    public class StaticPlatform : Platform
    {
        public StaticPlatform(ContentManager content, int cellX, int cellY)
            : base(content, cellX, cellY)
        {
            this.DisplayComponent = new StaticImageDisplay(content.Load<Texture2D>(@"Textures/Textures"), new Rectangle(0, 4*8, 8, 8));
            this.CollisionRectangle = new Rectangle(-3, -8, 6, 3);
        }
    }
}
