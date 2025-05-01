using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class BlueBat : BaseBat
    {


        public BlueBat(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            
        }

        protected override Rectangle GetTextureRectangle()
        {
            return Helpers.GetTileRect(3, 23);
        }
    }
}