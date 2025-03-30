using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class AlienBug : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 40;
        private float startLocationX;
        private float maxTravelDistance = 8 * Game1.TileScale;

        public AlienBug(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var walk = new AnimationStrip(textures, Helpers.GetTileRect(8, 24), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.14f;
            animations.Add(walk);

            animations.Play("walk");

            isEnemyTileColliding = true;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = true;

            SetWorldLocationCollisionRectangle(6, 7);

            startLocationX = WorldLocation.X;
        }

        public override void Kill()
        {
            EffectsManager.SmallEnemyPop(WorldCenter);

            Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (Alive)
            {
                velocity.X = speed;
                if (Flipped)
                {
                    velocity.X *= -1;
                }
            }

            var travelDistance = WorldCenter.X.ToInt() - startLocationX;

            if (velocity.X > 0 && travelDistance >= maxTravelDistance)
            {
                Flipped = !Flipped;
            }
            else if (velocity.X < 0 && travelDistance <= -maxTravelDistance)
            {
                Flipped = !Flipped;
            }

            base.Update(gameTime, elapsed);

        }
    }
}