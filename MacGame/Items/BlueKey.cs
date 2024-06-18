﻿using MacGame.DisplayComponents;
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
            SetCenteredCollisionRectangle(8, 8);
            _player = player;
            IsInChest = false;
            this.Enabled = !player.HasBlueKey;
        }

        public override void WhenCollected(Player player)
        {
            player.HasBlueKey = true;
            this.Enabled = false;
        }
    }
}
