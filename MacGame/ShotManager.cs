using System;
using MacGame.Enemies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
    public enum ShotSize { Small, Medium, Large }

    public static class ShotManager
    {
        public const int MAX_SHOTS = 120;

        private static GameObjectCircularBuffer SmallShots;
        private static GameObjectCircularBuffer MediumShots;
        private static GameObjectCircularBuffer LargeShots;

        public static void Initialize(ContentManager content)
        {
            SmallShots = new GameObjectCircularBuffer(MAX_SHOTS);
            MediumShots = new GameObjectCircularBuffer(MAX_SHOTS);
            LargeShots = new GameObjectCircularBuffer(MAX_SHOTS);

            for (int i = 0; i < MAX_SHOTS; i++)
            {
                var shot = new EnemyShot(content, Helpers.GetTileRect(5, 6), 2, 2);
                shot.Enabled = false;
                SmallShots.SetItem(i, shot);
            }

            for (int i = 0; i < MAX_SHOTS; i++)
            {
                var shot = new EnemyShot(content, Helpers.GetTileRect(6, 6), 4, 4);
                shot.Enabled = false;
                MediumShots.SetItem(i, shot);
            }

            for (int i = 0; i < MAX_SHOTS; i++)
            {
                var shot = new EnemyShot(content, Helpers.GetTileRect(7, 6), 6, 6);
                shot.Enabled = false;
                LargeShots.SetItem(i, shot);
            }
        }

        public static void ClearShotsInstant()
        {
            SmallShots.Disable();
            MediumShots.Disable();
            LargeShots.Disable();
        }

        public static void ClearShotsCinematic()
        {
            ScheduleCinematicClear(SmallShots);
            ScheduleCinematicClear(MediumShots);
            ScheduleCinematicClear(LargeShots);
        }

        private static void ScheduleCinematicClear(GameObjectCircularBuffer shots)
        {
            foreach (EnemyShot shot in shots)
            {
                if (shot.Enabled && shot.Alive)
                {
                    var s = shot;
                    TimerManager.AddNewTimer((float)Game1.Randy.NextDouble() * 0.5f, () =>
                    {
                        if (s.Enabled) 
                        { 
                            s.Kill(); 
                        }
                    });
                }
            }
        }

        public static void Update(GameTime gameTime, float elapsed)
        {
            SmallShots.Update(gameTime, elapsed);
            MediumShots.Update(gameTime, elapsed);
            LargeShots.Update(gameTime, elapsed);
        }

        public static void CheckPlayerCollisions(Player player)
        {
            if (!player.Enabled) return;

            foreach (EnemyShot shot in SmallShots)
            {
                if (shot.Enabled && shot.Alive)
                {
                    player.CheckEnemyInteractions(shot);
                }
            }

            foreach (EnemyShot shot in MediumShots)
            {
                if (shot.Enabled && shot.Alive)
                {
                    player.CheckEnemyInteractions(shot);
                }
            }

            foreach (EnemyShot shot in LargeShots)
            {
                if (shot.Enabled && shot.Alive)
                {
                    player.CheckEnemyInteractions(shot);
                }
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            SmallShots.Draw(spriteBatch);
            MediumShots.Draw(spriteBatch);
            LargeShots.Draw(spriteBatch);
        }

        public static void FireSmallShot(Vector2 position, Vector2 velocity, GameObject shooter, float? drawDepth = null)
        {
          _fireShot(position, velocity, ShotSize.Small, shooter, drawDepth);
        }

        public static void FireMediumShot(Vector2 position, Vector2 velocity, GameObject shooter, float? drawDepth = null)
        {
            _fireShot(position, velocity, ShotSize.Medium, shooter, drawDepth);
        }

        public static void FireLargeShot(Vector2 position, Vector2 velocity, GameObject shooter, float? drawDepth = null)
        {
            _fireShot(position, velocity, ShotSize.Large, shooter, drawDepth);
        }

        private static void _fireShot(Vector2 position, Vector2 velocity, ShotSize size, GameObject shooter, float? drawDepth = null)
        {
            EnemyShot shot;
            switch (size)
            {
                case ShotSize.Small:
                    shot = (EnemyShot)SmallShots.GetNextObject();
                    break;
                case ShotSize.Medium:
                    shot = (EnemyShot)MediumShots.GetNextObject();
                    break;
                case ShotSize.Large:
                    shot = (EnemyShot)LargeShots.GetNextObject();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(size), size, null);
            }
            shot.WorldLocation = position + new Vector2(0, shot.CollisionRectangle.Height / 2);
            shot.Velocity = velocity;
            shot.RotateCenter = null;
            shot.DisplayComponent.DrawDepth = drawDepth ?? shooter.DisplayComponent.DrawDepth + Game1.MIN_DRAW_INCREMENT;
            shot.Enabled = true;
            shot.Alive = true;
        }

        private static void CreateRing(Vector2 center, Vector2 centerVelocity, int count, float radius, float rotationSpeed, ShotSize size, GameObject shooter)
        {
            var pool = size == ShotSize.Large ? LargeShots : size == ShotSize.Medium ? MediumShots : SmallShots;
            float drawDepth = shooter.DisplayComponent.DrawDepth - Game1.MIN_DRAW_INCREMENT;
            for (int i = 0; i < count; i++)
            {
                var shot = (EnemyShot)pool.GetNextObject();
                float angle = i * MathHelper.TwoPi / count;
                shot.RotateCenter = center;
                shot.RotateCenterVelocity = centerVelocity;
                shot.RotateRadius = radius;
                shot.RotateAngle = angle;
                shot.RotateSpeed = rotationSpeed;
                shot.Velocity = Vector2.Zero;
                shot.WorldLocation = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                shot.DisplayComponent.DrawDepth = drawDepth;
                shot.Enabled = true;
                shot.Alive = true;
            }
        }

        public static void FireSmallRing(Vector2 position, Vector2 velocity, GameObject shooter)
        {
            CreateRing(position, velocity, 6, 24f, 3f, ShotSize.Small, shooter);
        }

        public static void FireMediumRing(Vector2 position, Vector2 velocity, GameObject shooter)
        {
            CreateRing(position, velocity, 6, 32f, 3f, ShotSize.Medium, shooter);
        }
    }
}
