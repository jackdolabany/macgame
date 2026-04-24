using System.Collections.Generic;
using MacGame.Enemies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    public static class MissileManager
    {
        private const int PoolSize = 20;

        private static Missile[] pool = new Missile[0];

        public static IReadOnlyList<Missile> Pool => pool;

        /// <summary>
        /// Call once per level load. Creates the missile pool and adds all missiles to the level's
        /// enemy list so they participate in player collisions and weapon hits automatically.
        /// </summary>
        public static void Initialize(ContentManager content, Player player, Camera camera, Level level)
        {
            pool = new Missile[PoolSize];
            for (int i = 0; i < PoolSize; i++)
            {
                var missile = new Missile(content, 0, 0, player, camera);
                missile.Enabled = false;
                pool[i] = missile;
                level.Enemies.Add(missile);
            }
        }

        /// <summary>
        /// Launches the next available missile as a homing missile that immediately tracks the player.
        /// </summary>
        public static Missile? LaunchHomingMissile(Vector2 position)
        {
            foreach (var missile in pool)
            {
                if (!missile.Enabled)
                {
                    missile.LaunchHoming(position);
                    return missile;
                }
            }
            return null;
        }

        /// <summary>
        /// Launches the next available missile in a fixed direction. If homingDelay is provided,
        /// the missile flies straight for that many seconds before switching to homing behavior.
        /// If omitted, the missile flies straight forever.
        /// </summary>
        public static Missile? LaunchMissile(Vector2 position, Vector2 direction, float? homingDelay = null)
        {
            foreach (var missile in pool)
            {
                if (!missile.Enabled)
                {
                    missile.Launch(position, direction, homingDelay ?? -1f);
                    return missile;
                }
            }
            return null;
        }

        public static Missile? LaunchMissile(Vector2 position, EightWayRotationDirection direction, float? homingDelay = null)
        {
            return LaunchMissile(position, new EightWayRotation(direction).Vector2, homingDelay);
        }

        public static void Clear()
        {
            foreach (var missile in pool)
            {
                missile.Enabled = false;
            }
        }
    }
}
