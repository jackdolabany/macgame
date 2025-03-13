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
using System.Linq;
using MacGame.Doors;
using System.Diagnostics;

namespace MacGame
{
    /// <summary>
    /// Represents a level in the game with a map, and GameObjects, and any state variables as the game goes on. 
    /// </summary>
    public class Level
    {

        public string Name = "";

        /// <summary>
        /// Each level has a unique number. The into is -1 and the hub world is 0.
        /// </summary>
        public int LevelNumber = 0;
        public string Description = "";

        /// <summary>
        /// Set a property on the map to change this. Some levels might want to default to having the Camera
        /// offset to the left or right a bit. For example a big stationary boss on the left or an auto scrolling
        /// level.
        /// </summary>
        public int CameraXOffset = 0;

        /// <summary>
        /// True if this map represents a room in the main hub world. As opposed to a level looking for a sock.
        /// </summary>
        public bool IsHubWorld
        {
            get
            {
                return LevelNumber == 0;
            }
        }

        private bool _isInitailized = false;

        public Player Player;
        public TileMap Map;
        public Camera Camera;
        public List<Enemy> Enemies;
        public List<Item> Items;
        public List<Npc> Npcs;
        public List<GameObject> GameObjects;
        public List<Platform> Platforms;
        public List<SpringBoard> SpringBoards;
        public List<Door> Doors;
        public List<Waypoint> Waypoints;

        /// <summary>
        /// These objects collide with other objects and block the way. They're checked when we check collisions deep inside GameObject.
        /// </summary>
        public List<ICustomCollisionObject> CustomCollisionObjects;

        /// <summary>
        /// Objects that Mac can pick up.
        /// </summary>
        public List<IPickupObject> PickupObjects;

        public List<MovingBlockGroup> MovingBlockGroups { get; set; } = new List<MovingBlockGroup>();

        public List<CollisionScript> CollisionScripts { get; set; } = new List<CollisionScript>();

        /// <summary>
        /// For enemies that need to add enemies. 
        /// They can add them to this list and they will be added to the world before the next update cycle.
        /// </summary>
        private static readonly Queue<Enemy> EnemiesToAdd = new Queue<Enemy>(20);

        public RevealBlockManager RevealBlockManager;

        /// <summary>
        /// Set this to start a countdown where a bomb goes off.
        /// </summary>
        public float BombTimer;
        private float _bombFadeToWhiteTimer;
        private float _bombFadeToWhiteTimerGoal = 1f;
        private float _bombFadeToBlackTimer;
        private float _bombFadeToBlackTimerGoal = 1f;
        private int _playToneWhenSecondsRemaining = 10;
        public bool AllBombsDisabled = false;
        private bool _isBlownUp = false;

        public Level(Player player, TileMap map, Camera camera)
        {
            Player = player;
            Map = map;
            Camera = camera;
            Enemies = new List<Enemy>();
            Items = new List<Item>();
            Platforms = new List<Platform>();
            SpringBoards = new List<SpringBoard>();
            GameObjects = new List<GameObject>();
            Npcs = new List<Npc>();
            Doors = new List<Door>();
            RevealBlockManager = new RevealBlockManager();
            Waypoints = new List<Waypoint>();
            PickupObjects = new List<IPickupObject>();
            CustomCollisionObjects = new List<ICustomCollisionObject>();
        }

        public static void AddEnemy(Enemy enemy)
        {
            EnemiesToAdd.Enqueue(enemy);
        }

        private void Initialize()
        {
            if (LevelNumber == -1)
            {
                // Custom intro code
                // Set Ottie stationary so he doesn't move out of frame.
                var ottie = (Ottie)this.Npcs.Single(npc => npc is Ottie);
                ottie.BeStationary();
            }
        }

        public void Update(GameTime gameTime, float elapsed)
        {
            if (!_isInitailized)
            {
                Initialize();
                _isInitailized = true;
            }
            RevealBlockManager.Update(elapsed);

            // Important that platforms update before player and enemies.
            foreach (var p in Platforms)
            {
                p.Update(gameTime, elapsed);
            }

            Player.Update(gameTime, elapsed);

            foreach (var sb in SpringBoards)
            {
                sb.Update(gameTime, elapsed);
            }

            Player.SetCameraTarget(Camera, elapsed);

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

            foreach (var gameObject in GameObjects.ToList())
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

                if (BombTimer == 0) // No escape if the bomb is counting down.
                {
                    foreach (var door in Doors)
                    {
                        if (door.Enabled && door.CollisionRectangle.Contains(Player.WorldCenter))
                        {
                            doorToEnter = door;
                            break;
                        }
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

            foreach (var script in this.CollisionScripts)
            {
                if (Player.CollisionRectangle.Intersects(script.CollisionRectangle))
                {
                    switch (script.Name)
                    {
                        case "IntroText":
                            DisplayIntroText();
                            break;
                        case "OttisIntro":
                            OttisIntro();
                            break;
                        default:
                            throw new NotImplementedException($"Unknown collision script: {script.Name}");
                    }
                }
            }

            if (BombTimer > 0)
            {
                var wasOverNextSecondForTone = BombTimer > _playToneWhenSecondsRemaining;

                BombTimer -= elapsed;

                if (wasOverNextSecondForTone && _playToneWhenSecondsRemaining > 0 && BombTimer < _playToneWhenSecondsRemaining)
                {
                    SoundManager.PlaySound("MenuChoice");
                    _playToneWhenSecondsRemaining -= 1;
                }

                if (BombTimer <= 0)
                {
                    _isBlownUp = true;
                    _bombFadeToWhiteTimer = 0;

                    SoundManager.PlaySound("Explosion");

                    // Kill player
                    Player.Kill();

                    foreach(var enemy in Enemies)
                    {
                        enemy.Kill();
                    }
                }

                // Check if the bombs are disabled
                var allBombsDisabled = true;
                foreach (var gameObject in this.GameObjects)
                {
                    if (gameObject is WaterBomb)
                    {
                        var waterBomb = (WaterBomb)gameObject;
                        if (!waterBomb.IsDisabled)
                        {
                            allBombsDisabled = false;
                            break;
                        }
                    }
                }

                if (allBombsDisabled && Player.Health > 0)
                {
                    BombTimer = 0;
                    this.AllBombsDisabled = true;
                    _isBlownUp = false;
                }
            }

            if (_isBlownUp && _bombFadeToWhiteTimer < _bombFadeToWhiteTimerGoal)
            {
                _bombFadeToWhiteTimer += elapsed;
            }

            if (_isBlownUp && _bombFadeToWhiteTimer >= _bombFadeToWhiteTimerGoal && _bombFadeToBlackTimer < _bombFadeToBlackTimerGoal)
            {
                _bombFadeToBlackTimer += elapsed;
            }

        }

        public void EnableBomb()
        {
            BombTimer = (float)new TimeSpan(0, 2, 20).TotalSeconds;

            foreach (var gameObject in this.GameObjects)
            {
                if (gameObject is WaterBomb)
                {
                    var waterBomb = (WaterBomb)gameObject;
                    waterBomb.Activate();
                }
            }

        }

        public void DisplayIntroText()
        {
            if (!Game1.StorageState.HasSeenIntroText)
            {
                Game1.StorageState.HasSeenIntroText = true;
                ConversationManager.AddMessage("Wow that was a good nap!", Helpers.GetReallyBigTileRect(0, 0), ConversationManager.ImagePosition.Left, pauseGameplay: false);
                ConversationManager.AddMessage("Crikey! My human forgot me in the yard. I'll have to find my way home.", Helpers.GetReallyBigTileRect(0, 0), ConversationManager.ImagePosition.Left, pauseGameplay: false);
            }
        }

        public void OttisIntro()
        {
            // Kill all enemies in case any are on screen.
            foreach (var enemy in this.Enemies)
            {
                enemy.Enabled = false;
            }

            Player.BecomeNpc();

            // Find the one waypoint expected in this map.
            var waypoint = Waypoints.Single();

            Player.GoToLocation(waypoint.BottomCenterLocation);
            var ottis = (Ottie)Npcs.Single(npc => npc is Ottie);
            ottis.GoToLocation(new Vector2(waypoint.BottomCenterLocation.X + 2 * Game1.TileSize, waypoint.BottomCenterLocation.Y));
        }

        Dictionary<Vector2, WaterWave> waterWaves;

        public void SetWaterHeight(WaterHeight height)
        {

            if (Game1.LevelState.WaterHeight == height)
            {
                return;
            }

            var oldHeight = Game1.LevelState.WaterHeight;
            Game1.LevelState.WaterHeight = height;

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

            // Shift the moving blocks according to their MovingBlockGroups
            foreach (var group in this.MovingBlockGroups)
            {
                var from = group.GetTileShiftForWaterHeight(oldHeight);
                var to = group.GetTileShiftForWaterHeight(height);

                var totalShifts = Math.Abs(from - to);

                var isMovingUp = from < to;

                for (int i = 0; i < totalShifts; i++)
                {
                    for (int x = group.Rectangle.X; x < group.Rectangle.Right; x++)
                    {
                        for (int y = group.Rectangle.Y; y < group.Rectangle.Bottom; y++)
                        {
                            var thisTileY = y;
                            var swapTileY = y - 1; // Tile below

                            // For the sake of not duplicating code we're going to loop top to bottom. But if we are actually shifting the 
                            // Tiles down, we want to start with the bottom tiles and work upwards. So that's what this is about.
                            if (!isMovingUp)
                            {
                                thisTileY = (group.Rectangle.Top - y + group.Rectangle.Bottom - 1);
                                swapTileY = thisTileY + 1; // Tile above
                            }

                            var mapSquare = Map.GetMapSquareAtCell(x, thisTileY)!;
                            var mapSquareSwapped = Map.GetMapSquareAtCell(x, swapTileY)!;

                            Map.MapCells[x][thisTileY] = mapSquareSwapped;
                            Map.MapCells[x][swapTileY] = mapSquare;

                            mapSquare.SwapEverythingButWater(mapSquareSwapped);
                        }
                    }

                    // Finally adjust the group's rectangle.
                    var newY = group.Rectangle.Y + 1;
                    if (isMovingUp)
                    {
                        newY = group.Rectangle.Y - 1;
                    }

                    group.Rectangle = new Rectangle(group.Rectangle.X, newY, group.Rectangle.Width, group.Rectangle.Height);

                }
            }

            // Now go through and remove water tiles above the height.
            int waterTileHeight = GetTileHightForWaterHeight(height);

            for (int x = 0; x < Map.MapWidth; x++)
            {
                for (int y = 0; y < waterTileHeight; y++)
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
                for (int y = waterTileHeight; y < Map.MapHeight; y++)
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
                                // Re-enable if it existed.
                                wave = waterWaves[new Vector2(x, y)];
                                wave.Enabled = true;
                            }
                            else
                            {
                                // Add a new wave if not.
                                wave = new WaterWave(x, y, 0.1f);
                                waterWaves.Add(new Vector2(x, y), wave);
                                GameObjects.Add(wave);
                            }
                            
                            // For this wave cell, set the draw depth of the wave object
                            // and don't draw the water graphics.
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

        }

        /// <summary>
        /// Last chance to reset the level as we leave it so we don't come back to madness.
        /// </summary>
        public void Reset()
        {
            // back to default water height.
            HighWater();
        }

        private void HighWater()
        {
            SetWaterHeight(WaterHeight.High);
        }

        private void MediumWater()
        {
            SetWaterHeight(WaterHeight.Medium);
        }

        private void LowWater()
        {
            SetWaterHeight(WaterHeight.Low);
        }

        /// <summary>
        /// Handles actions from buttons. You can set an UpAction or DownAction in the map editor by surrounding the button with an object.
        /// You can pass additional data in with the Args property.
        /// </summary>
        public void ButtonAction(Button button, string action, string args)
        {
            switch (action)
            {
                case "HighWater":
                    HighWater();
                    break;
                case "MediumWater":
                    MediumWater();
                    break;
                case "LowWater":
                    LowWater();
                    break;
                case "CloseBlockingPiston":
                    foreach (var gameObject in GameObjects)
                    {
                        if (gameObject is BlockingPiston)
                        {
                            var piston = (BlockingPiston)gameObject;
                            if (piston.Name == args)
                            {
                                piston.Close();
                            }
                        }
                    }
                    break;
                case "OpenBlockingPiston":
                    foreach (var gameObject in GameObjects)
                    {
                        if (gameObject is BlockingPiston)
                        {
                            var piston = (BlockingPiston)gameObject;
                            if (piston.Name == args)
                            {
                                piston.Open();
                            }
                        }
                    }
                    break;
                case "ShootCannon":
                    foreach (var gameObject in GameObjects)
                    {
                        if (gameObject is Cannon)
                        {
                            var cannon = (Cannon)gameObject;
                            if (cannon.Name == args)
                            {
                                cannon.Shoot();
                                break;
                            }
                        }
                    }
                    TimerManager.AddNewTimer(2f, () => button.MoveUpNoAction());
                    break;
                case "BreakBricks":
                    BreakBricks(args);
                    break;
                default:
                    if (Game1.IS_DEBUG)
                    {
                        throw new Exception($"Unknown button action: {action}");
                    }
                    break;
            }
        }

        public void BreakBricks(string brickGroupName)
        {
            foreach (var gameObject in GameObjects)
            {
                if (gameObject is BreakBrick)
                {
                    var bb = (BreakBrick)gameObject;
                    if (bb.GroupName == brickGroupName)
                    {
                        if (bb.IsBroken)
                        {
                            // They've already been broken. Don't do anything.
                            return;
                        }
                        bb.Break();
                    }
                }
            }
            SoundManager.PlaySound("Explosion");
            StorageManager.TrySaveGame();
        }

        public int GetTileHightForWaterHeight(WaterHeight height)
        {
            switch (height)
            {
                case WaterHeight.High:
                    return 130;
                case WaterHeight.Medium:
                    return 141;
                case WaterHeight.Low:
                    return 155;
                default:
                    throw new NotImplementedException($"Invalid height value: {height}");
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

            foreach (var sb in SpringBoards)
            {
                sb.Draw(spriteBatch);
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

            if (_isBlownUp)
            {
                // Figure out how much to fade to white based on the bomb fade to white timer
                Color fadeToWhite = Color.White * (1 - ((_bombFadeToWhiteTimerGoal - _bombFadeToWhiteTimer) / _bombFadeToWhiteTimerGoal));
                spriteBatch.Draw(Game1.TileTextures, Camera.ViewPort, Game1.WhiteSourceRect, fadeToWhite, 0f, Vector2.Zero, SpriteEffects.None, 0.0002f);

                // Figure out how much to fade to white based on the bomb fade to white timer
                Color fadeToBlack = Color.Black * (1 - ((_bombFadeToBlackTimerGoal - _bombFadeToBlackTimer) / _bombFadeToBlackTimerGoal));
                spriteBatch.Draw(Game1.TileTextures, Camera.ViewPort, Game1.WhiteSourceRect, fadeToBlack, 0f, Vector2.Zero, SpriteEffects.None, 0.0001f);
            }

        }
    }
}
