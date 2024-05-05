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
using MacGame.Npcs;

namespace MacGame
{
    /// <summary>
    /// Represents a level in the game with a map, and GameObjects, and any state variables as the game goes on. 
    /// </summary>
    public class Level
    {

        public string Name = "";

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
        public List<Npc> Npcs;
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
            Npcs = new List<Npc>();
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

            foreach (var npc in Npcs)
            {
                npc.Update(gameTime, elapsed);
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
            if (Player.InteractButtonPressedThisFrame)
            {

                Door? doorToEnter = null;
                Npc? npcToTalkTo = null;

                foreach (var door in Doors)
                {
                    if (door.Enabled && door.CollisionRectangle.Contains(Player.CollisionCenter))
                    {
                        doorToEnter = door;
                        break;
                    }
                }
                foreach (var npc in Npcs)
                {
                    if (npc.Enabled && npc.CollisionRectangle.Intersects(Player.CollisionRectangle))
                    {
                        npcToTalkTo = npc;
                        break;
                    }
                }
                

                // if there's a door to go through and an NPC to talk to, prefer the one closer to Mac
                if (doorToEnter != null && npcToTalkTo != null)
                {
                    if (Vector2.Distance(Player.WorldLocation, doorToEnter.WorldLocation) < Vector2.Distance(Player.WorldLocation, npcToTalkTo.WorldLocation))
                    {
                        doorToEnter.PlayerTriedToOpen(Player);
                    }
                    else
                    {
                        npcToTalkTo.CheckPlayerInteractions(Player);
                    }
                }
                else if (doorToEnter != null)
                {
                    doorToEnter.PlayerTriedToOpen(Player);
                }
                else if (npcToTalkTo != null)
                {
                    npcToTalkTo.CheckPlayerInteractions(Player);
                }   
            }

            while (EnemiesToAdd.Count > 0)
            {
                Enemies.Add(EnemiesToAdd.Dequeue());
            }
        }

        /// <summary>
        ///  Only updates a subset of elements for when the game is virutally paused like for state
        ///  transitions or menus. You can add things here as needed, but don't update the player or enemies.
        /// </summary>
        public void PausedUpdate(GameTime gameTime, float elapsed)
        {
            foreach (var door in Doors)
            {
                door.Update(gameTime, elapsed);
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

            foreach (var enemy in Enemies)
            {
                enemy.Draw(spriteBatch);
            }

            foreach (var npc in Npcs)
            {
                npc.Draw(spriteBatch);
            }

            foreach (var item in Items)
            {
                item.Draw(spriteBatch);
            }

            foreach (var gameObject in GameObjects)
            {
                gameObject.Draw(spriteBatch);
            }

            Player.Draw(spriteBatch);

        }
    }
}
