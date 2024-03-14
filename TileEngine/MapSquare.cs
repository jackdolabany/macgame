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
        
        // Keep track of sand that was dug up so we can put it back later.
        private bool wasSand = false;

        /// <summary>
        /// Not the best way to identify these. But works for now.
        /// </summary>
        private Rectangle[] SandTextures = new Rectangle[]
            {
                new Rectangle(4 * 8, 5 * 8, 8, 8),
                new Rectangle(5 * 8, 5 * 8, 8, 8),
                new Rectangle(6 * 8, 5 * 8, 8, 8)
            };

    public MapSquare(int depth, bool passable)
        {
            LayerTiles = new Tile[depth];
            Passable = passable;
        }

        public MapSquare()
        {
            LayerTiles = new Tile[0];
        }

        public void DigSand()
        {
            if (IsSand)
            {
                IsSand = false;
                Passable = true;
                wasSand = true;

                for (int i = 0; i < LayerTiles.Length; i++)
                {
                    if (LayerTiles[i].Texture != null && SandTextures.Contains(LayerTiles[i].TextureRectangle))
                    {
                        // Only clear the sand texture.
                        LayerTiles[i].Color = Color.Transparent;
                    }
                }
            }
        }

        public void ResetSand()
        {
            if(wasSand)
            {
                IsSand = true;
                Passable = false;
                wasSand = false;

                for (int i = 0; i < LayerTiles.Length; i++)
                {
                    if (LayerTiles[i].Texture != null && SandTextures.Contains(LayerTiles[i].TextureRectangle))
                    {
                        // Only reset the sand textures.
                        LayerTiles[i].Color = Color.White;
                    }
                }
            }
        }
    }
}
