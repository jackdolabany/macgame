using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class Shark : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 75;
        private float maxTravelDistance = 8 * Game1.TileSize;

        private float minXLocation;
        private float maxXLocation;

        public Shark(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\BigTextures");
            var swim = new AnimationStrip(textures, Helpers.GetBigTileRect(9, 3), 2, "swim");
            swim.LoopAnimation = true;
            swim.FrameLength = 0.3f;
            animations.Add(swim);

            animations.Play("swim");

            isEnemyTileColliding = false;
            Attack = 1;
            Health = 3;
            IsAffectedByGravity = false;
            isTileColliding = false;

            // TODO: Play here, get r id of it
            SetCenteredCollisionRectangle(14, 6);
            collisionRectangle.Y -= 4 * Game1.TileScale;

            var startLocationX = WorldLocation.X;
            minXLocation = startLocationX - maxTravelDistance / 2;
            maxXLocation = startLocationX + maxTravelDistance / 2;
        }

        public override void Kill()
        {
            EffectsManager.SmallEnemyPop(WorldCenter);

            Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {



            if (Alive)
            {
                velocity.X = speed;
                if (Flipped)
                {
                    velocity.X *= -1;
                }
            }

            if (velocity.X > 0 && (WorldLocation.X >= maxXLocation || OnRightWall))
            {
                Flipped = !Flipped;
                minXLocation = WorldLocation.X - maxTravelDistance;
            }
            else if (velocity.X < 0 && (WorldLocation.X <= minXLocation || OnLeftWall))
            {
                Flipped = !Flipped;
                maxXLocation = WorldLocation.X + maxTravelDistance;
            }

            // Wobble them up and down a bit
            var frequency = 5f;
            var amplitude = 1.5f;
            var offSetY = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * frequency) * amplitude;
            this.worldLocation.Y += offSetY;
            
            base.Update(gameTime, elapsed);

        }
    }
}