using MacGame.Platforms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileEngine;
using MacGame.RevealBlocks;
using System.Runtime.CompilerServices;
using MacGame.Enemies;
using MacGame.Items;

namespace MacGame
{
    /// <summary>
    /// Represents a level in the game with a map, and GameObjects, and any state variables as the game goes on. 
    /// </summary>
    public class Level
    {
        /// <summary>
        /// Each level has a unique number. The hub world is 0.
        /// </summary>
        public int LevelNumber = 0;
        public string Description = "";

        /// <summary>
        /// If you aren't in a hub world, this is the name of the door you came from.
        /// You'd return here if you quit or die.
        /// </summary>
        public string HubDoorNameYouCameFrom = "";

        /// <summary>
        /// True if this map represents a room in the main hub world. As opposed to a level looking for a specific cricket coin.
        /// </summary>
        public bool IsHubWorld
        {
            get
            {
                return LevelNumber == 0;
            }
        }

        public Player Player;
        public TileMap Map;
        public Camera Camera;
        public List<Enemy> Enemies;
        public List<Item> Items;
        public List<GameObject> GameObjects;
        public List<Platform> Platforms;
        public List<Door> Doors;

        /// <summary>
        /// The coin you get when you get 100 tacos.
        /// </summary>
        public CricketCoin TacoCoin;

        /// <summary>
        /// For enemies that need to add enemies. 
        /// They can add them to this list and they will be added to the world before the next update cycle.
        /// </summary>
        private static readonly Queue<Enemy> EnemiesToAdd = new Queue<Enemy>(20);

        public RevealBlockManager RevealBlockManager;

        public Level(Player player, TileMap map, Camera camera)
        {
            Player = player;
            Map = map;
            Camera = camera;
            Enemies = new List<Enemy>();
            Items = new List<Item>();
            Platforms = new List<Platform>();
            GameObjects = new List<GameObject>();
            Doors = new List<Door>();
            RevealBlockManager = new RevealBlockManager();
            CoinHints = new Dictionary<int, string>();
        }

        public int SelectedHintIndex;

        public Dictionary<int, string> CoinHints;

        public static void AddEnemy(Enemy enemy)
        {
            EnemiesToAdd.Enqueue(enemy);
        }

        public void Update(GameTime gameTime, float elapsed)
        {
            RevealBlockManager.Update(elapsed);

            foreach (var p in Platforms)
            {
                p.Update(gameTime, elapsed);
            }

            Player.Update(gameTime, elapsed);

            Camera.Position = Player.GetCameraPosition(Camera);

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

            foreach (var door in Doors)
            {
                door.Update(gameTime, elapsed);
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
            }

            foreach (var item in Items)
            {
                item.Update(gameTime, elapsed);
            }

            // Handle the player going through a door.
            if (Player.IsTryingToOpenDoor)
            {
                foreach (var door in Doors)
                {
                    if (door.Enabled && door.CollisionRectangle.Contains(Player.CollisionCenter))
                    {
                        if (door.CoinsNeeded > Player.CricketCoinCount)
                        {
                            // TODO: Temp
                            ConversationManager.AddMessage($"You need {door.CoinsNeeded} coins to unlock this door. This is an extra long message for testing stuff. Will it fit in one block? no.");
                            //ConversationManager.AddMessage($"You need {door.CoinsNeeded} coins to unlock this door.", ConversationManager.Float.Bottom);
                        }
                        else if (door.IsToSubworld)
                        {
                            GlobalEvents.FireSubWorldDoorEntered(this, door.Name, door.GoToMap);
                            break;
                        }
                        else
                        {
                            GlobalEvents.FireDoorEntered(this, door.GoToMap, door.GoToDoorName, door.Name);
                            break;
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

            foreach (var door in Doors)
            {
                door.Draw(spriteBatch);
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
