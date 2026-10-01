using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    /// <summary>
    /// The football helmet drawn on Mac's head. Not the item to get the helmet (FootballHelmet.cs).
    /// </summary>
    public class MacFootballHelmet : Headwear
    {
        protected override Rectangle frontSource => Helpers.GetBigTileRect(4, 2);
        protected override Rectangle backSource => Helpers.GetBigTileRect(5, 2);

        public MacFootballHelmet(ContentManager content) : base(content)
        {
            CollisionRectangle = new Rectangle(-4 * Game1.TileScale, -12 * Game1.TileScale, 6 * Game1.TileScale, 6 * Game1.TileScale);
            mirrorCollisionRectangleWhenFlipped = true;
        }
    }
}
