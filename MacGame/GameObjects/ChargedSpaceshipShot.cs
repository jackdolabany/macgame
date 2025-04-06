using MacGame.DisplayComponents;
using MacGame.Enemies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TileEngine;

namespace MacGame.Items
{
    /// <summary>
    /// Shot from Mac's spaceship.
    /// </summary>
    public class ChargedSpaceshipShot : GameObject
    {
        private Player _player;

        /// <summary>
        /// Player controls this to tag enemies hit with this shot so they don't get hit multiple times.
        /// </summary>
        public List<Enemy> EnemiesHit = new List<Enemy>();

        public int Strength 
        { 
            get
            {
                return 10;
            } 
        }

        public ChargedSpaceshipShot(ContentManager content, int cellX, int cellY, Player player, Camera camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\BigTextures");
            var image = new StaticImageDisplay(textures);
            DisplayComponent = image;
            image.Source = Helpers.GetBigTileRect(4, 11);
            
            SetCenteredCollisionRectangle(16, 16, 16, 8);

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
                    this.Enabled = false;
                }
            }

            Flipped = this.velocity.X < 0;

            base.Update(gameTime, elapsed);

        }

        public void Reset()
        {
            EnemiesHit.Clear();
            this.Enabled = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
