using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Items
{
    /// <summary>
    /// A red, green, or blue key that can be used to doors of the same color.
    /// </summary>
    public class BlueKey : Item
    {

        public BlueKey(ContentManager content, int cellX, int cellY, Player player, Camera camera) : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var image = new StaticImageDisplay(textures);
            DisplayComponent = image;
            image.Source = Helpers.GetTileRect(15, 4);
            SetWorldLocationCollisionRectangle(8, 8);
            _player = player;
            IsInChest = false;
        }

        public override void WhenCollected(Player player)
        {
            this.Enabled = false;
            Game1.StorageState.Levels[Game1.CurrentLevel.LevelNumber].Keys.HasBlueKey = true;
            StorageManager.TrySaveGame();
        }
    }
}
