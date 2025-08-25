using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Doors
{

    /// <summary>
    /// A black empty doorway that you can go through. As opposed to a door that opens or is locked.
    /// </summary>
    public class Doorway : Door
    {

        public Doorway(ContentManager content, int cellX, int cellY, Player player)
            : base(content, cellX, cellY, player)
        {
            //var sid = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\BigTextures"), Helpers.GetBigTileRect(3, 1));
            DisplayComponent = new NoDisplay();
        }

        public override void PlayerSlidingOut()
        {
            // Do nothing.
        }
    }
}
