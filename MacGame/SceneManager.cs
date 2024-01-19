using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MacGame.Platforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var map = contentManager.Load<TileMap>($@"Maps/{mapName}");

            var level = new Level(player, map, camera);

            // Do y direction first and count backwards. This is important because we want to add game objects bottom
            // to top. This helps the drawdepth code so that items above are always in front so you can stack objects that
            // are slightly facing downwards like barrels
            for (int y = map.MapHeight - 1; y >= 0; y--)
            {
                for (int x = 0; x < map.MapWidth; x++)
                {
                    var mapSquare = map.GetMapSquareAtCell(x, y);

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
                                Enemy enemy = (Enemy)Activator.CreateInstance(t, new object[] { contentManager, x, y, player, camera });
                                level.Enemies.Add(enemy);

                            }
                            else if (loadClass.StartsWith("Platform."))
                            {
                                // Use reflection to load the platform.
                                string classname = loadClass.Split('.')[1];
                                Type t = Type.GetType(typeof(Platform).Namespace + "." + classname);
                                Platform platform = (Platform)Activator.CreateInstance(t, new object[] { contentManager, x, y });
                                level.Platforms.Add(platform);
                            }
                        }
                    }
                }
            }

            player.WorldLocation = new Vector2((level.Map.PlayerStart.X * TileMap.TileSize) + 4, ((level.Map.PlayerStart.Y + 1) * TileMap.TileSize));
            camera.Map = level.Map;

            return level;

        }
    }
}
