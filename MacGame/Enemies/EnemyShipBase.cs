using System;
using MacGame.Behaviors;
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

        protected Behavior? Behavior { get; set; }

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
            if (Behavior != null)
            {
                Behavior.Update(this, gameTime, elapsed);
            }

            base.Update(gameTime, elapsed);

        }
    }
}