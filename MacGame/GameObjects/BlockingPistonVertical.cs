using MacGame.DisplayComponents;
using MacGame.Npcs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using TileEngine;

namespace MacGame
{
    /// <summary>
    /// A vertical blocking piston that blocks tiles above and below.
    /// </summary>
    public class BlockingPistonVertical : BlockingPiston
    {
        public BlockingPistonVertical(ContentManager content, int cellX, int cellY, Player player)
            : base(content, cellX, cellY, player)
        {
        }

        protected override Rectangle GetCollisionRectangle()
        {
            return new Rectangle(0, -64, 32, 64);
        }

        protected override Rectangle GetAnimationRectangle()
        {
            return Helpers.GetBigTileRect(0, 9);
        }

        protected override (int cellX, int cellY) GetFirstMapSquareCoords()
        {
            return (_cellX, _cellY - 1);
        }

        protected override (int cellX, int cellY) GetSecondMapSquareCoords()
        {
            return (_cellX, _cellY);
        }
    }
}
