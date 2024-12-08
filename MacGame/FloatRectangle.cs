using System;

namespace Microsoft.Xna.Framework
{

    /// <summary>
    /// Like Rectangle but all floats for when you need it.
    /// </summary>
    public struct FloatRectangle : IEquatable<FloatRectangle>
    {

        private static FloatRectangle emptyRectangle = new FloatRectangle();

        public float X;
        public float Y;
        public float Width;
        public float Height;

        public static FloatRectangle Empty
        {
            get { return emptyRectangle; }
        }

        public float Left
        {
            get { return this.X; }
        }

        public float Right
        {
            get { return (this.X + this.Width); }
        }

        public float Top
        {
            get { return this.Y; }
        }

        public float Bottom
        {
            get { return (this.Y + this.Height); }
        }


        public FloatRectangle(float x, float y, float width, float height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }


        public FloatRectangle(Rectangle rectangle)
        {
            this.X = rectangle.X;
            this.Y = rectangle.Y;
            this.Width = rectangle.Width;
            this.Height = rectangle.Height;
        }

        public static bool operator ==(FloatRectangle a, FloatRectangle b)
        {
            return ((a.X == b.X) && (a.Y == b.Y) && (a.Width == b.Width) && (a.Height == b.Height));
        }

        public bool Contains(int x, int y)
        {
            return ((((this.X <= x) && (x < (this.X + this.Width))) && (this.Y <= y)) && (y < (this.Y + this.Height)));
        }

        public bool Contains(Vector2 value)
        {
            return ((((this.X <= value.X) && (value.X < (this.X + this.Width))) && (this.Y <= value.Y)) && (value.Y < (this.Y + this.Height)));
        }

        public bool Contains(Point value)
        {
            return ((((this.X <= value.X) && (value.X < (this.X + this.Width))) && (this.Y <= value.Y)) && (value.Y < (this.Y + this.Height)));
        }

        public bool Contains(FloatRectangle value)
        {
            return ((((this.X <= value.X) && ((value.X + value.Width) <= (this.X + this.Width))) && (this.Y <= value.Y)) && ((value.Y + value.Height) <= (this.Y + this.Height)));
        }

        public static bool operator !=(FloatRectangle a, FloatRectangle b)
        {
            return !(a == b);
        }

        public void Offset(Point offset)
        {
            X += offset.X;
            Y += offset.Y;
        }

        public void Offset(int offsetX, int offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        public Vector2 Center
        {
            get
            {
                return new Vector2((this.X + this.Width) / 2f, (this.Y + this.Height) / 2f);
            }
        }

        public void Inflate(float horizontalValue, float verticalValue)
        {
            X -= horizontalValue;
            Y -= verticalValue;
            Width += horizontalValue * 2;
            Height += verticalValue * 2;
        }

        public bool IsEmpty
        {
            get
            {
                return ((((this.Width == 0) && (this.Height == 0)) && (this.X == 0)) && (this.Y == 0));
            }
        }

        public bool Equals(FloatRectangle other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return (obj is FloatRectangle) ? this == ((FloatRectangle)obj) : false;
        }

        public override string ToString()
        {
            return string.Format("{{X:{0} Y:{1} Width:{2} Height:{3}}}", X, Y, Width, Height);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode();
        }

        public bool Intersects(FloatRectangle r2)
        {
            return !(r2.Left > Right
                     || r2.Right < Left
                     || r2.Top > Bottom
                     || r2.Bottom < Top
                    );
        }

        public void Intersects(ref FloatRectangle value, out bool result)
        {
            result = !(value.Left > Right
                     || value.Right < Left
                     || value.Top > Bottom
                     || value.Bottom < Top
                    );

        }

    }
}