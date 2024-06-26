using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MacGame.Platforms;
using System;
using System.Collections.Generic;
using System.Linq;
using TileEngine;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework.Content;

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

        public virtual float DrawDepth
        {
            get
            {
                return DisplayComponent.DrawDepth;
            }
        }

        public virtual void SetDrawDepth(float depth)
        {
            DisplayComponent.DrawDepth = depth;
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

        public virtual Vector2 Gravity
        {
            get
            {
                return new Vector2(0, 1600);
            }
        }

        public float MaxFallSpeed
        {
            get
            {
                return 1000;
            }
        }

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
            if (moveAmount.X == 0) return moveAmount;

            onRightWall = false;
            onLeftWall = false;

            Rectangle currentPositionRect = this.CollisionRectangle;

            Vector2 newPosition = worldLocation;
            newPosition.X += moveAmount.X;
            Rectangle afterMoveRect = getCollisionRectangleForPosition(ref newPosition);

            // Collisions are based on Type #2: Tile Based (Smooth) described
            // here: http://higherorderfun.com/blog/2012/05/20/the-guide-to-implementing-2d-platformers/
            // Scan the tiles along x the pre-move rectangle of the player.
            // Scan as many tiles across as needed by the movement amount. 
            // find the closest obstacle and move the GameObject the min distance between 
            // the closest obstacle and how far they want to move.

            // Loop as many cells as we need top to bottom.
            var topCell = Game1.CurrentMap.GetCellByPixelY(currentPositionRect.Top);
            var bottomCell = Game1.CurrentMap.GetCellByPixelY(currentPositionRect.Bottom - 1);

            // How many should we check left or right?
            int startCellX;
            int endCellX;
            if (moveAmount.X > 0)
            {
                // moving to the right.
                startCellX = Game1.CurrentMap.GetCellByPixelX(currentPositionRect.Right);
                endCellX = Game1.CurrentMap.GetCellByPixelX(afterMoveRect.Right + 1); // Add one since they may have moved a fraction of a pixel
            }
            else
            {
                // Moving left
                startCellX = Game1.CurrentMap.GetCellByPixelX(currentPositionRect.Left + 1); // Subtract one since they may have moved a fraction of a pixel
                endCellX = Game1.CurrentMap.GetCellByPixelX(afterMoveRect.Left); 
            }

            for (int y = topCell; y <= bottomCell; y++)
            {

                // Determine the step direction based on the comparison
                int step = startCellX <= endCellX ? 1 : -1;

                // Loop through the cells in the x direction, incrementing or decrementing
                for (int x = startCellX; (step == 1) ? x <= endCellX : x >= endCellX; x += step)
                {

                    // if we're on a slope, we need to ignore a solid tile next to the slope as we're moving uphill so that 
                    // it doesn't prevent horizontal movement until we are on top of it
                    //         /      |
                    //      __/_______|_
                    // --> / |        |
                    //    /  | ignore |
                    int adjacentX = x - 1;
                    if (velocity.X < 0)
                    {
                        adjacentX = x + 1;
                    }
                    var adjacentCell = Game1.CurrentMap.GetMapSquareAtCell(adjacentX, y);
                    if (adjacentCell != null && adjacentCell.IsOnASlope())
                    {
                        continue;
                    }

                    var cell = Game1.CurrentMap.GetMapSquareAtCell(x, y);
                    if (cell != null)
                    {
                        if (!cell.Passable && !cell.IsOnASlope() || (isEnemyTileColliding && !cell.EnemyPassable))
                        {
                            // There was a collision, place the object to the edge of the tile.
                            if (moveAmount.X > 0)
                            {
                                // Moving right
                                var distanceToTile = (TileMap.TileSize * x) - currentPositionRect.Right;
                                moveAmount.X = Math.Min(moveAmount.X, distanceToTile);
                                onRightWall = true;
                            }
                            else if (moveAmount.X < 0)
                            {
                                // Moving left
                                var distanceToTile = currentPositionRect.Left - ((x + 1) * TileMap.TileSize);
                                moveAmount.X = Math.Max(moveAmount.X, -distanceToTile);
                                onLeftWall = true;
                            }
                            velocity.X = 0;

                            // We scan closest tiles first so no reason to continue with the for loop.
                            continue;
                        }
                    }
                }
            }

            return moveAmount;
        }

        private Vector2 verticalCollisionTest(Vector2 moveAmount)
        {
            if (moveAmount.Y == 0) return moveAmount;

            bool previouslyOnGround = onGround;
            onGround = false;
            Landed = false;
            LandingVelocity = 0f;
            Platform newPlatform;

            Rectangle currentPositionRect = this.CollisionRectangle;
            Vector2 newPosition = worldLocation + moveAmount;
            Rectangle afterMoveRect = getCollisionRectangleForPosition(ref newPosition);

            bool isFalling = moveAmount.Y > 0;

            // Slopes are special. If we're moving to a slope, ignore everything and just check the bottom center.
            bool isOnSlope = false;
            if (isFalling)
            {
                var bottomCenterCell = Game1.CurrentMap.GetMapSquareAtPixel(currentPositionRect.Center.X, currentPositionRect.Bottom - 1);
                isOnSlope = bottomCenterCell != null && bottomCenterCell.IsOnASlope();
                
                if (isOnSlope)
                {
                    var slopeCellX = Game1.CurrentMap.GetCellByPixelX(currentPositionRect.Center.X);
                    var slopeCellY = Game1.CurrentMap.GetCellByPixelY(currentPositionRect.Bottom - 1);

                    // Moving down
                    int distanceToBottomOfTile = (TileMap.TileSize * (slopeCellY + 1)) - currentPositionRect.Bottom;
                    
                    float xRelativeToTile = currentPositionRect.Center.X - (TileMap.TileSize * slopeCellX);
                    
                    float percent = xRelativeToTile / TileMap.TileSize;
                    float distanceToSlope = (1 - percent) * bottomCenterCell!.LeftHeight + percent * bottomCenterCell.RightHeight;
                    
                    moveAmount.Y = Math.Min(moveAmount.Y, distanceToBottomOfTile - distanceToSlope);
                    onGround = true;

                    velocity.Y = 0;

                    if (!previouslyOnGround)
                    {
                        Landed = true;
                        LandingVelocity = Math.Max(LandingVelocity, this.velocity.Y);
                    }
                }
            }

            if (!isOnSlope)
            {
                // Regular tile collision
                // Loop as many x cells as we need left to right.
                var leftCell = Game1.CurrentMap.GetCellByPixelX(currentPositionRect.Left);
                var rightCell = Game1.CurrentMap.GetCellByPixelX(currentPositionRect.Right - 1);

                // How many should we check top or bottom?
                int startCellY;
                int endCellY;
                if (isFalling)
                {
                    // moving down
                    startCellY = Game1.CurrentMap.GetCellByPixelY(currentPositionRect.Bottom);
                    endCellY = Game1.CurrentMap.GetCellByPixelY(afterMoveRect.Bottom + 1);
                }
                else
                {
                    // Moving up
                    startCellY = Game1.CurrentMap.GetCellByPixelY(currentPositionRect.Top + 1);
                    endCellY = Game1.CurrentMap.GetCellByPixelY(afterMoveRect.Top);
                }

                for (int x = leftCell; x <= rightCell; x++)
                {
                    // Determine the step direction based on the comparison
                    int step = startCellY <= endCellY ? 1 : -1;

                    // Loop through the cells in the y direction, incrementing or decrementing
                    for (int y = startCellY; (step == 1) ? y <= endCellY : y >= endCellY; y += step)
                    {
                        var cell = Game1.CurrentMap.GetMapSquareAtCell(x, y);
                        if (cell != null)
                        {
                            if (!cell.Passable && !cell.IsOnASlope() || (isEnemyTileColliding && !cell.EnemyPassable))
                            {
                                // There was a collision, place the object to the edge of the tile.
                                if (moveAmount.Y > 0)
                                {
                                    // Moving down
                                    int distanceToTile = (TileMap.TileSize * y) - currentPositionRect.Bottom;
                                    moveAmount.Y = Math.Min(moveAmount.Y, distanceToTile);
                                    onGround = true;

                                    if (!previouslyOnGround)
                                    {
                                        Landed = true;
                                        LandingVelocity = Math.Max(LandingVelocity, this.velocity.Y);
                                    }
                                }
                                else if (moveAmount.Y < 0)
                                {
                                    // Moving up.
                                    int distanceToTile = currentPositionRect.Top - ((y + 1) * TileMap.TileSize);
                                    moveAmount.Y = Math.Max(moveAmount.Y, -distanceToTile);
                                    onCeiling = true;
                                }
                                velocity.Y = 0;

                                // We scan closest tiles first so no reason to continue with the for loop.
                                continue;
                            }
                        }
                    }
                }
            }
            



            // Test platforms!
            if (IsAffectedByGravity && IsAffectedByPlatforms && isFalling)
            {

                // Reassign these to test platforms.
                newPosition = worldLocation + moveAmount;
                afterMoveRect = getCollisionRectangleForPosition(ref newPosition);

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
                        var wasAbove = currentPositionRect.Bottom <= platform.PreviousLocation.Y;
                        if (!wasAbove)
                        {
                            continue;
                        }
                    }

                    var pixelBelowMe = this.WorldLocation + new Vector2(0, 1);

                    var isPlatformBelowMe = platform.CollisionRectangle.X < currentPositionRect.Right
                        && platform.CollisionRectangle.Right > currentPositionRect.Left
                        && platform.CollisionRectangle.Top < pixelBelowMe.Y
                        && platform.CollisionRectangle.Bottom > pixelBelowMe.Y;
                    
                    // Special case for vertical moving platforms moving down, it may move faster than the GameObject
                    // so we need to lock the GameObject to the platform. We consider you on the platform if your X
                    // coordinates fall in range and if you didn't jump
                    bool isLockedOnVerticalMovingPlatform = false;
                    if (samePlatformAsBefore && platform.velocity.Y > 0)
                    {
                        isLockedOnVerticalMovingPlatform = afterMoveRect.X >= platform.CollisionRectangle.Left && afterMoveRect.X <= platform.CollisionRectangle.Right;
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
                            moveAmount.Y = Math.Min(moveAmount.Y, platform.CollisionRectangle.Top - currentPositionRect.Bottom);
                        }
                        else
                        {
                            // If a new platform was hit, adjust the position.
                            moveAmount.Y = Math.Min(moveAmount.Y, platform.CollisionRectangle.Top - currentPositionRect.Bottom);

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



            Landed = Landed && ((afterMoveRect.Y - currentPositionRect.Y) > 5);

            // TODO:
            //PlatformThatThisIsOn = newPlatform;
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

                velocity.Y = Math.Min(velocity.Y, MaxFallSpeed);
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
                var squareSize = 4;
                var offset = squareSize / 2;
                var location = WorldLocation;

                // Draw location in green
                spriteBatch.Draw(Game1.TileTextures, new Rectangle((int)(location.X - offset), (int)(location.Y - offset), squareSize, squareSize), Game1.WhiteSourceRect, Color.Green);

                location = WorldCenter;

                // Draw world center in Yellow
                spriteBatch.Draw(Game1.TileTextures, new Rectangle((int)(location.X - offset), (int)(location.Y - offset), squareSize, squareSize), Game1.WhiteSourceRect, Color.Yellow);
            }
        }

    }
}
