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
            this.DisplayComponent = new StaticImageDisplay(content.Load<Texture2D>(@"Textures/Textures"), Helpers.GetTileRect(0, 4));
            this.CollisionRectangle = new Rectangle(-12, -32, 24, 12);
        }

        /// <summary>
        /// Static Platforms have this helper because multiple tiles on the map are static platforms. This allows us to replace
        /// the image with whatever is coming from the map tile as we load this.
        /// </summary>
        public void SetTextureRectangle(Texture2D texture, Rectangle source)
        {
            this.DisplayComponent = new StaticImageDisplay(texture, source);
        }
    }
}
