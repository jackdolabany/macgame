using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MacGame.RevealBlocks
{
    public class RevealBlockGroup
    {
        public List<RevealBlock> RevealBlocks;

        /// <summary>
        /// A rectangle that overlaps the entire group that we can check
        /// before we need to do a more complex block by block collision test
        /// </summary>
        public Rectangle CollisionRectangle;
        public float CollisionTime;

        public RevealBlockGroup()
        {
            RevealBlocks = new List<RevealBlock>();
        }

        public void BuildCollisionRectangle()
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = 0;
            int maxY = 0;

            foreach (var block in RevealBlocks)
            {
                minX = Math.Min(minX, block.CellX);
                minY = Math.Min(minY, block.CellY);
                maxX = Math.Max(maxX, block.CellX);
                maxY = Math.Max(maxY, block.CellY);
            }

            int tileWidth = TileEngine.TileMap.TileSize;
            int tileHeight = TileEngine.TileMap.TileSize;

            CollisionRectangle = new Rectangle(minX * tileWidth, minY * tileHeight, (maxX - minX + 1) * tileWidth, (maxY - minY + 1) * tileHeight);
        }

        public bool IsColliding(Rectangle rectangleToTest)
        {
            if (rectangleToTest.Intersects(CollisionRectangle))
            {
                foreach (var block in RevealBlocks)
                {
                    if (block.IsColliding(rectangleToTest))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

    }
}
