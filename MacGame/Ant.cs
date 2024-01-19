﻿using System;
using MacGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
{
    public class Ant : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 10;
        private float startLocationX;
        private float maxWalkDistance = 8;

        public Ant(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            this.DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var walk = new AnimationStrip(textures, new Rectangle(24, 8, 8, 8), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.14f;
            animations.Add(walk);

            animations.Play("walk");

            isEnemyTileColliding = true;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = true;

            SetCenteredCollisionRectangle(6, 7);

            startLocationX = this.WorldLocation.X;
        }

        public override void Kill()
        {
            EffectsManager.EnemyPop(this.WorldCenter, 10, Color.Red, 50f);

            this.Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (Alive)
            {
                this.velocity.X = speed;
                if (flipped)
                {
                    this.velocity.X *= -1;
                }
            }

            var travelDistance = (int)this.WorldCenter.X - startLocationX;

            if(this.velocity.X > 0 && travelDistance >= maxWalkDistance)
            {
                this.flipped = !this.flipped;
            }
            else if (this.velocity.X < 0 && travelDistance <= -maxWalkDistance)
            {
                this.flipped = !this.flipped;
            }

            base.Update(gameTime, elapsed);

        }
    }
}