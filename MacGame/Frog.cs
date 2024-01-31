using System;
using MacGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
{
    public class Frog : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        float jumpTimer = 2f;

        public Frog(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            this.DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var idle = new AnimationStrip(textures, new Rectangle(0, 6 * 8, 8, 16), 1, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.14f;
            animations.Add(idle);

            animations.Play("idle");

            var jump = new AnimationStrip(textures, new Rectangle(0, 6 * 8, 8, 16), 3, "jump");
            jump.LoopAnimation = false;
            jump.FrameLength = 0.1f;
            jump.Oscillate = true;
            jump.NextAnimation = "reverseJump";
            animations.Add(jump);


            var reverseJump = new AnimationStrip(textures, new Rectangle(0, 6 * 8, 8, 16), 3, "reverseJump");
            reverseJump.LoopAnimation = false;
            reverseJump.FrameLength = 0.1f;
            reverseJump.Oscillate = true;
            reverseJump.Reverse = true;
            reverseJump.NextAnimation = "idle";
            animations.Add(reverseJump);

            isEnemyTileColliding = true;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = true;

            // Push him down 8 pixels because his collsiion rect is only the top 8 pixels to not
            // count his jumping frog legs.
            this.worldLocation.Y += 8;
            CollisionRectangle = new Rectangle(-4, -16, 8, 8);

        }

        public override void Kill()
        {
            EffectsManager.EnemyPop(this.WorldCenter, 10, Color.White, 30f);

            this.Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (Alive && Game1.Camera.IsObjectVisible(this.CollisionRectangle))
            {
                jumpTimer -= elapsed;

                if (jumpTimer <= 0)
                {
                    jumpTimer = 2;
                    animations.Play("jump");
                    SoundManager.PlaySound("jump", 0.5f, -0.2f);
                    this.velocity.Y -= 150;
                    this.velocity.X = 30;
                    if (Game1.Randy.NextBool())
                    {
                        this.velocity.X *= -1;
                    }
                }
            }

            if (velocity.Y == 0)
            {
                velocity.X = 0;
            }

            base.Update(gameTime, elapsed);

        }
    }
}