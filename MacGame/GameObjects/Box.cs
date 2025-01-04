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

            // check for force interactions between the player and this box
            // While the thing is moving it won't particiapte in perfect pixel x collisions.
            // isntead we'll do this half assed force thing.
            if (velocity != Vector2.Zero)
            {
                if (!IsPickedUp && !WasRecentlyDropped && this._player.CollisionRectangle.Intersects(this.CollisionRectangle))
                {

                    var directionToPushMac = _player.CollisionCenter- this.CollisionCenter;
                    directionToPushMac.Normalize();
                    var forceToPushMac =  ( _player.Velocity - this.Velocity);
                    forceToPushMac = new Vector2(Math.Abs(forceToPushMac.X), Math.Abs(forceToPushMac.Y));

                    _player.Velocity = directionToPushMac * forceToPushMac * 1.2f;
                }

            }

            base.Update(gameTime, elapsed);
        }

    }

}
