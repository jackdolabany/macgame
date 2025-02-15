using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class ElectricSeaweed : Enemy
    {

        StaticImageDisplay regularImage;
        StaticImageDisplay electrifiedImage;

        public float electrifiedTimer = 0f;
        public float electrifiedTimerGoal = 1f;

        public float imageFlipTimer = 0f;
        public float imageFlipTimerGoal = 0.1f;

        public ElectricSeaweed(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {

            var textures = content.Load<Texture2D>(@"Textures\Textures2");
            regularImage = new StaticImageDisplay(textures, Helpers.GetTileRect(0, 2));
            electrifiedImage = new StaticImageDisplay(textures, Helpers.GetTileRect(1, 2));

            DisplayComponent = regularImage;

            isTileColliding = false;
            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;

            SetCenteredCollisionRectangle(4, 4);

            // Shift it up a bit since we want it actualled centered and not centered on the bottom middle pixel.
            this.CollisionRectangle = new Rectangle(this.collisionRectangle.X, this.collisionRectangle.Y - 8, this.collisionRectangle.Width, this.collisionRectangle.Height);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (electrifiedTimer > 0 || this.DisplayComponent == electrifiedImage)
            {
                electrifiedTimer -= elapsed;
                imageFlipTimer += elapsed;
                if (imageFlipTimer >= imageFlipTimerGoal)
                {
                    imageFlipTimer -= imageFlipTimerGoal;
                    if (DisplayComponent == regularImage)
                    {
                        DisplayComponent = electrifiedImage;
                    }
                    else
                    {
                        DisplayComponent = regularImage;
                    }
                }
            }

            base.Update(gameTime, elapsed);
        }

        public override void AfterHittingPlayer()
        {
            base.AfterHittingPlayer();
            electrifiedTimer = 1f;
            SoundManager.PlaySound("Electric");
        }

    }

}