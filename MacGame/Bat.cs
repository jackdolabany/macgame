using System;
using MacGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
{
    public class Bat : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 10;
        private float startLocationX;
        private float maxTravelDistance = 12;

        public Bat(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            this.DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var fly = new AnimationStrip(textures, new Rectangle(0, 8 * 8, 8, 8), 2, "fly");
            fly.LoopAnimation = true;
            fly.FrameLength = 0.14f;
            animations.Add(fly);

            animations.Play("fly");

            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;

            SetCenteredCollisionRectangle(6, 7);

            startLocationX = this.WorldLocation.X;
        }

        public override void Kill()
        {
            EffectsManager.EnemyPop(this.WorldCenter, 10, Color.White, 30f);

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

            if(this.velocity.X > 0 && travelDistance >= maxTravelDistance)
            {
                this.flipped = !this.flipped;
            }
            else if (this.velocity.X < 0 && travelDistance <= -maxTravelDistance)
            {
                this.flipped = !this.flipped;
            }

            base.Update(gameTime, elapsed);

        }
    }
}