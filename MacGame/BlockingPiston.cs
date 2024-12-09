using MacGame.DisplayComponents;
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
    /// A piston like thing that blocks your path. You need to press a button or something
    /// to make it open up.
    /// </summary>
    public class BlockingPiston : GameObject
    {

        private Player _player;
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public BlockingPiston(ContentManager content, int cellX, int cellY, Player player)
        {
            WorldLocation = new Vector2(cellX * TileMap.TileSize, (cellY + 1) * TileMap.TileSize);

            this.CollisionRectangle = new Rectangle(0, -64, 32, 64);

            _player = player;

            Enabled = true;

            // This is a button. It doesn't do anything.
            IsAffectedByForces = false;
            IsAffectedByGravity = false;
            IsAffectedByPlatforms = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            isEnemyTileColliding = false;
            isTileColliding = true;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\BigTextures");
            var close = new AnimationStrip(textures, Helpers.GetBigTileRect(0, 9) , 1, "close");
            close.LoopAnimation = false;
            close.FrameLength = 0.14f;
            animations.Add(close);

            var open = (AnimationStrip)close.Clone();
            open.Name = "open";
            open.Reverse = true;
            animations.Add(open);

            animations.Play("close");
        }




    }
}
