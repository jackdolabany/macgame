using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
{

    public class RedKeyblock : Keyblock
    {

        public RedKeyblock(ContentManager content, int cellX, int cellY, Player player, bool isLocked) : base(content, cellX, cellY, player, isLocked)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");
            lockedImage = new StaticImageDisplay(textures, Helpers.GetTileRect(9, 27));
            unlockedImage = new StaticImageDisplay(textures, Helpers.GetTileRect(9, 28));
            this.DisplayComponent = lockedImage;
        }

        protected override bool HasKey()
        {
            return Game1.StorageState.Levels[Game1.CurrentLevel.LevelNumber].Keys.HasRedKey;
        }

    }
}
