using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class SpaceMouth : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 75;
        const int MaxHealth = 4;

        /// <summary>
        /// Small and fast.
        /// </summary>
        public SpaceMouth(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\BigTextures");
            var fly = new AnimationStrip(textures, Helpers.GetBigTileRect(11, 3), 2, "fly");
            fly.LoopAnimation = true;
            fly.FrameLength = 0.14f;
            animations.Add(fly);

            animations.Play("fly");

            isEnemyTileColliding = false;
            Attack = 1;
            Health = MaxHealth;
            IsAffectedByGravity = false;

            SetCenteredCollisionRectangle(16, 16, 10, 10);
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            isTileColliding = false;

            Flipped = true;

            InvincibleTimeAfterBeingHit = 0.1f;
        }

        public override void Kill()
        {
            EffectsManager.AddExplosion(WorldCenter);

            Enabled = false;
            base.Kill();
        }

        public void Reset()
        {
            Health = MaxHealth;
            Alive = true;
            Enabled = true;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!camera.IsWayOffscreen(this.CollisionRectangle))
            {
                velocity.X = -speed;
            }
            else
            {
                velocity = Vector2.Zero;
            }

            base.Update(gameTime, elapsed);

        }
    }
}