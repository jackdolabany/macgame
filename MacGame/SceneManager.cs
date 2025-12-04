using MacGame.DisappearBlocks;
using MacGame.Doors;
using MacGame.Enemies;
using MacGame.GameObjects;
using MacGame.Items;
using MacGame.Npcs;
using MacGame.Platforms;
using MacGame.RevealBlocks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using TileEngine;

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
            Game1.Camera.ClearRestrictions();
            Game1.BackgroundEffectsManager.Reset();

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

            if (map.Properties.ContainsKey("CustomRestartLevelName"))
            {
                level.CustomRestartLevelName = map.Properties["CustomRestartLevelName"];
            }

            if (map.Properties.ContainsKey("CustomRestartDoorName"))
            {
                level.CustomRestartDoorName = map.Properties["CustomRestartDoorName"];
            }

            if (map.Properties.ContainsKey("CameraXOffset"))
            {
                level.CameraXOffset = int.Parse(map.Properties["CameraXOffset"]);
            }

            if (level.LevelNumber == 4)
            {
                Game1.Gravity = Game1.MoonGravity;
            }
            else
            {
                Game1.Gravity = Game1.EarthGravity;
            }

            // Just in case!
            SoundManager.StopMinecart();

            var priorLevelNumber = -1;
            if (Game1.CurrentLevel != null)
            {
                priorLevelNumber = Game1.CurrentLevel.LevelNumber;
            }
            var isNewLevel = level.LevelNumber != priorLevelNumber;
            player.ResetStateForLevelTransition(isNewLevel);

            if (map.Properties.ContainsKey("IsSpace"))
            {
                var isSpace = map.Properties["IsSpace"].ToBoolean();
                if (isSpace)
                {
                    level.AutoScrollSpeed = new Vector2(100, 0);
                    player.EnterSpaceship();
                }
            }

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

            // Check layer properties
            for (int i = 0; i < map.Layers.Count; i++)
            {
                var layer = map.Layers[i];
                foreach (var property in layer.Properties)
                {
                    if (property.name == "Starfield" && property.value.ToBoolean())
                    {
                        // Layers in the raw map are reversed
                        var layerIndex = map.Layers.Count - i;
                        var depth = map.GetLayerDrawDepth(layerIndex);
                        Game1.BackgroundEffectsManager.ShowStars(depth + Game1.MIN_DRAW_INCREMENT);
                    }
                }
            }

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

                        var nullableTile = mapSquare.LayerTiles[z];
                        if (nullableTile == null)
                        {
                            continue;
                        }
                        
                        Tile tile = nullableTile!;

                        if (mapSquare.IsWater)
                        {
                            if (tile.WaterType == WaterType.AnimatingTopOfWater
                                || tile.WaterType == WaterType.AltAnimatingTopOfWater)
                            {
                                // The top of water is a special animating flyweight tile thing.
                                tile.ShouldDraw = false;
                                var drawDepth = map.GetLayerDrawDepth(z);
                                var isAlt = tile.WaterType == WaterType.AltAnimatingTopOfWater;
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

                        tile.Texture = contentManager.Load<Texture2D>(tile.TexturePath);

                        var loadClass = tile.LoadClass;
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
                                Type t = Type.GetType(typeof(Enemy).Namespace + "." + classname)!;
                                var enemy = (Enemy)Activator.CreateInstance(t, new object[] { contentManager, x, y, player, camera })!;
                                level.Enemies.Add(enemy);
                                layerDepthObjects[z].Add(enemy);

                                HandleObjectModifiers(x, y, enemy, map, (props) =>
                                {
                                    enemy.SetProps(props);
                                });

                                // Enemies might add extra enemies for projectiles and such.
                                foreach (var e in enemy.ExtraEnemiesToAddAfterConstructor)
                                {
                                    level.Enemies.Add(e);
                                    layerDepthObjects[z].Add(e);
                                }
                            }
                            else if (loadClass.StartsWith("Platform."))
                            {
                                // Use reflection to load the platform.
                                string classname = loadClass.Split('.')[1];
                                Type t = Type.GetType(typeof(Platform).Namespace + "." + classname)!;
                                var platform = (Platform)Activator.CreateInstance(t, new object[] { contentManager, x, y })!;
                                level.Platforms.Add(platform);

                                layerDepthObjects[z].Add(platform);

                                if (platform is StaticPlatform)
                                {
                                    // Use the image from the map tile.
                                    var staticPlatform = (StaticPlatform)platform;
                                    var texture = tile.Texture;
                                    var textureRect = tile.TextureRectangle;
                                    staticPlatform.SetTextureRectangle(texture!, textureRect);
                                }

                                if (platform is MovingPlatform)
                                {
                                    HandleObjectModifiers(x, y, platform, map, (props) => { 
                                        if (props.ContainsKey("Reverse"))
                                        {
                                            if (props["Reverse"].ToBoolean())
                                            {
                                                ((MovingPlatform)platform).Reverse();
                                            }
                                        }
                                        if (props.ContainsKey("MoveBlocks"))
                                        {
                                            ((MovingPlatform)platform).MoveBlocks = int.Parse(props["MoveBlocks"]);
                                        }
                                    
                                    });
                                }
                                if (platform is GhostPlatformBase)
                                {
                                    HandleObjectModifiers(x, y, platform, map, (props) =>
                                    {
                                        if (props.ContainsKey("GroupName"))
                                        {
                                            ((GhostPlatformBase)platform).GroupName = props["GroupName"];
                                        }
                                    });
                                }
                            }
                            else if (loadClass.StartsWith("Item."))
                            {
                                // Use reflection to load the items from the code
                                string classname = loadClass.Split('.')[1];
                                Type t = Type.GetType(typeof(Item).Namespace + "." + classname)!;
                                var item = (Item)Activator.CreateInstance(t, new object[] { contentManager, x, y, player })!;
                                level.Items.Add(item);

                                layerDepthObjects[z].Add(item);

                                // Socks are special. We expect each one to be wrapped in an object on the map that contains the number and hint.
                                if (item is Sock)
                                {
                                    var sock = (Sock)item;

                                    HandleObjectModifiers(x, y, sock, map, (props) => {
                                        // Validate the name in the master set of socks and hints
                                        if (!SockIndex.LevelNumberToSocks[level.LevelNumber].Any(c => c.Name == sock.Name))
                                        {
                                            throw new Exception($"Sock '{sock.Name}' not found in world {level.LevelNumber}.");
                                        }
                                    });

                                    // Validate that a name was set by HandleObjectModifiers
                                    if (string.IsNullOrWhiteSpace(sock.Name))
                                    {
                                        throw new Exception($"The sock has no name! x: {x}, y: {y}");
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
                            else if (loadClass == "Chest")
                            {
                                var chest = new Chest(contentManager, x, y, player);
                                level.GameObjects.Add(chest);
                                HandleObjectModifiers(x, y, chest, map);
                                layerDepthObjects[z].Add(chest);
                            }
                            else if (DoorClasses.Contains(loadClass))
                            {
                                // Use reflection to load the items from the code
                                Type t = Type.GetType(typeof(Door).Namespace + "." + loadClass)!;
                                var door = (Door)Activator.CreateInstance(t, new object[] { contentManager, x, y, player })!;
                                level.Doors.Add(door);
                                layerDepthObjects[z].Add(door);


                                HandleObjectModifiers(x, y, door, map, (props) => {
                                    // Doors need to know what level to go to. I expect an object on the map that contains the door and 
                                    // tells it where to go.
                                    if (props.ContainsKey("GoToMap"))
                                    {
                                        door.GoToMap = props["GoToMap"];
                                    }
                                    if (props.ContainsKey("GoToDoor"))
                                    {
                                        door.GoToDoorName = props["GoToDoor"];
                                    }
                                    if (props.ContainsKey("IsToSubworld"))
                                    {
                                        ((OpenCloseDoor)door).IsToSubworld = props["IsToSubworld"] == "1";
                                    }
                                    if (props.ContainsKey("SocksNeeded"))
                                    {
                                        ((OpenCloseDoor)door).SocksNeeded = int.Parse(props["SocksNeeded"]);
                                    }

                                });

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

                                // Validate that the right props were set.
                                if (string.IsNullOrEmpty(door.GoToMap) && string.IsNullOrEmpty(door.GoToDoorName))
                                {
                                    throw new Exception("Doors must have a custom object on the map that specify the map or door it goes to (or both).");
                                }
                            }
                            else if (loadClass == "RevealBlock")
                            {
                                level.RevealBlockManager.AddRawBlock(new RevealBlock(x, y, z));
                            }
                            else if (loadClass == "DisappearBlock")
                            {
                                var block = new DisappearBlock(contentManager, x, y);
                                level.DisappearBlockManager.AddRawBlock(block);
                                layerDepthObjects[z].Add(block);

                                HandleObjectModifiers(x, y, block, map, (props) =>
                                {
                                    if (props.ContainsKey("GroupName"))
                                    {
                                        block.GroupName = props["GroupName"];
                                    }
                                    else if (props.ContainsKey("Group"))
                                    {
                                        block.GroupName = props["Group"];
                                    }
                                    if (props.ContainsKey("Series"))
                                    {
                                        block.Series = int.Parse(props["Series"]);
                                    }
                                });

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
                            else if (loadClass == "Submarine")
                            {
                                var sub = new Submarine(contentManager, x, y, player);
                                level.GameObjects.Add(sub);
                                layerDepthObjects[z].Add(sub);
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

                                HandleObjectModifiers(x, y, npc, map, (props) =>
                                {
                                    // NPC modifiers
                                    if (props.ContainsKey("Convo"))
                                    {
                                        npc.CreateConversationOverride(props["Convo"]);
                                    }
                                });
                            }
                            else if (loadClass == "Cannon")
                            {
                                var cannon = new Cannon(contentManager, x, y, player, camera);
                                level.GameObjects.Add(cannon);
                                layerDepthObjects[z].Add(cannon);

                                HandleObjectModifiers(x, y, cannon, map, (props) =>
                                {
                                    if (props.ContainsKey("AutoShoot"))
                                    {
                                        var direction = Enum.Parse<EightWayRotationDirection>(props["AutoShoot"]);
                                        cannon.AutoShootDirection = new EightWayRotation(direction);
                                    }
                                    if (props.ContainsKey("SuperShot"))
                                    {
                                        cannon.IsSuperShot = true;
                                    }
                                    if (props.ContainsKey("DefaultDirection"))
                                    {
                                        var direction = Enum.Parse<EightWayRotationDirection>(props["DefaultDirection"]);
                                        cannon.DefaultDirection = new EightWayRotation(direction);
                                    }
                                });
                            }
                            else if (loadClass == "ButtonUp" || loadClass == "ButtonDown" || loadClass == "SpringButton")
                            {

                                var isUp = loadClass == "ButtonUp" || loadClass == "SpringButton";
                                var isSpring = loadClass == "SpringButton";

                                var button = new Button(contentManager, x, y, player, isUp, isSpring);
                                level.GameObjects.Add(button);
                                layerDepthObjects[z].Add(button);

                                HandleObjectModifiers(x, y, button, map, (props) =>
                                {
                                    // Button modifiers
                                    foreach (var prop in props)
                                    {
                                        if (prop.Key == "UpAction")
                                        {
                                            // Actions are scripts to run. They are level methods.
                                            button.UpActions = ParseButtonActions(prop.Key, props);
                                        }
                                        else if (prop.Key == "DownAction")
                                        {
                                            button.DownActions = ParseButtonActions(prop.Key, props);
                                        }
                                    }
                                });
                            }
                            else if (loadClass == "SpringBoard")
                            {
                                var springBoard = new SpringBoard(contentManager, x, y, player);
                                level.SpringBoards.Add(springBoard);
                                level.PickupObjects.Add(springBoard);
                                HandleObjectModifiers(x, y, springBoard, map);
                                layerDepthObjects[z].Add(springBoard);
                            }
                            else if (loadClass == "Box")
                            {
                                var box = new Box(contentManager, x, y, player);
                                level.GameObjects.Add(box);
                                level.CustomCollisionObjects.Add(box);
                                level.PickupObjects.Add(box);
                                HandleObjectModifiers(x, y, box, map);
                                layerDepthObjects[z].Add(box);
                            }
                            else if (loadClass == "Rock")
                            {
                                var rock = new Rock(contentManager, x, y, player);
                                level.GameObjects.Add(rock);
                                level.PickupObjects.Add(rock);
                                HandleObjectModifiers(x, y, rock, map);
                                layerDepthObjects[z].Add(rock);
                            }
                            else if (loadClass == "Cannonball")
                            {
                                var cb = new Cannonball(contentManager, x, y, player);
                                level.GameObjects.Add(cb);
                                level.PickupObjects.Add(cb);
                                HandleObjectModifiers(x, y, cb, map);
                                layerDepthObjects[z].Add(cb);
                            }
                            else if (loadClass == "TNT")
                            {
                                var tnt = new TNT(contentManager, x, y, player);
                                level.GameObjects.Add(tnt);
                                level.PickupObjects.Add(tnt);
                                HandleObjectModifiers(x, y, tnt, map);
                                layerDepthObjects[z].Add(tnt);
                            }
                            else if (loadClass == "BlockingPistonVertical")
                            {
                                var blockingPiston = new BlockingPistonVertical(contentManager, x, y, player);
                                level.GameObjects.Add(blockingPiston);
                                layerDepthObjects[z].Add(blockingPiston);
                                HandleObjectModifiers(x, y, blockingPiston, map);
                            }
                            else if (loadClass == "BlockingPistonHorizontal")
                            {
                                var blockingPiston = new BlockingPistonHorizontal(contentManager, x, y, player);
                                level.GameObjects.Add(blockingPiston);
                                layerDepthObjects[z].Add(blockingPiston);
                                HandleObjectModifiers(x, y, blockingPiston, map);
                            }
                            else if (loadClass.EndsWith("Keyblock"))
                            {
                                // RedKeyblock, GreenKeyblock, or BlueKeyblock.
                                Type t = Type.GetType(typeof(Keyblock).Namespace + "." + loadClass);

                                var isLocked = !Game1.StorageState.IsMapSquareUnblocked(level.LevelNumber, level.Name, x, y);

                                var keyblock = (Keyblock)Activator.CreateInstance(t, new object[] { contentManager, x, y, player, isLocked });
                                level.GameObjects.Add(keyblock);
                                layerDepthObjects[z].Add(keyblock);
                            }
                            else if (loadClass.EndsWith("BreakBrick"))
                            {
                                var isBroken = Game1.StorageState.IsMapSquareUnblocked(level.LevelNumber, level.Name, x, y);
                                var bb = new BreakBrick(contentManager, x, y, player, isBroken);
                                level.GameObjects.Add(bb);
                                layerDepthObjects[z].Add(bb);
                                HandleObjectModifiers(x, y, bb, map, (props) => {
                                    if (props.ContainsKey("GroupName"))
                                    {
                                        bb.GroupName = props["GroupName"];
                                    }
                                    if (props.ContainsKey("OverrideSave"))
                                    {
                                        bb.OverrideSave = props["OverrideSave"].ToBoolean();
                                    }
                                });
                            }
                            else if (loadClass == "DestroyPickupObjectField")
                            {
                                var field = new DestroyPickupObjectField(contentManager, x, y, player);
                                level.GameObjects.Add(field);
                                layerDepthObjects[z].Add(field);
                            }
                            else if (loadClass == "WaterBomb")
                            {
                                var bomb = new WaterBomb(contentManager, x, y, player);
                                level.GameObjects.Add(bomb);
                                layerDepthObjects[z].Add(bomb);
                            }
                            else if (loadClass == "BreakSnow")
                            {
                                var bs = new BreakSnow(contentManager, x, y, player);
                                level.GameObjects.Add(bs);
                                layerDepthObjects[z].Add(bs);
                            }
                            else if (loadClass == "BreakRock")
                            {
                                var br = new BreakRock(contentManager, x, y, player);
                                level.GameObjects.Add(br);
                                layerDepthObjects[z].Add(br);
                            }
                            else if (loadClass == "CrystalSwitch")
                            {
                                var cs = new CrystalSwitch(contentManager, x, y, player);
                                level.GameObjects.Add(cs);
                                layerDepthObjects[z].Add(cs);
                            }
                            else if (loadClass == "OrangeCrystalBlock" || loadClass == "BlueCrystalBlock")
                            {
                                CrystalBlock cb;
                                if (loadClass == "OrangeCrystalBlock")
                                {
                                    cb = new OrangeCrystalBlock(contentManager, x, y);
                                }
                                else
                                {
                                    cb = new BlueCrystalBlock(contentManager, x, y);
                                }
                                level.GameObjects.Add(cb);
                                layerDepthObjects[z].Add(cb);
                            }
                            else if (loadClass == "BlockPlayerField")
                            {
                                var field = new BlockPlayerField(contentManager, x, y, player);
                                level.GameObjects.Add(field);
                                layerDepthObjects[z].Add(field);
                            }
                            else if (loadClass == "GhostBlock")
                            {
                                var ghostBlock = new GhostBlock(contentManager, x, y);
                                level.GameObjects.Add(ghostBlock);
                                layerDepthObjects[z].Add(ghostBlock);
                                HandleObjectModifiers(x, y, ghostBlock, map);

                                if (Game1.IS_DEBUG)
                                {
                                    if (string.IsNullOrEmpty(ghostBlock.Name))
                                    {
                                        throw new Exception("GhostBlock must have a name set in the map editor.");
                                    }
                                }
                            }
                            else if (loadClass == "GhostPlatformController")
                            {
                                var controller = new GhostPlatformController(contentManager, x, y, player);
                                HandleObjectModifiers(x, y, controller, map, (props) => {
                                    if (props.ContainsKey("PlatformName"))
                                    {
                                        controller.PlatformName = props["PlatformName"];
                                    }
                                });

                                if (Game1.IS_DEBUG)
                                {
                                    if (string.IsNullOrEmpty(controller.PlatformName))
                                    {
                                        throw new Exception("GhostPlatformController must have a PlatformName set in the map editor.");
                                    }
                                }

                                level.GameObjects.Add(controller);
                                layerDepthObjects[z].Add(controller);
                            }
                            else if (loadClass == "SpaceShip")
                            {
                                var spaceShip = new SpaceShip(contentManager, x, y, player);
                                HandleObjectModifiers(x, y, spaceShip, map, (props) => {
                                    if (props.ContainsKey("PlatformName"))
                                    {
                                        spaceShip.GoToMap = props["GoToMap"];
                                        spaceShip.GoToDoor = props["GoToDoor"];
                                    }
                                });

                                spaceShip.AddStuffToLevel(level, contentManager);

                                level.GameObjects.Add(spaceShip);
                                layerDepthObjects[z].Add(spaceShip);
                            }
                            else if (loadClass == "WineGlass")
                            {
                                var wineGlass = new WineGlass(contentManager, x, y);
                                level.GameObjects.Add(wineGlass);
                                layerDepthObjects[z].Add(wineGlass);
                            }
                        }
                    }
                }
            }

            // Seaweeds have a list of adjacent seaweeds so they can all electrify together.
            // Organize them into groups.
            var processedIntoGroups = new List<ElectricSeaweed>();

            var allSeaweeds = level.Enemies
                .Where(e => e is ElectricSeaweed)
                .Select(e => (ElectricSeaweed)e);

            var locationsToSeaweed = allSeaweeds.ToDictionary(es => new Vector2(es.X, es.Y), es => es);

            foreach (var seaweed in allSeaweeds)
            {
                if (seaweed.AdjacentSeaweeds == null)
                {
                    // Recursively scan for adjacent seaweed
                    var group = new HashSet<ElectricSeaweed>();
                    FindAdjacentSeaweed(seaweed, group, locationsToSeaweed);

                    // Put them all in the group
                    foreach (var esw in group)
                    {
                        esw.AdjacentSeaweeds = group;
                    }
                }
            }

            // Loop through ObjectModifiers to do random things to the map.
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
                        script.Script = obj.Properties["Script"];
                        level.CollisionScripts.Add(script);
                    }
                }
                else if (obj.Properties.ContainsKey("CameraOffset"))
                {
                    // Special areas of the map where the camera pans elsewhere.
                    var offset = new CameraOffsetZone();
                    offset.CollisionRectangle = obj.GetScaledRectangle();
                    var offsetString = obj.Properties["CameraOffset"];
                    float x = float.Parse(offsetString.Split(',')[0]);
                    float y = float.Parse(offsetString.Split(',')[1]);
                    offset.Offset = new Vector2(x, y);
                    level.CameraOffsetZones.Add(offset);
                }
            }

            // Set the draw depths and initialize all 
            var singleLayerDepth = map.GetLayerIncrement();

            foreach (var layer in layerDepthObjects.Keys)
            {
                // Highest number things end up in the front.
                var gameObjects = layerDepthObjects[layer].OrderBy(o => {
                    if (o is Door) return 1;
                    if (o is BlockingPiston) return 1;
                    if (o is WaterBomb) return 1;
                    if (o is SpaceShip) return 1;
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
            level.DisappearBlockManager.OrganizeRawBlocksIntoGroups();
            player.WorldLocation = new Vector2((level.Map.PlayerStart.X * TileMap.TileSize) + (Game1.TileSize / 2), ((level.Map.PlayerStart.Y + 1) * TileMap.TileSize));
            camera.Map = level.Map;

            // Start the camera on the player. This would reset auto scrolling levels.
            camera.Position = player.WorldLocation;

            level.Map.PlayerDrawDepth = player.DrawDepth;

            return level;
        }

        /// <summary>
        /// Parses the Actions list from button properties. Buttons may have a property UpAction or DownAction.
        /// If there's just one Action for either, there may be an "Args" property. 
        /// 
        /// More complex buttons will lump a series of Actions together like: DownAction: OpenBlockingPiston:Door1;CloseBlockingPiston:Door2;RaiseButton:Button1.
        /// </summary>
        private List<ButtonAction> ParseButtonActions(string ActionKeyName, Dictionary<string, string> properties)
        {
            var actions = new List<ButtonAction>();

            // first figure out if it's one Action or multiple
            if (properties.ContainsKey(ActionKeyName))
            {
                var actionString = properties[ActionKeyName];
                var actionParts = actionString.Split(';');
                foreach (var actionPart in actionParts)
                {
                    if (string.IsNullOrWhiteSpace(actionPart))
                    {
                        continue;
                    }
                    var actionAndArgs = actionPart.Split(':');
                    if (actionAndArgs.Length == 2)
                    {
                        actions.Add(new ButtonAction(actionAndArgs[0], actionAndArgs[1]));
                    }
                    else if (actionAndArgs.Length == 1)
                    {
                        // See if there's an Args property
                        var args = "";
                        if (properties.ContainsKey("Args"))
                        {
                            args = properties["Args"];
                        }
                        actions.Add(new ButtonAction(actionAndArgs[0], args));
                    }
                }
            }
            return actions;
        }

        private void HandleObjectModifiers(int x, int y, GameObject obj, TileMap map, Action<Dictionary<string, string>>? action = null)
        {
            foreach (var om in map.ObjectModifiers)
            {
                if (om.GetScaledRectangle().Contains(new Rectangle(x * Game1.TileSize, y * Game1.TileSize, Game1.TileSize, Game1.TileSize)))
                {
                    // Don't override a name with an OM that has no name.
                    // There can be multiple OM's over an object
                    if (!string.IsNullOrEmpty(om.Name))
                    {
                        obj.Name = om.Name;
                    }

                    if (action != null)
                    {
                        action(om.Properties);
                    }
                }
            }
        }

        private void FindAdjacentSeaweed(ElectricSeaweed currentSeaweed, HashSet<ElectricSeaweed> group, Dictionary<Vector2, ElectricSeaweed> locationsToSeaweeds)
        {
            if (group.Contains(currentSeaweed)) return;

            group.Add(currentSeaweed);

            // search above
            var aboveKey = new Vector2(currentSeaweed.X, currentSeaweed.Y - 1);
            if (locationsToSeaweeds.ContainsKey(aboveKey))
            {
                FindAdjacentSeaweed(locationsToSeaweeds[aboveKey], group, locationsToSeaweeds);
            }

            // search below
            var belowKey = new Vector2(currentSeaweed.X, currentSeaweed.Y + 1);
            if (locationsToSeaweeds.ContainsKey(belowKey))
            {
                FindAdjacentSeaweed(locationsToSeaweeds[belowKey], group, locationsToSeaweeds);
            }

            // Search Left
            var leftKey = new Vector2(currentSeaweed.X - 1, currentSeaweed.Y);
            if (locationsToSeaweeds.ContainsKey(leftKey))
            {
                FindAdjacentSeaweed(locationsToSeaweeds[leftKey], group, locationsToSeaweeds);
            }

            // Search Right
            var rightKey = new Vector2(currentSeaweed.X + 1, currentSeaweed.Y);
            if (locationsToSeaweeds.ContainsKey(rightKey))
            {
                FindAdjacentSeaweed(locationsToSeaweeds[rightKey], group, locationsToSeaweeds);
            }
        }

    }
}
