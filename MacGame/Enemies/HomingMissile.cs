using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// A homing missile that tracks the player and moves toward them in 8-way directions.
    /// Health is 1 — destroyed in a single hit. Intended to be launched by other enemies or guns.
    /// </summary>
    public class HomingMissile : Enemy
    {
        private const float Speed = 90;
        private const float TurnInterval = 0.15f;

        private float turnTimer = 0f;

        private readonly Rectangle rightRect;
        private readonly Rectangle upRightRect;

        private StaticImageDisplay display => (StaticImageDisplay)DisplayComponent;

        public EightWayRotation RotationDirection { get; set; }

        public HomingMissile(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\SpaceTextures");

            // (3,5) points right, (4,5) points up-right
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
            RotationDirection = new EightWayRotation(VectorToEightWayDirection(dir));
        }

        private static EightWayRotationDirection VectorToEightWayDirection(Vector2 dir)
        {
            if (dir.X > 0.5f && dir.Y < -0.5f) return EightWayRotationDirection.UpRight;
            if (dir.X > 0.5f && dir.Y > 0.5f) return EightWayRotationDirection.DownRight;
            if (dir.X < -0.5f && dir.Y < -0.5f) return EightWayRotationDirection.UpLeft;
            if (dir.X < -0.5f && dir.Y > 0.5f) return EightWayRotationDirection.DownLeft;
            if (dir.X > 0.5f) return EightWayRotationDirection.Right;
            if (dir.X < -0.5f) return EightWayRotationDirection.Left;
            if (dir.Y < -0.5f) return EightWayRotationDirection.Up;
            return EightWayRotationDirection.Down;
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

        public void Launch(Vector2 position)
        {
            WorldLocation = position;
            Velocity = Vector2.Zero;
            Health = 1;
            Enabled = true;
            Alive = true;
            InvincibleTimer = 0;
            turnTimer = 0f;
        }

        public override void AfterHittingPlayer()
        {
            Kill();
        }

        public override void Kill()
        {
            EffectsManager.AddExplosion(WorldCenter);
            Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Enabled && Alive)
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

            base.Update(gameTime, elapsed);
        }
    }
}
