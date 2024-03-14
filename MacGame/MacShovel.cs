using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using TileEngine;

namespace MacGame
{

    public enum DigDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    
    // This is the shovel Mac uses. Not the item to get the shovel (Shovel.cs).
    public class MacShovel : GameObject
    {
        private Player _player;

        /// <summary>
        /// Kind of a crappy way to identify the sand textures, we can do something better later.
        /// </summary>
        private IEnumerable<Rectangle> SandTextures;
        Vector2 localLocation;
        
        /// <summary>
        ///  When the shovel starts moving out or back we can use this to see how far it's moved so we know when to 
        ///  reel it in or stop the animation.
        /// </summary>
        Vector2 startOfMovementLocation;

        Vector2 movementDirection;

        // To help us animate the shovel going out and then coming back.
        bool isShovelGoingOut = false;
        const int shovelSpeed = 35;

        public MacShovel(Player player, Texture2D textures)
        {
            _player = player;

            // We'll draw the wings all custom like.
            var image = new StaticImageDisplay(textures, new Rectangle(11 * TileMap.TileSize, 1 * TileMap.TileSize, TileMap.TileSize, TileMap.TileSize));
            DisplayComponent = image;
            Enabled = false;

            this.SetCenteredCollisionRectangle(8, 8);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Enabled)
            {
                if (isShovelGoingOut)
                {
                    this.localLocation += movementDirection * shovelSpeed * elapsed;
                    if ((startOfMovementLocation - localLocation).Length() > 5)
                    {
                        isShovelGoingOut = false;
                        startOfMovementLocation = localLocation;
                    }
                }
                else
                {
                    this.localLocation -= movementDirection * shovelSpeed * elapsed;
                    if((startOfMovementLocation - localLocation).Length() > 5)
                    {
                        this.Enabled = false;
                    }
                }

                this.WorldLocation = _player.WorldLocation + localLocation;

                // Check collisions with enemies
                foreach (var enemy in Game1.CurrentLevel.Enemies)
                {
                    if (enemy.Enabled && enemy.CollisionRectangle.Intersects(this.CollisionRectangle))
                    {
                        enemy.TakeHit(1, Vector2.Zero);
                    }
                }
            }


            base.Update(gameTime, elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Enabled)
            {
                base.Draw(spriteBatch);
            }
        }

        public void TryDig(DigDirection digDirection)
        {

            if (Enabled) return;

            Vector2 tileToCheck;

            switch(digDirection)
            {
                case DigDirection.Up:
                    this.localLocation = new Vector2(0, -2);
                    tileToCheck = new Vector2(0, -10);
                    break;
                case DigDirection.Down:
                    this.localLocation = new Vector2(0, 2);
                    tileToCheck = new Vector2(0, 10);
                    break;
                case DigDirection.Left:
                    this.localLocation = new Vector2(-2, 0);
                    tileToCheck = new Vector2(-10, 0);
                    break;
                case DigDirection.Right:
                    this.localLocation = new Vector2(2, 0);
                    tileToCheck = new Vector2(10, 0);
                    break;
                default:
                    throw new Exception("Invalid dig direction");
            }

            // It will already start a bit in the direction it's moving so juts normalize
            // movement direction.
            movementDirection = this.localLocation;
            movementDirection.Normalize();
            startOfMovementLocation = this.localLocation;
            this.RotateTo(this.movementDirection);
            this.Rotation -= MathHelper.PiOver2;
            this.Enabled = true;
            isShovelGoingOut = true;

            var tileToDig = Game1.CurrentMap.GetMapSquareAtPixel(_player.WorldCenter + tileToCheck);
            tileToDig.DigSand();
        }
    }

}
