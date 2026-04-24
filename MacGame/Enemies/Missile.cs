using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class Missile : Enemy
    {
        private const float Speed = 90;
        private const float TurnInterval = 0.15f;

        private float turnTimer = 0f;

        private CircularBuffer<ShipFire> _fires;
        private float _fireTimer = 0f;
        private const float FireInterval = 0.1f;

        private readonly Rectangle rightRect;
        private readonly Rectangle upRightRect;

        private StaticImageDisplay display => (StaticImageDisplay)DisplayComponent;

        public EightWayRotation RotationDirection { get; set; }

        // True when actively tracking the player.
        private bool _isHoming;

        // When > 0, counts down before switching to homing. < 0 means never home.
        private float _homingCountdown;

        public Missile(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\SpaceTextures");

            _fires = new CircularBuffer<ShipFire>(10);
            for (int i = 0; i < 10; i++)
            {
                _fires.SetItem(i, new ShipFire(textures));
            }

            rightRect = Helpers.GetTileRect(3, 5);
            upRightRect = Helpers.GetTileRect(4, 5);

            DisplayComponent = new StaticImageDisplay(textures, rightRect);

            RotationDirection = new EightWayRotation(EightWayRotationDirection.Right);

            isEnemyTileColliding = false;
            isTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;
            IsAffectedByForces = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            InvincibleTimeAfterBeingHit = 0f;

            SetCenteredCollisionRectangle(8, 8, 6, 6);
        }

        private void UpdateDirectionTowardsPlayer()
        {
            var dir = Helpers.GetEightWayDirectionTowardsTarget(CollisionCenter, Player.CollisionCenter);
            RotationDirection = new EightWayRotation(Helpers.VectorToEightWayDirection(dir));
        }

        private void UpdateDisplay()
        {
            display.Rotation = 0f;
            display.Effect = SpriteEffects.None;

            switch (RotationDirection.Direction)
            {
                case EightWayRotationDirection.Right:
                    display.Source = rightRect;
                    break;
                case EightWayRotationDirection.Down:
                    display.Source = rightRect;
                    display.Rotation = MathHelper.PiOver2;
                    break;
                case EightWayRotationDirection.Left:
                    display.Source = rightRect;
                    display.Effect = SpriteEffects.FlipHorizontally;
                    break;
                case EightWayRotationDirection.Up:
                    display.Source = rightRect;
                    display.Rotation = -MathHelper.PiOver2;
                    break;
                case EightWayRotationDirection.UpRight:
                    display.Source = upRightRect;
                    break;
                case EightWayRotationDirection.UpLeft:
                    display.Source = upRightRect;
                    display.Effect = SpriteEffects.FlipHorizontally;
                    break;
                case EightWayRotationDirection.DownRight:
                    display.Source = upRightRect;
                    display.Effect = SpriteEffects.FlipVertically;
                    break;
                case EightWayRotationDirection.DownLeft:
                    display.Source = upRightRect;
                    display.Effect = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
                    break;
            }
        }

        private void ResetCommon(Vector2 position)
        {
            WorldLocation = position;
            Health = 1;
            Enabled = true;
            Alive = true;
            InvincibleTimer = 0;
            turnTimer = 0f;
            _fireTimer = 0f;
            for (int i = 0; i < _fires.Length; i++)
            {
                _fires.GetItem(i).Enabled = false;
            }
        }

        public void LaunchHoming(Vector2 position)
        {
            ResetCommon(position);
            Velocity = Vector2.Zero;
            _isHoming = true;
            _homingCountdown = 0f;
        }

        /// <summary>
        /// Launches the missile in a fixed direction. If homingDelay is >= 0, the missile turns
        /// into a homing missile after that many seconds. If homingDelay is negative, it flies
        /// straight forever.
        /// </summary>
        public void Launch(Vector2 position, Vector2 direction, float homingDelay = -1f)
        {
            ResetCommon(position);
            _isHoming = false;
            _homingCountdown = homingDelay;

            var normalized = Vector2.Normalize(direction);
            RotationDirection = new EightWayRotation(Helpers.VectorToEightWayDirection(normalized));
            UpdateDisplay();
            Velocity = RotationDirection.Vector2 * Speed;
        }

        public override void AfterHittingPlayer()
        {
            Kill();
        }

        public override void Kill()
        {
            EffectsManager.AddExplosion(WorldCenter);
            Enabled = false;
            for (int i = 0; i < _fires.Length; i++)
            {
                _fires.GetItem(i).Enabled = false;
            }
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Enabled && Alive)
            {
                if (!_isHoming)
                {
                    if (_homingCountdown >= 0f)
                    {
                        _homingCountdown -= elapsed;
                        if (_homingCountdown <= 0f)
                        {
                            _isHoming = true;
                            turnTimer = 0f;
                        }
                    }
                }

                if (_isHoming && Player.Enabled)
                {
                    turnTimer -= elapsed;
                    if (turnTimer <= 0f)
                    {
                        UpdateDirectionTowardsPlayer();
                        UpdateDisplay();
                        velocity = RotationDirection.Vector2 * Speed;
                        turnTimer = TurnInterval;
                    }
                }

                _fireTimer += elapsed;
                if (_fireTimer >= FireInterval)
                {
                    _fireTimer = 0f;
                    var fire = _fires.GetNextObject();
                    fire.Reset();
                    fire.SetDrawDepth(DrawDepth + Game1.MIN_DRAW_INCREMENT);
                    var behind = -RotationDirection.Vector2 * 12f;
                    fire.WorldLocation = WorldLocation + behind;
                    fire.Velocity = -RotationDirection.Vector2 * 30f;
                }

                for (int i = 0; i < _fires.Length; i++)
                {
                    var fire = _fires.GetItem(i);
                    if (fire.Enabled)
                    {
                        fire.Update(gameTime, elapsed);
                    }
                }
            }

            base.Update(gameTime, elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _fires.Length; i++)
            {
                var fire = _fires.GetItem(i);
                if (fire.Enabled)
                {
                    fire.Draw(spriteBatch);
                }
            }

            base.Draw(spriteBatch);
        }
    }
}
