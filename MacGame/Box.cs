using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TileEngine;

namespace MacGame
{
    public class Box : PickupObject
    {

        public Box(ContentManager content, int x, int y, Player player) : base (content, x, y, player)
        {
            var idle = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), Helpers.GetTileRect(15, 2));
            this.DisplayComponent = idle;

            Enabled = true;

            IsAffectedByGravity = true;

            this.SetCenteredCollisionRectangle(8, 8);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);
        }

    }

}
