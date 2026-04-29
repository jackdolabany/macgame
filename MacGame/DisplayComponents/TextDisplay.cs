using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MacGame.DisplayComponents
{
    public class TextDisplay : DisplayComponent
    {

        public string Text;

        SpriteFont Font => Game1.FontLarge;

        /// <summary>
        /// Queries how much space this menu entry requires.
        /// </summary>
        public float GetHeight()
        {
            return Font.LineSpacing * Scale;
        }

        /// <summary>
        /// Queries how wide the entry is, used for centering on the screen.
        /// </summary>
        public float GetWidth()
        {
            return Font.MeasureString(Text).X * Scale;
        }

        public TextDisplay(string text)
            : base()
        {
            Text = text;
            RotationAndDrawOrigin = new Vector2(GetWidth() / 2, GetHeight() / 2);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 position, bool flipped)
        {
            if (!string.IsNullOrEmpty(Text))
            {
                spriteBatch.DrawString(Font,
                    Text,
                    (Offset + position).ToIntegerVector(),
                    TintColor,
                    Rotation,
                    RotationAndDrawOrigin,
                    Scale,
                    flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    DrawDepth);
            }
        }

        public override Vector2 GetWorldCenter(ref Vector2 worldLocation)
        {
            return worldLocation;
        }
    }
}
