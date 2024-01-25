using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TileEngine
{
    /// <summary>
    /// Represents all of the tiles in a grid location in the map. 
    /// </summary>
    public class MapSquare
    {
        public Tile[] LayerTiles { get; set; }
        public bool Passable { get; set; } = true;

        public bool EnemyPassable { get; set; } = true;
        public bool PlatformPassable { get; set; } = true;
        public bool IsSand { get; set; } = false;
        public bool IsIce { get; set; } = false;
        public bool IsLadder { get; set; } = false;
        public bool IsVine { get; set; } = false;

        public MapSquare(int depth, bool passable)
        {
            LayerTiles = new Tile[depth];
            Passable = passable;
        }

        public MapSquare()
        {
            LayerTiles = new Tile[0];
        }
    }
}
