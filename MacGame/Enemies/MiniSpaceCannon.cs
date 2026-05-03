using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class MiniSpaceCannon : Enemy
    {
        private const float MIN_SHOOT_TIME = 1f;
        private const float MAX_SHOOT_TIME = 2f;
        private const float ShootSpeed = 150f;

        private float _shootTimer = 0f;

        private readonly Rectangle _leftRect;
        private readonly Rectangle _upLeftRect;
        private readonly Rectangle _upRect;

        private StaticImageDisplay display => (StaticImageDisplay)DisplayComponent;

        public bool UpsideDown { get; set; }

        // The 5 canonical directions; when UpsideDown, Up/UpLeft/UpRight become Down/DownLeft/DownRight.
        private enum FacingDirection { Left, UpLeft, Up, UpRight, Right }
        private FacingDirection _currentDirection = FacingDirection.Left;

        /// <summary>
        /// A small space cannon that can shoot left, up-left, up, up-right, or right, depending on the player's position. 
        /// When UpsideDown is true, it shoots down instead of up.
        /// </summary>
        public MiniSpaceCannon(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\SpaceTextures");
            _leftRect = Helpers.GetTileRect(5, 7);
            _upLeftRect = Helpers.GetTileRect(6, 7);
            _upRect = Helpers.GetTileRect(7, 7);

            DisplayComponent = new StaticImageDisplay(textures, _leftRect);

            isEnemyTileColliding = false;
            isTileColliding = false;
            Attack = 1;
            Health = 4;
            IsAffectedByGravity = false;
            IsAffectedByForces = false;
            IsAbleToMoveOutsideOfWorld = false;
            InvincibleTimeAfterBeingHit = 0.1f;

            SetCenteredCollisionRectangle(8, 8, 6, 6);

            ResetShootTimer();
        }

        private void ResetShootTimer()
        {
            _shootTimer = MIN_SHOOT_TIME + (Game1.Randy.NextFloat() * (MAX_SHOOT_TIME - MIN_SHOOT_TIME));
        }

        private void UpdateFacingDirection()
        {
            var dir = Helpers.GetEightWayDirectionTowardsTarget(CollisionCenter, Player.CollisionCenter);

            if (!UpsideDown)
            {
                // Mirrors SpaceCannon: avoid downward shots.
                if (dir.X > 0.5f)
                {
                    _currentDirection = dir.Y < -0.5f ? FacingDirection.UpRight : FacingDirection.Right;
                }
                else if (dir.X < -0.5f)
                {
                    _currentDirection = dir.Y < -0.5f ? FacingDirection.UpLeft : FacingDirection.Left;
                }
                else
                {
                    _currentDirection = FacingDirection.Up;
                }
            }
            else
            {
                // Inverted: avoid upward shots, prefer downward.
                if (dir.X > 0.5f)
                {
                    _currentDirection = dir.Y > 0.5f ? FacingDirection.UpRight : FacingDirection.Right;
                }
                else if (dir.X < -0.5f)
                {
                    _currentDirection = dir.Y > 0.5f ? FacingDirection.UpLeft : FacingDirection.Left;
                }
                else
                {
                    _currentDirection = FacingDirection.Up;
                }
            }

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            SpriteEffects baseEffect = UpsideDown ? SpriteEffects.FlipVertically : SpriteEffects.None;

            switch (_currentDirection)
            {
                case FacingDirection.Left:
                    display.Source = _leftRect;
                    display.Effect = baseEffect;
                    break;
                case FacingDirection.UpLeft:
                    display.Source = _upLeftRect;
                    display.Effect = baseEffect;
                    break;
                case FacingDirection.Up:
                    display.Source = _upRect;
                    display.Effect = baseEffect;
                    break;
                case FacingDirection.UpRight:
                    display.Source = _upLeftRect;
                    display.Effect = baseEffect | SpriteEffects.FlipHorizontally;
                    break;
                case FacingDirection.Right:
                    display.Source = _leftRect;
                    display.Effect = baseEffect | SpriteEffects.FlipHorizontally;
                    break;
            }
        }

        private Vector2 GetShootDirection()
        {
            float ySign = UpsideDown ? 1f : -1f;

            switch (_currentDirection)
            {
                case FacingDirection.Left:    return new Vector2(-1f, 0f);
                case FacingDirection.UpLeft:  return new Vector2(-0.707f, ySign * 0.707f);
                case FacingDirection.Up:      return new Vector2(0f, ySign);
                case FacingDirection.UpRight: return new Vector2(0.707f, ySign * 0.707f);
                case FacingDirection.Right:   return new Vector2(1f, 0f);
                default:                      return new Vector2(1f, 0f);
            }
        }

        private void Shoot()
        {
            ShotManager.FireSmallShot(CollisionCenter, GetShootDirection() * ShootSpeed);
            PlaySoundIfOnScreen("Fire", 0.5f);
            ResetShootTimer();
        }

        public override void Kill()
        {
            EffectsManager.AddExplosion(WorldCenter, false);
            Dead = true;
            PlayDeathSound();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Alive)
            {
                UpdateFacingDirection();

                if (IsOnScreen())
                {
                    _shootTimer -= elapsed;
                    if (_shootTimer <= 0f)
                    {
                        Shoot();
                    }
                }
            }

            base.Update(gameTime, elapsed);
        }
    }
}
