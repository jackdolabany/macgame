using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
    /// <summary>
    /// The football helmet drawn on Mac's head. Not the item to get the helmet (FootballHelmet.cs).
    /// </summary>
    public class MacFootballHelmet : GameObject
    {
        private StaticImageDisplay _frontDisplay;
        private StaticImageDisplay _backDisplay;

        public MacFootballHelmet(Texture2D textures2)
        {
            _frontDisplay = new StaticImageDisplay(textures2, Helpers.GetTileRect(15, 15));
            _backDisplay = new StaticImageDisplay(textures2, Helpers.GetTileRect(15, 16));
            DisplayComponent = _frontDisplay;
            Enabled = true;

            CollisionRectangle = new Rectangle(-3 * Game1.TileScale, -8 * Game1.TileScale, 6 * Game1.TileScale, 6 * Game1.TileScale);
        }

        public void Front()
        {
            DisplayComponent = _frontDisplay;
        }

        public void Back()
        {
            DisplayComponent = _backDisplay;
        }

        public override void SetDrawDepth(float depth)
        {
            _frontDisplay.DrawDepth = depth;
            _backDisplay.DrawDepth = depth;
        }
    }
}
