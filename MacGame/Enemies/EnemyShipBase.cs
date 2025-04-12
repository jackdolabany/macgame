using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public abstract class EnemyShipBase : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 30;

        public EnemyShipBase(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;
            Flipped = true;
            InvincibleTimeAfterBeingHit = 0.1f;
        }

        public override void Kill()
        {
            EffectsManager.AddExplosion(WorldCenter);
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