using MacGame.DisplayComponents;
using MacGame.Npcs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

        MapSquare mapSquareTop;
        MapSquare mapSquareBottom;

        private int _cellX;
        private int _cellY;
        private bool isInitialized = false;

        /// <summary>
        /// You can set this name by surrounding the BlockingPiston with a named object in the tile map.
        /// </summary>
        public string? Name { get; set; }

        public bool requestClose = false;

        public BlockingPiston(ContentManager content, int cellX, int cellY, Player player)
        {

            _cellX = cellX;
            _cellY = cellY;

            WorldLocation = new Vector2(cellX * TileMap.TileSize, (cellY + 1) * TileMap.TileSize);

            this.CollisionRectangle = new Rectangle(-1, -65, 34, 66);

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
            var open = new AnimationStrip(textures, Helpers.GetBigTileRect(0, 9), 4, "open");
            open.LoopAnimation = false;
            open.FrameLength = 0.1f;
            animations.Add(open);

            var close = (AnimationStrip)open.Clone();
            close.Name = "close";
            close.Reverse = true;
            animations.Add(close);
        }

        public void Initialize()
        {
            mapSquareTop = Game1.CurrentMap.GetMapSquareAtCell(_cellX, _cellY - 1)!;
            mapSquareBottom = Game1.CurrentMap.GetMapSquareAtCell(_cellX, _cellY)!;
            Close();
        }

        public void Open()
        {
            // We can always just open.
            animations.Play("open");
            mapSquareTop.Passable = true;
            mapSquareBottom.Passable = true;
            requestClose = false;
        }

        public void Close()
        {
            // closing is tricky, we can't close on the player because
            // it could put him in a weird place in the map to add blocking tiles
            // instead we'll request a close.
            requestClose = true;
           
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!isInitialized)
            {
                Initialize();
                isInitialized = true;
            }

            if (requestClose)
            {
                // before closing make sure the player isn't blocking the door.

                if (!_player.CollisionRectangle.Intersects(this.CollisionRectangle))
                {
                    // Kill all enemies blocking
                    foreach (var enemy in Game1.CurrentLevel.Enemies)
                    {
                        if (enemy.Alive && enemy.Enabled && enemy.CollisionRectangle.Intersects(this.CollisionRectangle))
                        {
                            enemy.TakeHit(1000, Vector2.Zero);
                        }
                    }

                    animations.Play("close");
                    mapSquareTop.Passable = false;
                    mapSquareBottom.Passable = false;
                    requestClose = false;
                }
            }

            base.Update(gameTime, elapsed);
        }
    }
}
