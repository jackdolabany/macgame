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
            Behavior?.Reset();
        }

        protected void SetInitialHealth(int health)
        {
            Health = health;
            _initialHealth = health;
        }

        public override void Kill()
        {
            PlayDeathEffects();
            base.Kill();
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
        /// Fires off several explosions, staggered so they don't all go off at the same instant,
        /// each placed randomly within the ship's collision rectangle, with screen shake. Meant
        /// for bigger ships whose death should feel like more of an event.
        /// </summary>
        protected void PlayMultiExplosionDeathEffect(int count = 5)
        {
            for (int i = 0; i < count; i++)
            {
                var offset = new Vector2(
                    Game1.Randy.Next(-CollisionRectangle.Width / 2, CollisionRectangle.Width / 2 + 1),
                    Game1.Randy.Next(-CollisionRectangle.Height / 2, CollisionRectangle.Height / 2 + 1));
                var explosionLocation = CollisionCenter + offset;

                var delay = i * 0.1f + (float)Game1.Randy.NextDouble() * 0.15f;
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