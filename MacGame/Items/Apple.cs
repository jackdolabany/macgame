using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TileEngine;

namespace MacGame.Items
{
    /// <summary>
    /// The apple projectile object. Not to be confused with the Apple item that you pick up that looks the same.
    /// </summary>
    public class Apple : GameObject
    {
        private Player _player;

        public Apple(ContentManager content, int cellX, int cellY, Player player)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var image = new StaticImageDisplay(textures);
            DisplayComponent = image;
            image.Source = Helpers.GetTileRect(14, 0);
            SetWorldLocationCollisionRectangle(8, 8);
            IsAffectedByGravity = false;
            isTileColliding = false;
            _player = player;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Enabled)
            {
                if (!Game1.Camera.IsObjectVisible(this.CollisionRectangle))
                {
                    ReturnApple();
                }

                var mapSquare = Game1.CurrentMap.GetMapSquareAtPixel(this.WorldCenter);
                if (mapSquare != null && !mapSquare.Passable)
                {
                    this.Smash();
                }
            }
            
            base.Update(gameTime, elapsed);

        }

        private void ReturnApple()
        {
            this.Enabled = false;
            _player.Apples.ReturnObject(this);
        }

        public void Smash()
        {
            ReturnApple();
            EffectsManager.EnemyPop(this.WorldCenter, 7, Color.Red, 80);
        }
    }
}
