using System.Collections.Generic;
using MacGame.Enemies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame
{
    public static class MissileManager
    {
        private const int PoolSize = 20;

        private static HomingMissile[] pool = new HomingMissile[0];

        public static IReadOnlyList<HomingMissile> Pool => pool;

        /// <summary>
        /// Call once per level load. Creates the missile pool and adds all missiles to the level's
        /// enemy list so they participate in player collisions and weapon hits automatically.
        /// </summary>
        public static void Initialize(ContentManager content, Player player, Camera camera, Level level)
        {
            pool = new HomingMissile[PoolSize];
            for (int i = 0; i < PoolSize; i++)
            {
                var missile = new HomingMissile(content, 0, 0, player, camera);
                missile.Enabled = false;
                pool[i] = missile;
                level.Enemies.Add(missile);
            }
        }

        /// <summary>
        /// Launches the next available missile from the pool toward the player.
        /// Silently does nothing if the pool is exhausted.
        /// </summary>
        public static void Launch(Vector2 position)
        {
            foreach (var missile in pool)
            {
                if (!missile.Enabled)
                {
                    missile.Launch(position);
                    return;
                }
            }
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
