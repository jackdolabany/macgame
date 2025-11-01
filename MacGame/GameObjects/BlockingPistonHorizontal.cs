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
    /// A horizontal blocking piston that blocks tiles to the left and right.
    /// </summary>
    public class BlockingPistonHorizontal : BlockingPiston
    {
        public BlockingPistonHorizontal(ContentManager content, int cellX, int cellY, Player player)
            : base(content, cellX, cellY, player)
        {
        }

        protected override Rectangle GetCollisionRectangle()
        {
            return new Rectangle(-32, -32, 64, 32);
        }

        protected override Rectangle GetAnimationRectangle()
        {
            return Helpers.GetBigTileRect(0, 12);
        }

        protected override (int cellX, int cellY) GetFirstMapSquareCoords()
        {
            return (_cellX - 1, _cellY);
        }

        protected override (int cellX, int cellY) GetSecondMapSquareCoords()
        {
            return (_cellX, _cellY);
        }
    }
}


