using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Enemies
{
    public class MegaSpaceRocketLauncher : Enemy
    {
        private const float HeadTravelPixels = 100f;
        private const float HeadMoveDuration = 0.8f;
        private const float HeadVelocity = 100f;

        private const float DownDuration = 3f;
        private const float UpDuration = 3f;
        private const float FireDelay = 1f;

        private const float DyingDuration = 1f;
        private const float ExplosionInterval = 0.1f;

        private const float SinkVelocity = 30f;
        private const int SinkPixels = 300;

        private Texture2D _megaTextures;

        private StaticImageDisplay _bodyDisplay;
        private StaticImageDisplay _backDisplay;
        private StaticImageDisplay _destroyedDisplay;

        private MegaSpaceRocketLauncherHead _head;

        private enum LauncherState { Down, MovingUp, Up, MovingDown, Dying, Dead }
        private LauncherState _launcherState = LauncherState.Down;

        private float _stateTimer;
        private float _explosionTimer = 0f;
        private float _sinkOffset = 0f;
        private bool _hasFired = false;

        // Target Y world positions for the head, computed once WorldLocation is known.
        private float _headDownY;
        private float _headUpY;

        private bool _isInitialized = false;

        public MegaSpaceRocketLauncher(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            _megaTextures = content.Load<Texture2D>(@"Textures\MegaTextures");

            _bodyDisplay = new StaticImageDisplay(_megaTextures, Helpers.GetMegaTileRect(3, 3));
            _backDisplay = new StaticImageDisplay(_megaTextures, Helpers.GetMegaTileRect(4, 3));
            _destroyedDisplay = new StaticImageDisplay(_megaTextures, Helpers.GetMegaTileRect(6, 3));

            DisplayComponent = new AggregateDisplay(new DisplayComponent[] { _bodyDisplay, _backDisplay });

            isEnemyTileColliding = false;
            isTileColliding = false;
            Attack = 2;
            Health = 10;
            IsAffectedByGravity = false;
            IsAffectedByForces = false;
            IsAbleToMoveOutsideOfWorld = false;
            InvincibleTimeAfterBeingHit = 0f;

            SetWorldLocationCollisionRectangle(36, 44);

            _headDownY = WorldLocation.Y - HeadTravelPixels;
            _headUpY = WorldLocation.Y - 2f * HeadTravelPixels;

            _head = new MegaSpaceRocketLauncherHead(content, cellX, cellY, player, camera, this);
            _head.WorldLocation = new Vector2(WorldLocation.X, _headDownY);
            AddEnemyInConstructor(_head);

            _stateTimer = DownDuration;
        }

        public override void SetDrawDepth(float depth)
        {
            _bodyDisplay.DrawDepth = depth;
            _backDisplay.DrawDepth = depth + 2f * Game1.MIN_DRAW_INCREMENT;
            _destroyedDisplay.DrawDepth = depth;
            _head.SetDrawDepth(depth + Game1.MIN_DRAW_INCREMENT);
        }

        public override void Kill()
        {
            if (_launcherState == LauncherState.Dying || _launcherState == LauncherState.Dead) return;

            _launcherState = LauncherState.Dying;
            _stateTimer = DyingDuration;
            _explosionTimer = 0f;
            Attack = 0;
            PlayDeathSound();
        }

        private void LaunchMissiles()
        {
            Vector2 topLeftMissileLocation = _head.WorldLocation;
             topLeftMissileLocation += new Vector2(-20, -50);
            const int missileVerticalSpacing = 30;
            const int missileHorizontalSpacing = 40;
            MissileManager.Launch(topLeftMissileLocation);
            MissileManager.Launch(topLeftMissileLocation + new Vector2(0, missileVerticalSpacing));
            MissileManager.Launch(topLeftMissileLocation + new Vector2(0, 2 * missileVerticalSpacing));
            MissileManager.Launch(topLeftMissileLocation + new Vector2(missileHorizontalSpacing, 0));
            MissileManager.Launch(topLeftMissileLocation + new Vector2(missileHorizontalSpacing, missileVerticalSpacing));
            MissileManager.Launch(topLeftMissileLocation + new Vector2(missileHorizontalSpacing, 2 * missileVerticalSpacing));

            PlaySoundIfOnScreen("Fire", 0.5f);
        }

        private void Initialize()
        {
            // Reset draw depth so it resets the Head Draw depth correctly.
            SetDrawDepth(DrawDepth);
        }


        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!_isInitialized)
            {
                Initialize();
                _isInitialized = true;
            }

            if (Alive && IsOnScreen())
            {
                switch (_launcherState)
                {
                    case LauncherState.Down:
                        _stateTimer -= elapsed;
                        if (_stateTimer <= 0f)
                        {
                            _head.Velocity = new Vector2(0, -HeadVelocity);
                            _launcherState = LauncherState.MovingUp;
                        }
                        break;

                    case LauncherState.MovingUp:
                        if (_head.WorldLocation.Y <= _headUpY)
                        {
                            _head.WorldLocation = new Vector2(_head.WorldLocation.X, _headUpY);
                            _head.Velocity = Vector2.Zero;
                            _launcherState = LauncherState.Up;
                            _stateTimer = UpDuration;
                            _hasFired = false;
                        }
                        break;

                    case LauncherState.Up:
                        _stateTimer -= elapsed;
                        if (!_hasFired && _stateTimer <= UpDuration - FireDelay)
                        {
                            LaunchMissiles();
                            _hasFired = true;
                        }
                        if (_stateTimer <= 0f)
                        {
                            _head.Velocity = new Vector2(0, HeadVelocity);
                            _launcherState = LauncherState.MovingDown;
                        }
                        break;

                    case LauncherState.MovingDown:
                        if (_head.WorldLocation.Y >= _headDownY)
                        {
                            _head.WorldLocation = new Vector2(_head.WorldLocation.X, _headDownY);
                            _head.Velocity = Vector2.Zero;
                            _launcherState = LauncherState.Down;
                            _stateTimer = DownDuration;
                        }
                        break;

                    case LauncherState.Dying:
                        _stateTimer -= elapsed;
                        _explosionTimer -= elapsed;
                        if (_explosionTimer <= 0f)
                        {
                            _explosionTimer = ExplosionInterval;
                            var randomX = CollisionRectangle.Left + Game1.Randy.Next(CollisionRectangle.Width);
                            var randomY = CollisionRectangle.Top + Game1.Randy.Next(CollisionRectangle.Height);
                            _head.Velocity = Vector2.Zero;
                            EffectsManager.AddExplosion(new Vector2(randomX, randomY));
                        }
                        if (_stateTimer <= 0f)
                        {
                            DisplayComponent = _destroyedDisplay;
                            _head.Enabled = false;
                            _head.Dead = true;
                            _launcherState = LauncherState.Dead;
                        }
                        break;

                    case LauncherState.Dead:
                        _sinkOffset += SinkVelocity * elapsed;
                        _destroyedDisplay.Offset = new Vector2(0, _sinkOffset);
                        if (_sinkOffset >= SinkPixels)
                        {
                            Dead = true;
                        }
                        break;
                }
            }

            base.Update(gameTime, elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsOnScreen()) return;
            base.Draw(spriteBatch);
        }
    }
}
