using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class Bird : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        // This is where the starting tile is. If this tile is on the screen (it's invisible)
        // the bird will fly across the screen repeatedly.
        private Vector2 tileLocation;

        private float nextBirdTimer;

        public Bird(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var fly = new AnimationStrip(textures, Helpers.GetTileRect(8, 3), 2, "fly");
            fly.LoopAnimation = true;
            fly.FrameLength = 0.14f;
            animations.Add(fly);

            animations.Play("fly");

            Attack = 1;
            Health = 1;

            isTileColliding = false;
            isEnemyTileColliding = false;
            IsAffectedByGravity = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;

            SetCenteredCollisionRectangle(6, 6);

            tileLocation = WorldLocation;
            Enabled = false;
            Flipped = true;
            nextBirdTimer = 1f;
        }

        public override void Kill()
        {
            EffectsManager.EnemyPop(WorldCenter, 10, Color.White, 120f);

            Enabled = false;
            base.Kill();

            // Next bird is delayed a bit if you kill it.
            nextBirdTimer = 6f;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!Enabled && camera.IsPointVisible(tileLocation))
            {
                nextBirdTimer -= elapsed;
                if (nextBirdTimer <= 0)
                {
                    Alive = true;
                    Enabled = true;
                    worldLocation = new Vector2(camera.ViewPort.Right + 8, tileLocation.Y);

                    // Randomly the bird might come across the middle or 
                    // top or bottom of the screen.
                    var rando = Game1.Randy.Next(1, 4);
                    if (rando == 1)
                    {
                        worldLocation.Y += Game1.TileSize * 2;
                    }
                    else if (rando == 2)
                    {
                        worldLocation.Y -= Game1.TileSize * 2;
                    }

                    Velocity = new Vector2(-120, 0);
                }
            }

            if (Enabled && Alive)
            {
                // Reset the timer for the bird to come across the screen after he flies off it.
                if (CollisionRectangle.Right < camera.ViewPort.Left - 8)
                {
                    Enabled = false;
                    nextBirdTimer = 1f;
                }
            }

            base.Update(gameTime, elapsed);

        }
    }
}