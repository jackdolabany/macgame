using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class BlackBat : BaseBat
    {


        public BlackBat(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            
        }

        protected override Rectangle GetTextureRectangle()
        {
            return Helpers.GetTileRect(0, 8);
        }
    }
}