using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class SpaceCannon : Enemy
    {
        private const float MIN_SHOOT_TIME = 1f;
        private const float MAX_SHOOT_TIME = 2f;

        private float shootTimer = 0f;
        private Texture2D textures;

        private Rectangle leftRect;
        private Rectangle upLeftRect;
        private Rectangle upRect;
        private Rectangle deadRect;

        private enum FacingDirection
        {
            Left,
            UpLeft,
            Up,
            UpRight,
            Right
        }

        private FacingDirection currentDirection = FacingDirection.Left;

        public SpaceCannon(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            textures = content.Load<Texture2D>(@"Textures\BigTextures");

            leftRect = Helpers.GetBigTileRect(8, 8);
            upLeftRect = Helpers.GetBigTileRect(9, 8);
            upRect = Helpers.GetBigTileRect(10, 8);
            deadRect = Helpers.GetBigTileRect(11, 8);

            DisplayComponent = new StaticImageDisplay(textures, leftRect);

            isEnemyTileColliding = false;
            isTileColliding = false;
            Attack = 1;
            Health = 4;
            IsAffectedByGravity = false;
            IsAffectedByForces = false;
            IsAbleToMoveOutsideOfWorld = false;
            InvincibleTimeAfterBeingHit = 0.1f;

            SetCenteredCollisionRectangle(16, 16, 14, 14);

            ResetShootTimer();
        }

        private void ResetShootTimer()
        {
            shootTimer = MIN_SHOOT_TIME + (Game1.Randy.NextFloat() * (MAX_SHOOT_TIME - MIN_SHOOT_TIME));
        }

        private void UpdateFacingDirection()
        {
            // Get the 8-way direction and map it to our 5 directions
            var direction = Helpers.GetEightWayDirectionTowardsTarget(CollisionCenter, Player.CollisionCenter);

            // Map 8-way to 5-way (no downward shots)
            // Check X first, then Y to determine direction
            if (direction.X > 0.5f)
            {
                // Pointing right
                if (direction.Y < -0.5f)
                {
                    // UpRight
                    currentDirection = FacingDirection.UpRight;
                }
                else
                {
                    // Right or DownRight -> Right
                    currentDirection = FacingDirection.Right;
                }
            }
            else if (direction.X < -0.5f)
            {
                // Pointing left
                if (direction.Y < -0.5f)
                {
                    // UpLeft
                    currentDirection = FacingDirection.UpLeft;
                }
                else
                {
                    // Left or DownLeft -> Left
                    currentDirection = FacingDirection.Left;
                }
            }
            else
            {
                // Pointing vertically (up or down)
                // Up or Down -> Up (no downward shots)
                currentDirection = FacingDirection.Up;
            }

            // Update display based on direction
            var staticDisplay = (StaticImageDisplay)DisplayComponent;
            switch (currentDirection)
            {
                case FacingDirection.Left:
                    staticDisplay.Source = leftRect;
                    Flipped = false;
                    break;
                case FacingDirection.UpLeft:
                    staticDisplay.Source = upLeftRect;
                    Flipped = false;
                    break;
                case FacingDirection.Up:
                    staticDisplay.Source = upRect;
                    Flipped = false;
                    break;
                case FacingDirection.UpRight:
                    staticDisplay.Source = upLeftRect;
                    Flipped = true;
                    break;
                case FacingDirection.Right:
                    staticDisplay.Source = leftRect;
                    Flipped = true;
                    break;
            }
        }

        private Vector2 GetShootDirection()
        {
            // Return direction vector based on current facing
            switch (currentDirection)
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

        private void Shoot()
        {
            var shootSpeed = 150f;
            var direction = GetShootDirection();
            var velocity = direction * shootSpeed;

            var shotLocation = GetShotLocation();

            ShotManager.FireMediumShot(shotLocation, velocity);

            PlaySoundIfOnScreen("Fire", 0.5f);

            ResetShootTimer();
        }

        private Vector2 GetShotLocation()
        {
            var shotLocation = CollisionCenter + new Vector2(0, 12);

            switch(currentDirection)
            {
                case FacingDirection.Left:
                    shotLocation += new Vector2(-28, 0);
                    break;
                case FacingDirection.UpLeft:
                    shotLocation += new Vector2(-20, -20);
                    break;
                case FacingDirection.Up:
                    shotLocation += new Vector2(0, -24);
                    break;
                case FacingDirection.UpRight:
                    shotLocation += new Vector2(20, -20);
                    break;
                case FacingDirection.Right:
                    shotLocation += new Vector2(28, 0);
                    break;
            }

            return shotLocation;
        }

        public override void Kill()
        {
            EffectsManager.AddExplosion(WorldCenter, false);
            Dead = true;
            PlayDeathSound();

            // Don't call the base method because I don't want to disable on death.
            // Because we want to draw the destroyed tower.
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Alive)
            {
                // Update facing direction to track player
                UpdateFacingDirection();

                if (IsOnScreen())
                {
                    shootTimer -= elapsed;

                    if (shootTimer <= 0)
                    {
                        Shoot();
                    }
                }
            }
            else
            {
                var staticDisplay = (StaticImageDisplay)DisplayComponent;
                staticDisplay.Source = deadRect;
            }

            base.Update(gameTime, elapsed);
        }
    }
}
