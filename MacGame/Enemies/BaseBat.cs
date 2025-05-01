using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// Base colors to handle blue and black bats. Why? Because they're in dark environments
    /// and this makes them visible in both background types.
    /// </summary>
    public abstract class BaseBat : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 40;
        private float startLocationX;
        private float maxTravelDistance = 12;
        
        public BaseBat(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var fly = new AnimationStrip(textures, GetTextureRectangle(), 2, "fly");
            fly.LoopAnimation = true;
            fly.FrameLength = 0.14f;
            animations.Add(fly);

            animations.Play("fly");

            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;

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

        protected abstract Rectangle GetTextureRectangle();
    }
}