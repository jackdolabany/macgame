using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MacGame
{
    public class TextDisplay : DisplayComponent
    {

        public string Text;

        /// <summary>
        /// Queries how much space this menu entry requires.
        /// </summary>
        public float GetHeight()
        {
            return (float)Game1.Font.LineSpacing * Scale;
        }

        /// <summary>
        /// Queries how wide the entry is, used for centering on the screen.
        /// </summary>
        public float GetWidth()
        {
            return Game1.Font.MeasureString(Text).X * Scale;
        }

        public TextDisplay(string text)
            : base()
        {
            this.Text = text;
            this.RotationAndDrawOrigin = new Vector2(GetWidth() / 2, GetHeight() / 2);
        }

        private Vector2 WorldLocation { get; set; }

        public override void Update(GameTime gameTime, float elapsed, Vector2 position, bool flipped)
        {
            base.Update(gameTime, elapsed, position, flipped);
            WorldLocation = position;
            this.Flipped = flipped;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!string.IsNullOrEmpty(Text))
            {
                spriteBatch.DrawString(Game1.Font,
                    this.Text,
                    this.WorldLocation,
                    this.TintColor,
                    this.Rotation,
                    this.RotationAndDrawOrigin,
                    this.Scale,
                    this.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    this.DrawDepth);
            }
        }

        public override Vector2 GetWorldCenter(ref Vector2 worldLocation)
        {
            return worldLocation;
        }
    }
}
