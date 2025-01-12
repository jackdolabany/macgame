using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MacGame.DisplayComponents
{
    public class StaticImageDisplay : DisplayComponent
    {

        public DrawObject DrawObject;

        public Rectangle Source
        {
            get
            {
                return DrawObject.SourceRectangle;
            }
            set
            {
                DrawObject.SourceRectangle = value;
            }
        }

        public Texture2D Texture
        {
            get
            {
                return DrawObject.Texture;
            }
            set
            {
                DrawObject.Texture = value;
            }
        }

        public SpriteEffects Effect
        {
            get
            {
                return DrawObject.Effect;
            }
            set
            {
                DrawObject.Effect = value;
            }
        }

        public StaticImageDisplay(Texture2D texture, Rectangle textureSourceRectangle)
            : base()
        {
            DrawObject = new DrawObject()
            {
                Texture = texture,
                SourceRectangle = textureSourceRectangle
            };
        }

        public StaticImageDisplay(Texture2D texture)
            : base()
        {
            DrawObject = new DrawObject()
            {
                Texture = texture,
                SourceRectangle = texture.BoundingRectangle()
            };
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 position, bool flipped)
        {

            var center = GetWorldCenter(ref position);
            var drawPosition = center - new Vector2(DrawObject.SourceRectangle.Width / 2, DrawObject.SourceRectangle.Height / 2) * Scale;
            DrawObject.Position = RotateAroundOrigin(drawPosition, GetWorldCenter(ref position), Rotation);
            DrawObject.Position += Offset;
            var effect = DrawObject.Effect;
            if (flipped)
            {
                effect |= SpriteEffects.FlipHorizontally;
            }

            if (DrawObject.Texture != null)
            {
                spriteBatch.Draw(
                    DrawObject.Texture,
                    DrawObject.Position.ToIntegerVector(),
                    DrawObject.SourceRectangle,
                    TintColor,
                    Rotation,
                    RotationAndDrawOrigin,
                    Scale,
                    effect,
                    DrawDepth);
            }
        }

        public override Vector2 GetWorldCenter(ref Vector2 worldLocation)
        {
            return new Vector2(
              worldLocation.X,
              worldLocation.Y - DrawObject.SourceRectangle.Height / 2f);
        }
    }
}
