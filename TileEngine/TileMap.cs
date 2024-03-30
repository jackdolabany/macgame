using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TileEngine
{
    public class TileMap
    {
        public const float FRONTMOST_DRAW_DEPTH = 0f;
        public const float MAP_DRAW_DEPTH = 0.2f;
        public const float EFFECTS_DRAW_DEPTH = 0.23f;
        public const float OVERLAY_DRAW_DEPTH = 0.3f;
        /// <summary>
        /// The level and all enemies and effects are drawn between this and the overlay
        /// </summary>
        public const float LEVEL_DRAW_DEPTH = 0.8f;
        public const float BACKGROUND_DRAW_DEPTH = 0.9f;
        public const float BACKMOST_DRAW_DEPTH = 1f;

        /// <summary>
        /// A default starting spot for the player. It's expected that the Player is put on each map somewhere.
        /// The player would start elsewhere though if he came in from a door.
        /// </summary>
        public Vector2 PlayerStart { get; set; }

        public float PlayerDrawDepth = 0f;
        public float Zoom = 1f;

        public TileMap()
        {
            this.Backgrounds = new List<String>();
            this.Layers = new List<Layer>();
        }

        public List<ObjectModifier> ObjectModifiers = new List<ObjectModifier>();

        public const int TileSize = 8;

        public List<Layer> Layers;

        public int MapWidth
        {
            get
            {
                return MapCells.Length;
            }
        }

        public int MapHeight
        {
            get
            {
                return MapCells[0].Length;
            }
        }

        public int MapWithInPixels
        {
            get
            {
                return MapWidth * TileMap.TileSize;
            }
        }

        public int MapHeightInPixels
        {
            get
            {
                return MapHeight * TileMap.TileSize;
            }
        }

        public int MapDepth
        {
            get
            {
                return MapCells[0][0].LayerTiles.Length;
            }
        }

        public List<String> Backgrounds { get; set; }

        public float GetLayerDrawDepth(int layer)
        {
            return LEVEL_DRAW_DEPTH - layer * GetLayerIncrement();
        }

        public float GetLayerIncrement()
        {
            return (LEVEL_DRAW_DEPTH - OVERLAY_DRAW_DEPTH) / (float)MapDepth;
        }

        public float GetForegroundLayerDrawDepth()
        {
            return GetLayerDrawDepth(MapDepth - 1);
        }

        private const int defaultTile = 0;

        public MapSquare[][] MapCells = new MapSquare[100][];

        /// <summary>
        /// Gets the draw depth of some things relative to the player
        /// </summary>
        public enum DrawObjectGroups
        {
            PlayerAndEnemies,
            Effects
        }

        public float GetObjectDrawDepth(DrawObjectGroups group)
        {
            return PlayerDrawDepth - ((int)group * 0.001f);
        }

        public Rectangle GetWorldRectangle()
        {
            return new Rectangle(0, 0, MapWidth * TileSize, MapHeight * TileSize);
        }

        public int GetCellByPixelX(int pixelX)
        {
            return pixelX / TileSize;
        }

        public int GetCellByPixelY(int pixelY)
        {
            return pixelY / TileSize;
        }

        public Vector2 GetCellByPixel(Vector2 pixelLocation)
        {
            return new Vector2(
                GetCellByPixelX((int)pixelLocation.X),
                GetCellByPixelY((int)pixelLocation.Y));
        }

        public Vector2 GetCellCenter(int cellX, int cellY)
        {
            return new Vector2(
                (cellX * TileSize) + (TileSize / 2),
                (cellY * TileSize) + (TileSize / 2));
        }

        public Vector2 GetCellCenter(Vector2 cell)
        {
            return GetCellCenter(
                (int)cell.X,
                (int)cell.Y);
        }

        public Rectangle CellWorldRectangle(int cellX, int cellY)
        {
            return new Rectangle(
                cellX * TileSize,
                cellY * TileSize,
                TileSize,
                TileSize);
        }

        public Rectangle CellWorldRectangle(Vector2 cell)
        {
            return CellWorldRectangle(
                (int)cell.X,
                (int)cell.Y);
        }

        public bool CellIsPassable(int cellX, int cellY)
        {
            MapSquare? square = GetMapSquareAtCell(cellX, cellY);

            if (square == null)
                return true;
            else
                return square.Passable;
        }

        public bool CellIsPassable(Vector2 cell)
        {
            return CellIsPassable((int)cell.X, (int)cell.Y);
        }

        public bool CellIsPassableByPixel(Vector2 pixelLocation)
        {
            return CellIsPassable(
                GetCellByPixelX((int)pixelLocation.X),
                GetCellByPixelY((int)pixelLocation.Y));
        }

        public MapSquare? GetMapSquareAtCell(int tileX, int tileY)
        {
            if ((tileX >= 0) && (tileX < MapWidth) &&
                (tileY >= 0) && (tileY < MapHeight))
            {
                return MapCells[tileX][tileY];
            }
            else
            {
                return null;
            }
        }

        public void SetMapSquareAtCell(
            int tileX,
            int tileY,
            MapSquare tile)
        {
            if ((tileX >= 0) && (tileX < MapWidth) &&
                (tileY >= 0) && (tileY < MapHeight))
            {
                MapCells[tileX][tileY] = tile;
            }
        }

        public MapSquare? GetMapSquareAtPixel(int pixelX, int pixelY)
        {
            return GetMapSquareAtCell(
                GetCellByPixelX(pixelX),
                GetCellByPixelY(pixelY));
        }

        public MapSquare? GetMapSquareAtPixel(Vector2 pixelLocation)
        {
            return GetMapSquareAtPixel(
                (int)pixelLocation.X,
                (int)pixelLocation.Y);
        }

        public void Initialize(int height, int width)
        {
            //initialize
            MapCells = new MapSquare[width][];
            for (int i = 0; i <= MapCells.Length - 1; i++)
            {
                MapCells[i] = new MapSquare[height];
                for (int j = 0; j <= height - 1; j++)
                {
                    MapCells[i][j] = new MapSquare(1, true);
                }
            }
        }

        public void DrawAllWithOffset(SpriteBatch spriteBatch, int xOffset, int yOffset)
        {
            for (int x = 0; x < MapCells.Length; x++)
            {
                for (int y = 0; y < MapCells[0].Length; y++)
                {
                    var mapCell = MapCells[x][y];
                    var rect = CellWorldRectangle(x, y);
                    rect.X += xOffset;
                    rect.Y += yOffset;

                    for (int z = 0; z < MapDepth; z++)
                    {
                        var texture = mapCell.LayerTiles[z].Texture;
                        if (texture != null)
                        {

                            spriteBatch.Draw(
                                texture,
                                rect,
                                mapCell.LayerTiles[z].TextureRectangle,
                                Color.White,
                                0.0f,
                                Vector2.Zero,
                                SpriteEffects.None,
                                GetLayerDrawDepth(z));
                        }
                    }
                }
            }
        }

        public void DrawParallaxScrollingLayer(SpriteBatch spriteBatch, Rectangle scaledViewPort)
        {
            Draw(spriteBatch, scaledViewPort, true);
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle scaledViewPort, bool isDrawingParallaxLayer = false)
        {

            int startX = GetCellByPixelX(scaledViewPort.X - 1);
            int endX = GetCellByPixelX(scaledViewPort.Right + 1);
            int startY = GetCellByPixelY(scaledViewPort.Y - 1);
            int endY = GetCellByPixelY(scaledViewPort.Bottom + 1);

            for (int z = 0; z < MapDepth; z++)
            {
                if ((Layers[z].IsParallax && !isDrawingParallaxLayer) || (!Layers[z].IsParallax && isDrawingParallaxLayer))
                {
                    continue;
                }

                var depth = GetLayerDrawDepth(z);

                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        if ((x >= 0) && (y >= 0) && (x < MapWidth) && (y < MapHeight))
                        {
                            if (MapCells[x][y].LayerTiles[z].Texture != null)
                            {
                                spriteBatch.Draw(
                                    MapCells[x][y].LayerTiles[z].Texture,
                                    new Vector2(x * TileSize, y * TileSize),
                                    MapCells[x][y].LayerTiles[z].TextureRectangle,
                                    MapCells[x][y].LayerTiles[z].Color,
                                    0.0f,
                                    Vector2.Zero,
                                    1f,
                                    SpriteEffects.None,
                                    depth);
                            }
                        }
                    }
                }
            }
        }
    }

    public class Layer
    {
        public List<Property> Properties = new List<Property>();
        public bool IsParallax = false;
    }

    public class Property
    {
        public string name = "";
        public string value = "";
    }
}
