using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TileEngine;

namespace MacGame
{
    // Flappy wings that render behind Mac when he has the Infinite Jump Item.
    public class MacWings : GameObject
    {
        Rectangle wingSourceRect;
        Texture2D wingImage;
        bool areWingsFlappedDown = false;
        private Player _player;

        public MacWings(Player player, Texture2D textures)
        {
            wingSourceRect = Helpers.GetTileRect(11, 0);
            wingImage = textures;
            _player = player;

            // We'll draw the wings all custom like.
            DisplayComponent = new NoDisplay();

        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            // Update the position of the wings to be behind Mac.
            // Flap Mac's wings
            areWingsFlappedDown = _player.OnGround || _player.Velocity.Y < 0;
            base.Update(gameTime, elapsed);

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Vector2 flapAdjust = Vector2.Zero;
            if (areWingsFlappedDown)
            {
                flapAdjust = new Vector2(0, 2);
            }

            // Right wing
            var rightWingEffect = areWingsFlappedDown ? SpriteEffects.FlipVertically : SpriteEffects.None;
            spriteBatch.Draw(wingImage, _player.WorldLocation + new Vector2(0, -32) + flapAdjust, wingSourceRect, Color.White, 0, Vector2.Zero, 1f, rightWingEffect, this.DrawDepth);
            
            // Left wing
            var leftWingEffect = areWingsFlappedDown ? SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally : SpriteEffects.FlipHorizontally;
            spriteBatch.Draw(wingImage, _player.WorldLocation + new Vector2(-32, -32) + flapAdjust, wingSourceRect, Color.White, 0, Vector2.Zero, 1f, leftWingEffect, this.DrawDepth + Game1.MIN_DRAW_INCREMENT);

        }
    }

}
