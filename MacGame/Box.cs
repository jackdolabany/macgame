using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TileEngine;

namespace MacGame
{
    public class Box : GameObject//, IPickupObject
    {


        public Box(ContentManager content, int x, int y, Player player)
        {

            //_player = player;

            var idle = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), Helpers.GetTileRect(15, 2));
            this.DisplayComponent = idle;

            Enabled = true;

            WorldLocation = new Vector2(x * TileMap.TileSize + TileMap.TileSize / 2, (y + 1) * TileMap.TileSize);
            
            
            // TODO: Temp
            IsAffectedByGravity = false;


            this.SetCenteredCollisionRectangle(8, 8);
        }


        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);
        }

    }

}
