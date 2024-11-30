using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Threading;
using System.Reflection;
using TileEngine;
using System.Runtime.InteropServices;

namespace MacGame
{

    public enum RectangleRotation
    {
        Ninety, OneEighty, TwoSeventy, ThreeSixty
    }

    public static class Helpers
    {

        /// <summary>
        /// Gets a rectangle for tile sprites. The tile sprite processor adds a 1px border around
        /// every tile on the sheet.
        /// </summary>
        public static Rectangle GetTileRect(int x, int y)
        {
            return TileEngine.Helpers.GetTileRect(x, y);
        }

        /// <summary>
        /// Gets a big 16 x 16 tile from a big tile set. Expects a 1px border around every tile.
        /// x and y refer to the tile's position in units of 16x16 tiles.
        /// </summary>
        public static Rectangle GetBigTileRect(int x, int y)
        {
            return TileEngine.Helpers.GetBigTileRect(x, y);
        }

        /// <summary>
        /// Gets a big 24 x 24 tile from a big tile set. Expects a 1px border around every tile.
        /// x and y refer to the tile's position in units of 16x16 tiles.
        /// </summary>
        public static Rectangle GetReallyBigTileRect(int x, int y)
        {
            return TileEngine.Helpers.GetReallyBigTileRect(x, y);
        }

        /// <summary>
        /// Gets a 64 x 64 tile from a big tile set. Expects a 1px border around every tile.
        /// </summary>
        public static Rectangle GetMegaTileRect(int x, int y)
        {
            return TileEngine.Helpers.GetMegaTileRect(x, y);
        }

        /// <summary>
        /// Scales a rectangle up or down, keeping it centered.
        /// </summary>
        public static Rectangle Scale(this Rectangle input, float scale)
        {
            int newWidth = (input.Width * scale).ToInt();
            int newHeight = (input.Height * scale).ToInt();
            return new Rectangle(input.X + ((input.Width - newWidth) / 2), input.Y + ((input.Height - newHeight) / 2), newWidth, newHeight);
        }

        public static Rectangle Rotate(this Rectangle input, Vector2 point, RectangleRotation rotation)
        {
            // TODO this may need some tweaking to support points other than 0, 0 (whoops!)
            int x = point.X.ToInt();
            int y = point.Y.ToInt();
            if (rotation == RectangleRotation.ThreeSixty) return input;

            if (rotation == RectangleRotation.OneEighty)
            {
                return new Rectangle(2 * x - input.Right, 2 * y - input.Bottom, input.Width, input.Height);
            }
            if (rotation == RectangleRotation.Ninety)
            {
                return new Rectangle(input.Top, -input.Right, input.Height, input.Width);
            }
            if (rotation == RectangleRotation.TwoSeventy)
            {
                return new Rectangle(y - input.Bottom, input.Left, input.Height, input.Width);
            }

            return input;
        }

        /// <summary>
        /// Gives the vector a "wobble" by moving it in a slightly random direction based on the percentage you pass in.
        /// </summary>
        /// <param name="vector">The vector to wobble.</param>
        /// <param name="amount">The percentage to wobble it. between 0 and 1.</param>
        public static Vector2 Wobble(this Vector2 vector, float amount)
        {
            if (amount > 1f) throw new Exception("You can't wobble a vector more than 100%.");
            if (amount < 0f) throw new Exception("You can't wobble a vector less than nothing.");
            var rotationAmount = MathHelper.TwoPi * amount * Game1.Randy.NextFloat();
            if (Game1.Randy.NextBool())
            {
                rotationAmount = -rotationAmount;
            }
            return vector.RotateAroundOrigin(Vector2.Zero, rotationAmount);
        }

        public static Vector2 ReverseIfTrue(this Vector2 vector, bool reverse)
        {
            return reverse ? -vector : vector;
        }

        public static Vector2 RotateAroundOrigin(this Vector2 point, Vector2 origin, float rotation)
        {
            return Vector2.Transform(point - origin, Matrix.CreateRotationZ(rotation)) + origin;
        }

        public static Vector2 GetVectorFromDegrees(int degrees)
        {
            return GetVectorFromDegrees((float)degrees);
        }

        public static Vector2 GetVectorFromDegrees(float degrees)
        {
            return GetVectorFromRadians(MathHelper.ToRadians(degrees));
        }

        public static Vector2 GetVectorFromRadians(float radians)
        {
            var vect = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
            return vect;
        }

        public static float ToDegrees(this Vector2 direction)
        {
            return MathHelper.ToDegrees(direction.ToRadians());
        }

        public static float ToRadians(this Vector2 direction)
        {
            return (float)Math.Atan2(direction.Y, direction.X);
        }

        public static bool NextBool(this Random randy)
        {
            return randy.Next(0, 2) == 0;
        }

        public static float NextFloat(this Random randy)
        {
            return (float)randy.NextDouble();
        }

        /// <summary>
        /// Gets a random float rotation between 0 and 2pi
        /// </summary>
        public static float GetRandomRotation(this Random randy)
        {
            return randy.NextVector().ToRadians();
        }

        public static float GetRandomFourWayRotation(this Random randy)
        {
            return GetRandomValue(new [] { 0f, MathHelper.PiOver2, MathHelper.Pi, MathHelper.PiOver2 + MathHelper.Pi });
        }

        public static T GetRandomValue<T>(T[] array)
        {
            var index = Game1.Randy.Next(array.Length);
            return array[index];
        }

        //Get a unit vector in a completely random direction.
        public static Vector2 NextVector(this Random randy)
        {
            Vector2 direction;
            do
            {
                direction = new Vector2(
                    Game1.Randy.Next(0, 100) - 50,
                    Game1.Randy.Next(0, 100) - 50);
            } while (direction.Length() == 0);
            direction.Normalize();
            return direction;
        }

        public static Vector2 ToVector(this Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        public static float GetValueInBounds(this float number, float min, float max)
        {
            if (number < min)
            {
                return min;
            }
            else if (number > max)
            {
                return max;
            }
            else
            {
                return number;
            }
        }

        public static Rectangle BoundingRectangle(this Texture2D texture)
        {
            return new Rectangle(0, 0, texture.Width, texture.Height);
        }

        public static bool Contains(this Rectangle rect, Vector2 point)
        {
            return rect.Contains(point.ToPoint());
        }

        /// <summary>
        /// Gets a vector that is a random point in the rectangle
        /// </summary>
        public static Vector2 GetRandomLocation(this Rectangle rect)
        {
            return new Vector2(Game1.Randy.Next(rect.Left, rect.Right + 1), Game1.Randy.Next(rect.Top, rect.Bottom + 1));
        }

        public static Point ToPoint(this Vector2 vector)
        {
            return new Point(vector.X.ToInt(), vector.Y.ToInt());
        }

        public static Vector2 ToGridLocation(this Vector2 worldLocation)
        {
            return new Vector2((float)Math.Floor(worldLocation.X / TileMap.TileSize), (float)Math.Floor(worldLocation.Y / TileMap.TileSize));
        }
        
        public static Vector2 ToIntegerVector(this Vector2 vector)
        {
            return new Vector2(vector.X.ToInt(), vector.Y.ToInt());
        }

        public static Vector2 RelativeCenterVector(this Texture2D texture)
        {
            return new Vector2(texture.Width / 2f, texture.Height / 2f);
        }

        public static Vector2 RelativeCenterVector(this Rectangle rect)
        {
            return new Vector2(rect.Width / 2f, rect.Height / 2f);
        }

        /// <summary>
        /// This method returns the unit vector of the direction from the source to the target,
        /// but only in 8 directions. Up, UpRight, Right, DownRight, Down, DownLeft, Left, or UpLeft.
        /// </summary>
        public static Vector2 GetEightWayDirectionTowardsTarget(Vector2 source, Vector2 target)
        {
            var direction = target - source;
            direction.Normalize();
            if (direction.X > 0.5f)
            {
                if (direction.Y > 0.5f)
                {
                    direction = new Vector2(1, 1);
                }
                else if (direction.Y < -0.5f)
                {
                    direction = new Vector2(1, -1);
                }
                else
                {
                    direction = new Vector2(1, 0);
                }
            }
            else if (direction.X < -0.5f)
            {
                if (direction.Y > 0.5f)
                {
                    direction = new Vector2(-1, 1);
                }
                else if (direction.Y < -0.5f)
                {
                    direction = new Vector2(-1, -1);
                }
                else
                {
                    direction = new Vector2(-1, 0);
                }
            }
            else
            {
                if (direction.Y > 0.5f)
                {
                    direction = new Vector2(0, 1);
                }
                else if (direction.Y < -0.5f)
                {
                    direction = new Vector2(0, -1);
                }
                else
                {
                    direction = new Vector2(0, 0);
                }
            }

            direction.Normalize();
            return direction;
        }

        public static Rectangle GetScaledRectangle(this ObjectModifier om)
        {
            return new Rectangle(om.Rectangle.X * Game1.TileScale,
                om.Rectangle.Y * Game1.TileScale,
                om.Rectangle.Width * Game1.TileScale,
                om.Rectangle.Height * Game1.TileScale);
        }

        public static int ToInt(this float number)
        {
            return TileEngine.Helpers.ToInt(number);
        }
    }
}
