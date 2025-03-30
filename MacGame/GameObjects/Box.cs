using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TileEngine;

namespace MacGame
{
    public class Box : PickupObject, ICustomCollisionObject
    {
        /// <summary>
        /// Temporarily disable collision with the player when the box is dropped. That will help so that Mac
        /// doesn't trap himself.
        /// </summary>
        private bool collideWithPlayer = true;

        public Box(ContentManager content, int x, int y, Player player) : base (content, x, y, player)
        {
            var idle = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), Helpers.GetTileRect(15, 2));
            this.DisplayComponent = idle;

            Enabled = true;

            IsAffectedByGravity = true;

            this.SetWorldLocationCollisionRectangle(8, 8);
        }

        public bool DoesCollideWithObject(GameObject obj)
        {
            if (obj is Player && !collideWithPlayer)
            {
                return false;
            }
            return true;
        }

        public override void Drop()
        {
            base.Drop();
            collideWithPlayer = false;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!collideWithPlayer)
            {
                // Collide with the player again as soon as they aren't overlapping.
                if (!CollisionRectangle.Intersects(_player.CollisionRectangle))
                {
                    collideWithPlayer = true;
                }
            }
            

            base.Update(gameTime, elapsed);
        }


    }

}
