using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;
using TileEngine;

namespace MacGame.Enemies
{
    public abstract class BigShipWeakSpot : Enemy
    {
        protected BigShipBoss _bigShip;

        private float _flashTimer = 0f;

        private const float FlashInterval = 2f;
        private const float FlashDuration = 1f / 3f;

        protected AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        protected BigShipWeakSpot(ContentManager content, int cellX, int cellY, Player player, Camera camera, BigShipBoss bigShip)
            : base(content, cellX, cellY, player, camera)
        {
            _bigShip = bigShip;

            DisplayComponent = new AnimationDisplay();

            IsAffectedByGravity = false;
            IsAffectedByForces = false;
            isEnemyTileColliding = false;
            isTileColliding = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            IsPlayerColliding = false;
            Attack = 0;
            Health = int.MaxValue;
            InvincibleTimeAfterBeingHit = 0.1f;
            FlashesInvisibleWhenHit = false;
        }

        public override void TakeHit(GameObject attacker, int damage, Vector2 force)
        {
            if (IsTempInvincibleFromBeingHit || Dead || !Enabled)
            {
                return;
            }
   
            _bigShip.TakeHit(attacker, damage, force);
            InvincibleTimer += InvincibleTimeAfterBeingHit;
        }

        public override void Kill()
        {
            // Weak spots never die on their own — the boss controls their lifetime.
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!_bigShip.HasBeenSeen || _bigShip.Dead)
            {
                Enabled = false;
                return;
            }

            Enabled = true;

            if (IsTempInvincibleFromBeingHit)
            {
                animations.PlayIfNotAlreadyPlaying("white");
            }
            else
            {
                _flashTimer += elapsed;
                if (_flashTimer >= FlashInterval)
                {
                    _flashTimer -= FlashInterval;
                }

                if (_flashTimer >= FlashInterval - FlashDuration)
                {
                    animations.PlayIfNotAlreadyPlaying("orange");
                }
                else
                {
                    animations.PlayIfNotAlreadyPlaying("normal");
                }
            }

            base.Update(gameTime, elapsed);
        }
    }
}
