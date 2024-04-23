using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var translationMatrix = Matrix.CreateTranslation(new Vector3(-position.X, -position.Y, 0));
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
                return (int)(viewPortSize.X / Zoom);
            }
        }

        public int ViewHeight
        {
            get
            {
                return (int)(viewPortSize.Y / Zoom);
            }
        }

        public int ViewPortWidth
        {
            get { return (int)viewPortSize.X; }
            set { viewPortSize.X = value; }
        }

        public int ViewPortHeight
        {
            get { return (int)viewPortSize.Y; }
            set { viewPortSize.Y = value; }
        }

        public Rectangle ViewPort
        {
            get
            {
                int width = ViewWidth;
                int height = ViewHeight;
                return new Rectangle(
                    (int)(Position.X - width / 2f), (int)(Position.Y - height / 2f),
                    width, height);
            }
        }

        public Rectangle ParallaxScaledViewPort
        {
            get
            {
                var svp = viewPortSize / Zoom / ParallaxScale;
                return new Rectangle(
                    (int)(Position.X - svp.X / 2f),
                    (int)(Position.Y - svp.Y / 2),
                    (int)svp.X,
                    (int)svp.Y);
            }
        }

        public bool IsObjectVisible(Rectangle bounds)
        {
            // Pad the bounds by 20% since sometimes the collision rect is smaller than the object.
            // Better to overdraw than have stuff pop in.
            const float paddingPercent = 0.25f;
            var widthPadding = bounds.Width * paddingPercent;
            var heightPadding = bounds.Width * paddingPercent;

            var paddedBounds = new Rectangle(
                (int)(bounds.X - (widthPadding / 2f)),
                (int)(bounds.Y - (heightPadding / 2f)), 
                (int)(bounds.Width + widthPadding), 
                (int)(bounds.Height + heightPadding));

            return ViewPort.Intersects(paddedBounds);
        }

        public Vector2 GetRelativeScreenPosition(Vector2 worldPosition)
        {
            return Vector2.Transform(worldPosition, Transform);
        }

        public bool IsPointVisible(Vector2 point)
        {
            return ViewPort.Contains((int)point.X, (int)point.Y);
        }

    }
}
