using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace MacGame.Enemies
{
    public class BigShipSatellite : Enemy
    {
        /// <summary>
        /// Controls how frequent the death explosions are.
        /// </summary>
        private const float ExplosionInterval = 0.2f;
        private float _explosionTimer;

        /// <summary>
        /// Once dead this is the time it will explode before changing to the 
        /// damaged sprite.
        /// </summary>
        private float _firstDeathPhaseTimer;
        private const float _firstDeathPhaseTimerGoal = 0.5f;
        
        /// <summary>
        /// This is the time it will continue to explode after it has swaped over
        /// to the damaged sprite.
        /// </summary>
        private float _secondDeathPhaseTimer;
        private const float _secondDeathPhaseTimerGoal = 2f;

        private Texture2D _megaTextures;

        private enum SatelliteState { Alive, DyingPhase1, DyingPhase2, Dead }
        private SatelliteState _state = SatelliteState.Alive;

        StaticImageDisplay _normalImage;
        StaticImageDisplay _destroyedImage;

        private BigShipBoss _bigShipBoss; 

        public BigShipSatellite(ContentManager content, int cellX, int cellY, Player player, Camera camera, BigShipBoss boss)
            : base(content, cellX, cellY, player, camera)
        {
            _megaTextures = content.Load<Texture2D>(@"Textures\MegaTextures");
            var normalRect = Helpers.GetMegaTileRect(1, 4);
            _normalImage = new StaticImageDisplay(_megaTextures, normalRect);

            var destroyedRect = Helpers.GetMegaTileRect(2, 4);
            _destroyedImage = new StaticImageDisplay(_megaTextures, destroyedRect);

            DisplayComponent = _normalImage;

            isEnemyTileColliding = false;
            isTileColliding = false;
            Attack = 1;
            Health = 20;
            IsAffectedByGravity = false;
            IsAbleToMoveOutsideOfWorld = true;
            InvincibleTimeAfterBeingHit = 0.1f;

            SetWorldLocationCollisionRectangle(20, 34);

            _bigShipBoss = boss;
        }

        public override void SetDrawDepth(float depth)
        {
            _normalImage.DrawDepth = depth;
            _destroyedImage.DrawDepth = depth;
        }

        public override void Kill()
        {
            _state = SatelliteState.DyingPhase1;
            Dead = true;
            Attack = 0;
            IsPlayerColliding = false;
            CanBeHitWithWeapons = false;
            PlayDeathSound();
            _bigShipBoss.HandleSatelliteDestroyed();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            switch (_state)
            {
                case SatelliteState.DyingPhase1:
                    _firstDeathPhaseTimer += elapsed;
                    _explosionTimer += elapsed;
                    if (_explosionTimer >= ExplosionInterval)
                    {
                        _explosionTimer -= ExplosionInterval;
                        EffectsManager.AddExplosion(CollisionRectangle.GetRandomLocation(), true);
                    }
                    if (_firstDeathPhaseTimer >= _firstDeathPhaseTimerGoal)
                    {
                        _state = SatelliteState.DyingPhase2;
                        DisplayComponent = _destroyedImage;
                    }
                    break;

                case SatelliteState.DyingPhase2:
                    _secondDeathPhaseTimer += elapsed;
                    _explosionTimer += elapsed;
                    if (_explosionTimer >= 0f)
                    {
                        _explosionTimer -= ExplosionInterval;
                        EffectsManager.AddExplosion(CollisionRectangle.GetRandomLocation(), true);
                    }
                    if (_secondDeathPhaseTimer >= _secondDeathPhaseTimerGoal)
                    {
                        _state = SatelliteState.Dead;
                    }
                    break;
            }

            base.Update(gameTime, elapsed);
        }
    }
}
