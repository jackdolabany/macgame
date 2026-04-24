using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Enemies
{
    public class MegaSpaceRocketLauncher : Enemy
    {
        private const float HeadTravelPixels = 88;
        private const float HeadVelocity = 100f;

        private const float DownDuration = 3f;
        private const float UpDuration = 3f;
        private const float FireDelay = 1f;

        private const float DyingDuration = 3f;
        private const float ExplosionInterval = 0.07f;

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

        private float missileDrawDepth;

        private Vector2 _initialWorldLocation;

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

            SetWorldLocationCollisionRectangle(36, 46);

            _headDownY = WorldLocation.Y - 96;
            _headUpY = _headDownY - HeadTravelPixels;

            _head = new MegaSpaceRocketLauncherHead(content, cellX, cellY, player, camera, this);
            _head.WorldLocation = new Vector2(WorldLocation.X, _headDownY);
            AddEnemyInConstructor(_head);

            _stateTimer = DownDuration;

            _initialWorldLocation = WorldLocation;
        }

        public override void SetDrawDepth(float depth)
        {
            _bodyDisplay.DrawDepth = depth;
            _backDisplay.DrawDepth = depth + (2f * Game1.MIN_DRAW_INCREMENT);
            _destroyedDisplay.DrawDepth = depth;
            _head.SetDrawDepth(depth + Game1.MIN_DRAW_INCREMENT);

            // Missiles are in front
            missileDrawDepth = depth - (3f * Game1.MIN_DRAW_INCREMENT);
        }

        public override void Kill()
        {
            if (_launcherState == LauncherState.Dying || _launcherState == LauncherState.Dead) return;

            _launcherState = LauncherState.Dying;
            _stateTimer = DyingDuration;
            _explosionTimer = 0f;
            Attack = 0;

            // Start moving the head down to the start position.
            _head.Velocity = new Vector2(0, HeadVelocity);

            this.IsPlayerColliding = false;
            _head.IsPlayerColliding = false;

            PlayDeathSound();
        }

        private void LaunchMissiles()
        {
            Vector2 topLeftMissileLocation = _head.WorldLocation;
             topLeftMissileLocation += new Vector2(-20, -50);
            const int missileVerticalSpacing = 24;
            const int missileHorizontalSpacing = 36;
            const float delay = 2f;
            
            var missile1 = MissileManager.LaunchMissile(topLeftMissileLocation, EightWayRotationDirection.UpLeft, delay);
            if (missile1 != null)
            {
                missile1.SetDrawDepth(missileDrawDepth);
            }

            var missile2 = MissileManager.LaunchMissile(topLeftMissileLocation + new Vector2(0, missileVerticalSpacing), EightWayRotationDirection.Left, delay);
            if (missile2 != null)
            {
                missile2.SetDrawDepth(missileDrawDepth - Game1.MIN_DRAW_INCREMENT);
            }

            var missile3 = MissileManager.LaunchMissile(topLeftMissileLocation + new Vector2(0, 2 * missileVerticalSpacing), EightWayRotationDirection.DownLeft, delay);
            if (missile3 != null)
            {
                missile3.SetDrawDepth(missileDrawDepth - (2f * Game1.MIN_DRAW_INCREMENT));
            }   

            var missile4 = MissileManager.LaunchMissile(topLeftMissileLocation + new Vector2(missileHorizontalSpacing, 0), EightWayRotationDirection.UpRight, delay);
            if (missile4 != null)
            {
                missile4.SetDrawDepth(missileDrawDepth - (3f * Game1.MIN_DRAW_INCREMENT));
            }

            var missile5 = MissileManager.LaunchMissile(topLeftMissileLocation + new Vector2(missileHorizontalSpacing, missileVerticalSpacing), EightWayRotationDirection.Right, delay);
            if (missile5 != null)
            {
                missile5.SetDrawDepth(missileDrawDepth - (4f * Game1.MIN_DRAW_INCREMENT));
            }

            var missile6 = MissileManager.LaunchMissile(topLeftMissileLocation + new Vector2(missileHorizontalSpacing, 2 * missileVerticalSpacing), EightWayRotationDirection.DownRight, delay);
            if (missile6 != null)
            {
                missile6.SetDrawDepth(missileDrawDepth - (5f * Game1.MIN_DRAW_INCREMENT));
            }

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
                        
                        // Is the head returning to psoition independent of the body?
                        if (_head.Velocity.Y > 0 && this.Velocity.Y == 0)
                        {
                            // Head drops until back to original position.
                            if (_head.WorldLocation.Y >= _headDownY)
                            {
                                // Start sinking both head and body
                                _head.WorldLocation = new Vector2(_head.WorldLocation.X, _headDownY);
                                this.Velocity = new Vector2(0, 70);
                                _head.Velocity = this.Velocity;
                            }
                        }

                        // Add random explosions.
                        if (_explosionTimer <= 0f)
                        {
                            // Add an offset for the head.
                            var top = CollisionRectangle.Top + 36;
                            var rectHeight = _initialWorldLocation.Y.ToInt() - top;

                            // Don't add explosions if it's too tight or negative.
                            if (rectHeight > 24)
                            {
                                var explosionRectangle = new Rectangle(CollisionRectangle.Left, top, CollisionRectangle.Width, rectHeight);
                                _explosionTimer = ExplosionInterval;
                                var randomX = explosionRectangle.Left + Game1.Randy.Next(explosionRectangle.Width);
                                var randomY = explosionRectangle.Top + Game1.Randy.Next(explosionRectangle.Height);
                                EffectsManager.AddExplosion(new Vector2(randomX, randomY));
                            }
                        }

                        // Transition to the dead texture halfway through the explosions.
                        if (_stateTimer <= DyingDuration / 2 && DisplayComponent != _destroyedDisplay)
                        {
                            DisplayComponent = _destroyedDisplay;
                            _head.Enabled = false;
                            _head.Dead = true;
                        }

                        if (_stateTimer <= 0f)
                        {
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
