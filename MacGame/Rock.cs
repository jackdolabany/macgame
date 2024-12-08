using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TileEngine;

namespace MacGame
{
    public class Rock : PickupObject
    {
      
        public GameObject? GameObjectOnMe { get; set; }

        public Rock(ContentManager content, int x, int y, Player player) : base(content, x, y, player)
        {

            this.DisplayComponent = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), Helpers.GetTileRect(12, 3));

            Enabled = true;

            WorldLocation = new Vector2(x * TileMap.TileSize + TileMap.TileSize / 2, (y + 1) * TileMap.TileSize);
            IsAffectedByGravity = true;

            this.SetCenteredCollisionRectangle(8, 8);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);
        }

    }
}
