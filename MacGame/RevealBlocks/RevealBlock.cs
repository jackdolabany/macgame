using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MacGame.RevealBlocks
{
    /// <summary>
    /// tile map blocks that slowly dissappear to reveal secret areas
    /// </summary>
    public class RevealBlock
    {
        public int CellX;
        public int CellY;
        public int CellZ;

        public RevealBlock(int cellX, int cellY, int cellZ)
        {
            this.CellX = cellX;
            this.CellY = cellY;
            this.CellZ = cellZ;
        }

        public bool IsColliding(Rectangle rectangleToTest)
        {
            return new Rectangle(CellX * TileEngine.TileMap.TileSize, CellY * TileEngine.TileMap.TileSize, TileEngine.TileMap.TileSize, TileEngine.TileMap.TileSize)
                .Intersects(rectangleToTest);
        }
    }
}
