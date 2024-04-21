using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class Ant : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 10;
        private float startLocationX;
        private float maxTravelDistance = 8;

        public Ant(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var walk = new AnimationStrip(textures, Helpers.GetTileRect(3, 1), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.14f;
            animations.Add(walk);

            animations.Play("walk");

            isEnemyTileColliding = true;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = true;

            SetCenteredCollisionRectangle(6, 7);

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

            base.Update(gameTime, elapsed);

        }
    }
}