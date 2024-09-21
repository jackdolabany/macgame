using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Threading;
using System.Reflection;
using TileEngine;

namespace TileEngine
{
    /// <summary>
    /// Helps you get tiles after the custom content processor adds a 1px border around everything and possibly scales things.
    /// </summary>
    public static class Helpers
    {

        /// <summary>
        /// Gets a rectangle for tile sprites. The tile sprite processor adds a 1px border around
        /// every tile on the sheet and scales it.
        /// </summary>
        public static Rectangle GetTileRect(int x, int y)
        {
            return new Rectangle(x * TileMap.TileSize + (2 * x) + 1, y * TileMap.TileSize + (2 * y) + 1, TileMap.TileSize, TileMap.TileSize);
        }

        /// <summary>
        /// Gets a big 16 x 16 tile from a big tile set. Expects a 1px border around every tile.
        /// x and y refer to the tile's position in units of 16x16 tiles.
        /// </summary>
        public static Rectangle GetBigTileRect(int x, int y)
        {
            return new Rectangle(x * TileMap.TileSize * 2 + (2 * x) + 1, y * TileMap.TileSize * 2 + (2 * y) + 1, TileMap.TileSize * 2, TileMap.TileSize * 2);
        }

        /// <summary>
        /// Gets a big 24 x 24 tile from a big tile set. Expects a 1px border around every tile.
        /// x and y refer to the tile's position in units of 24x24 tiles.
        /// </summary>
        public static Rectangle GetReallyBigTileRect(int x, int y)
        {
            return new Rectangle(x * TileMap.TileSize * 3 + (2 * x) + 1, y * TileMap.TileSize * 3 + (2 * y) + 1, TileMap.TileSize * 3, TileMap.TileSize * 3);
        }

        /// <summary>
        /// Not sure what's better.
        /// </summary>
        public static int ToInt(this float number)
        {
            return (int)Math.Round(number);
            //return (int)number;
        }
    }
}
