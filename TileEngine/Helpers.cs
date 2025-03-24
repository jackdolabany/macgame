using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Threading;
using System.Reflection;
using TileEngine;
using System.Runtime.CompilerServices;

namespace TileEngine
{
    /// <summary>
    /// Helps you get tiles after the custom content processor adds a 1px border around everything and possibly scales things.
    /// </summary>
    public static class Helpers
    {

        public static bool ToBoolean(this string? str)
        {
            return !str.IsFalse();
        }

        private static bool IsFalse(this string? str)
        {
            var isFalse = string.IsNullOrWhiteSpace(str) || str == "0" || str.Equals("false", StringComparison.InvariantCultureIgnoreCase);
            return isFalse;
        }

        /// <summary>
        /// Gets a rectangle for tile sprites. The tile sprite processor adds a 1px border around
        /// every tile on the sheet and scales it.
        /// </summary>
        public static Rectangle GetTileRect(int x, int y)
        {
            return GetPaddedTileRect(x, y, TileMap.TileSize);
        }

        /// <summary>
        /// Gets a big 16 x 16 tile from a big tile set. Expects a 1px border around every tile.
        /// x and y refer to the tile's position in units of 16x16 tiles.
        /// </summary>
        public static Rectangle GetBigTileRect(int x, int y)
        {
            return GetPaddedTileRect(x, y, TileMap.TileSize * 2);
        }

        /// <summary>
        /// Gets a big 24 x 24 tile from a big tile set. Expects a 1px border around every tile.
        /// x and y refer to the tile's position in units of 24x24 tiles.
        /// </summary>
        public static Rectangle GetReallyBigTileRect(int x, int y)
        {
            return GetPaddedTileRect(x, y, TileMap.TileSize * 3);
        }

        /// <summary>
        /// Gets a 64 x 64 tile rect.
        /// </summary>
        public static Rectangle GetMegaTileRect(int x, int y)
        {
            return GetPaddedTileRect(x, y, TileMap.TileSize * 8);
        }

        /// <summary>
        /// Given a tile size, this finds the tile at the x and y position while expecting a 1 pixel border around each tile.
        /// In processing we add a 1px border around each tile so that scaling it doesn't add artifacts from the tiles next door 
        /// which is what the GPU would like to do. Why? idk, that's how GPUs work.
        /// </summary>
        public static Rectangle GetPaddedTileRect(int x, int y, int tileSize)
        {
            return new Rectangle(x * tileSize + (2 * x) + 1, y * tileSize + (2 * y) + 1, tileSize, tileSize);
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
