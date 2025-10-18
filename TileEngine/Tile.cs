using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace TileEngine
{
    /// <summary>
    /// A single tile in a single layer on the map.
    /// </summary>
    public class Tile
    {
        public int TileIndex { get; set; }

        [ContentSerializerIgnore]
        public Texture2D? Texture { get; set; }

        public Color Color = Color.White;

        public string TexturePath { get; set; } = "";

        /// <summary>
        /// A class to load at this spot on the map, via reflection.
        /// </summary>
        public string LoadClass { get; set; } = "";

        public bool ShouldDraw { get; set; } = true;

        public WaterType WaterType { get; set; } = WaterType.NotWater;

        public Rectangle TextureRectangle
        {
            get
            {
                var x = TileIndex % TilesPerRow;
                var y = TileIndex / TilesPerRow;

                // Padding to take the 2px border between tiles into account.
                var rect =  Helpers.GetTileRect(x, y);
                return rect;
            }
        }

        public int TilesPerRow
        {
            get
            {
                return (Texture?.Width ?? 0) / (TileMap.TileSize + 2);
            }
        }
    }
}
