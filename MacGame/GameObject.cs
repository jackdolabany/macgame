using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MacGame.Platforms;
using System;
using System.Collections.Generic;
using System.Linq;
using TileEngine;
using MacGame.DisplayComponents;

namespace MacGame
{
    public class GameObject
    {
        protected Vector2 worldLocation;

        private bool _flipped = false;
        /// <summary>
        /// Default is facing to the right. If you're flipped you are facing left.
        /// </summary>
        public bool Flipped
        {
            get
            {
                if (initiallyFlipped)
                {
                    return !_flipped;
                }
                else
                {
                    return _flipped;
                }
            }
            set
            {
                if (initiallyFlipped)
                {
                    _flipped = !value;
                }
                else
                {
                    _flipped = value;
                }
            }
        }

        protected bool initiallyFlipped = false;

        public bool Landed;

        // The speed at which you were falling when you landed.
        public float LandingVelocity;

        protected bool onGround;
        protected bool onCeiling;
        protected bool onLeftWall;
        protected bool onRightWall;

        public Platform? PlatformThatThisIsOn;

        /// <summary>
        /// A single platform that this game object won't consider. You can use this to jump
        /// down from a platform. Esp if that platform is moving down.
        /// </summary>
        public List<Platform> PoisonPlatforms = new List<Platform>(2);

        protected Rectangle collisionRectangle;

        /// <summary>
        /// if true, this enemy will be blocked by enemy blocking tiles. This is a way to restrict enemies to walk back
        /// and forth in a given area. This should be false for any enemy that isn't affected by gravity or that can move in the 
        /// y direction (jump) because they can land on the enemy tiles and it will look weird. 
        /// </summary>
        public bool isEnemyTileColliding = true;
        protected bool isTileColliding = true;

        public float RotationsPerSecond = 0;
        public bool IsRotationClockwise;

        public bool DrawCollisionRect = false;
        public bool DrawLocation = false;

        private Point[] pixelsToTest = new Point[20];

        // Encapsulates common display logic and state
        public DisplayComponent DisplayComponent;

        // So old stuff doesnt' break
        public float DrawDepth
        {
            get
            {
                return DisplayComponent.DrawDepth;
            }
            set
            {
                DisplayComponent.DrawDepth = value;
            }
        }

        public float Rotation
        {
            get { return this.DisplayComponent.Rotation; }
            set { this.DisplayComponent.Rotation = value; }
        }

        public Vector2 RotationAndDrawOrigin
        {
            get { return DisplayComponent.RotationAndDrawOrigin; }
            set { DisplayComponent.RotationAndDrawOrigin = value; }
        }

        public virtual float Scale
        {
            get { return this.DisplayComponent.Scale; }
            set { this.DisplayComponent.Scale = value; }
        }

        public bool Enabled { get; set; }

        /// <summary>
        /// If this is true the gameObject can move outside the bounds of the game world.
        /// If false, the character's position will be clamped in the game world.
        /// </summary>
        public bool IsAbleToMoveOutsideOfWorld;

        /// <summary>
        /// If this is true, the object will be disabled upon leaving the game world.
        /// </summary>
        public bool IsAbleToSurviveOutsideOfWorld;

        /// <summary>
        /// True if the object is colliding with a surface, top, bottom, left, or right
        /// </summary>
        public bool IsTouchingSurface
        {
            get
            {
                return (onCeiling || onGround || onLeftWall || onRightWall);
            }
        }

        public bool OnCeiling { get { return onCeiling; } }
        public bool OnGround { get { return onGround; } }
        public bool OnLeftWall { get { return onLeftWall; } }
        public bool OnRightWall { get { return onRightWall; } }
        public bool OnPlatform { get; private set; }

        /// <summary>
        /// Warning, this fan fuck up updating the gameObject
        /// </summary>
        public void SetToNotTouchingAnything()
        {
            onCeiling = false;
            onGround = false;
            onLeftWall = false;
            onRightWall = false;
        }

        public bool IsAffectedByGravity { get; set; }
        public bool IsAffectedByPlatforms { get; set; }

        public static Vector2 Gravity = new Vector2(0, 1200);
        public const float MaxFallSpeed = 16;

        protected Vector2 velocity;
        public Vector2 Velocity
        {
            get { return velocity + ForceVelocity; }
            set { velocity = value; }
        }

        public bool IsAffectedByForces = true;

        private Vector2 forceVelocity;
        public Vector2 ForceVelocity
        {
            get
            {
                return forceVelocity;
            }
            set
            {
                if (IsAffectedByForces)
                {
                    forceVelocity = value;
                }
            }
        }

        /// <summary>
        /// The bottom center of where you want a character.
        /// Center for certian particles.
        /// </summary>
        public Vector2 WorldLocation
        {
            get { return worldLocation; }
            set { worldLocation = value; }
        }

        public Vector2 WorldCenter
        {
            get
            {
                return DisplayComponent.GetWorldCenter(ref worldLocation);
            }
        }

        /// <summary>
        /// This is the rectangle we'll test to see if we should bother drawing the character.
        /// </summary>
        public virtual Rectangle GetDrawRectangle()
        {
            var rect = this.CollisionRectangle;
            return new Rectangle(rect.X - 100, rect.Y - 100, rect.Width + 200, rect.Height + 200);
        }

        /// <summary>
        /// Lazy load helpers
        /// </summary>
        public virtual Rectangle CollisionRectangle
        {
            get
            {
                return getCollisionRectangleForPosition(ref worldLocation);
            }
            set { collisionRectangle = value; }
        }

        public Vector2 CollisionCenter
        {
            get
            {
                if (collisionRectangle.IsEmpty)
                {
                    return this.WorldCenter;
                }
                else
                {
                    return CollisionRectangle.Center.ToVector();
                }
            }
        }

        protected Rectangle getCollisionRectangleForPosition(ref Vector2 position)
        {
            if (DisplayComponent == null)
            {
                return new Rectangle(
                  (int)(position.X) + collisionRectangle.X,
                  (int)(position.Y) + collisionRectangle.Y,
                  collisionRectangle.Width,
                  collisionRectangle.Height);
            }
            else
            {
                return new Rectangle(
                  (int)(position.X - DisplayComponent.RotationAndDrawOrigin.X) + collisionRectangle.X,
                  (int)(position.Y - DisplayComponent.RotationAndDrawOrigin.Y) + collisionRectangle.Y,
                  collisionRectangle.Width,
                  collisionRectangle.Height);
            }
        }

        /// <summary>
        /// Creates a collision Rectangle for the game object centered vertically on the GameObject. Assumes the default
        /// world location of the bottom center of the GameObject.
        /// Enter Width and Height in units of the original art, they'll be scaled from 8 pixel to 32 pixel tiles.
        /// </summary>
        protected void SetCenteredCollisionRectangle(int width, int height)
        {
            this.CollisionRectangle = new Rectangle(-width * Game1.TileScale / 2, -height * Game1.TileScale, width * Game1.TileScale, height * Game1.TileScale);
        }

        public GameObject()
        {
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = false;
            IsAffectedByGravity = false;
            IsAffectedByPlatforms = true;
            PlatformThatThisIsOn = null;
        }

        private Vector2 horizontalCollisionTest(Vector2 moveAmount)
        {
            if (moveAmount.X == 0)
            {
                return moveAmount;
            }

            onRightWall = false;
            onLeftWall = false;

            Vector2 newPosition = worldLocation;
            newPosition.X += moveAmount.X;
            Rectangle afterMoveRect = getCollisionRectangleForPosition(ref newPosition);

            bool isMovingRight = moveAmount.X > 0;

            int moveRectX;
            if (isMovingRight)
            {
                moveRectX = afterMoveRect.Right - 1;
            }
            else
            {
                moveRectX = afterMoveRect.Left;
            }

            //always test the corners
            int pixelCount = 2;
            pixelsToTest[0] = new Point(moveRectX, afterMoveRect.Top); //top corner
            pixelsToTest[1] = new Point(moveRectX, afterMoveRect.Bottom - 1); //bottom corner

            //Check some intermediate pixels if necessary.
            if (collisionRectangle.Height > TileMap.TileSize)
            {
                int testY = afterMoveRect.Top + TileMap.TileSize;
                while (testY < afterMoveRect.Bottom)
                {
                    var pixel = new Point(moveRectX, testY);
                    pixelsToTest[pixelCount] = pixel;
                    pixelCount++;
                    testY += TileMap.TileSize;
                }
            }

            for (int i = 0; i <= pixelCount - 1; i++)
            {
                var pixel = pixelsToTest[i];
                var mapSquare = Game1.CurrentMap.GetMapSquareAtPixel(pixel.X, pixel.Y);

                if (mapSquare != null && (!mapSquare.Passable || (isEnemyTileColliding && !mapSquare.EnemyPassable)))
                {
                    // There was a collision, place the object to the edge of the tile.
                    if (isMovingRight)
                    {
                        int mapCellLeft = Game1.CurrentMap.GetCellByPixelX(pixel.X) * TileMap.TileSize;
                        moveAmount.X = Math.Min(moveAmount.X, mapCellLeft - CollisionRectangle.Right);
                        onRightWall = true;
                    }
                    else
                    {
                        // Moving left
                        int mapCellRight = ((Game1.CurrentMap.GetCellByPixelX(pixel.X) + 1) * TileMap.TileSize) - 1;
                        moveAmount.X = Math.Max(moveAmount.X, mapCellRight - CollisionRectangle.Left + 1);
                        onLeftWall = true;
                    }
                    velocity.X = 0;
                }
            }

            return moveAmount;
        }

        private Vector2 verticalCollisionTest(Vector2 moveAmount)
        {
            if (moveAmount.Y == 0)
                return moveAmount;

            Vector2 newPosition = worldLocation + moveAmount;
            Rectangle afterMoveRect = getCollisionRectangleForPosition(ref newPosition);
            Rectangle cachedCollisionRectangle = this.CollisionRectangle;

            bool isFalling = moveAmount.Y >= 0;

            int moveRectY = 0;
            if (isFalling)
            {
                // special case, if we are falling we want to check a pixels below so that we 
                // can set onground = true if they are 1 pixel above the ground.
                moveRectY = afterMoveRect.Bottom;
            }
            else
            {
                moveRectY = afterMoveRect.Top;
            }

            //always test the corners
            int pixelCount = 2;
            pixelsToTest[0] = new Point(afterMoveRect.Left, moveRectY);
            pixelsToTest[1] = new Point(afterMoveRect.Right - 1, moveRectY);

            //Check some intermediate pixels if necessary.
            if (collisionRectangle.Width > TileMap.TileSize)
            {
                int testX = afterMoveRect.Left + TileMap.TileSize;
                while (testX < afterMoveRect.Right - 1)
                {
                    var pixel = new Point(testX, moveRectY);
                    pixelsToTest[pixelCount] = pixel;
                    pixelCount++;
                    testX += TileMap.TileSize;
                }
            }

            Platform newPlatform = null;

            bool previouslyOnGround = onGround;
            onGround = false;
            Landed = false;
            LandingVelocity = 0f;

            for (int i = 0; i <= pixelCount - 1; i++)
            {
                var pixel = pixelsToTest[i];

                // Test non passable blocks
                var mapSquare = Game1.CurrentMap.GetMapSquareAtPixel(pixel.X, pixel.Y);
                if (mapSquare != null && (!mapSquare.Passable || (isEnemyTileColliding && !mapSquare.EnemyPassable)))
                {
                    //there was a collision, place the object to the edge of the tile.
                    if (isFalling)
                    {
                        if (!previouslyOnGround)
                        {
                            Landed = true;
                            LandingVelocity = Math.Max(LandingVelocity, this.velocity.Y);
                        }
                        int mapCellTop = Game1.CurrentMap.GetCellByPixelY(pixel.Y) * TileMap.TileSize;
                        moveAmount.Y = Math.Min(moveAmount.Y, mapCellTop - cachedCollisionRectangle.Bottom);
                        onGround = true;
                    }
                    else
                    {
                        //moving up!
                        int mapCellBottom = (((Game1.CurrentMap.GetCellByPixelY(pixel.Y) + 1) * TileMap.TileSize) - 1);
                        moveAmount.Y = Math.Max(moveAmount.Y, mapCellBottom - cachedCollisionRectangle.Top + 1);
                        onCeiling = true;
                    }
                    velocity.Y = 0;
                }
                else
                {

                    // Test platforms!
                    if (IsAffectedByGravity && IsAffectedByPlatforms && isFalling)
                    {
                        foreach (var platform in Game1.Platforms)
                        {

                            if (platform.IsAffectedByGravity)
                            {
                                continue;
                            }

                            if (!platform.Enabled || PoisonPlatforms.Contains(platform))
                            {
                                continue;
                            }

                            var samePlatformAsBefore = (platform == PlatformThatThisIsOn);

                            if (!samePlatformAsBefore)
                            {
                                var wasAbove = cachedCollisionRectangle.Bottom <= platform.PreviousLocation.Y;
                                if (!wasAbove)
                                {
                                    continue;
                                }
                            }

                            var isPlatformBelowMe = platform.CollisionRectangle.Contains(new Point(pixel.X, pixel.Y + 1));

                            // Special case for vertical moving platforms moving down, it may move faster than the GameObject
                            // so we need to lock the GameObject to the platform. We consider you on the platform if your X
                            // coordinates fall in range and if you didn't jump
                            bool isLockedOnVerticalMovingPlatform = false;
                            if (samePlatformAsBefore && platform.velocity.Y > 0)
                            {
                                isLockedOnVerticalMovingPlatform = pixel.X >= platform.CollisionRectangle.Left && pixel.X <= platform.CollisionRectangle.Right;
                            }

                            if (isPlatformBelowMe || isLockedOnVerticalMovingPlatform)
                            {
                                // They are on a platform.
                                newPlatform = platform;
                                onGround = true;
                                OnPlatform = true;
                                velocity.Y = 0;
                                if (samePlatformAsBefore)
                                {
                                    // Previous platform. The GameObject will be moved along with the platform outside of this function.
                                    moveAmount.Y = Math.Min(moveAmount.Y, platform.CollisionRectangle.Top - cachedCollisionRectangle.Bottom);
                                }
                                else
                                {
                                    // If a new platform was hit, adjust the position.
                                    moveAmount.Y = Math.Min(moveAmount.Y, platform.CollisionRectangle.Top - cachedCollisionRectangle.Bottom);

                                    if (!previouslyOnGround)
                                    {
                                        Landed = true;
                                        LandingVelocity = Math.Max(LandingVelocity, this.velocity.Y);
                                    }

                                }
                                break;
                            }
                        }
                    }
                }
            }

            Landed = Landed && ((afterMoveRect.Y - cachedCollisionRectangle.Y) > 5);

            PlatformThatThisIsOn = newPlatform;
            return moveAmount;
        }

        public virtual void Flip()
        {
            Flipped = !Flipped;
        }

        public void RotateTo(Vector2 direction)
        {
            Rotation = (float)Math.Atan2(direction.Y, direction.X);
        }

        public virtual void Update(GameTime gameTime, float elapsed)
        {
            if (!Enabled)
                return;

            Vector2 previousLocation = this.worldLocation;

            if (RotationsPerSecond > 0)
            {
                var rotation = (IsRotationClockwise ? 1 : -1) * elapsed * RotationsPerSecond * (float)Math.PI * 2;
                Rotation += rotation;
            }

            onCeiling = false;
            OnPlatform = false;

            if (IsAffectedByGravity)
            {
                velocity += Gravity * elapsed;
            }

            Vector2 moveAmount = Velocity * elapsed;

            var previousUpdatePlatForm = PlatformThatThisIsOn;

            if (isTileColliding)
            {
                moveAmount = horizontalCollisionTest(moveAmount);
                moveAmount = verticalCollisionTest(moveAmount);

                //// They are on a platform that they were already on, move them with the platform
                //if (PlatformThatThisIsOn != null && PlatformThatThisIsOn == previousUpdatePlatForm)
                //{
                //    moveAmount += PlatformThatThisIsOn.Delta;
                //    moveAmount = horizontalCollisionTest(moveAmount);
                //}
            }

            Vector2 newPosition = worldLocation + moveAmount;

            if (!IsAbleToMoveOutsideOfWorld)
            {
                // only clamp the x position in the world. If they fall out of the world, they are toast.
                if (CollisionRectangle.Left < 0)
                {
                    newPosition.X -= CollisionRectangle.Left;
                    velocity.X = 0;
                    onLeftWall = true;
                }
                else if (CollisionRectangle.Right > Game1.Camera.WorldRectangle.Width)
                {
                    newPosition.X -= (CollisionRectangle.Right - Game1.Camera.WorldRectangle.Width);
                    velocity.X = 0;
                    onRightWall = true;
                }
            }

            if (!IsAbleToSurviveOutsideOfWorld)
            {
                if (!this.CollisionRectangle.Intersects(Game1.Camera.WorldRectangle))
                {
                    Enabled = false;
                }
            }

            AdjustPositionBeforeDraw(ref newPosition, ref previousLocation);

            worldLocation = newPosition;

            DisplayComponent.Update(gameTime, elapsed, this.worldLocation, this._flipped);
        }

        public virtual void AdjustPositionBeforeDraw(ref Vector2 newPosition, ref Vector2 previousLocation) { }

        public void SetupDraw(GameTime gameTime, float elapsed)
        {
            this.DisplayComponent.Update(gameTime, elapsed, this.worldLocation, this.Flipped);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (Enabled)
            {
                this.DisplayComponent.Draw(spriteBatch);
            }

            // Draw Collision Rectangle in reddish
            if (DrawCollisionRect || Game1.DrawAllCollisisonRects && !collisionRectangle.IsEmpty)
            {
                Color color = Color.Red * 0.25f;
                spriteBatch.Draw(Game1.TileTextures, CollisionRectangle, Game1.WhiteSourceRect, color);
            }

            // Draw a square at the GameObjects location
            if (DrawLocation || Game1.DrawAllCollisisonRects)
            {
                var rectSize = 4;
                var location = WorldLocation;

                // Draw location in green
                spriteBatch.Draw(Game1.TileTextures, new Rectangle(-(int)(rectSize / 2f + location.X), -(int)(rectSize / 2f + location.Y), rectSize, rectSize), Game1.WhiteSourceRect, Color.Green);

                location = WorldCenter;

                // Draw world center in Yellow
                spriteBatch.Draw(Game1.TileTextures, new Rectangle(-(int)(rectSize / 2f + location.X), -(int)(rectSize / 2 + location.Y), rectSize, rectSize), Game1.WhiteSourceRect, Color.Yellow);
            }
        }

    }
}
