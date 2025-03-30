using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class EnemyShip4 : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 30;

        public EnemyShip4(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\SpaceTextures");
            var fly = new AnimationStrip(textures, Helpers.GetTileRect(3, 2), 1, "fly");
            fly.LoopAnimation = true;
            fly.FrameLength = 0.14f;
            animations.Add(fly);

            animations.Play("fly");

            isEnemyTileColliding = false;
            Attack = 1;
            Health = 4;
            IsAffectedByGravity = false;

            SetCenteredCollisionRectangle(8, 8, 8, 8);

            Flipped = true;

            InvincibleTimeAfterBeingHit = 0.1f;
        }

        public override void Kill()
        {
            EffectsManager.AddExplosion(WorldCenter);

            Enabled = false;
            base.Kill();
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