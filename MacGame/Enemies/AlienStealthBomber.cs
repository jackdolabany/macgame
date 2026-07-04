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
        MovingToPosition,
        Attack,
        Dying,
        Dead
    }

    /// <summary>
    /// A giant and mysterious alien ship.
    /// </summary>
    public class AlienStealthBomber : Enemy
    {
        private AlienStealthBomberState _state = AlienStealthBomberState.Unseen;

        private const int MaxHealth = 200;

        private float _dyingTimer = 0f;
        private const float DyingDuration = 4f;
        private float _explosionTimer = 0f;

        private bool _isInitialized = false;
        private Sock _sock;

        private const int SpriteSize = 64;

        // Moving to position on first appearance
        private Vector2 _targetPosition;
        private const float MoveToCenterSpeed = 50f;
        private const float BelowCenterOffset = 150f;
        private const float RightOfCenterOffset = 60f;

        // Bobbing while attacking
        private float _bobTimer = 0f;
        private float _baseY;
        private const float BobAmplitude = 12f;
        private const float BobSpeed = 1.5f;

        // Grenade firing
        private ShotGrenade[] _grenades = new ShotGrenade[2];
        private float _fireTimer = 0f;
        private bool _firstGrenadeFired = false;
        private const float FirstGrenadeDelay = 2f;
        private const float SecondGrenadeDelay = 1f;
        private const float GrenadeUpwardSpeed = -60f;
        private const float GrenadeMaxXSpeed = 80f;

        // Ring shot
        private float _ringShotTimer = 0f;
        private const float RingShotInterval = 5f;
        private const float RingShotSpeed = 300;

        // Homing missiles
        private Missile[] _missiles = new Missile[2];
        private float _missileLaunchTimer = 0f;
        private const float MissileLaunchInterval = 3.3f;
        private const float MissileHomingDelay = 1.5f;

        // Hatch overlay animation
        private AnimationDisplay _hatchDisplay;
        private const float HatchLeadTime = 0.75f;
        private const float HatchFrameLength = 0.1f;

        // Damage sprites
        private StaticImageDisplay _shipDisplay;
        private static readonly Rectangle NormalSpriteRect = Helpers.GetMegaTileRect(4, 5);
        private static readonly Rectangle LightDamageSpriteRect = Helpers.GetMegaTileRect(5, 5);
        private static readonly Rectangle MediumDamageSpriteRect = Helpers.GetMegaTileRect(6, 5);
        private static readonly Rectangle FullDamageSpriteRect = Helpers.GetMegaTileRect(7, 5);

        // Damage smokes
        private BaseSmoke[] _smokes = new BaseSmoke[6];
        private static readonly Vector2[] SmokeOffsets = new Vector2[]
        {
            new Vector2(100, 10),
            new Vector2(50, -18),
            new Vector2(20, 10),
            new Vector2(-20, -10),
            new Vector2(-50, -30),
            new Vector2(-100, 44)
        };

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
            _shipDisplay = new StaticImageDisplay(megaTextures, NormalSpriteRect);

            var spaceTextures = content.Load<Texture2D>(@"Textures\SpaceTextures");
            _hatchDisplay = new AnimationDisplay();
            var hatchOpen = new AnimationStrip(spaceTextures, Helpers.GetTileRect(14, 9), 2, "open");
            hatchOpen.LoopAnimation = false;
            hatchOpen.FrameLength = HatchFrameLength;
            _hatchDisplay.Add(hatchOpen);
            var hatchClose = (AnimationStrip)hatchOpen.Clone();
            hatchClose.Reverse = true;
            hatchClose.Name = "close";
            _hatchDisplay.Add(hatchClose);
            _hatchDisplay.StopPlaying();
            var megaHeight = Helpers.GetMegaTileRect(4, 5).Height;
            var tileHeight = Helpers.GetTileRect(14, 9).Height;
            _hatchDisplay.Offset = new Vector2(8, -(megaHeight / 2f - tileHeight / 2f));

            DisplayComponent = new AggregateDisplay(new DisplayComponent[] { _shipDisplay, _hatchDisplay });

            Attack = 1;
            Health = MaxHealth;
            InvincibleTimeAfterBeingHit = 0f;

            this.WorldLocation += new Vector2(0, SpriteSize / 2);

            SetCenteredCollisionRectangle(SpriteSize, SpriteSize, (SpriteSize * 0.8f).ToInt(), (SpriteSize * 0.25f).ToInt());

            for (int i = 0; i < _grenades.Length; i++)
            {
                _grenades[i] = new ShotGrenade(content, 0, 0, player, camera);
                _grenades[i].Enabled = false;
                Level.AddEnemy(_grenades[i]);
            }

            for (int i = 0; i < _smokes.Length; i++)
            {
                _smokes[i] = i % 2 == 0 ? (BaseSmoke)new GraySmoke1(content) : new GraySmoke2(content);
                _smokes[i].Enabled = false;
            }

            for (int i = 0; i < _missiles.Length; i++)
            {
                _missiles[i] = new Missile(content, 0, 0, player, camera);
                _missiles[i].Enabled = false;
                Level.AddEnemy(_missiles[i]);
            }
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

            SetSockReturnAction(_sock);

            _isInitialized = true;
        }

        private void LaunchGrenade(ShotGrenade grenade)
        {
            if (grenade.Enabled) return;
            float xVel = (float)(Game1.Randy.NextDouble() * GrenadeMaxXSpeed * 2 - GrenadeMaxXSpeed);
            grenade.Launch(WorldCenter + new Vector2(8, 0), new Vector2(xVel, GrenadeUpwardSpeed));
            SoundManager.PlaySound("Shoot2");
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
                    var viewport = Game1.Camera.ViewPort;
                    _targetPosition = new Vector2(viewport.Center.X + RightOfCenterOffset, viewport.Center.Y + BelowCenterOffset);
                    _state = AlienStealthBomberState.MovingToPosition;
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

                if (Health <= (int)(MaxHealth * 0.2f))
                {
                    _shipDisplay.Source = FullDamageSpriteRect;
                }
                else if (Health <= (int)(MaxHealth * 0.4f))
                {
                    _shipDisplay.Source = MediumDamageSpriteRect;
                }
                else if (Health <= (int)(MaxHealth * 0.6f))
                {
                    _shipDisplay.Source = LightDamageSpriteRect;
                }
                else
                {
                    _shipDisplay.Source = NormalSpriteRect;
                }
            }

            if (_state == AlienStealthBomberState.MovingToPosition)
            {
                _targetPosition.X = Game1.Camera.ViewPort.Center.X + RightOfCenterOffset;
                var toTarget = _targetPosition - WorldLocation;
                float dist = toTarget.Length();
                float step = MoveToCenterSpeed * elapsed;
                if (dist <= step)
                {
                    WorldLocation = _targetPosition;
                    _baseY = _targetPosition.Y;
                    _state = AlienStealthBomberState.Attack;
                    _fireTimer = 2f;
                    _ringShotTimer = 3f;
                }
                else
                {
                    WorldLocation += Vector2.Normalize(toTarget) * step;
                }
            }
            else if (_state == AlienStealthBomberState.Attack)
            {
                _bobTimer += elapsed;
                WorldLocation = new Vector2(Game1.Camera.ViewPort.Center.X + RightOfCenterOffset, _baseY + (float)Math.Sin(_bobTimer * BobSpeed) * BobAmplitude);

                _fireTimer += elapsed;

                if (_hatchDisplay.CurrentAnimationName == "" && _fireTimer >= FirstGrenadeDelay - HatchLeadTime)
                {
                    _hatchDisplay.Play("open");
                    SoundManager.PlaySound("OpenHatch");
                }

                if (_hatchDisplay.CurrentAnimationName == "close" && (_hatchDisplay.CurrentAnimation?.FinishedPlaying ?? false))
                {
                    _hatchDisplay.StopPlaying();
                }

                _ringShotTimer += elapsed;
                if (_ringShotTimer >= RingShotInterval)
                {
                    _ringShotTimer = 0f;
                    ShotManager.FireMediumRing(WorldCenter + new Vector2(-96, 16), new Vector2(-RingShotSpeed, 0), this);
                    SoundManager.PlaySound("ShootRing");
                }

                if (!_missiles[0].Enabled && !_missiles[1].Enabled)
                {
                    _missileLaunchTimer += elapsed;
                    if (_missileLaunchTimer >= MissileLaunchInterval)
                    {
                        _missileLaunchTimer = 0f;
                        _missiles[0].Launch(WorldCenter + new Vector2(0, -30), new Vector2(0, -1), MissileHomingDelay);
                        _missiles[1].Launch(WorldCenter + new Vector2(0, 40), new Vector2(0, 1), MissileHomingDelay);
                        SoundManager.PlaySound("ShootMissile");
                    }
                }

                if (!_firstGrenadeFired && _fireTimer >= FirstGrenadeDelay)
                {
                    LaunchGrenade(_grenades[0]);
                    _firstGrenadeFired = true;
                }
                else if (_firstGrenadeFired && _fireTimer >= FirstGrenadeDelay + SecondGrenadeDelay)
                {
                    LaunchGrenade(_grenades[1]);
                    _fireTimer = 0f;
                    _firstGrenadeFired = false;
                    _hatchDisplay.Play("close");
                    SoundManager.PlaySound("OpenHatch");
                }
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

            if (_state == AlienStealthBomberState.Attack || _state == AlienStealthBomberState.Dying)
            {
                for (int i = 0; i < _smokes.Length; i++)
                {
                    if (!_smokes[i].Enabled && Health <= (int)(MaxHealth * (0.5f - i * (0.4f / 5f))))
                    {
                        _smokes[i].Enabled = true;
                    }
                    if (_smokes[i].Enabled)
                    {
                        _smokes[i].WorldLocation = WorldCenter + SmokeOffsets[i];
                        _smokes[i].Update(gameTime, elapsed);
                    }
                }
            }

            base.Update(gameTime, elapsed);

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _hatchDisplay.DrawDepth = DrawDepth - Game1.MIN_DRAW_INCREMENT;
            base.Draw(spriteBatch);
            foreach (var smoke in _smokes)
            {
                if (smoke.Enabled)
                {
                    smoke.SetDrawDepth(DrawDepth - Game1.MIN_DRAW_INCREMENT);
                    smoke.Draw(spriteBatch);
                }
            }
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

            ShotManager.ClearShotsCinematic();

            foreach (var missile in _missiles)
            {
                if (missile.Enabled)
                {
                    missile.Kill();
                }
            }

            foreach (var grenade in _grenades)
            {
                if (grenade.Enabled)
                {
                    grenade.Enabled = false;
                }
            }
        }

        public override void PlayDeathSound()
        {
            // Explosions during the dying sequence provide sound.
        }
    }
}
