using System;
using System.Collections.Generic;
using MacGame.DisplayComponents;
using MacGame.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public enum SpaceTrashState
    {
        NotAppearedYet,
        Moving,
        MovedOffScreen,
    }

    /// <summary>
    /// Just some trash floating in space.
    /// </summary>
    public class SpaceTrash : Enemy
    {
        private static readonly Random _random = new Random();

        private const float Speed = 60f;

        private StaticImageDisplay display => (StaticImageDisplay)DisplayComponent;

        private readonly Texture2D _textures;

        private int _imageIndex;
        private int _rotationDegrees;

        // Angle of travel, in degrees, measured from horizontal. 0 is straight left, positive is
        // down-left, negative is up-left. Always moves right to left.
        private float _angleDegrees;

        private Vector2 _spawnLocation;

        // The trash's own travel speed, in pixels/sec, independent of the level's auto scroll.
        private Vector2 _direction;

        private SpaceTrashState _state = SpaceTrashState.NotAppearedYet;
        private bool _hasBeenOnScreen;

        public SpaceTrash(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            _textures = content.Load<Texture2D>(@"Textures\BigTextures");
            DisplayComponent = new StaticImageDisplay(_textures, Helpers.GetBigTileRect(11, 4));

            _spawnLocation = WorldLocation;

            _imageIndex = _random.Next(0, 3);
            _rotationDegrees = _random.Next(0, 4) * 90;
            _angleDegrees = (float)(_random.NextDouble() * 180.0 - 90.0);

            ApplyImage();
            ApplyRotation();
            ApplyMovement();

            Attack = 1;
            Health = 1;

            CanBeHitWithWeapons = true;
            CanBeJumpedOn = false;
            isEnemyTileColliding = false;
            isTileColliding = false;
            IsAffectedByGravity = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;

            SetCenteredCollisionRectangle(16, 16, 10, 10);

            // Stay hidden until the placement location comes on screen and the journey starts.
            Enabled = false;
        }

        public override void SetProps(Dictionary<string, string> props)
        {
            base.SetProps(props);

            if (props.ContainsKey("Image") && int.TryParse(props["Image"], out int image))
            {
                _imageIndex = Math.Clamp(image, 0, 2);
                ApplyImage();
            }

            if (props.ContainsKey("Rotation") && int.TryParse(props["Rotation"], out int rotation))
            {
                if (rotation == 0 || rotation == 90 || rotation == 180 || rotation == 270)
                {
                    _rotationDegrees = rotation;
                    ApplyRotation();
                }
            }

            if (props.ContainsKey("Angle") && float.TryParse(props["Angle"], out float angle))
            {
                _angleDegrees = MathHelper.Clamp(angle, -90f, 90f);
                ApplyMovement();
            }
        }

        public override void TakeHit(GameObject attacker, int damage)
        {
            // Mostly invincible: weapons still collide with (and are destroyed by) the trash,
            // but only a charged shot is strong enough to actually blow it up.
            if (attacker is ChargedSpaceshipShot)
            {
                base.TakeHit(attacker, damage);
            }
        }

        public override void Kill()
        {
            EffectsManager.AddExplosion(WorldCenter);
            base.Kill();
        }

        public override void PlayDeathSound()
        {
            // No sound when the trash is blown up.
        }

        private void BlowUpCollidingMissiles()
        {
            foreach (var missile in MissileManager.Pool)
            {
                if (missile.Enabled && missile.Alive && missile.CollisionRectangle.Intersects(CollisionRectangle))
                {
                    missile.Kill();
                }
            }
        }

        private void DamageCollidingShips()
        {
            foreach (var enemy in Game1.CurrentLevel.Enemies)
            {
                if (enemy is EnemyShipBase ship && ship.Alive && ship.Enabled && ship.CollisionRectangle.Intersects(CollisionRectangle))
                {
                    ship.Kill();
                }
            }
        }

        private void ApplyImage()
        {
            display.Source = Helpers.GetBigTileRect(11 + _imageIndex, 4);
        }

        private void ApplyRotation()
        {
            display.Rotation = MathHelper.ToRadians(_rotationDegrees);
        }

        private void ApplyMovement()
        {
            var thetaRadians = MathHelper.ToRadians(_angleDegrees);
            _direction = new Vector2(-(float)Math.Cos(thetaRadians), (float)Math.Sin(thetaRadians)) * Speed;
        }

        private void StartJourney()
        {
            _state = SpaceTrashState.Moving;
            Enabled = true;

            WorldLocation = _spawnLocation - _direction * GetOffscreenDistance();

            // Subtract the level's current auto scroll speed so the trash's on-screen speed
            // matches _direction exactly, regardless of how fast the level is scrolling.
            Velocity = _direction - AutoScrollSpeed;
        }

        // Distance backward along _direction needed to just clear the viewport, so the trash
        // doesn't have to travel further than necessary to reach the spawn point off screen.
        // Padding adds a bit of extra runway so it doesn't pop in right at the edge.
        private float GetOffscreenDistance()
        {
            const float padding = 96f;
            var viewPort = camera.ViewPort;
            var backward = -_direction;

            var distance = float.MaxValue;

            if (backward.X > 0.0001f)
            {
                distance = Math.Min(distance, (viewPort.Right + padding - _spawnLocation.X) / backward.X);
            }
            else if (backward.X < -0.0001f)
            {
                distance = Math.Min(distance, (_spawnLocation.X - (viewPort.Left - padding)) / -backward.X);
            }

            if (backward.Y > 0.0001f)
            {
                distance = Math.Min(distance, (viewPort.Bottom + padding - _spawnLocation.Y) / backward.Y);
            }
            else if (backward.Y < -0.0001f)
            {
                distance = Math.Min(distance, (_spawnLocation.Y - (viewPort.Top - padding)) / -backward.Y);
            }

            return distance;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            switch (_state)
            {
                case SpaceTrashState.NotAppearedYet:
                    if (camera.IsPointVisible(_spawnLocation))
                    {
                        StartJourney();
                    }
                    break;

                case SpaceTrashState.Moving:
                    if (IsOnScreen())
                    {
                        _hasBeenOnScreen = true;
                    }

                    if (_hasBeenOnScreen && camera.IsWayOffscreen(CollisionRectangle))
                    {
                        _state = SpaceTrashState.MovedOffScreen;
                        Enabled = false;
                    }

                    BlowUpCollidingMissiles();
                    DamageCollidingShips();
                    break;

                case SpaceTrashState.MovedOffScreen:
                    return;
            }

            base.Update(gameTime, elapsed);
        }
    }
}
