﻿using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class Frog : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        float jumpTimer = 2f;

        public Frog(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\BigTextures");
            var idle = new AnimationStrip(textures, Helpers.GetBigTileRect(3, 0), 1, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.14f;
            animations.Add(idle);

            animations.Play("idle");

            var jump = new AnimationStrip(textures, Helpers.GetBigTileRect(3, 0), 3, "jump");
            jump.LoopAnimation = false;
            jump.FrameLength = 0.1f;
            jump.Oscillate = true;
            jump.NextAnimation = "reverseJump";
            animations.Add(jump);


            var reverseJump = new AnimationStrip(textures, Helpers.GetBigTileRect(3, 0), 3, "reverseJump");
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
            worldLocation.Y += 8;
            CollisionRectangle = new Rectangle(-4, -16, 8, 8);

        }

        public override void Kill()
        {
            EffectsManager.EnemyPop(WorldCenter, 10, Color.White, 30f);

            Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (Alive && Game1.Camera.IsObjectVisible(CollisionRectangle))
            {
                jumpTimer -= elapsed;

                if (jumpTimer <= 0)
                {
                    jumpTimer = 2;
                    animations.Play("jump");
                    SoundManager.PlaySound("jump", 0.5f, -0.2f);
                    velocity.Y -= 150;
                    velocity.X = 30;
                    if (Game1.Randy.NextBool())
                    {
                        velocity.X *= -1;
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