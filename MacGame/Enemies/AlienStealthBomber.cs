using System;
using MacGame.DisplayComponents;
using MacGame.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Enemies
{
    public enum AlienStealthBomberState
    {
        Unseen,
        Alive,
        Dying,
        Dead
    }

    /// <summary>
    /// A giant and mysterious alien ship.
    /// </summary>
    public class AlienStealthBomber : Enemy
    {
        private AlienStealthBomberState _state = AlienStealthBomberState.Unseen;

        private const int MaxHealth = 40;

        private float _dyingTimer = 0f;
        private const float DyingDuration = 4f;
        private float _explosionTimer = 0f;

        private bool _isInitialized = false;
        private Sock _sock;

        private const int SpriteSize = 64;

        // Bobbing while alive
        private float _bobTimer = 0f;
        private float _baseY;
        private const float BobAmplitude = 12f;
        private const float BobSpeed = 1.5f;

        // Falling while dying
        private float _fallSpeedY = 0f;
        private const float FallAcceleration = 120f;
        private const float FallDriftX = -80f;

        public AlienStealthBomber(ContentManager content, int cellX, int cellY, Player player, Camera camera)
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

            var megaTextures = content.Load<Texture2D>(@"Textures\MegaTextures");
            DisplayComponent = new StaticImageDisplay(megaTextures, Helpers.GetMegaTileRect(4, 5));

            Attack = 1;
            Health = MaxHealth;
            InvincibleTimeAfterBeingHit = 0f;

            this.WorldLocation += new Vector2(0, SpriteSize / 2);

            // Collision starts at vertical center of sprite, full width, 1/3 height.
            SetCenteredCollisionRectangle(SpriteSize, SpriteSize, SpriteSize, SpriteSize / 3);
        }

        private void Initialize()
        {
            foreach (var item in Game1.CurrentLevel.Items)
            {
                if (item is Sock sock && sock.Name == "AlienStealthBomberSock")
                {
                    _sock = sock;
                    break;
                }
            }

            if (_sock == null)
            {
                throw new Exception("You need a sock named AlienStealthBomberSock in the level!");
            }

            if (!_sock.IsCollected)
            {
                _sock.Enabled = false;
            }

            _isInitialized = true;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            if (_state == AlienStealthBomberState.Unseen)
            {
                if (Game1.Camera.IsObjectVisible(CollisionRectangle))
                {
                    _state = AlienStealthBomberState.Alive;
                    _baseY = WorldLocation.Y;
                }
                else
                {
                    return;
                }
            }

            if (_state != AlienStealthBomberState.Dead)
            {
                Game1.DrawBossHealth = true;
                Game1.MaxBossHealth = MaxHealth;
                Game1.BossHealth = Health;
                Game1.BossName = "Alien Ship";
            }

            if (_state == AlienStealthBomberState.Alive)
            {
                _bobTimer += elapsed;
                WorldLocation = new Vector2(WorldLocation.X, _baseY + (float)Math.Sin(_bobTimer * BobSpeed) * BobAmplitude);
            }
            else if (_state == AlienStealthBomberState.Dying)
            {
                _fallSpeedY += FallAcceleration * elapsed;
                WorldLocation += new Vector2(FallDriftX, _fallSpeedY) * elapsed;

                if (Game1.Camera.IsObjectVisible(CollisionRectangle))
                {
                    _explosionTimer += elapsed;
                    if (_explosionTimer >= 0.13f)
                    {
                        _explosionTimer = 0f;
                        EffectsManager.AddExplosion(CollisionRectangle.GetRandomLocation(), true);
                    }
                }

                _dyingTimer += elapsed;
                if (_dyingTimer >= DyingDuration)
                {
                    Dead = true;
                    Enabled = false;
                    _state = AlienStealthBomberState.Dead;
                    TimerManager.AddNewTimer(1f, () => { _sock.FadeIn(); });
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
            if (!CanTakeHit()) return;
            if (_state == AlienStealthBomberState.Dying || _state == AlienStealthBomberState.Dead) return;

            base.TakeHit(attacker, damage);
        }

        public override void Kill()
        {
            _state = AlienStealthBomberState.Dying;
            Attack = 0;
        }

        public override void PlayDeathSound()
        {
            // Explosions during the dying sequence provide sound.
        }
    }
}
