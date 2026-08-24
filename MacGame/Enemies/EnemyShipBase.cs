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

        private int _initialHealth;
        private bool _hasBeenOnScreen = false;

        // When set by PlayMultiExplosionDeathEffect, the ship stays visible/enabled until this
        // much time has passed, instead of disappearing the instant Kill() is called.
        private float _disableAfterDeathDelay = 0f;

        public EnemyShipBase(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            isEnemyTileColliding = false;
            isTileColliding = false;
            Attack = 1;
            IsAffectedByGravity = false;
            Flipped = true;
            InvincibleTimeAfterBeingHit = 0.1f;
        }

        public void Revive(Vector2 worldLocation, float speed)
        {
            WorldLocation = worldLocation;
            Velocity = new Vector2(-speed, 0);
            Enabled = true;
            Alive = true;
            Health = _initialHealth;
            InvincibleTimer = 0;
            _hasBeenOnScreen = false;
            _disableAfterDeathDelay = 0f;
            Behavior?.Reset();
        }

        protected void SetInitialHealth(int health)
        {
            Health = health;
            _initialHealth = health;
        }

        public override void Kill()
        {
            _disableAfterDeathDelay = 0f;
            PlayDeathEffects();
            base.Kill();
        }

        /// <summary>
        /// Some ships might explode for a bit as they die. Use this to delay them being disabled so they 
        /// don't disappear right away.
        /// </summary>
        protected override void DisableAfterDeath()
        {
            if (_disableAfterDeathDelay > 0f)
            {
                TimerManager.AddNewTimer(_disableAfterDeathDelay, () => Enabled = false);
            }
            else
            {
                Enabled = false;
            }
        }

        /// <summary>
        /// Override to customize the visual/audio effects played when the ship dies. Defaults to
        /// a single explosion at its center.
        /// </summary>
        protected virtual void PlayDeathEffects()
        {
            EffectsManager.AddExplosion(WorldCenter);
        }

        /// <summary>
        /// Fire off several explosions. 
        /// </summary>
        protected void PlayMultiExplosionDeathEffect(int count = 4)
        {
            // So the ship doesn't disappear right away.
            _disableAfterDeathDelay = 0.4f;

            for (int i = 0; i < count; i++)
            {
                var offset = new Vector2(
                    Game1.Randy.Next(-CollisionRectangle.Width / 2, CollisionRectangle.Width / 2 + 1),
                    Game1.Randy.Next(-CollisionRectangle.Height / 2, CollisionRectangle.Height / 2 + 1));
                var explosionLocation = CollisionCenter + offset;

                var delay = i * 0.18f;
                TimerManager.AddNewTimer(delay, () => EffectsManager.AddExplosion(explosionLocation, withShake: true));
            }
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (IsOnScreen())
            {
                _hasBeenOnScreen = true;
            }

            if (Alive && _hasBeenOnScreen && camera.IsWayOffscreen(CollisionRectangle))
            {
                Enabled = false;
                return;
            }

            if (Behavior != null)
            {
                Behavior.Update(this, gameTime, elapsed);
            }

            base.Update(gameTime, elapsed);
        }
    }
}