using System;
using MacGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
{
    public class Bee : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 10;
        private float startLocationY;
        private float maxWalkDistance = 8;

        public Bee(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            this.DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var walk = new AnimationStrip(textures, new Rectangle(24, 16, 8, 8), 2, "fly");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.14f;
            animations.Add(walk);

            animations.Play("fly");

            isEnemyTileColliding = true;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;

            SetCenteredCollisionRectangle(6, 7);

            startLocationY = this.WorldLocation.Y;
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
                this.velocity.Y = speed;
                if (flipped)
                {
                    this.velocity.Y *= -1;
                }
            }

            var travelDistance = (int)this.WorldCenter.Y - startLocationY;

            if(this.velocity.Y > 0 && travelDistance >= maxWalkDistance)
            {
                this.flipped = !this.flipped;
            }
            else if (this.velocity.Y < 0 && travelDistance <= -maxWalkDistance)
            {
                this.flipped = !this.flipped;
            }

            base.Update(gameTime, elapsed);

        }
    }
}