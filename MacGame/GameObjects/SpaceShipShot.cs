using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TileEngine;

namespace MacGame.Items
{
    /// <summary>
    /// Shot from Mac's spaceship.
    /// </summary>
    public class SpaceshipShot : GameObject
    {
        private Player _player;

        public SpaceshipShot(ContentManager content, int cellX, int cellY, Player player, Camera camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\SpaceTextures");
            var image = new StaticImageDisplay(textures);
            DisplayComponent = image;
            image.Source = Helpers.GetTileRect(0, 1);
            
            SetCenteredCollisionRectangle(8, 8, 2, 2);

            IsAffectedByGravity = false;
            isTileColliding = false;
            // These are true because the update method gets them when they're off camera.
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            _player = player;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Enabled)
            {
                if (!Game1.Camera.IsObjectVisible(this.CollisionRectangle))
                {
                    ReturnShot();
                }

                var mapSquare = Game1.CurrentMap.GetMapSquareAtPixel(this.CollisionCenter);
                if (mapSquare != null && !mapSquare.Passable)
                {
                    this.Break();
                }
            }

            Flipped = this.velocity.X < 0;

            base.Update(gameTime, elapsed);

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        private void ReturnShot()
        {
            this.Enabled = false;
            _player.Shots.ReturnObject(this);
        }

        public void Break()
        {
            ReturnShot();
            EffectsManager.SmallEnemyPop(this.WorldCenter);
            SoundManager.PlaySound("Break");
        }
    }
}
