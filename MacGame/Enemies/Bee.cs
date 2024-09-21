using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class Bee : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 40;
        private float startLocationY;
        private float maxTravelDistance = Game1.TileSize;
        private bool goingUp = false;

        public Bee(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var fly = new AnimationStrip(textures, Helpers.GetTileRect(3, 2), 2, "fly");
            fly.LoopAnimation = true;
            fly.FrameLength = 0.14f;
            animations.Add(fly);

            animations.Play("fly");

            isTileColliding = false;
            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;

            SetCenteredCollisionRectangle(6, 7);

            startLocationY = WorldLocation.Y;
        }

        public override void Kill()
        {
            EffectsManager.EnemyPop(WorldCenter, 10, Color.White, 120f);

            Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (Alive)
            {
                velocity.Y = speed;
                if (goingUp)
                {
                    velocity.Y *= -1;
                }
            }

            var travelDistance = WorldLocation.Y.ToInt() - startLocationY;

            if (velocity.Y > 0 && travelDistance >= maxTravelDistance)
            {
                goingUp = !goingUp;
            }
            else if (velocity.Y < 0 && travelDistance <= -maxTravelDistance)
            {
                goingUp = !goingUp;
            }

            Flipped = WorldCenter.X >= Player.WorldCenter.X;

            base.Update(gameTime, elapsed);

        }
    }
}