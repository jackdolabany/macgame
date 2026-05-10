using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class BomberCarriage : Enemy
    {
        private bool _isFalling = false;
        private Vector2 _fallVelocity = Vector2.Zero;
        private const float FallAccelerationY = 30f;
        private const float RightDriftSpeed = 40f;
        private float _explosionTimer = 0f;
        private const float ExplosionInterval = 0.1f;

        public BomberCarriage(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            var texture = content.Load<Texture2D>(@"Textures\Bomber");
            DisplayComponent = new StaticImageDisplay(texture);

            isEnemyTileColliding = false;
            isTileColliding = false;
            Attack = 1;
            Health = 40;
            IsAffectedByGravity = false;
            IsAffectedByForces = false;
            IsAbleToMoveOutsideOfWorld = true;
            InvincibleTimeAfterBeingHit = 0.1f;

            // Bomber sprite is 71x51 px (~18x13 art units at TileScale=4)
            SetCenteredCollisionRectangle(71, 51, 70, 50);
        }

        public override void Kill()
        {
            _isFalling = true;
            _fallVelocity = new Vector2(RightDriftSpeed, 0f);
            Dead = true;
            DisplayComponent.TintColor = Color.White;
            PlayDeathSound();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (_isFalling)
            {
                _fallVelocity.Y += FallAccelerationY * elapsed;
                WorldLocation += _fallVelocity * elapsed;

                if (IsOnScreen())
                {
                    _explosionTimer -= elapsed;
                    if (_explosionTimer <= 0f)
                    {
                        _explosionTimer = ExplosionInterval;
                        EffectsManager.AddExplosion(CollisionRectangle.GetRandomLocation(), false);
                    }
                }

                base.Update(gameTime, elapsed);
                return;
            }

            if (Alive)
            {
                // no active behavior
            }

            base.Update(gameTime, elapsed);
        }
    }
}
