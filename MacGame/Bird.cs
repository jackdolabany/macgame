using System;
using MacGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
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
            this.DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var fly = new AnimationStrip(textures, new Rectangle(8 * 8, 3 * 8, 8, 8), 2, "fly");
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

            tileLocation = this.WorldLocation;
            Enabled = false;
            this.flipped = true;
            nextBirdTimer = 1f;
        }

        public override void Kill()
        {
            EffectsManager.EnemyPop(this.WorldCenter, 10, Color.White, 30f);

            this.Enabled = false;
            base.Kill();

            // Next bird is delayed a bit if you kill it.
            nextBirdTimer = 6f;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!Enabled && this.camera.IsPointVisible(tileLocation))
            {
                nextBirdTimer -= elapsed;
                if (nextBirdTimer <= 0)
                {
                    this.Alive = true;
                    this.Enabled = true;
                    this.worldLocation = new Vector2(this.camera.ViewPort.Right + 8, tileLocation.Y);

                    // Randomly the bird might come across the middle or 
                    // top or bottom of the screen.
                    var rando = Game1.Randy.Next(1, 4);
                    if (rando == 1)
                    {
                        this.worldLocation.Y += 16;
                    }
                    else if (rando == 2)
                    {
                        this.worldLocation.Y -= 16;
                    }

                    this.Velocity = new Vector2(-30, 0);
                }
            }

            if (Enabled && Alive)
            {
                // Reset the timer for the bird to come across the screen after he flies off it.
                if (this.CollisionRectangle.Right < (this.camera.ViewPort.Left - 8))
                {
                    this.Enabled = false;
                    nextBirdTimer = 1f;
                }
            }

            base.Update(gameTime, elapsed);

        }
    }
}