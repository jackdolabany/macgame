using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Enemies
{
    public class MegaSpaceCannon : Enemy
    {
        private const float MIN_SHOOT_TIME = 2f;
        private const float MAX_SHOOT_TIME = 5f;
        private const int MegaSize = 256;
        private const int ReallyBigSize = 24 * Game1.TileScale;
        private const int TopRaisePixels = 26 * Game1.TileScale; // 104 pixels

        private const float DyingDuration = 3f;
        private const float ExplosionInterval = 0.07f;
        private const float SinkVelocity = 30f;
        private const int SinkPixels = 300;

        private float _shootTimer;

        private Texture2D _megaTextures;
        private Texture2D _reallyBigTextures;

        private Rectangle _baseRect;
        private Rectangle _topRect;
        private Rectangle _barrelLeftRect;
        private Rectangle _barrelDiagonalRect;
        private Rectangle _destroyedRect;

        private enum FacingDirection { Left, UpLeft, Up, UpRight, Right }
        private FacingDirection _currentDirection;

        private enum CannonState { Alive, Dying, Dead }
        private CannonState _cannonState = CannonState.Alive;
        private float _stateTimer;
        private float _explosionTimer = 0f;
        private float _sinkOffset = 0f;
        private bool _isDestroyed = false;
        private bool _hasLockedCamera = false;

        /// <summary>
        /// Compute the center point for the barrel and shots on every update after the direction is figured out.
        /// </summary>
        private Vector2 _barrelRotationCenter;

        public MegaSpaceCannon(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            WorldLocation = WorldLocation + new Vector2(0, 12);

            _megaTextures = content.Load<Texture2D>(@"Textures\MegaTextures");
            _reallyBigTextures = content.Load<Texture2D>(@"Textures\ReallyBigTextures");

            _baseRect = Helpers.GetMegaTileRect(2, 2);
            _topRect = Helpers.GetMegaTileRect(3, 2);
            _barrelLeftRect = Helpers.GetReallyBigTileRect(4, 6);
            _barrelDiagonalRect = Helpers.GetReallyBigTileRect(5, 6);
            _destroyedRect = Helpers.GetMegaTileRect(4, 2);

            // Use the base as the primary display component for IsOnScreen checks
            DisplayComponent = new StaticImageDisplay(_megaTextures, _baseRect);

            isEnemyTileColliding = false;
            isTileColliding = false;
            Attack = 2;
            Health = 30;
            IsAffectedByGravity = false;
            IsAffectedByForces = false;
            IsAbleToMoveOutsideOfWorld = false;
            InvincibleTimeAfterBeingHit = 0f;

            // Collision rect in logical units (multiplied by TileScale=4 internally)
            SetWorldLocationCollisionRectangle(36, 44);

            ResetShootTimer();
        }

        public int CurrentHealth => Health;

        private void ResetShootTimer()
        {
            _shootTimer = MIN_SHOOT_TIME + Game1.Randy.NextFloat() * (MAX_SHOOT_TIME - MIN_SHOOT_TIME);
        }

        private Vector2 GetShootDirection()
        {
            switch (_currentDirection)
            {
                case FacingDirection.Left:    
                    return new Vector2(-1, 0);
                case FacingDirection.UpLeft:  
                    return new Vector2(-0.707f, -0.707f);
                case FacingDirection.Up:      
                    return new Vector2(0, -1);
                case FacingDirection.UpRight: 
                    return new Vector2(0.707f, -0.707f);
                case FacingDirection.Right:   
                    return new Vector2(1, 0);
                default:                      
                    return new Vector2(1, 0);
            }
        }

        private void UpdateFacingDirection()
        {
            var direction = Helpers.GetEightWayDirectionTowardsTarget(CollisionCenter, Player.CollisionCenter);

            if (direction.X > 0.5f)
            {
                _currentDirection = direction.Y < -0.5f ? FacingDirection.UpRight : FacingDirection.Right;
            }
            else if (direction.X < -0.5f)
            {
                _currentDirection = direction.Y < -0.5f ? FacingDirection.UpLeft : FacingDirection.Left;
            }
            else
            {
                _currentDirection = FacingDirection.Up;
            }
        }

        private void Shoot()
        {
            var direction = GetShootDirection();
            var shotLocation = GetShotLocation(_currentDirection);
            ShotManager.FireLargeShot(shotLocation, direction * 150f);
            PlaySoundIfOnScreen("Fire", 0.5f);
            ResetShootTimer();
        }

        public override void Kill()
        {
            if (_cannonState == CannonState.Dying || _cannonState == CannonState.Dead) return;

            _cannonState = CannonState.Dying;
            _stateTimer = DyingDuration;
            _explosionTimer = 0f;
            Attack = 0;
            IsPlayerColliding = false;
            CanBeHitWithWeapons = false;
            Game1.Camera.MaxX = null;
            Game1.CurrentLevel.StartSpaceAutoScrolling();
            PlayDeathSound();
            Dead = true;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Enabled)
            {
                _barrelRotationCenter = new Vector2(WorldLocation.X, WorldLocation.Y - CollisionRectangle.Height + 32);

                if (!_hasLockedCamera && Game1.Camera.ViewPort.Contains(CollisionRectangle))
                {
                    Game1.Camera.MaxX = (int)Game1.Camera.Position.X + 32;
                    Game1.CurrentLevel.StopSpaceAutoScrolling();
                    _hasLockedCamera = true;
                }

                switch (_cannonState)
                {
                    case CannonState.Alive:
                        UpdateFacingDirection();
                        if (IsOnScreen())
                        {
                            _shootTimer -= elapsed;
                            if (_shootTimer <= 0)
                            {
                                Shoot();
                            }
                        }
                        break;

                    case CannonState.Dying:
                        _stateTimer -= elapsed;
                        _explosionTimer -= elapsed;

                        if (_explosionTimer <= 0f)
                        {
                            _explosionTimer = ExplosionInterval;
                            var randomX = CollisionRectangle.Left + Game1.Randy.Next(CollisionRectangle.Width);
                            var randomY = CollisionRectangle.Top + Game1.Randy.Next(CollisionRectangle.Height);
                            EffectsManager.AddExplosion(new Vector2(randomX, randomY));
                        }

                        if (_stateTimer <= DyingDuration / 2 && !_isDestroyed)
                        {
                            _isDestroyed = true;
                        }

                        if (_isDestroyed)
                        {
                            _sinkOffset += SinkVelocity * elapsed;
                        }

                        if (_stateTimer <= 0f)
                        {
                            _cannonState = CannonState.Dead;
                        }
                        break;

                    case CannonState.Dead:
                        _sinkOffset += SinkVelocity * elapsed;
                        if (_sinkOffset >= SinkPixels)
                        {
                            Enabled = false;
                        }
                        break;
                }
            }

            base.Update(gameTime, elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsOnScreen()) return;

            if (_isDestroyed)
            {
                var destroyedTopLeft = new Vector2(WorldLocation.X - MegaSize / 2f, WorldLocation.Y - MegaSize + _sinkOffset).ToIntegerVector();
                spriteBatch.Draw(_megaTextures, destroyedTopLeft, _destroyedRect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, DrawDepth);
                return;
            }

            float baseDepth = DrawDepth;
            float topDepth = baseDepth - Game1.MIN_DRAW_INCREMENT;
            float barrelDepth = topDepth - Game1.MIN_DRAW_INCREMENT;

            var baseTopLeft = new Vector2(WorldLocation.X - MegaSize / 2f, WorldLocation.Y - MegaSize).ToIntegerVector();

            // Draw top: centered over base, raised TopRaisMIN_DRAW_INCREMENTePixels
            spriteBatch.Draw(_megaTextures, baseTopLeft - new Vector2(0, TopRaisePixels), _topRect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, topDepth);

            if (Game1.DrawAllCollisionRects)
            {
                spriteBatch.Draw(Game1.TileTextures, new Rectangle(_barrelRotationCenter.X.ToInt() - 2, _barrelRotationCenter.Y.ToInt() - 2, 4, 4), Game1.WhiteSourceRect, Color.Orange * 0.5f, 0f, Vector2.Zero, SpriteEffects.None, 0);

                spriteBatch.Draw(Game1.TileTextures, GetShotLocation(FacingDirection.Left), Game1.WhiteSourceRect, Color.Red * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
                spriteBatch.Draw(Game1.TileTextures, GetShotLocation(FacingDirection.UpLeft), Game1.WhiteSourceRect, Color.Red * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
                spriteBatch.Draw(Game1.TileTextures, GetShotLocation(FacingDirection.Up), Game1.WhiteSourceRect, Color.Red * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
                spriteBatch.Draw(Game1.TileTextures, GetShotLocation(FacingDirection.UpRight), Game1.WhiteSourceRect, Color.Red * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
                spriteBatch.Draw(Game1.TileTextures, GetShotLocation(FacingDirection.Right), Game1.WhiteSourceRect, Color.Red * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);

            }

            DrawBarrel(spriteBatch, _barrelRotationCenter, barrelDepth);

            base.Draw(spriteBatch);
        }

        private Vector2 GetShotLocation(FacingDirection direction)
        {
            switch (direction)
            {
                case FacingDirection.Left:
                    return _barrelRotationCenter + new Vector2(-104, 0);
                case FacingDirection.Right:
                    return _barrelRotationCenter + new Vector2(104, 0);
                case FacingDirection.Up:
                    return _barrelRotationCenter + new Vector2(0, -76);
                case FacingDirection.UpLeft:
                    return _barrelRotationCenter + new Vector2(-72, -60);
                case FacingDirection.UpRight:
                    return _barrelRotationCenter + new Vector2(72, -60);
                default:
                    return _barrelRotationCenter;
            }
        }

        private void DrawBarrel(SpriteBatch spriteBatch, Vector2 center, float depth)
        {

            var drawCenter = center + new Vector2(-ReallyBigSize / 2, -ReallyBigSize / 2);

            switch (_currentDirection)
            {
                case FacingDirection.Left:
                    spriteBatch.Draw(_reallyBigTextures,
                        drawCenter + new Vector2(-72, 0),
                        _barrelLeftRect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, depth);
                    break;

                case FacingDirection.Right:
                    spriteBatch.Draw(_reallyBigTextures,
                        drawCenter + new Vector2(72, 0),
                        _barrelLeftRect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, depth);
                    break;

                case FacingDirection.UpLeft:
                    spriteBatch.Draw(_reallyBigTextures,
                        drawCenter + new Vector2(-52, -42),
                        _barrelDiagonalRect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, depth);
                    break;

                case FacingDirection.UpRight:
                    spriteBatch.Draw(_reallyBigTextures,
                        drawCenter + new Vector2(52, -42),
                        _barrelDiagonalRect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, depth);
                    break;

                case FacingDirection.Up:
                    spriteBatch.Draw(_reallyBigTextures,
                        drawCenter + new Vector2(64, -24),
                        _barrelLeftRect, Color.White, MathHelper.PiOver2, new Vector2(32, 32), 1f, SpriteEffects.None, depth);
                    break;
            }
        }
    }
}
