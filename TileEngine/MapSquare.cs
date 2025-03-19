using Microsoft.Xna.Framework;

namespace TileEngine
{
    /// <summary>
    /// Represents all of the tiles in a grid location in the map. 
    /// </summary>
    public class MapSquare
    {
        public Tile[] LayerTiles { get; set; }
        
        private bool _passable = true;
        public bool Passable
        {
            get
            {
                return _passable;
            }
            set
            {
                if (value == false && RightHeight == 0 && LeftHeight == 0)
                {
                    // it wasn't a slope, they're trying to make it fully blocking.
                    RightHeight = TileMap.TileSize;
                    LeftHeight = TileMap.TileSize;
                }
                _passable = value;
            }
        }

        public bool EnemyPassable { get; set; } = true;
        public bool PlatformPassable { get; set; } = true;
        public bool IsSand { get; set; } = false;
        public bool IsMinecartTrack { get; set; } = false;
        public bool IsIce { get; set; } = false;
        public bool IsWater { get; set; } = false;
        public bool IsLadder { get; set; } = false;
        public bool IsVine { get; set; } = false;

        /// <summary>
        /// Special tiles that get the player out of the Minecart if they are in it.
        /// </summary>
        public bool IsDestroyMinecart { get; set; } = false;

        /// <summary>
        /// The left of the blocking tile in pixels. 8 is fully blocking
        /// and 1 would be the minimum. Left height to right height
        /// creates a slope of collision points.
        /// </summary>
        public int LeftHeight { get; set; } = 0;

        /// <summary>
        /// The right height of the blocking tile in pixels. 8 is fully blocking
        /// and 1 would be the minimum. Left height to right height
        /// creates a slope of collision points.
        /// </summary>
        public int RightHeight { get; set; } = 0;

        // Keep track of sand that was dug up so we can put it back later.
        private bool wasSand = false;

        // Keep track of tiles that were water for when we adjust the water level.
        private bool wasWater = false;

        /// <summary>
        /// Not the best way to identify these. But works for now.
        /// </summary>
        private static Rectangle[] SandTextures = new Rectangle[]
            {
                Helpers.GetTileRect(4, 5),
                Helpers.GetTileRect(5, 5),
                Helpers.GetTileRect(6, 5)
            };

        /// <summary>
        /// Not the best way to identify these. But works for now.
        /// </summary>
        private static Rectangle[] WaterTextures = new Rectangle[]
            {
                Helpers.GetTileRect(7, 6),
                Helpers.GetTileRect(8, 6),
                Helpers.GetTileRect(9, 6),
                Helpers.GetTileRect(10, 6),
                Helpers.GetTileRect(9, 7),
                Helpers.GetTileRect(8, 16),
                Helpers.GetTileRect(9, 16),
                Helpers.GetTileRect(10, 16),
                Helpers.GetTileRect(11, 16),
                Helpers.GetTileRect(10, 17)
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

        public void DisableWater()
        {
            if (IsWater)
            {
                IsWater = false;
                wasWater = true;

                for (int i = 0; i < LayerTiles.Length; i++)
                {
                    if (LayerTiles[i].Texture != null && WaterTextures.Contains(LayerTiles[i].TextureRectangle))
                    {
                        // Only clear the water texture.
                        LayerTiles[i].Color = Color.Transparent;
                    }
                }
            }
        }
        public void ResetWater()
        {
            if (wasWater)
            {
                IsWater = true;
                wasWater = false;

                for (int i = 0; i < LayerTiles.Length; i++)
                {
                    if (LayerTiles[i].Texture != null && WaterTextures.Contains(LayerTiles[i].TextureRectangle))
                    {
                        // Only reset the water textures.
                        LayerTiles[i].Color = Color.White;
                    }
                }
            }
        }

        /// <summary>
        /// Swap all graphics and state except water stuff so that we can make solid blocks move up and 
        /// down as the water changes, or for whatever other reason you want to swap tiles around.
        /// </summary>
        public void SwapEverythingButWater(MapSquare mapSquareSwapped)
        {
            var mapSquareIsWater = this.IsWater;
            var mapSquareWasWater = this.wasWater;
            var mapSquareSwappedIsWater = mapSquareSwapped.IsWater;
            var mapSquareSwappedWasWater = mapSquareSwapped.wasWater;

            // objects float out of the water but the water doesn't change
            mapSquareSwapped.IsWater = mapSquareIsWater;
            mapSquareSwapped.wasWater = mapSquareWasWater;
            this.IsWater = mapSquareSwappedIsWater;
            this.wasWater = mapSquareSwappedWasWater;

            // Swap water layer tiles back so the graphics don't change.
            for (int z = 0; z < this.LayerTiles.Length; z++)
            {
                if (this.LayerTiles[z].WaterType != WaterType.NotWater || mapSquareSwapped.LayerTiles[z].WaterType != WaterType.NotWater)
                {
                    var temp = this.LayerTiles[z];
                    this.LayerTiles[z] = mapSquareSwapped.LayerTiles[z];
                    mapSquareSwapped.LayerTiles[z] = temp;
                }
            }
        }

        public bool IsSlope()
        {
            return !Passable && (LeftHeight < TileMap.TileSize || RightHeight < TileMap.TileSize);
        }

    }
}
