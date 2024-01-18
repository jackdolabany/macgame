using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
    public abstract class DisplayComponent
    {

        public virtual Color TintColor { get; set; }
        public virtual Vector2 RotationAndDrawOrigin { get; set; }
        public virtual float DrawDepth { get; set; }
        public virtual float Scale { get; set; }
        public virtual bool Flipped { get; set; }

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
            DrawDepth = 0f;// TODO: Figure this out based on layer or whatever.
            Scale = 1f;
        }

        public abstract Vector2 GetWorldCenter(ref Vector2 worldLocation);

        public string currentAnimationName;

        public abstract void Draw(SpriteBatch spriteBatch);

        public virtual void Update(GameTime gameTime, float elapsed, Vector2 position, bool flipped)
        {
            this.Flipped = flipped;
        }

        protected static Vector2 RotateAroundOrigin(Vector2 point, Vector2 origin, float rotation)
        {
            return Vector2.Transform(point - origin, Matrix.CreateRotationZ(rotation)) + origin;
        }

    }
}
