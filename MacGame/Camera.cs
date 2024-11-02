using Microsoft.Xna.Framework;
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

        // Change this for a boss fight so the player can't escape.
        public bool CanScrollLeft = true;

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

        public Vector2 ParallaxScale = new Vector2(0.75f, 0.75f);

        public void UpdateTransformation()
        {
            var translationMatrix = Matrix.CreateTranslation(new Vector3(-position.X.ToInt(), -position.Y.ToInt(), 0));
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

                position = new Vector2(x, y);

                if (!CanScrollLeft && position.X < previousX)
                {
                    position.X = previousX;
                }

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
            // Pad the bounds by 25% since sometimes the collision rect is smaller than the object.
            // Better to overdraw than have stuff pop in.
            const float paddingPercent = 0.25f;
            var widthPadding = bounds.Width * paddingPercent;
            var heightPadding = bounds.Height * paddingPercent;

            var paddedBounds = new Rectangle(
                (bounds.X - widthPadding).ToInt(),
                (bounds.Y - heightPadding).ToInt(), 
                (bounds.Width + widthPadding * 2).ToInt(), 
                (bounds.Height + heightPadding * 2).ToInt());

            return ViewPort.Intersects(paddedBounds);
        }

        /// <summary>
        /// Makes sure hte object isn't just off screen, but off by like 100 pixels.
        /// </summary>
        public bool IsWayOffscreen(Rectangle bounds)
        {
            var paddedBounds = new Rectangle(
                (bounds.X - 50),
                (bounds.Y - 50),
                (bounds.Width + 100),
                (bounds.Height + 100));

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
