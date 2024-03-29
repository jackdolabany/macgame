﻿using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileEngine;

namespace MacGame.Items
{
    public abstract class Item : GameObject
    {
        /// <summary>
        /// This is always expected to be a static image.
        /// </summary>
        public StaticImageDisplay ItemIcon
        {
            get
            {
                return (StaticImageDisplay)this.DisplayComponent;
            }
        }

        private Camera _camera;
        protected Player _player;

        StaticImageDisplay ClosedChest;
        StaticImageDisplay OpenChestBottom;
        StaticImageDisplay OpenChestTop;

        private bool isOpen = false;
        
        // The item may move up, but the chest never moves.
        Vector2 ChestPosition;

        bool isInitialized = false;
        int _cellX;
        int _cellY;

        /// <summary>
        /// Determines whether or not the item goes back in the box if you walk the box off screen.
        /// </summary>
        protected bool IsReenabled = false;

        public Item(ContentManager content, int cellX, int cellY, Player player, Camera camera) : base()
        {
            WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            ChestPosition = WorldLocation;
            _cellX = cellX;
            _cellY = cellY;

            Enabled = true;
            _player = player;
            _camera = camera;
            

            // Initialize the chest images.
            ClosedChest = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), new Rectangle(14 * Game1.TileSize, 1 * Game1.TileSize, Game1.TileSize, Game1.TileSize));
            OpenChestBottom = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), new Rectangle(15 * Game1.TileSize, 1 * Game1.TileSize, Game1.TileSize, Game1.TileSize));
            OpenChestTop = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), new Rectangle(15 * Game1.TileSize, 0 * Game1.TileSize, Game1.TileSize, Game1.TileSize));

            
        
        }

        private void Collect(Player player)
        {
            WhenCollected(player);
            Enabled = false;
        }

        public abstract void WhenCollected(Player player);

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!isInitialized)
            {
                // Items render in a chest, they're blocking and only open up when the player jumps on them from below.
                var cell = Game1.CurrentMap.GetMapSquareAtCell(_cellX, _cellY);
                if (cell != null)
                {
                    cell.Passable = false;
                }
                isInitialized = true;
            }

            ClosedChest.Update(gameTime, elapsed, ChestPosition, false);
            OpenChestBottom.Update(gameTime, elapsed, ChestPosition, false);
            OpenChestTop.Update(gameTime, elapsed, ChestPosition, false);

            if (!isOpen)
            {
                // Check if the pixel above the player is hitting the bottom of the chest.
                var topOfPlayer = new Rectangle(_player.CollisionRectangle.X, _player.CollisionRectangle.Top, _player.CollisionRectangle.Width, 1);
                var bottomOfChest = new Rectangle((int)ChestPosition.X - 4, (int)ChestPosition.Y, 8, 1);
                if (topOfPlayer.Intersects(bottomOfChest))
                {
                    isOpen = true;
                    //var cell = Game1.CurrentMap.GetMapSquareAtCell(_cellX, _cellY);
                    //if (cell != null)
                    //{
                    //    cell.Passable = true;
                    //}
                    // this.worldLocation.Y -= 3;
                }
            }
            else if (Enabled)
            {
                // Check for player/item collision.
                if (_player.CollisionRectangle.Intersects(this.CollisionRectangle))
                {
                    this.Collect(_player);
                }
            }
            else if (IsReenabled)
            {

                // re-enable if the player doesn't have it and they item is off screen.
                if (_player.CurrentItem != this && !_camera.IsObjectVisible(this.CollisionRectangle))
                {
                    Enabled = true;
                    isOpen = false;
                    WorldLocation = ChestPosition;
                }

            }

            // Move up until it's just above the chest.
            if(isOpen && this.worldLocation.Y > ChestPosition.Y - 7)
            {
                this.velocity.Y = -4;
            }
            else
            {
                this.velocity.Y = 0;
            }

            base.Update(gameTime, elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!isOpen)
            {
                // Item renders as a chest until it's opened.
                ClosedChest.Draw(spriteBatch);
            }
            else
            {
                // Draw the item between the open top and bottom of the chest.
                OpenChestTop.Draw(spriteBatch);
                base.Draw(spriteBatch);
                OpenChestBottom.Draw(spriteBatch);
            }

        }
    }
}
