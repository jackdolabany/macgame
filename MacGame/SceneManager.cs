using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MacGame.Platforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;
using MacGame.RevealBlocks;
using MacGame.Enemies;
using MacGame.Items;
using System.Reflection.Emit;

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
            
            player.Velocity = Vector2.Zero;
            player.IsInMineCart = false;

            SoundManager.PlaySong("Stage1", true, 0.2f);

            var map = contentManager.Load<TileMap>($@"Maps/{mapName}");

            var level = new Level(player, map, camera);

            level.LevelNumber = int.Parse(map.Properties["LevelNumber"]);

            if(map.Properties.ContainsKey("Description"))
            {
                level.Description = map.Properties["Description"];
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

                    for (int z = 0; z < mapSquare.LayerTiles.Length; z++)
                    {
                        // Load the textures so the map can draw.
                        if (mapSquare.LayerTiles[z].TileIndex > 0) // by convention 0 is a null texture on all tile sets
                        {
                            mapSquare.LayerTiles[z].Texture = contentManager.Load<Texture2D>(mapSquare.LayerTiles[z].TexturePath);
                        }
                        var loadClass = mapSquare.LayerTiles[z].LoadClass;
                        if (!string.IsNullOrEmpty(loadClass))
                        {
                            if (loadClass.StartsWith("Enemy."))
                            {
                                // Use reflection to load the enemies from the code
                                string classname = loadClass.Split('.')[1];
                                Type t = Type.GetType(typeof(Enemy).Namespace + "." + classname);
                                var enemy = (Enemy)Activator.CreateInstance(t, new object[] { contentManager, x, y, player, camera });
                                level.Enemies.Add(enemy);

                            }
                            else if (loadClass.StartsWith("Platform."))
                            {
                                // Use reflection to load the platform.
                                string classname = loadClass.Split('.')[1];
                                Type t = Type.GetType(typeof(Platform).Namespace + "." + classname);
                                var platform = (Platform)Activator.CreateInstance(t, new object[] { contentManager, x, y });
                                level.Platforms.Add(platform);
                            }
                            else if (loadClass.StartsWith("Item."))
                            {
                                // Use reflection to load the enemies from the code
                                string classname = loadClass.Split('.')[1];
                                Type t = Type.GetType(typeof(Item).Namespace + "." + classname);
                                var item = (Item)Activator.CreateInstance(t, new object[] { contentManager, x, y, player, camera });
                                level.Items.Add(item);

                                // Coins are special. We expect each one to be wrapped in an object on the map that contains the number and hint.
                                if (item is CricketCoin)
                                {
                                    foreach (var obj in map.ObjectModifiers)
                                    {
                                        if (obj.Rectangle.Contains(item.CollisionRectangle))
                                        {
                                            // Coins are special items.
                                            var coin = (CricketCoin)item;
                                            coin.Number = int.Parse(obj.Properties["Number"]);
                                            coin.Hint = obj.Properties["Hint"];
                                            level.CoinHints.Add(coin.Number, coin.Hint);
                                            coin.InitializeAlreadyCollected(level);
                                        }
                                    }
                                }

                            }
                            else if (loadClass == "Door")
                            {
                                var door = new Door(contentManager, x, y, player, camera);
                                level.Doors.Add(door);

                                // Doors need to know what level to go to. I expect an object on the map that contains the door and 
                                // tells it where to go.
                                foreach (var obj in map.ObjectModifiers)
                                {
                                    if (obj.Rectangle.Contains(door.CollisionRectangle))
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
                                                door.IsToSubworld = obj.Properties["IsToSubworld"] == "1";
                                            }
                                            door.Name = obj.Name;
                                        }
                                    }
                                }

                                if(string.IsNullOrEmpty(door.GoToMap) && string.IsNullOrEmpty(door.GoToDoorName))
                                {
                                    throw new Exception("Doors must have a custom object on the map that specify the map or door it goes to (or both).");
                                }
                            }
                            else if (loadClass == "RevealBlock")
                            {
                                level.RevealBlockManager.AddRawBlock(new RevealBlock(x, y, z));
                                mapSquare.Passable = true;
                            }
                            else if (loadClass == "MineCart")
                            {
                                var mineCart = new MineCart(contentManager, x, y, player);
                                level.GameObjects.Add(mineCart);
                            }
                        }
                    }
                }
            }

            level.RevealBlockManager.OrganizeRawBlocksIntoGroups();
            player.WorldLocation = new Vector2((level.Map.PlayerStart.X * TileMap.TileSize) + 4, ((level.Map.PlayerStart.Y + 1) * TileMap.TileSize));
            camera.Map = level.Map;

            return level;
        }

        /// <summary>
        ///  Sneak a peek at another level without actually calling LoadLevel which has tons of side effects.
        ///  This can be used to see if you are going to a sub world and what the hints are and whatever you need.
        /// </summary>
        public NextLevelInfo GetNextLevelInfo(string mapName, ContentManager contentManager)
        {
            var map = contentManager.Load<TileMap>($@"Maps/{mapName}");

            var hintObjects = map.ObjectModifiers.Where(obj => obj.Properties.ContainsKey("Hint"))
                .Select(x => new { Number = int.Parse(x.Properties["Number"]), Hint = x.Properties["Hint"] })
                .OrderBy(x => x.Number);

            var nextLevelInfo = new NextLevelInfo();
            nextLevelInfo.MapName = mapName;
            nextLevelInfo.LevelNumber = int.Parse(map.Properties["LevelNumber"]);
            if (map.Properties.ContainsKey("Description"))
            {
                nextLevelInfo.Description = map.Properties["Description"];
            }
            
            foreach (var hintObject in hintObjects)
            {
                nextLevelInfo.CoinHints.Add(hintObject.Number, hintObject.Hint);
            }

            return nextLevelInfo;
        }
    }
}
