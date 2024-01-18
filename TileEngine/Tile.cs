using System;
using System.Collections.Generic;
using System.Text;
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
        public int TileWidth
        {
            get;
            set;
        }
        public int TileHeight { get; set; }

        [ContentSerializerIgnore]
        public Texture2D? Texture { get; set; }

        public Color Color = Color.White;

        public string TexturePath { get; set; } = "";

        /// <summary>
        /// A class to load at this spot on the map, via reflection.
        /// </summary>
        public string LoadClass { get; set; } = "";

        /// <summary>
        /// Some custom properties to apply to the LoadClass
        /// </summary>
        public Dictionary<string, string> Properties = new Dictionary<string, string>();

        public Rectangle TextureRectangle
        {
            get
            {
                var x = TileIndex % TilesPerRow;
                var y = TileIndex / TilesPerRow;

                return new Rectangle(
                    x * TileWidth,
                    y * TileHeight,
                    TileWidth,
                    TileHeight);
            }
        }

        public int TilesPerRow
        {
            get
            {
                return (Texture?.Width ?? 0) / (TileWidth);
            }
        }
    }
}
