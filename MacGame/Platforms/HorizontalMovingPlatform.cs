﻿using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Platforms
{
    /// <summary>
    /// A moving platform that you can inherit from and subclasses can move in different directions or be static.
    /// </summary>
    public class HorizontalMovingPlatform : MovingPlatform
    {
        public HorizontalMovingPlatform(ContentManager content, int cellX, int cellY)
            : base(content, cellX, cellY)
        {
            MoveDirection = new Vector2(1, 0);
            MoveSpeed /= 2;
            Initialize();
        }
    }
}
