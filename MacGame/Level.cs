using MacGame.Platforms;
using MacGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileEngine;

namespace MacGame
{
    /// <summary>
    /// Represents a level in the game with a map, and GameObjects, and any state variables as the game goes on. 
    /// </summary>
    public class Level
    {

        public Player Player;
        public TileMap Map;
        public Camera Camera;
        public List<Enemy> Enemies;
        public List<Item> Items;
        public List<GameObject> GameObjects;
        public List<Platform> Platforms;

        /// <summary>
        /// For enemies that need to add enemies. 
        /// They can add them to this list and they will be added to the world before the next update cycle.
        /// </summary>
        private static readonly Queue<Enemy> EnemiesToAdd = new Queue<Enemy>(20);

        public Level(Player player, TileMap map, Camera camera)
        {
            Player = player;
            Map = map;
            Camera = camera;
            Enemies = new List<Enemy>();
            Items = new List<Item>();
            Platforms = new List<Platform>();
            GameObjects = new List<GameObject>();
        }

        public static void AddEnemy(Enemy enemy)
        {
            EnemiesToAdd.Enqueue(enemy);
        }

        public void Update(GameTime gameTime, float elapsed)
        {

            foreach (var p in Platforms)
            {
                p.Update(gameTime, elapsed);
            }

            Player.Update(gameTime, elapsed);

            Camera.Position = Player.WorldLocation;

            foreach (var enemy in Enemies)
            {
                enemy.Update(gameTime, elapsed);
            }

            foreach (var item in Items)
            {
                item.Update(gameTime, elapsed);
            }

            foreach (var gameObject in GameObjects)
            {
                gameObject.Update(gameTime, elapsed);
            }

            // Check collisions
            if (Player.Enabled)
            {
                foreach (var enemy in Enemies)
                {
                    if (enemy.Alive)
                    {
                        Player.CheckEnemyInteractions(enemy);
                    }
                }

                foreach (var item in Items)
                {
                    if (item.Enabled)
                    {
                        if (Player.CollisionRectangle.Intersects(item.CollisionRectangle))
                        {
                            item.Collect(Player);
                        }
                    }
                }
            }

            while (EnemiesToAdd.Count > 0)
            {
                Enemies.Add(EnemiesToAdd.Dequeue());
            }
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle scaledViewPort)
        {
            Map.Draw(spriteBatch, scaledViewPort);

            foreach (var p in Platforms)
            {
                p.Draw(spriteBatch);
            }

            Player.Draw(spriteBatch);

            foreach (var enemy in Enemies)
            {
                enemy.Draw(spriteBatch);
            }

            foreach (var item in Items)
            {
                item.Draw(spriteBatch);
            }

            foreach (var gameObject in GameObjects)
            {
                gameObject.Draw(spriteBatch);
            }

        }
    }
}
