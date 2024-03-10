using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class Cricket : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 10;
        private float startLocationX;
        private float maxTravelDistance = 16;
        private float jumpTimer = 1f;

        public Cricket(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var walk = new AnimationStrip(textures, new Rectangle(3 * 8, 7 * 8, 8, 8), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.14f;
            animations.Add(walk);

            animations.Play("walk");

            isEnemyTileColliding = true;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = true;

            SetCenteredCollisionRectangle(8, 5);

            startLocationX = WorldLocation.X;
        }

        public override void Kill()
        {
            EffectsManager.EnemyPop(WorldCenter, 10, Color.White, 30f);

            Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (Alive)
            {
                velocity.X = speed;
                if (flipped)
                {
                    velocity.X *= -1;
                }
            }

            var travelDistance = (int)WorldCenter.X - startLocationX;

            if (velocity.X > 0 && travelDistance >= maxTravelDistance)
            {
                flipped = !flipped;
            }
            else if (velocity.X < 0 && travelDistance <= -maxTravelDistance)
            {
                flipped = !flipped;
            }

            jumpTimer -= elapsed;
            if (jumpTimer <= 0)
            {
                jumpTimer = 1.0f;
                if (OnGround)
                {
                    velocity.Y -= 100;
                }
            }

            base.Update(gameTime, elapsed);

        }
    }
}