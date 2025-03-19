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

        // Track if the character was on a slope tile which can lock them to it.
        protected bool onSlope;

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
                return Game1.Gravity;
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
        public virtual Vector2 WorldLocation
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
                  position.X.ToInt() + collisionRectangle.X,
                  position.Y.ToInt() + collisionRectangle.Y,
                  collisionRectangle.Width,
                  collisionRectangle.Height);
            }
            else
            {
                return new Rectangle(
                  (position.X - DisplayComponent.RotationAndDrawOrigin.X).ToInt() + collisionRectangle.X,
                  (position.Y - DisplayComponent.RotationAndDrawOrigin.Y).ToInt() + collisionRectangle.Y,
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
            var rect = new Rectangle(-width * Game1.TileScale / 2, -height * Game1.TileScale, width * Game1.TileScale, height * Game1.TileScale);
            this.CollisionRectangle = rect;
        }

        public bool HasCollisionRectangle
        {
            get
            {
                return collisionRectangle.Height > 0 && collisionRectangle.Width > 0;
            }
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

            // Need to check this here and store it in varaible. As we adjust the object it may change the
            // moveAmount.X value but we want to preserve the original motion.
            bool isMovingRight = moveAmount.X > 0;

            if (isMovingRight)
            {
                // moving to the right.
                startCellX = Game1.CurrentMap.GetCellByPixelX(currentPositionRect.Right - 1);
                endCellX = Game1.CurrentMap.GetCellByPixelX(afterMoveRect.Right + 1); // Add one since they may have moved a fraction of a pixel
            }
            else
            {
                // Moving left
                startCellX = Game1.CurrentMap.GetCellByPixelX(currentPositionRect.Left + 1); // Subtract one since they may have moved a fraction of a pixel
                endCellX = Game1.CurrentMap.GetCellByPixelX(afterMoveRect.Left - 1); 
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
                    if (!isMovingRight)
                    {
                        adjacentX = x + 1;
                    }
                    var adjacentCell = Game1.CurrentMap.GetMapSquareAtCell(adjacentX, y);
                    if (adjacentCell != null && adjacentCell.IsSlope())
                    {
                        continue;
                    }

                    var cell = Game1.CurrentMap.GetMapSquareAtCell(x, y);
                    if (cell != null)
                    {
                        if (!cell.Passable && !cell.IsSlope() || (isEnemyTileColliding && !cell.EnemyPassable))
                        {
                            // There was a collision, place the object to the edge of the tile.
                            if (isMovingRight)
                            {
                                // Moving right
                                float rightMostPoint = this.WorldLocation.X + collisionRectangle.X + collisionRectangle.Width;
                                float distanceToTile = (float)(TileMap.TileSize * x) - rightMostPoint;
                                moveAmount.X = Math.Min(moveAmount.X, distanceToTile);
                                onRightWall = true;
                            }
                            else
                            {
                                // Moving left
                                float leftMostPoint = this.WorldLocation.X + collisionRectangle.X;
                                float distanceToTile = leftMostPoint - ((x + 1) * TileMap.TileSize);
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

            // Check against all custom collision objects
            newPosition = worldLocation + moveAmount;
            afterMoveRect = getCollisionRectangleForPosition(ref newPosition);

            foreach (var collisionObject in Game1.CustomCollisionObjects)
            {
                if (!collisionObject.DoesCollideWithObject(this))
                {
                    continue;
                }

                // They are not in the same vertical space, no need to check horizontal collision.
                if (afterMoveRect.Top >= collisionObject.CollisionRectangle.Bottom || afterMoveRect.Bottom <= collisionObject.CollisionRectangle.Top)
                {
                    continue;
                }

                if (isMovingRight)
                {
                    float beforeMoveRight = this.WorldLocation.X + collisionRectangle.X + collisionRectangle.Width;
                    float afterMoveRight = beforeMoveRight + moveAmount.X;
                    var leftOfObject = collisionObject.CollisionRectangle.Left;

                    if (beforeMoveRight <= leftOfObject && afterMoveRight > leftOfObject)
                    {
                        var distanceToObject = leftOfObject - beforeMoveRight;
                        moveAmount.X = distanceToObject;
                        onRightWall = true;
                    }
                }
                else
                {
                    float beforeMoveLeft = this.WorldLocation.X + collisionRectangle.X;
                    float afterMoveLeft = beforeMoveLeft + moveAmount.X;
                    var rightOfObject = collisionObject.CollisionRectangle.Right;
                    if (beforeMoveLeft >= rightOfObject && afterMoveLeft < rightOfObject)
                    {
                        var distanceToObject = rightOfObject - beforeMoveLeft;
                        moveAmount.X = distanceToObject;
                        onLeftWall = true;
                    }
                }
            }

            return moveAmount;
        }

        private Vector2 verticalCollisionTest(Vector2 moveAmount)
        {

            if (moveAmount.Y == 0) return moveAmount;

            bool previouslyOnGround = onGround;
            bool previouslyOnSlope = onSlope;

            onGround = false;
            onSlope = false;

            Landed = false;
            LandingVelocity = 0f;

            Rectangle currentPositionRect = this.CollisionRectangle;
            Vector2 newPosition = worldLocation + moveAmount;
            Rectangle afterMoveRect = getCollisionRectangleForPosition(ref newPosition);

            bool isFalling = moveAmount.Y > 0;

            // Slope collision
            // We may need to check slopes even if you aren't falling. A fast movement in the x direction could
            // cause a slope collision even if you are not moving down or moving slightly up.
            var shouldCheckSlopes = moveAmount.Y > 0 || moveAmount.X != 0f;
            
            if (shouldCheckSlopes)
            {
                var startCellY = Game1.CurrentMap.GetCellByPixelY(currentPositionRect.Bottom - 8);
                var endCellY = Game1.CurrentMap.GetCellByPixelY(afterMoveRect.Bottom + 8);
                
                var startY = Math.Min(startCellY, endCellY);
                var endY = Math.Max(startCellY, endCellY);

                for (int y = startY; y <= endY; y++)
                {
                    float centerPoint = this.worldLocation.X + moveAmount.X;
                    int x = Game1.CurrentMap.GetCellByPixelX((int)Math.Round(centerPoint));
                    var cell = Game1.CurrentMap.GetMapSquareAtCell(x, y);
                    if (cell != null && !cell.Passable && cell.IsSlope())
                    {

                        // A slope collision was found! If there's a slope tile on the GameObject's center
                        // pixel it takes precedence over all other y axis collisions.

                        // reset stuff
                        onGround = false;
                        Landed = false;
                        LandingVelocity = 0f;

                        var afterMoveBottom = this.worldLocation.Y + collisionRectangle.Y + collisionRectangle.Height + moveAmount.Y;

                        float distanceToBottomOfTile = (TileMap.TileSize * (y + 1)) - afterMoveBottom;

                        float xRelativeToTile = centerPoint - (TileMap.TileSize * x);

                        float percent = xRelativeToTile / TileMap.TileSize;
                        float relativeDistanceToSlope = (1 - percent) * cell.LeftHeight + percent * cell.RightHeight;
                        var distanceToSlope = distanceToBottomOfTile - relativeDistanceToSlope;

                        // Lock them to the slope if they are running down so they don't just fall as they move forward.
                        if (previouslyOnGround && isFalling)
                        {
                            // Ignore any other tiles and lock them to the slope.
                            moveAmount.Y = distanceToSlope;
                        }
                        else
                        {
                            // Don't lock them to the slope if they are falling to it, but if we encourter
                            // a slope in the middle x coordinate we don't care about any other blocking tiles.
                            moveAmount.Y = Math.Min(moveAmount.Y, distanceToSlope);
                        }

                        if (moveAmount.Y == distanceToSlope)
                        {
                            velocity.Y = 0;
                            onGround = true;
                            if (!previouslyOnGround)
                            {
                                Landed = true;
                                LandingVelocity = Math.Max(LandingVelocity, this.velocity.Y);
                            }
                        }

                        // We scan closest tiles first so no reason to continue with the for loop.
                        // the slope is the only thing that matters.
                        onSlope = true;
                        break;

                    }
                }
            }

            // Regular tile collision
            if (!onSlope)
            {
                // Regular tile collision
                // Loop as many x cells as we need left to right.
                var leftCell = Game1.CurrentMap.GetCellByPixelX(afterMoveRect.Left);
                var rightCell = Game1.CurrentMap.GetCellByPixelX(afterMoveRect.Right - 1);

                // How many should we check top or bottom?
                int startCellY;
                int endCellY;
                if (isFalling)
                {
                    // moving down. Add an extra buffer to the squares when falling as padding
                    // for some padding for tricky scenarios where you are running across steep
                    // slopes. We don't want to miss the slope tiles.
                    startCellY = Game1.CurrentMap.GetCellByPixelY(currentPositionRect.Bottom - 8);
                    endCellY = Game1.CurrentMap.GetCellByPixelY(afterMoveRect.Bottom + 8);
                }
                else
                {
                    // Moving up
                    startCellY = Game1.CurrentMap.GetCellByPixelY(currentPositionRect.Top + 4);
                    endCellY = Game1.CurrentMap.GetCellByPixelY(afterMoveRect.Top - 4);
                }

                // Determine the step direction based on the comparison
                int step = startCellY <= endCellY ? 1 : -1;

                // Store the original amount they would like to move and adjust as necessary.
                var yToMove = moveAmount.Y;

                // Loop through the cells in the y direction, incrementing or decrementing
                for (int y = startCellY; (step == 1) ? y <= endCellY : y >= endCellY; y += step)
                {

                    // Scan each x direction cell
                    for (int x = leftCell; x <= rightCell; x++)
                    {
                        var cell = Game1.CurrentMap.GetMapSquareAtCell(x, y);
                        if (cell != null)
                        {

                            // Only check slope tiles if you are moving up. When moving up, they count as
                            // full blocking tiles since the slope is never on the bottom.
                            // but only if you were previously below the tile, otherwise characters shorter than
                            // 1 tile will get stuck in them.
                            bool slopeCheck;
                            if (!cell.IsSlope())
                            {
                                // not a slope, check the tile normally.
                                slopeCheck = true;
                            }
                            else if (isFalling)
                            {
                                // Always ignore slopes when falling, the slope logic above will handle it.
                                slopeCheck = false;
                            }
                            else
                            {
                                // You're moving up and the tile is a slope. We'll consider it as a fully blocking
                                // tile if you were previously completely below it.
                                var collisionTop = this.WorldLocation.Y + this.collisionRectangle.Y;
                                var tileBottom = (y + 1) * TileMap.TileSize;
                                slopeCheck = collisionTop >= tileBottom;
                            }

                            // Sand in the game is funny. It only collides when you are falling. But we do check
                            // cells that may start slightly above your feet to help with walking into slopes. So make sure
                            // we only check sand that was below you.
                            var collideWithSandCell = isFalling && cell.IsSand;
                            if (collideWithSandCell)
                            {
                                // Don't check sand if it was below you on the previous frame
                                if (WorldLocation.Y > (TileMap.TileSize * y))
                                {
                                    collideWithSandCell = false;
                                }
                            }

                            if ((!cell.Passable && slopeCheck) || (isEnemyTileColliding && !cell.EnemyPassable) || collideWithSandCell)
                            {
                                // There was a collision with a non-slope tile, place the object to the edge of the tile.
                                if (isFalling)
                                {
                                    // Moving down
                                    var bottomY = this.worldLocation.Y + collisionRectangle.Y + collisionRectangle.Height;
                                    float distanceToTile = (TileMap.TileSize * y) - bottomY;
                                    yToMove = Math.Min(yToMove, distanceToTile);

                                    if (previouslyOnSlope && distanceToTile <= 12f)
                                    {
                                        // They were on a slope and now they are within a few pixels of a flat
                                        // tile. Treat it as if they were on a slope still and lock them to the
                                        // flat tile. This prevents them from being airborne for a bit if they are moving 
                                        // quickly down a steep slope.
                                        yToMove = distanceToTile;
                                    }

                                    if (yToMove == distanceToTile)
                                    {
                                        onGround = true;
                                        velocity.Y = 0;
                                        if (!previouslyOnGround)
                                        {
                                            Landed = true;
                                            LandingVelocity = Math.Max(LandingVelocity, this.velocity.Y);
                                        }
                                    }

                                }
                                else
                                {
                                    // Moving up.
                                    float collisionTop = this.WorldLocation.Y + this.collisionRectangle.Y;
                                    float distanceToTile = collisionTop - ((y + 1) * TileMap.TileSize);
                                    yToMove = Math.Max(yToMove, -distanceToTile);

                                    if (yToMove == -distanceToTile)
                                    {
                                        onCeiling = true;
                                        velocity.Y = 0;
                                    }
                                }

                            }

                        }
                    }
                }

                moveAmount.Y = yToMove;
            }

            // Check custom collision objects
            newPosition = worldLocation + moveAmount;
            afterMoveRect = getCollisionRectangleForPosition(ref newPosition);

            foreach (var collisionObject in Game1.CustomCollisionObjects)
            {
                
                if (!collisionObject.DoesCollideWithObject(this))
                {
                    continue;
                }

                // If they aren't in the same horizontal space, don't check vertical
                if (afterMoveRect.Left >= collisionObject.CollisionRectangle.Right || afterMoveRect.Right <= collisionObject.CollisionRectangle.Left)
                {
                    continue;
                }

                if (isFalling)
                {
                    float beforeMoveBottom = this.WorldLocation.Y + collisionRectangle.Y + collisionRectangle.Height;
                    float afterMoveBottom = beforeMoveBottom + moveAmount.Y;
                    var topOfObject = collisionObject.CollisionRectangle.Top;

                    if (beforeMoveBottom <= topOfObject && afterMoveBottom > topOfObject)
                    {
                        var distanceToObject = topOfObject - beforeMoveBottom;
                        moveAmount.Y = distanceToObject;
                        onGround = true;
                        velocity.Y = 0;
                        if (!previouslyOnGround)
                        {
                            Landed = true;
                            LandingVelocity = Math.Max(LandingVelocity, this.velocity.Y);
                        }
                    }
                }
                else
                {
                    // Moving up.
                    float beforeMoveTop = this.WorldLocation.Y + collisionRectangle.Y;
                    float afterMoveTop = beforeMoveTop + moveAmount.Y;
                    var bottomOfObject = collisionObject.CollisionRectangle.Bottom;
                    if (beforeMoveTop >= bottomOfObject && afterMoveTop < bottomOfObject)
                    {
                        var distanceToObject = bottomOfObject - beforeMoveTop;
                        moveAmount.Y = distanceToObject;
                        onCeiling = true;
                        this.velocity.Y = 0;
                    }
                }
            }

            // Test platforms. We'll reassign PlatformThatThisIsOn here. Moving platforms should be updated before GameObjects
            // and are responsible for moving the GameObjects that are on them. Jumping down through platforms is handled by 
            // the player using PoisonPlatforms.
            if (IsAffectedByGravity && IsAffectedByPlatforms && isFalling)
            {

                PlatformThatThisIsOn = null;

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

                    // fudge some numbers to account for rounding errors since we intermix floats and 
                    // rectangles that round down to ints.
                    const float fudgePixels = 2f;

                    // Was the platform below the player before movement?
                    var wasPlatformBelowMe = platform.CollisionRectangle.X <= currentPositionRect.Right
                        && platform.CollisionRectangle.Right >= currentPositionRect.Left
                        && platform.CollisionRectangle.Top > (currentPositionRect.Bottom - fudgePixels);

                    if (wasPlatformBelowMe)
                    {
                        float topOfPlatform = platform.WorldLocation.Y + platform.collisionRectangle.Y;
                        float bottomY = this.worldLocation.Y + collisionRectangle.Y + collisionRectangle.Height;
                        float distanceToPlatform = topOfPlatform - bottomY;

                        // You're considered on the platform if you are within movement distance a pixel of it.
                        if (distanceToPlatform <= moveAmount.Y || distanceToPlatform <= fudgePixels)
                        {
                            // If a new platform was hit, adjust the position.
                            moveAmount.Y = Math.Min(moveAmount.Y, distanceToPlatform);
                            
                            PlatformThatThisIsOn = platform;
                            onGround = true;
                            OnPlatform = true;
                            velocity.Y = 0;
                            
                            if (!previouslyOnGround)
                            {
                                Landed = true;
                                LandingVelocity = Math.Max(LandingVelocity, this.velocity.Y);
                            }

                            // Of course they could be on multiple platforms, but we'll just consider the first one they interact with.
                            break;
                        }
                    }

                }
            }

            // Check Spring Boards.
            if (IsAffectedByGravity && IsAffectedByPlatforms && PlatformThatThisIsOn == null && !OnGround)
            {
                foreach (var springBoard in Game1.SpringBoards)
                {
                    if (!springBoard.Enabled)
                    {
                        continue;
                    }

                    var wasAbove = currentPositionRect.Bottom <= springBoard.TopHeight;
                    var nowBelow = afterMoveRect.Bottom + 2f /*fudge*/ >= springBoard.TopHeight;

                    if (!springBoard.IsPickedUp && this != springBoard && isFalling && afterMoveRect.X <= springBoard.CollisionRectangle.Right && afterMoveRect.Right >= springBoard.CollisionRectangle.X && wasAbove && nowBelow)
                    {
                        springBoard.GameObjectOnMe = this;

                        // The springboard will adjust the height of the gameObject on it when it gets updated
                        // after this.

                        onGround = true;
                        velocity.Y = 0;

                        if (!previouslyOnGround)
                        {
                            Landed = true;
                            LandingVelocity = Math.Max(LandingVelocity, this.velocity.Y);
                        }

                    }
                    else
                    {
                        if (springBoard.GameObjectOnMe == this)
                        {
                            springBoard.GameObjectOnMe = null;

                            // If they're moving up give them a little boost
                            if (velocity.Y < 0)
                            {
                                this.velocity.Y -= 200 * springBoard.Compression;
                            }
                        }
                    }

                }
            }

            Landed = Landed && moveAmount.Y > 0;

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

        /// <summary>
        /// Moves an object left/right/up/down to avoid colliding iwht other objects. Ignores velocity. This is for when an object wasn't tile 
        /// blocking but then becomes that way. We don't want it to wark into walls and such and will prefer to just make sure it's not colliding.
        /// </summary>
        protected void MoveToIgnoreCollisions()
        {
            // Run through the four corners, if any of them are not tile blocked, move the object slightly in the direction of not being blocked.
            // if all corners are free we are good, if all corners are blocked, the object must be disabled. Sorry!
            bool allCornersAreFree = true;
            bool allCornersAreBlocked = true;

            var velocityAwayFromBlocked = Vector2.Zero;

            var topLeftCell = Game1.CurrentMap.GetMapSquareAtPixel(this.CollisionRectangle.Left, this.CollisionRectangle.Top);
            if (topLeftCell == null || !topLeftCell.Passable)
            {
                allCornersAreFree = false;
            }
            else
            {
                allCornersAreBlocked = false;
                velocityAwayFromBlocked += new Vector2(1, 1);
            }

            var topRightCell = Game1.CurrentMap.GetMapSquareAtPixel(this.CollisionRectangle.Right, this.CollisionRectangle.Top);
            if (topRightCell == null || !topRightCell.Passable)
            {
                allCornersAreFree = false;
            }
            else
            {
                allCornersAreBlocked = false;
                velocityAwayFromBlocked += new Vector2(-1, 1);
            }

            var bottomLeftCell = Game1.CurrentMap.GetMapSquareAtPixel(this.CollisionRectangle.Left, this.CollisionRectangle.Bottom);
            if (bottomLeftCell == null || !bottomLeftCell.Passable)
            {
                allCornersAreFree = false;
            }
            else
            {
                allCornersAreBlocked = false;
                velocityAwayFromBlocked += new Vector2(1, -1);
            }

            var bottomRightCell = Game1.CurrentMap.GetMapSquareAtPixel(this.CollisionRectangle.Right, this.CollisionRectangle.Bottom);
            if (bottomRightCell == null || !bottomRightCell.Passable)
            {
                allCornersAreFree = false;
            }
            else
            {
                allCornersAreBlocked = false;
                velocityAwayFromBlocked += new Vector2(-1, -1);
            }

            if (allCornersAreFree)
            {
                return;
            }

            if (allCornersAreBlocked)
            {
                this.Enabled = false;
            }

            var oldVelocity = this.Velocity;

            Vector2 moveAmount = velocityAwayFromBlocked;
            if (isTileColliding)
            {
                moveAmount = horizontalCollisionTest(moveAmount);
                moveAmount = verticalCollisionTest(moveAmount);
            }

            // Move them out of harm's way.
            this.worldLocation += moveAmount;

            // preserve the old velocity in case the collision tests changed it.
            this.velocity = oldVelocity;
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

            if (isTileColliding)
            {
                moveAmount = horizontalCollisionTest(moveAmount);
                moveAmount = verticalCollisionTest(moveAmount);
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

            DisplayComponent.Update(gameTime, elapsed);
        }

        public virtual void AdjustPositionBeforeDraw(ref Vector2 newPosition, ref Vector2 previousLocation) { }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (Enabled)
            {
                this.DisplayComponent.Draw(spriteBatch, this.WorldLocation, this.Flipped);
            }

            // Draw Collision Rectangle in reddish
            if (DrawCollisionRect || Game1.DrawAllCollisionRects && !collisionRectangle.IsEmpty)
            {
                Color color = Color.Red * 0.25f;
                spriteBatch.Draw(Game1.TileTextures, CollisionRectangle, Game1.WhiteSourceRect, color);
            }

            // Draw a square at the GameObjects location
            if (DrawLocation || Game1.DrawAllCollisionRects)
            {
                var squareSize = 4;
                var offset = squareSize / 2;
                var location = WorldLocation;

                // Draw location in green
                spriteBatch.Draw(Game1.TileTextures, new Rectangle((location.X - offset).ToInt(), (location.Y - offset).ToInt(), squareSize, squareSize), Game1.WhiteSourceRect, Color.Green);

                location = WorldCenter;

                // Draw world center in Yellow
                spriteBatch.Draw(Game1.TileTextures, new Rectangle((location.X - offset).ToInt(), (location.Y - offset).ToInt(), squareSize, squareSize), Game1.WhiteSourceRect, Color.Yellow);
            }
        }

    }
}
