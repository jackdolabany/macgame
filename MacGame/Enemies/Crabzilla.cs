using MacGame.DisplayComponents;
using MacGame.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MacGame.Enemies
{
    public enum CrabzillaState
    {
        Unseen,
        Idle,
        ClosingArms,
        ArmsClosedWaiting,
        OpeningArms,
        Dying,
        Dead
    }

    public class Crabzilla : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        CrabzillaState _state = CrabzillaState.Unseen;

        private const int MaxHealth = 100;

        private float _idleTimer = 0f;
        private const float IdleDuration = 4f;

        private float _armsClosedWaitTimer = 0f;
        private const float ArmsClosedWaitDuration = 1f;

        private float _dyingTimer = 0f;
        private const float DyingDuration = 4f;
        private float _explosionTimer = 0f;

        private bool _isInitialized = false;
        private Sock _sock;

        // Sweep gun: single bullet fired along an angle that pivots up and down.
        private float _sweepTimer = 0f;
        private float _sweepFireTimer = 0f;
        private const float SweepFireInterval = 0.1f;
        private const float SweepOscillateSpeed = 1.5f;
        private const float SweepAmplitude = (float)(Math.PI / 3.0); // ±60° from left

        // Circle gun: ring of bullets that slowly rotates.
        private float _circleAngle = 0f;
        private float _circleFireTimer = 0f;
        private const float CircleFireInterval = 0.6f;
        private const int CircleBulletCount = 8;
        private const float CircleRotateSpeed = 0.5f; // radians per second

        private const float BulletSpeed = 160f;

        // Wall gun: dense horizontal line of large shots fired from the front of the body.
        private float _wallShotTimer = 5f;
        private const float WallShotInterval = 5f;
        private const float WallBulletSpacing = 12f; // matches large shot collision size — no gap
        private const float WallBulletSpeed = 100f;
        private const float WallSpreadRate = 0.16f; // shots drift outward slowly as they travel

        // AlienShip spawning
        private const int MaxAliveShips = 3;
        private const float ShipSpawnInterval = 3f;
        private readonly List<AlienShip> _ships = new List<AlienShip>();
        private float _shipSpawnTimer = ShipSpawnInterval;

        int width = 222 / 3 * Game1.TileScale;
        int height = 128 * Game1.TileScale;

        /// <summary>
        ///  To know when the thing is on screen. This will be much larger than the hit box regular collision rectangle.
        /// </summary>
        Rectangle isSeenRectangle;

        public List<Rectangle> frame1CollisionRectangles;
        public List<Rectangle> frame2CollisionRectangles;
        public List<Rectangle> frame3CollisionRectangles;
        public List<Rectangle> extraCollisionRectangles;

        AnimationDisplay animationDisplay => (AnimationDisplay)DisplayComponent;

        public Crabzilla(ContentManager content, int cellX, int cellY, Player player, Camera camera)
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

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Crabzilla");

            var firstFrameRect = new Rectangle(0, 0, width, height);
            var idle = new AnimationStrip(textures, firstFrameRect, 1, "idle");
            idle.LoopAnimation = false;
            idle.FrameLength = 0.15f;
            animations.Add(idle);

            var closeArms = new AnimationStrip(textures, firstFrameRect, 3, "closeArms");
            closeArms.LoopAnimation = false;
            closeArms.FrameLength = 0.15f;
            
            animations.Add(closeArms);

            var openArms = (AnimationStrip)closeArms.Clone();
            openArms.Reverse = true;
            openArms.Name = "openArms";
            animations.Add(openArms);

            animations.Play("idle");

            Attack = 1;
            Health = MaxHealth;
            InvincibleTimeAfterBeingHit = 0f;


            this.WorldLocation += new Vector2(0, (height / 2) - Game1.TileSize);

            // Just around the crab body
            CollisionRectangle = new Rectangle(8, -80 * Game1.TileScale, 27 * Game1.TileScale, 33 * Game1.TileScale);

            isSeenRectangle = new Rectangle(WorldLocation.X.ToInt() - (width / 2), WorldLocation.Y.ToInt() - height, width, height);

            // Figure out different collision rectangles for each frame.
            frame1CollisionRectangles = new List<Rectangle>
            {
                // Top claw
                _getRelativeCrabRectangle(-20, -500, 100, 100),
                // Top arm
                _getRelativeCrabRectangle(30, -400, 30, 76),
                // bottom claw
                _getRelativeCrabRectangle(-20, -110, 100, 100),
                // bottom arm
                _getRelativeCrabRectangle(30, -180, 30, 76),
            };

            frame2CollisionRectangles = new List<Rectangle>
            {
                // Top claw.
                _getRelativeCrabRectangle(-100, -430, 100, 100),
                // Top arm.
                _getRelativeCrabRectangle(10, -400, 30, 76),
                // bottom claw
                _getRelativeCrabRectangle(-100, -180, 100, 100),
                // bottom arm
                _getRelativeCrabRectangle(10, -180, 30, 76),
            };

            frame3CollisionRectangles = new List<Rectangle>
            {
                 // Top claw.
                _getRelativeCrabRectangle(-140, -368, 100, 100),
                // Top arm.
                _getRelativeCrabRectangle(-40, -350, 40, 30),
                // bottom claw
                _getRelativeCrabRectangle(-140, -240, 100, 100),
                // bottom arm
                _getRelativeCrabRectangle(-40, -190, 40, 30),
            };

            for (int i = 0; i < MaxAliveShips; i++)
            {
                var ship = new AlienShip(content, cellX, cellY, player, camera);
                ship.Enabled = false;
                ship.IsAbleToMoveOutsideOfWorld = true;
                ship.IsAbleToSurviveOutsideOfWorld = true;
                _ships.Add(ship);
                AddEnemyInConstructor(ship);
            }
        }

        private void Initialize()
        {
            foreach (var item in Game1.CurrentLevel.Items)
            {
                if (item is Sock sock && sock.Name == "CrabzillaSock")
                {
                    _sock = sock;
                    break;
                }
            }

            if (_sock == null)
            {
                throw new Exception("You need a sock named CrabzillaSock in the level!");
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

            if (_state == CrabzillaState.Unseen)
            {
                if (Game1.Camera.IsObjectVisible(isSeenRectangle))
                {
                    _state = CrabzillaState.Idle;
                }
                else
                {
                    return;
                }
            }

            var isActiveAndAttacking = Alive && _state != CrabzillaState.Unseen && _state != CrabzillaState.Dying && _state != CrabzillaState.Dead;
            if (isActiveAndAttacking)
            {
                Game1.DrawBossHealth = true;
                Game1.MaxBossHealth = MaxHealth;
                Game1.BossHealth = Health;
                Game1.BossName = "Crabzilla";
                UpdateSweepGun(elapsed);
                UpdateCircleGun(elapsed);
                UpdateWallGun(elapsed);

                _shipSpawnTimer -= elapsed;
                if (_shipSpawnTimer <= 0f)
                {
                    _shipSpawnTimer = ShipSpawnInterval;
                    TrySpawnShip();
                }
            }

            if (_state == CrabzillaState.Idle)
            {
                _idleTimer += elapsed;
                if (_idleTimer >= IdleDuration)
                {
                    _idleTimer = 0f;
                    _state = CrabzillaState.ClosingArms;
                    animations.Play("closeArms");
                }
            }
            else if (_state == CrabzillaState.ClosingArms)
            {
                if (animations.CurrentAnimation!.FinishedPlaying)
                {
                    _armsClosedWaitTimer = 0f;
                    _state = CrabzillaState.ArmsClosedWaiting;
                }
            }
            else if (_state == CrabzillaState.ArmsClosedWaiting)
            {
                _armsClosedWaitTimer += elapsed;
                if (_armsClosedWaitTimer >= ArmsClosedWaitDuration)
                {
                    _state = CrabzillaState.OpeningArms;
                    animations.Play("openArms");
                }
            }
            else if (_state == CrabzillaState.OpeningArms)
            {
                if (animations.CurrentAnimation!.FinishedPlaying)
                {
                    _state = CrabzillaState.Idle;
                }
            }
            else if (_state == CrabzillaState.Dying)
            {
                _explosionTimer += elapsed;
                if (_explosionTimer >= 0.13f)
                {
                    _explosionTimer = 0f;
                    EffectsManager.AddExplosion(isSeenRectangle.GetRandomLocation(), true);
                }

                _dyingTimer += elapsed;

                if (_dyingTimer >= DyingDuration)
                {
                    Dead = true;
                    Enabled = false;
                    _state = CrabzillaState.Dead;
                    TimerManager.AddNewTimer(1f, () => { _sock.FadeIn(); });
                }
            }

            base.Update(gameTime, elapsed);

            // Set a different set of extra collision rectangles for each frame.
            if (animationDisplay.CurrentAnimationName == "idle"
                || animationDisplay.CurrentAnimationName == "openArms" && animationDisplay.CurrentAnimation.currentFrameIndex == 2
                || animationDisplay.CurrentAnimationName == "closeArms" && animationDisplay.CurrentAnimation.currentFrameIndex == 0)
            {
                extraCollisionRectangles = frame1CollisionRectangles;
            }
            else if (animationDisplay.CurrentAnimation.currentFrameIndex == 1)
            {
                extraCollisionRectangles = frame2CollisionRectangles;
            }
            else if (animationDisplay.CurrentAnimationName == "openArms" && animationDisplay.CurrentAnimation.currentFrameIndex == 0
                || animationDisplay.CurrentAnimationName == "closeArms" && animationDisplay.CurrentAnimation.currentFrameIndex == 2)
            {
                extraCollisionRectangles = frame3CollisionRectangles;
            }

            CheckExtraCollisionRectangles(extraCollisionRectangles);

            if (_state == CrabzillaState.Dying)
            {
                var deadPercentage = _dyingTimer / DyingDuration;
                DisplayComponent.TintColor = Color.Lerp(Color.White, Color.Transparent, deadPercentage);
            }
        }

        /// <summary>
        /// The wall gun periodically shoots a whole wall of shots from the collision rectangle. This is so 
        /// the player can't just camp in front.
        /// </summary>
        private void UpdateWallGun(float elapsed)
        {
            _wallShotTimer -= elapsed;
            if (_wallShotTimer <= 0f)
            {
                _wallShotTimer = WallShotInterval;
                float spawnX = CollisionRectangle.Left;
                float centerY = CollisionRectangle.Center.Y;
                float topY = CollisionRectangle.Top + 22;
                float bottomY = CollisionRectangle.Bottom - 22;

                int shotCount = 1;

                for (float y = topY; y <= bottomY; y += WallBulletSpacing)
                {
                    shotCount++;
                    var drawDepth = this.DrawDepth + Game1.MIN_DRAW_INCREMENT * shotCount;
                    float vy = (y - centerY) * WallSpreadRate;
                    ShotManager.FireLargeShot(new Vector2(spawnX, y), new Vector2(-WallBulletSpeed, vy), this, drawDepth);
                }
            }
        }

        private void TrySpawnShip()
        {
            if (_ships.Count(s => s.Enabled) >= MaxAliveShips) return;

            var ship = _ships.FirstOrDefault(s => !s.Enabled);
            if (ship == null) return;

            var viewport = Game1.Camera.ViewPort;
            var minY = viewport.Top + (3 * Game1.TileSize);
            var maxY = viewport.Bottom - (3 * Game1.TileSize);
            if (maxY <= minY) return;

            ship.Revive(new Vector2(viewport.Right + 50, minY + Game1.Randy.Next(maxY - minY)));
        }

        private void UpdateSweepGun(float elapsed)
        {
            _sweepTimer += elapsed;
            _sweepFireTimer += elapsed;

            if (_sweepFireTimer >= SweepFireInterval)
            {
                _sweepFireTimer = 0f;
                // Pivot from upper-left to lower-left: Math.PI = left, ±SweepAmplitude sweeps up/down.
                var angle = Math.PI + Math.Sin(_sweepTimer * SweepOscillateSpeed) * SweepAmplitude;
                var dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                ShotManager.FireMediumShot(CollisionCenter, dir * BulletSpeed, this);
            }
        }

        private void UpdateCircleGun(float elapsed)
        {
            _circleAngle += CircleRotateSpeed * elapsed;
            _circleFireTimer += elapsed;

            if (_circleFireTimer >= CircleFireInterval)
            {
                _circleFireTimer = 0f;
                for (int i = 0; i < CircleBulletCount; i++)
                {
                    var angle = _circleAngle + i * (MathHelper.TwoPi / CircleBulletCount);
                    var dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    ShotManager.FireMediumShot(CollisionCenter, dir * BulletSpeed, this);
                }
            }
        }

        private Rectangle _getRelativeCrabRectangle(int x, int y, int width, int height)
        {
            return new Rectangle(this.WorldLocation.X.ToInt() + x,
                this.WorldLocation.Y.ToInt() + y,
                width,
                height);
        }

        public override void PlayTakeHitSound()
        {
            SoundManager.PlaySound("HitEnemy2");
        }

        public override void TakeHit(GameObject attacker, int damage)
        {
            if (!CanTakeHit()) return;
            if (_state == CrabzillaState.Dying || _state == CrabzillaState.Dead) return;

            base.TakeHit(attacker, damage);
        }

        public override void Kill()
        {
            _state = CrabzillaState.Dying;
            Attack = 0;
            foreach (var ship in _ships)
            {
                if (ship.Enabled) { ship.Kill(); }
            }
        }

        public override void PlayDeathSound()
        {
            // Explosions during the dying sequence provide sound.
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawExtraDebugRectangle(spriteBatch, isSeenRectangle, Color.Orange * 0.15f);
            DrawExtraDebugRectangles(spriteBatch, extraCollisionRectangles, Color.Green * 0.5f);

            base.Draw(spriteBatch);
        }
    }
}
