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
    /// A piston like thing that blocks your path. You need to press a button or something
    /// to make it open up.
    /// </summary>
    public abstract class BlockingPiston : GameObject
    {

        protected Player _player;
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        protected MapSquare firstMapSquare;
        protected MapSquare secondMapSquare;

        protected int _cellX;
        protected int _cellY;
        private bool isInitialized = false;

        public bool requestClose = false;

        public BlockingPiston(ContentManager content, int cellX, int cellY, Player player)
        {

            _cellX = cellX;
            _cellY = cellY;

            WorldLocation = new Vector2(cellX * TileMap.TileSize, (cellY + 1) * TileMap.TileSize);

            this.CollisionRectangle = GetCollisionRectangle();

            _player = player;

            Enabled = true;

            IsAffectedByForces = false;
            IsAffectedByGravity = false;
            IsAffectedByPlatforms = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            isEnemyTileColliding = false;
            isTileColliding = false;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\BigTextures");
            var open = new AnimationStrip(textures, GetAnimationRectangle(), 4, "open");
            open.LoopAnimation = false;
            open.FrameLength = 0.1f;
            animations.Add(open);

            var close = (AnimationStrip)open.Clone();
            close.Name = "close";
            close.Reverse = true;
            animations.Add(close);
        }

        /// <summary>
        /// Gets the collision rectangle for this piston type.
        /// </summary>
        protected abstract Rectangle GetCollisionRectangle();

        /// <summary>
        /// Gets the animation rectangle (tile rect) for this piston type.
        /// </summary>
        protected abstract Rectangle GetAnimationRectangle();

        /// <summary>
        /// Gets the coordinates for the first map square this piston blocks.
        /// </summary>
        protected abstract (int cellX, int cellY) GetFirstMapSquareCoords();

        /// <summary>
        /// Gets the coordinates for the second map square this piston blocks.
        /// </summary>
        protected abstract (int cellX, int cellY) GetSecondMapSquareCoords();

        public void Initialize()
        {
            var first = GetFirstMapSquareCoords();
            var second = GetSecondMapSquareCoords();
            firstMapSquare = Game1.CurrentMap.GetMapSquareAtCell(first.cellX, first.cellY)!;
            secondMapSquare = Game1.CurrentMap.GetMapSquareAtCell(second.cellX, second.cellY)!;
            Close();
        }

        public void Open()
        {
            // We can always just open.
            animations.Play("open");
            firstMapSquare.Passable = true;
            secondMapSquare.Passable = true;
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

                var slightlyLargerRect = this.CollisionRectangle;
                slightlyLargerRect.X -= 1;
                slightlyLargerRect.Width += 2;
                slightlyLargerRect.Y -= 1;
                slightlyLargerRect.Height += 2;

                if (!_player.CollisionRectangle.Intersects(slightlyLargerRect))
                {
                    // Kill all enemies blocking
                    foreach (var enemy in Game1.CurrentLevel.Enemies)
                    {
                        if (enemy.Alive && enemy.Enabled && enemy.CollisionRectangle.Intersects(slightlyLargerRect))
                        {
                            enemy.TakeHit(this, 1000, Vector2.Zero);
                        }
                    }

                    animations.Play("close");
                    firstMapSquare.Passable = false;
                    secondMapSquare.Passable = false;
                    requestClose = false;
                }
            }

            base.Update(gameTime, elapsed);
        }
    }
}
