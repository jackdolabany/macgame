using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class GreenFish : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 40;
        private float maxTravelDistance = 5 * Game1.TileSize;

        private float minXLocation;
        private float maxXLocation;

        public GreenFish(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var swim = new AnimationStrip(textures, Helpers.GetTileRect(12, 5), 2, "swim");
            swim.LoopAnimation = true;
            swim.FrameLength = 0.3f;
            animations.Add(swim);

            animations.Play("swim");

            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;

            SetCenteredCollisionRectangle(6, 5);

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

            base.Update(gameTime, elapsed);

        }
    }
}