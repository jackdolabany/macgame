using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MacGame.Platforms;
using System;
using System.Collections.Generic;
using System.Linq;
using TileEngine;
using MacGame.RevealBlocks;
using MacGame.Enemies;
using MacGame.Items;
using MacGame.Npcs;
using System.Data;
using MacGame.Doors;

namespace MacGame
{

    /// <summary>
    /// Use to load levels and title screens and whatever else. Update handles interactions between unrelated GameObjects, like collisions.
    /// </summary>
    public class SceneManager
    {

        public Level LoadLevel(string mapName, ContentManager contentManager, Player player, Camera camera)
        {
            TimerManager.Clear();
            Game1.Camera.CanScrollLeft = true;

            // Music is annoying for testing.
            if (!Game1.IS_DEBUG)
            {
                SoundManager.PlaySong("Stage1", true, 0.2f);
            }

            // Set false on every level start. Boss enemies can set this true themselves.
            Game1.DrawBossHealth = false;

            var map = contentManager.Load<TileMap>($@"Maps/{mapName}");

            var level = new Level(player, map, camera);

            level.Name = mapName;

            level.LevelNumber = int.Parse(map.Properties["LevelNumber"]);

            if (level.LevelNumber == 4)
            {
                Game1.Gravity = Game1.MoonGravity;
            }
            else
            {
                Game1.Gravity = Game1.EarthGravity;
            }

            var priorLevelNumber = -1;
            if (Game1.CurrentLevel != null)
            {
                priorLevelNumber = Game1.CurrentLevel.LevelNumber;
            }
            var isNewLevel = level.LevelNumber != priorLevelNumber;
            player.ResetStateForLevelTransition(isNewLevel);

            // Make sure this exists for each level.
            if (!Game1.StorageState.Levels.ContainsKey(level.LevelNumber))
            {
                Game1.StorageState.Levels.Add(level.LevelNumber, new LevelStorageState());
            }

            if(map.Properties.ContainsKey("Description"))
            {
                level.Description = map.Properties["Description"];
            }

            // we can stuff objects for a layer here and then properly calculate their depth after they are all placed.
            var layerDepthObjects = new Dictionary<int, List<GameObject>>();
            for (int i = 0; i < map.MapCells[0][0].LayerTiles.Length; i++)
            {
                layerDepthObjects.Add(i, new List<GameObject>());
            }

            // We'll need him later to fully set up his race.
            Froggy? froggy = null;

            // Do y direction first and count backwards. This is important because we want to add game objects bottom
            // to top. This helps the drawdepth code so that items above are always in front so you can stack objects that
            // are slightly facing downwards like barrels
            for (int y = map.MapHeight - 1; y >= 0; y--)
            {
                for (int x = 0; x < map.MapWidth; x++)
                {
                    var mapSquare = map.GetMapSquareAtCell(x, y);

                    mapSquare.ResetSand();

                    if (mapSquare.IsLadder)
                    {
                        var mapSquareAbove = map.GetMapSquareAtCell(x, y - 1);
                        if (!mapSquareAbove.IsLadder)
                        {
                            // Add a hidden platform at the top of ladders so you can climb to the top and stand on them.
                            var ladderPlatform = new LadderPlatform(contentManager, x, y);
                            level.Platforms.Add(ladderPlatform);
                        }
                    }

                    string[] DoorClasses = new string[] { "Doorway", "OpenCloseDoor", "RedDoor", "GreenDoor", "BlueDoor", "FrogDoor", "TacoDoor" };

                    for (int z = 0; z < mapSquare.LayerTiles.Length; z++)
                    {

                        if (mapSquare.IsWater)
                        {
                            if (mapSquare.LayerTiles[z].WaterType == WaterType.AnimatingTopOfWater 
                                || mapSquare.LayerTiles[z].WaterType == WaterType.AltAnimatingTopOfWater)
                            {
                                // The top of water is a special animating flyweight tile thing.
                                mapSquare.LayerTiles[z].ShouldDraw = false;
                                var drawDepth = map.GetLayerDrawDepth(z);
                                var isAlt = mapSquare.LayerTiles[z].WaterType == WaterType.AltAnimatingTopOfWater;
                                GameObject waterWave;
                                if (isAlt)
                                {
                                    waterWave = new WaterWaveAlt(x, y, drawDepth);
                                }
                                else
                                {
                                    waterWave = new WaterWave(x, y, drawDepth);
                                }
                                level.GameObjects.Add(waterWave);
                            }

                        }

                        // Load the textures so the map can draw.
                        if (mapSquare.LayerTiles[z].TileIndex > 0) // by convention 0 is a null texture on all tile sets
                        {
                            mapSquare.LayerTiles[z].Texture = contentManager.Load<Texture2D>(mapSquare.LayerTiles[z].TexturePath);
                        }
                        var loadClass = mapSquare.LayerTiles[z].LoadClass;
                        if (!string.IsNullOrEmpty(loadClass))
                        {
                            if (loadClass == "PlayerStart")
                            {
                                layerDepthObjects[z].Add(player);
                            }
                            else if (loadClass.StartsWith("Enemy."))
                            {
                                // Use reflection to load the enemies from the code
                                string classname = loadClass.Split('.')[1];
                                Type t = Type.GetType(typeof(Enemy).Namespace + "." + classname);
                                var enemy = (Enemy)Activator.CreateInstance(t, new object[] { contentManager, x, y, player, camera });
                                level.Enemies.Add(enemy);
                                layerDepthObjects[z].Add(enemy);

                                // Enemies might add extra enemies for projectiles and such.
                                foreach(var e in enemy.ExtraEnemiesToAddAfterConstructor)
                                {
                                    level.Enemies.Add(e);
                                    layerDepthObjects[z].Add(e);
                                }
                            }
                            else if (loadClass.StartsWith("Platform."))
                            {
                                // Use reflection to load the platform.
                                string classname = loadClass.Split('.')[1];
                                Type t = Type.GetType(typeof(Platform).Namespace + "." + classname);
                                var platform = (Platform)Activator.CreateInstance(t, new object[] { contentManager, x, y });
                                level.Platforms.Add(platform);

                                layerDepthObjects[z].Add(platform);

                                if (platform is StaticPlatform)
                                {
                                    // Use the image from the map tile.
                                    var staticPlatform = (StaticPlatform)platform;
                                    var texture = mapSquare.LayerTiles[z].Texture;
                                    var textureRect = mapSquare.LayerTiles[z].TextureRectangle;
                                    staticPlatform.SetTextureRectangle(texture!, textureRect);
                                }

                                if (platform is MovingPlatform)
                                {
                                    foreach (var obj in map.ObjectModifiers)
                                    {
                                        if (obj.GetScaledRectangle().Contains(platform.CollisionRectangle))
                                        {
                                            if (obj.Properties.ContainsKey("Reverse"))
                                            {
                                                ((MovingPlatform)platform).Reverse();
                                            }
                                        }
                                    }
                                }
                            }
                            else if (loadClass.StartsWith("Item."))
                            {
                                // Use reflection to load the items from the code
                                string classname = loadClass.Split('.')[1];
                                Type t = Type.GetType(typeof(Item).Namespace + "." + classname);
                                var item = (Item)Activator.CreateInstance(t, new object[] { contentManager, x, y, player, camera });
                                level.Items.Add(item);

                                layerDepthObjects[z].Add(item);

                                // Socks are special. We expect each one to be wrapped in an object on the map that contains the number and hint.
                                if (item is Sock)
                                {
                                    foreach (var obj in map.ObjectModifiers)
                                    {
                                        if (obj.GetScaledRectangle().Contains(item.CollisionRectangle))
                                        {
                                            // Socks are special items.
                                            var sock = (Sock)item;
                                            sock.Name = obj.Name;

                                            // Validate the name in the master set of socks and hints
                                            if (!SockIndex.LevelNumberToSocks[level.LevelNumber].Any(c => c.Name == sock.Name))
                                            {
                                                throw new Exception($"Sock '{sock.Name}' not found in world {level.LevelNumber}.");
                                            }

                                            sock.CheckIfAlreadyCollected(level.LevelNumber);
                                        }
                                    }
                                }
                                else if (item is Taco)
                                {
                                    // Disable the taco if it was already collected
                                    if (Game1.WasTacoCollected(level.Name, x, y))
                                    {
                                        item.Enabled = false;
                                    }
                                }
                                else if (item is RedKey)
                                {
                                    item.Enabled = !Game1.StorageState.Levels[level.LevelNumber].Keys.HasRedKey;
                                }
                                else if (item is GreenKey)
                                {
                                    item.Enabled = !Game1.StorageState.Levels[level.LevelNumber].Keys.HasGreenKey;
                                }
                                else if (item is BlueKey)
                                {
                                    item.Enabled = !Game1.StorageState.Levels[level.LevelNumber].Keys.HasBlueKey;
                                }
                            }
                            else if (DoorClasses.Contains(loadClass))
                            {
                                // Use reflection to load the items from the code
                                Type t = Type.GetType(typeof(Door).Namespace + "." + loadClass);
                                var door = (Door)Activator.CreateInstance(t, new object[] { contentManager, x, y, player, camera });
                                level.Doors.Add(door);
                                layerDepthObjects[z].Add(door);

                                // Doors need to know what level to go to. I expect an object on the map that contains the door and 
                                // tells it where to go.
                                foreach (var obj in map.ObjectModifiers)
                                {
                                    if (obj.GetScaledRectangle().Contains(door.CollisionRectangle))
                                    {
                                        foreach (var prop in obj.Properties)
                                        {
                                            if (obj.Properties.ContainsKey("GoToMap"))
                                            {
                                                door.GoToMap = obj.Properties["GoToMap"];
                                            }
                                            if (obj.Properties.ContainsKey("GoToDoor"))
                                            {
                                                door.GoToDoorName = obj.Properties["GoToDoor"];
                                            }
                                            if (obj.Properties.ContainsKey("IsToSubworld"))
                                            {
                                                ((OpenCloseDoor)door).IsToSubworld = obj.Properties["IsToSubworld"] == "1";
                                            }
                                            if (obj.Properties.ContainsKey("SocksNeeded"))
                                            {
                                                ((OpenCloseDoor)door).SocksNeeded = int.Parse(obj.Properties["SocksNeeded"]);
                                            }

                                            door.Name = obj.Name;

                                            if (door is OpenCloseDoor)
                                            {
                                                var openClosedDoor = (OpenCloseDoor)door;

                                                var unlockedDoors = Game1.StorageState.Levels[level.LevelNumber].UnlockedDoors;

                                                if (!openClosedDoor.IsInitiallyLocked || (unlockedDoors.Contains(door.Name) && openClosedDoor.CanPlayerUnlock(player)))
                                                {
                                                    openClosedDoor.IsLocked = false;
                                                }
                                                else
                                                {
                                                    openClosedDoor.IsLocked = true;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (string.IsNullOrEmpty(door.GoToMap) && string.IsNullOrEmpty(door.GoToDoorName))
                                {
                                    throw new Exception("Doors must have a custom object on the map that specify the map or door it goes to (or both).");
                                }
                            }
                            else if (loadClass == "RevealBlock")
                            {
                                level.RevealBlockManager.AddRawBlock(new RevealBlock(x, y, z));
                            }
                            else if (loadClass == "Waypoint")
                            {
                                level.Waypoints.Add(new Waypoint(x, y));
                            }
                            else if (loadClass == "MineCart")
                            {
                                var mineCart = new MineCart(contentManager, x, y, player);
                                level.GameObjects.Add(mineCart);
                                layerDepthObjects[z].Add(mineCart);
                            }
                            else if (loadClass.StartsWith("Npc."))
                            {
                                // Use reflection to load the items from the code
                                string classname = loadClass.Split('.')[1];
                                Type t = Type.GetType(typeof(Npc).Namespace! + "." + classname)!;
                                var npc = (Npc)Activator.CreateInstance(t, new object[] { contentManager, x, y, player, camera })!;

                                if (npc is Froggy)
                                {
                                    froggy = (Froggy)npc;
                                }

                                level.Npcs.Add(npc);
                                layerDepthObjects[z].Add(npc);

                                // NPC modifiers
                                foreach (var obj in map.ObjectModifiers)
                                {
                                    if (obj.GetScaledRectangle().Contains(npc.CollisionRectangle))
                                    {
                                        foreach (var prop in obj.Properties)
                                        {
                                            if (obj.Properties.ContainsKey("Convo"))
                                            {
                                                npc.CreateConversationOverride(obj.Properties["Convo"]);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (loadClass == "Cannon")
                            {
                                var cannon = new Cannon(contentManager, x, y, player, camera);
                                level.GameObjects.Add(cannon);
                                layerDepthObjects[z].Add(cannon);

                                // Cannon modifiers
                                foreach (var obj in map.ObjectModifiers)
                                {
                                    if (obj.GetScaledRectangle().Contains(cannon.CollisionRectangle))
                                    {
                                        cannon.Name = obj.Name;
                                        foreach (var prop in obj.Properties)
                                        {
                                            if (obj.Properties.ContainsKey("AutoShoot"))
                                            {
                                                var direction = Enum.Parse<RotationDirection>(obj.Properties["AutoShoot"]);
                                                cannon.AutoShootDirection = direction;
                                            }
                                            if (obj.Properties.ContainsKey("SuperShot"))
                                            {
                                                cannon.IsSuperShot = true;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (loadClass == "ButtonUp" || loadClass == "ButtonDown" || loadClass == "SpringButton")
                            {

                                var isUp = loadClass == "ButtonUp" || loadClass == "SpringButton";
                                var isSpring = loadClass == "SpringButton";

                                var button = new Button(contentManager, x, y, player, isUp, isSpring);
                                level.GameObjects.Add(button);
                                layerDepthObjects[z].Add(button);

                                // Button modifiers
                                foreach (var obj in map.ObjectModifiers)
                                {
                                    if (obj.GetScaledRectangle().Contains(button.CollisionRectangle))
                                    {
                                        button.Name = obj.Name;
                                        foreach (var prop in obj.Properties)
                                        {
                                            if (prop.Key == "UpAction")
                                            {
                                                // Actions are scripts to run. They are level methods.
                                                button.UpAction = prop.Value;
                                            }
                                            else if (prop.Key == "DownAction")
                                            {
                                                button.DownAction = prop.Value;
                                            }
                                            else if (prop.Key == "Args")
                                            {
                                                button.Args = prop.Value;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (loadClass == "SpringBoard")
                            {
                                var springBoard = new SpringBoard(contentManager, x, y, player);
                                level.SpringBoards.Add(springBoard);
                                level.PickupObjects.Add(springBoard);
                                layerDepthObjects[z].Add(springBoard);
                            }
                            else if (loadClass == "Box")
                            {
                                var box = new Box(contentManager, x, y, player);
                                level.GameObjects.Add(box);
                                level.CustomCollisionObjects.Add(box);
                                level.PickupObjects.Add(box);
                                layerDepthObjects[z].Add(box);
                            }
                            else if (loadClass == "Rock")
                            {
                                var rock = new Rock(contentManager, x, y, player);
                                level.GameObjects.Add(rock);
                                level.PickupObjects.Add(rock);
                                layerDepthObjects[z].Add(rock);
                            }
                            else if (loadClass == "Cannonball")
                            {
                                var cb = new Cannonball(contentManager, x, y, player);
                                level.GameObjects.Add(cb);
                                level.PickupObjects.Add(cb);
                                layerDepthObjects[z].Add(cb);
                            }
                            else if (loadClass == "BlockingPiston")
                            {
                                var blockingPiston = new BlockingPiston(contentManager, x, y, player);
                                level.GameObjects.Add(blockingPiston);
                                layerDepthObjects[z].Add(blockingPiston);

                                // BlockingPiston modifiers
                                foreach (var obj in map.ObjectModifiers)
                                {
                                    if (obj.GetScaledRectangle().Contains(blockingPiston.CollisionRectangle))
                                    {
                                        blockingPiston.Name = obj.Name;
                                    }
                                }
                            }
                            else if (loadClass.EndsWith("Keyblock"))
                            {
                                // RedKeyblock, GreenKeyblock, or BlueKeyblock.
                                Type t = Type.GetType(typeof(Keyblock).Namespace + "." + loadClass);

                                var isLocked = !Game1.StorageState.IsKeyblockUnlocked(level.LevelNumber, level.Name, x, y);

                                var keyblock = (Keyblock)Activator.CreateInstance(t, new object[] { contentManager, x, y, player, isLocked });
                                level.GameObjects.Add(keyblock);
                                layerDepthObjects[z].Add(keyblock);
                            }
                        }
                    }
                }
            }

            // Scan for groups of moving blocks.
            foreach (var obj in map.ObjectModifiers)
            {
                if (obj.Properties.ContainsKey("MoveGroup"))
                {
                    var group = new MovingBlockGroup();
                    group.Name = obj.Properties["MoveGroup"];
                    group.MediumWaterOffset = int.Parse(obj.Properties["MediumWater"]);
                    group.LowWaterOffset = int.Parse(obj.Properties["LowWater"]);

                    // The object fully encases the tiles to move, not partially. #MATH
                    group.Rectangle = new Rectangle(
                        (int)Math.Ceiling((float)obj.Rectangle.X / 8f),
                        (int)Math.Ceiling((float)obj.Rectangle.Y / 8f),
                        obj.Rectangle.Width / 8,
                        obj.Rectangle.Height / 8);

                    level.MovingBlockGroups.Add(group);
                }
                else if (obj.Name == "RaceVictoryZone")
                {
                    // This is a special rectangle for when you race Froggy.
                    // Just add a rectangle with this name to the map so he knows where to finish.
                    if (froggy == null)
                    {
                        throw new Exception("You have a race victory zone but not Froggy was found.");
                    }
                    froggy.SetVictoryZone(obj.GetScaledRectangle());
                }
                else if (obj.Properties.ContainsKey("LoadClass"))
                {
                    if (obj.Properties["LoadClass"] == "CollisionScript")
                    {
                        var script = new CollisionScript();
                        script.CollisionRectangle = obj.GetScaledRectangle();
                        script.Name = obj.Name;
                        level.CollisionScripts.Add(script);
                    }
                }
            }

            // Set the draw depths and initialize all 
            var singleLayerDepth = map.GetLayerIncrement();

            foreach (var layer in layerDepthObjects.Keys)
            {
                // Highest number things end up in the back.
                var gameObjects = layerDepthObjects[layer].OrderBy(o => {
                    if (o is Door) return 1;
                    if (o is BlockingPiston) return 1;
                    if (o is Npc) return 2;
                    if (o is Platform) return 3;
                    if (o is Player) return 4;
                    return 5; // enemies and items in front of the player.
                }).ToList();

                // +2 Add a fudge factor of a game object on either side
                var singleItemDepthLimit = singleLayerDepth / (gameObjects.Count + 2);
                float layerDepth = map.GetLayerDrawDepth(layer);

                for (int i = 0; i < gameObjects.Count; i++)
                {
                    // The one is for the fudge factor
                    var myDepth = (i + 1) * singleItemDepthLimit;
                    var drawDepth = layerDepth - myDepth;
                    gameObjects[i].SetDrawDepth(drawDepth);
                }
            }

            level.RevealBlockManager.OrganizeRawBlocksIntoGroups();
            player.WorldLocation = new Vector2((level.Map.PlayerStart.X * TileMap.TileSize) + (Game1.TileSize / 2), ((level.Map.PlayerStart.Y + 1) * TileMap.TileSize));
            camera.Map = level.Map;

            level.Map.PlayerDrawDepth = player.DrawDepth;

            return level;
        }

    }
}
