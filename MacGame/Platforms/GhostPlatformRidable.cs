using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Platforms
{
    /// <summary>
    /// This haunted platform moves with the player when he's on it. Like a magic carpet ride!
    /// </summary>
    public class GhostPlatformRidable : GhostPlatformBase
    {

        bool wasOnPlatformLastUpdate = false;
        bool isJumpingAbovePlatform = false;

        public GhostPlatformRidable(ContentManager content, int cellX, int cellY)
            : base(content, cellX, cellY, 4, 5)
        {
        }

        // Helpers to check collisions on corners
        protected bool IsPointPassable(Vector2 point)
        {
            var cell = Game1.CurrentMap.GetMapSquareAtPixel((int)point.X, (int)point.Y);
            return cell == null || cell.Passable;
        }

        public bool IsTopLeftOfPlayerPassable()
        {
            return IsPointPassable(new Vector2(Game1.Player.CollisionRectangle.Left - 2, Game1.Player.CollisionRectangle.Top - 2));
        }

        public bool IsTopRightOfPlayerPassable()
        {
            return IsPointPassable(new Vector2(Game1.Player.CollisionRectangle.Right + 2, Game1.Player.CollisionRectangle.Top - 2));
        }

        public bool IsBottomLeftOfPlayerPassable()
        {
            return IsPointPassable(new Vector2(Game1.Player.CollisionRectangle.Left - 2, Game1.Player.CollisionRectangle.Bottom + 2));
        }

        public bool IsBottomRightOfPlayerPassable()
        {
            return IsPointPassable(new Vector2(Game1.Player.CollisionRectangle.Right + 2, Game1.Player.CollisionRectangle.Bottom + 2));
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            var player = Game1.Player;

            this.Velocity = Vector2.Zero;

            // Check if the player jumped off the platform and move up if they did.
            if (wasOnPlatformLastUpdate && player.Velocity.Y < 0)
            {
                isJumpingAbovePlatform = true;
            }
            else if (player.OnGround)
            {
                isJumpingAbovePlatform = false;
            }

            // Make sure they are within a magic square above the platform.
            if (isJumpingAbovePlatform)
            {
                var height = Game1.TileSize * 6;
                var abovePlatform = new Rectangle(
                    this.CollisionRectangle.Left,
                    this.CollisionRectangle.Top - height,
                    this.CollisionRectangle.Width,
                    height);

                isJumpingAbovePlatform = player.CollisionRectangle.Intersects(abovePlatform);

                if (isJumpingAbovePlatform)
                {
                    this.velocity.Y = -Speed * 2;
                }
                else if (this.velocity.Y < 0)
                {
                    this.velocity.Y = 0f;
                }
            }


            // If the player is on this platform, set the velocity to move in the direction the player is facing.
            if (isJumpingAbovePlatform || (player.PlatformThatThisIsOn == this && player.Velocity.Y >= 0))
            {

                if (player.IsFacingLeft())
                {
                    // Move left if the player isn't touching a left wall.
                    if (IsTopLeftOfPlayerPassable() && IsBottomLeftOfPlayerPassable())
                    {
                        this.velocity.X = -Speed;
                    }
                }
                else
                {
                    if (IsTopRightOfPlayerPassable() && IsBottomRightOfPlayerPassable())
                    {
                        this.velocity.X = Speed;
                    }
                }


                //if (player.InputManager.CurrentAction.down)
                //{
                if (!isJumpingAbovePlatform && player.PlatformThatThisIsOn == this && player.Velocity.Y >= 0 && IsBottomLeftOfPlayerPassable() && IsBottomRightOfPlayerPassable())
                {
                    this.velocity.Y = Speed;
                }
            }
                    //else
                    //{
                    //    this.velocity.Y = 0;
                    //}
                //}
                //else
                //{
                //    this.velocity.Y = 0;
                //}
            //}
            //else
            //{
            //    this.Velocity = Vector2.Zero;
            //}



            base.Update(gameTime, elapsed);

            wasOnPlatformLastUpdate = (player.PlatformThatThisIsOn == this && player.OnGround);
        }
    }

}
