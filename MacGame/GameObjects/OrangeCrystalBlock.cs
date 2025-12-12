using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
{

    public class OrangeCrystalBlock : CrystalBlock
    {

        public OrangeCrystalBlock(ContentManager content, int cellX, int cellY) : base(content, cellX, cellY)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");
            solidImage = new StaticImageDisplay(textures, Helpers.GetTileRect(12, 28));
            passableImage = new StaticImageDisplay(textures, Helpers.GetTileRect(12, 29));
            this.DisplayComponent = solidImage;
        }

        public override void Initialize()
        {
            if (Game1.LevelState.CrystalSwitchIsOrange)
            {
                Open();
            }
            else
            {
                Close();
            }
        }

    }
}
