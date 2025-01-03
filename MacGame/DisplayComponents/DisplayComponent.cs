﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.DisplayComponents
{
    public abstract class DisplayComponent
    {

        public virtual Color TintColor { get; set; }
        public virtual Vector2 RotationAndDrawOrigin { get; set; }
        public virtual float DrawDepth { get; set; }
        public virtual float Scale { get; set; }

        /// <summary>
        /// Use this to draw the component at a different location than its world location.
        /// </summary>
        public Vector2 Offset { get; set; }

        protected float _rotation;
        public virtual float Rotation
        {
            get { return _rotation; }
            set { _rotation = value % MathHelper.TwoPi; }
        }

        public DisplayComponent()
        {
            Initialize();
        }

        public virtual void Initialize()
        {
            TintColor = Color.White;
            RotationAndDrawOrigin = Vector2.Zero;
            DrawDepth = 0f;
            Scale = 1f;
        }

        public abstract Vector2 GetWorldCenter(ref Vector2 worldLocation);

        public abstract void Draw(SpriteBatch spriteBatch, Vector2 position, bool flipped);

        public virtual void Update(GameTime gameTime, float elapsed)
        {
        }

        protected static Vector2 RotateAroundOrigin(Vector2 point, Vector2 origin, float rotation)
        {
            return Vector2.Transform(point - origin, Matrix.CreateRotationZ(rotation)) + origin;
        }

    }
}
