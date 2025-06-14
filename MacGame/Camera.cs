using Microsoft.Xna.Framework;
using System;
using TileEngine;

namespace MacGame
{
    public class Camera
    {
        private Vector2 position = Vector2.Zero;
        private Vector2 viewPortSize = Vector2.Zero;
        public float Zoom; // Camera Zoom
        public Matrix Transform; // Matrix Transform
        public float Rotation; // Camera Rotation

        public int? MinX { get; set; } = null;
        public int? MaxX { get; set; } = null;
        public int? MinY { get; set; } = null;
        public int? MaxY { get; set; } = null;

        private TileMap _map;
        public TileMap Map
        {
            get
            {
                return _map;
            }
            set
            {
                _map = value;
                WorldRectangle = _map.GetWorldRectangle();

                var zoom = _map.Zoom;
                if (zoom == 0)
                {
                    zoom = 1;
                }

                Zoom = _map.Zoom * DEFAULT_ZOOM;
            }
        }

        public bool IsMaxed;
        public float Velocity { get; set; }

        public const float DEFAULT_ZOOM = 1f;

        public Camera()
        {
            Velocity = 120f;
        }

        public void ClearRestrictions()
        {
            MaxX = null;
            MinX = null;
            MaxY = null;
            MinY = null;
        }

        public Vector2 ParallaxScale = new Vector2(0.75f, 0.75f);

        public void UpdateTransformation()
        {
            var translationMatrix = Matrix.CreateTranslation(new Vector3(-(int)position.X, -(int)position.Y, 0));
            var rotationMatrix = Matrix.CreateRotationZ(Rotation);
            var scaleMatrix = Matrix.CreateScale(new Vector3(Zoom, Zoom, 1f));
            var originMatrix = Matrix.CreateTranslation(new Vector3(viewPortSize.X / 2f, viewPortSize.Y / 2f, 0));

            Transform = translationMatrix * rotationMatrix * scaleMatrix * originMatrix;
        }

        public Matrix GetParallaxScrollingBackgroundTransformation()
        {
            var translationMatrix = Matrix.CreateTranslation(new Vector3(-position, 0));
            var rotationMatrix = Matrix.CreateRotationZ(Rotation);
            var scaleMatrix = Matrix.CreateScale(new Vector3(ParallaxScale * Zoom, 1f));
            var originMatrix = Matrix.CreateTranslation(new Vector3(viewPortSize.X / 2f, viewPortSize.Y / 2f, 0));

            return translationMatrix * rotationMatrix * scaleMatrix * originMatrix;
        }

        /// <summary>
        /// The center of the Camera
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set
            {

                // Check constraints against map once it exists.
                float previousX = position.X;

                float x = 0;
                float y = 0;

                var mapWidth = Map.MapWidth * TileMap.TileSize;
                if (mapWidth < ViewWidth)
                {
                    x = mapWidth / 2;
                }
                else
                {
                    x = MathHelper.Clamp(value.X,
                        WorldRectangle.X + (ViewWidth / 2),
                        WorldRectangle.Width - (ViewWidth / 2));
                }
                var mapHeight = Map.MapHeight * TileMap.TileSize;
                if (mapHeight < ViewHeight)
                {
                    y = mapHeight / 2;
                }
                else
                {
                    y = MathHelper.Clamp(value.Y,
                        WorldRectangle.Y + (ViewHeight / 2),
                        WorldRectangle.Height - (ViewHeight / 2));
                }

                // Clamp the camera by the Max/Min X and Y values.
                if (MaxX.HasValue)
                {
                    x = Math.Min(x, MaxX.Value);
                }
                if (MinX.HasValue)
                {
                    x = Math.Max(x, MinX.Value);
                }
                if (MaxY.HasValue)
                {
                    y = Math.Min(y, MaxY.Value);
                }
                if (MinY.HasValue)
                {
                    y = Math.Max(y, MinY.Value);
                }

                position = new Vector2(x, y);

            }
        }

        private Rectangle _worldRectangle = new Rectangle(0, 0, 0, 0);
        public Rectangle WorldRectangle
        {
            get
            {
                return _worldRectangle;
            }
            set
            {
                _worldRectangle = value;
            }
        }

        public int ViewWidth
        {
            get
            {
                return (viewPortSize.X / Zoom).ToInt();
            }
        }

        public int ViewHeight
        {
            get
            {
                return (viewPortSize.Y / Zoom).ToInt();
            }
        }

        public int ViewPortWidth
        {
            get { return viewPortSize.X.ToInt(); }
            set { viewPortSize.X = value; }
        }

        public int ViewPortHeight
        {
            get { return viewPortSize.Y.ToInt(); }
            set { viewPortSize.Y = value; }
        }

        public Rectangle ViewPort
        {
            get
            {
                int width = ViewWidth;
                int height = ViewHeight;
                return new Rectangle(
                    (Position.X - width / 2f).ToInt(), (Position.Y - height / 2f).ToInt(),
                    width, height);
            }
        }

        public Rectangle ParallaxScaledViewPort
        {
            get
            {
                var svp = viewPortSize / Zoom / ParallaxScale;
                return new Rectangle(
                    (Position.X - svp.X / 2f).ToInt(),
                    (Position.Y - svp.Y / 2).ToInt(),
                    svp.X.ToInt(),
                    svp.Y.ToInt());
            }
        }

        public bool IsObjectVisible(Rectangle bounds)
        {
            // Pad the viewport by the dimensions of the object.
            var paddedViewPort = new Rectangle(
                ViewPort.X - bounds.Width,
                ViewPort.Y - bounds.Height,
                ViewPort.Width + bounds.Width * 2,
                ViewPort.Height + bounds.Height * 2);

            return paddedViewPort.Intersects(bounds);
        }

        /// <summary>
        /// Makes sure the object isn't just off screen, but off by 100 pxiels
        /// </summary>
        public bool IsWayOffscreen(Rectangle bounds)
        {
            var paddedBounds = new Rectangle(
                (bounds.X - 100),
                (bounds.Y - 100),
                (bounds.Width + 200),
                (bounds.Height + 200));

            return !ViewPort.Intersects(paddedBounds);
        }

        public Vector2 GetRelativeScreenPosition(Vector2 worldPosition)
        {
            return Vector2.Transform(worldPosition, Transform);
        }

        public bool IsPointVisible(Vector2 point)
        {
            return ViewPort.Contains(point.X.ToInt(), point.Y.ToInt());
        }

    }
}
