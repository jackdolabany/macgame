using MacGame.Platforms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TileEngine;
using MacGame.RevealBlocks;
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
        /// Default show all water. You can adjust as necessary.
        /// </summary>
        private int waterHeight = 0;
        private int desiredWaterHeight = 0;

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
        public List<Waypoint> Waypoints;

        public List<MovingBlockGroup> MovingBlockGroups { get; set; } = new List<MovingBlockGroup>();

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
            Waypoints = new List<Waypoint>();
        }

        public static void AddEnemy(Enemy enemy)
        {
            EnemiesToAdd.Enqueue(enemy);
        }

        public void Update(GameTime gameTime, float elapsed)
        {
            RevealBlockManager.Update(elapsed);

            // Important that platforms update before player and enemies.
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
                    if (door.Enabled && door.CollisionRectangle.Contains(Player.WorldCenter))
                    {
                        doorToEnter = door;
                        break;
                    }
                }
                foreach (var npc in Npcs)
                {
                    if (npc.Enabled && npc.CollisionRectangle.Intersects(Player.NpcRectangle))
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

            if (desiredWaterHeight != this.waterHeight)
            {
                SetWaterHeight(desiredWaterHeight);
            }

        }

        Dictionary<Vector2, WaterWave> waterWaves;

        public void SetWaterHeight(int height)
        {
            this.waterHeight = height;

            // Track waves because they are special objects that animate waves at the
            // top of water. we'll need to futz around with them.
            if (waterWaves == null)
            {
                waterWaves = new Dictionary<Vector2, WaterWave>();
                foreach (var obj in this.GameObjects)
                {
                    if (obj is WaterWave)
                    {
                        var wave = (WaterWave)obj;
                        waterWaves.Add(new Vector2(wave.CellX, wave.CellY), wave);
                    }
                }
            }

            // Now go through and remove water tiles above the height.
            for (int x = 0; x < Map.MapWidth; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = Map.GetMapSquareAtCell(x, y);
                    if (cell != null)
                    {
                        cell.DisableWater();
                        if (waterWaves.ContainsKey(new Vector2(x, y)))
                        {
                            waterWaves[new Vector2(x, y)].Enabled = false;
                        }
                    }
                }
            }

            // Now add back any water that used to be below the height.
            for (int x = 0; x < Map.MapWidth; x++)
            {
                for (int y = height; y < Map.MapHeight; y++)
                {
                    var cell = Map.GetMapSquareAtCell(x, y);
                    if (cell != null)
                    {
                        cell.ResetWater();

                        var cellAbove = Map.GetMapSquareAtCell(x, y - 1);
                        if (cell.IsWater && cellAbove != null && !cellAbove.IsWater && cellAbove.Passable)
                        {
                            WaterWave wave;
                            // Water with no water or blocking tile above should be a wave.
                            if (waterWaves.ContainsKey(new Vector2(x, y)))
                            {
                                wave = waterWaves[new Vector2(x, y)];
                                wave.Enabled = true;
                            }
                            else
                            {
                                wave = new WaterWave(x, y, 0.1f);
                                waterWaves.Add(new Vector2(x, y), wave);
                                GameObjects.Add(wave);
                            }
                            
                            for (int z = 0; z < Map.MapDepth; z++)
                            {
                                if (cell.LayerTiles[z].WaterType != WaterType.NotWater)
                                {
                                    cell.LayerTiles[z].ShouldDraw = false;
                                    var drawDepth = Map.GetLayerDrawDepth(z);
                                    wave.SetDrawDepth(drawDepth);
                                }
                            }
                            
                        }
                        else if (cell.IsWater && cellAbove != null && (cellAbove.IsWater || !cellAbove.Passable))
                        {
                            // this cell is water but make sure it's not a wave.
                            if (waterWaves.ContainsKey(new Vector2(x, y)))
                            {
                                waterWaves[new Vector2(x, y)].Enabled = false;

                                // re-enable the regular flat water tile
                                for (int z = 0; z < Map.MapDepth; z++)
                                {
                                    if (cell.LayerTiles[z].WaterType != WaterType.NotWater)
                                    {
                                        cell.LayerTiles[z].ShouldDraw = true;
                                    }
                                }

                            }
                        }
                    }
                }
            }

            // Now move any floating blocks.

            // first find a group.



        }

        public void HighWater()
        {
            desiredWaterHeight = 130;
        }

        public void MediumWater()
        {
            desiredWaterHeight = 141;
        }

        public void LowWater()
        {
            desiredWaterHeight = 155;
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
