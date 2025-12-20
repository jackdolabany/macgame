using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TileEngine;

namespace MacGame.Platforms
{
    public class Platform : GameObject
    {
        public Vector2 PreviousLocation { get; set; }

        public Platform(ContentManager content, int cellX, int cellY)
            : base()
        {
            this.WorldLocation = new Vector2((cellX * TileMap.TileSize) + (TileMap.TileSize / 2), (cellY + 1) * TileMap.TileSize);
            this.PreviousLocation = this.worldLocation;
            Enabled = true;
            isEnemyTileColliding = false;
            IsAbleToMoveOutsideOfWorld = false;
            IsAffectedByGravity = false;
            isTileColliding = false;
            IsAffectedByPlatforms = false;
        }

        public Vector2 Delta
        {
            get
            {
                return worldLocation - PreviousLocation;
            }
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
           
            base.Update(gameTime, elapsed);

            var player = Game1.Player;

            // Move the player if he was on this platform
            if (player.PlatformThatThisIsOn == this)
            {
                // Adjust the player to how the platform moved.
                player.WorldLocation += this.WorldLocation - PreviousLocation;

                // Jitter can occur on platforms. What if the player is at 10.0 and the platform is at 10.8
                // If the platform moves 0.2 then it moved a pixel while the player still draws at the same spot.
                // If the player isn't moving, adjust his sub pixel fraction to be the same as the platform.
                // This may cause a slight snap when you stop but looks like smooth movement.
                if (player.Velocity.X == 0)
                {
                    // Align the sub pixel x position offset the the player and platform.
                    var wholeNumberLocation = (int)player.WorldLocation.X;
                    var platformFraction = this.worldLocation.X - (float)Math.Truncate(this.WorldLocation.X);
                    player.WorldLocation = new Vector2(wholeNumberLocation + platformFraction, player.WorldLocation.Y);
                }
            }

            PreviousLocation = this.WorldLocation;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Account for WorldLocation being the bottom center of the platform, and then pad a bit.
            if (Game1.Camera.IsObjectVisible(new Rectangle(this.WorldLocation.X.ToInt() - (Game1.TileSize / 2f).ToInt() - 8, this.WorldLocation.Y.ToInt() - Game1.TileSize - 8, Game1.TileSize + 16, Game1.TileSize + 16)))
            {
                base.Draw(spriteBatch);
            }
        }
    }
}
