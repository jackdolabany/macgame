using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Enemies
{
    public enum GalaxyTwinState { Alive, Dying, Dead }

    /// <summary>
    /// A ship that is part of the GalaxyTwinsBoss. The boss controls two of these bad boys
    /// and when they both die the boss fight is over.
    /// </summary>
    public class GalaxyTwin : Enemy
    {
        private GalaxyTwinState _state = GalaxyTwinState.Alive;

        public const int MaxHealth = 20;
        private const float MoveSpeed = 180f;
        private const float AtTargetDistance = 6f;

        // This controls how the ship falls when it dies.
        public float FallDriftX = -80f;
        private float _fallSpeedY = 0f;
        private const float FallAcceleration = 120f;
        private float _dyingTimer = 0f;
        private const float DyingDuration = 3f;
        private float _explosionTimer = 0f;

        private Vector2 _targetLocation;
        private bool _hasTarget = false;

        public int CurrentHealth => Health;
        public bool IsAlive => _state == GalaxyTwinState.Alive;
        public bool IsAtTarget => !_hasTarget || Vector2.Distance(WorldLocation, _targetLocation) < AtTargetDistance;

        public GalaxyTwin(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            isEnemyTileColliding = false;
            isTileColliding = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            IsAffectedByGravity = false;
            IsAffectedByPlatforms = false;
            CanBeHitWithWeapons = true;
            CanBeJumpedOn = true;

            DisplayComponent = new StaticImageDisplay(Game1.ReallyBigTileTextures, Helpers.GetReallyBigTileRect(3, 7));

            Attack = 1;
            Health = MaxHealth;
            InvincibleTimeAfterBeingHit = 0f;

            SetCenteredCollisionRectangle(24, 24, 24, 24);
        }

        public void SetTargetLocation(Vector2 worldLocation)
        {
            _targetLocation = worldLocation;
            _hasTarget = true;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (_state == GalaxyTwinState.Alive)
            {
                if (_hasTarget)
                {
                    var toTarget = _targetLocation - WorldLocation;
                    if (toTarget.Length() > AtTargetDistance)
                    {
                        toTarget.Normalize();
                        Velocity = toTarget * MoveSpeed;
                    }
                    else
                    {
                        Velocity = Vector2.Zero;
                        WorldLocation = _targetLocation;
                    }
                }
            }
            else if (_state == GalaxyTwinState.Dying)
            {
                _fallSpeedY += FallAcceleration * elapsed;
                WorldLocation += new Vector2(FallDriftX, _fallSpeedY) * elapsed;

                if (Game1.Camera.IsObjectVisible(CollisionRectangle))
                {
                    _explosionTimer += elapsed;
                    if (_explosionTimer >= 0.13f)
                    {
                        _explosionTimer = 0f;
                        EffectsManager.AddExplosion(CollisionRectangle.GetRandomLocation(), false);
                    }
                }

                _dyingTimer += elapsed;
                if (_dyingTimer >= DyingDuration)
                {
                    Dead = true;
                    Enabled = false;
                    _state = GalaxyTwinState.Dead;
                }
            }

            base.Update(gameTime, elapsed);
        }

        public override void PlayTakeHitSound()
        {
            SoundManager.PlaySound("HitEnemy2");
        }

        public override void TakeHit(GameObject attacker, int damage)
        {
            if (_state != GalaxyTwinState.Alive) return;
            base.TakeHit(attacker, damage);
        }

        public override void Kill()
        {
            _state = GalaxyTwinState.Dying;
            Attack = 0;
            Velocity = Vector2.Zero;
        }

        public override void PlayDeathSound()
        {
            // Explosions during the dying sequence provide sound.
        }
    }
}
