using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacGame
{
    /// <summary>
    /// A star that can be drawn in the background by the BackgroundEffectsManager.
    /// </summary>
    public class Star
    {
        public Vector2 Position { get; set; }
        public float Transparency { get; set; }
        public Vector2 Velocity { get; set; }

        public float DrawDepth { get; set; }
        public void Update(float elapsed, GameTime gameTime)
        {
            Position += Velocity * elapsed;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Game1.TileTextures, new Rectangle(Position.X.ToInt(), Position.Y.ToInt(), 4, 4), Game1.WhiteSourceRect, Color.White * Transparency, 0f, Vector2.Zero, SpriteEffects.None, DrawDepth);
        }
    }
}
