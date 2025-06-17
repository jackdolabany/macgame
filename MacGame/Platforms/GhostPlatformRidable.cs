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
            // If the player is on this platform, set the velocity to move in the direction the player is facing.
            if (player.PlatformThatThisIsOn == this)
            {

                if(player.IsFacingLeft())
                {
                    // Move left if the player isn't touching a left wall.
                    if (IsTopLeftOfPlayerPassable() && IsBottomLeftOfPlayerPassable())
                    {
                        this.velocity.X = -Speed;
                    }
                    else
                    {
                        this.velocity.X = 0;
                    }
                }
                else
                {
                    if (IsTopRightOfPlayerPassable() && IsBottomRightOfPlayerPassable())
                    {
                        this.velocity.X = Speed;
                    }
                    else
                    {
                        this.velocity.X = 0;
                    }
                }

                if (player.InputManager.CurrentAction.up)
                {
                    if (IsTopLeftOfPlayerPassable() && IsTopRightOfPlayerPassable())
                    {
                        this.velocity.Y = -Speed;
                    }
                    else
                    {
                        this.velocity.Y = 0;
                    }
                }
                else if (player.InputManager.CurrentAction.down)
                {
                    if (IsBottomLeftOfPlayerPassable() && IsBottomRightOfPlayerPassable())
                    {
                        this.velocity.Y = Speed;
                    }
                    else
                    {
                        this.velocity.Y = 0;
                    }
                }
                else
                {
                    this.velocity.Y = 0;
                }
            }
            else
            {
                this.Velocity = Vector2.Zero;
            }

            base.Update(gameTime, elapsed);

        }
    }

}
