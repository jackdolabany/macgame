using System;
using MacGame.Enemies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
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

            // Initialize small shots (2x2 pixels at 5,6)
            for (int i = 0; i < MAX_SHOTS; i++)
            {
                var shot = new EnemyShot(content, Helpers.GetTileRect(5, 6), 2, 2);
                shot.Enabled = false;
                SmallShots.SetItem(i, shot);
            }

            // Initialize medium shots (4x4 pixels at 6,6)
            for (int i = 0; i < MAX_SHOTS; i++)
            {
                var shot = new EnemyShot(content, Helpers.GetTileRect(6, 6), 4, 4);
                shot.Enabled = false;
                MediumShots.SetItem(i, shot);
            }

            // Initialize large shots (6x6 pixels at 7,6)
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
                    s.IsAbleToSurviveOutsideOfWorld = true;
                    TimerManager.AddNewTimer((float)Game1.Randy.NextDouble() * 0.7f, () =>
                    {
                        s.IsAbleToSurviveOutsideOfWorld = false;
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
            var shot = (EnemyShot)SmallShots.GetNextObject();
            shot.WorldLocation = position + new Vector2(0, shot.CollisionRectangle.Height / 2);
            shot.Velocity = velocity;
            shot.DisplayComponent.DrawDepth = drawDepth ?? shooter.DisplayComponent.DrawDepth + Game1.MIN_DRAW_INCREMENT;
            shot.IsAbleToSurviveOutsideOfWorld = false;
            shot.Enabled = true;
            shot.Alive = true;
        }

        public static void FireMediumShot(Vector2 position, Vector2 velocity, GameObject shooter, float? drawDepth = null)
        {
            var shot = (EnemyShot)MediumShots.GetNextObject();
            shot.WorldLocation = position + new Vector2(0, shot.CollisionRectangle.Height / 2);
            shot.Velocity = velocity;
            shot.DisplayComponent.DrawDepth = drawDepth ?? shooter.DisplayComponent.DrawDepth + Game1.MIN_DRAW_INCREMENT;
            shot.IsAbleToSurviveOutsideOfWorld = false;
            shot.Enabled = true;
            shot.Alive = true;
        }

        public static void FireLargeShot(Vector2 position, Vector2 velocity, GameObject shooter, float? drawDepth = null)
        {
            var shot = (EnemyShot)LargeShots.GetNextObject();
            shot.WorldLocation = position + new Vector2(0, shot.CollisionRectangle.Height / 2);
            shot.Velocity = velocity;
            shot.DisplayComponent.DrawDepth = drawDepth ?? shooter.DisplayComponent.DrawDepth + Game1.MIN_DRAW_INCREMENT;
            shot.IsAbleToSurviveOutsideOfWorld = false;
            shot.Enabled = true;
            shot.Alive = true;
        }
    }
}
