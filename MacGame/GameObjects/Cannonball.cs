﻿using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
{
    public class Cannonball : PickupObject
    {

        public override float Friction => 2f;
        Cannon CannonHoldingMe;

        // True for the initial shot out of hte cannon until it hits a wall or something.
        public bool IsShootingOutOfCannon = false;

        /// <summary>
        /// Check for collisions with break bricks from shot out of a cannon until it hits the ground.
        /// </summary>
        private bool CanSmashBreakBricks = false;

        public Cannonball(ContentManager content, int x, int y, Player player) : base(content, x, y, player)
        {
            this.DisplayComponent = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), Helpers.GetTileRect(4, 7)); ;

            Enabled = true;

            WorldLocation = new Vector2(x * TileMap.TileSize + TileMap.TileSize / 2, (y + 1) * TileMap.TileSize);
            IsAffectedByGravity = true;

            this.SetWorldLocationCollisionRectangle(8, 8);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (CannonHoldingMe == null && !IsPickedUp)
            {
                foreach (var gameObject in Game1.CurrentLevel.GameObjects)
                {
                    if (gameObject is Cannon)
                    {
                        var cannon = (Cannon)gameObject;
                        if (cannon.CanAcceptCannonball())
                        {
                            if (cannon.CollisionRectangle.Contains(this.WorldCenter))
                            {
                                // Enter cannon.
                                this.Enabled = false;
                                CannonHoldingMe = cannon;
                                cannon.LoadCannonball(this);
                                IsShootingOutOfCannon = false;
                                CanSmashBreakBricks = false;
                                break;
                            }
                        }
                    }
                }
            }

            base.Update(gameTime, elapsed);

            if (OnLeftWall || OnGround || OnRightWall || OnCeiling)
            {
                IsShootingOutOfCannon = false;
            }



            if (CanSmashBreakBricks)
            {
                if (OnLeftWall)
                {
                    var toTheLeft = this.CollisionRectangle;
                    toTheLeft.X -= toTheLeft.Width;
                    SmashAnyBricks(toTheLeft);
                }
                if (OnRightWall)
                {
                    var toTheRight = this.CollisionRectangle;
                    toTheRight.X += toTheRight.Width;
                    SmashAnyBricks(toTheRight);
                }
                if (OnCeiling)
                {
                    var above = this.CollisionRectangle;
                    above.Y -= above.Height;
                    SmashAnyBricks(above);
                }
                if (OnGround)
                {
                    var below = this.CollisionRectangle;
                    below.Y += below.Height;
                    SmashAnyBricks(below);
                }
            }

            if (IsPickedUp || CannonHoldingMe != null || this.Velocity.Length() < 100)
            {
                CanSmashBreakBricks = false;
            }

        }

        public void SmashAnyBricks(Rectangle collisionRectangle)
        {
            //var collisionRectangle = new Rectangle(this.CollisionRectangle.X - 8, this.CollisionRectangle.Y - 8, this.CollisionRectangle.Width + 16, this.CollisionRectangle.Height + 16);
            foreach (var gameObject in Game1.CurrentLevel.GameObjects)
            {
                if (gameObject is BreakBrick)
                {
                    var breakBrick = (BreakBrick)gameObject;
                    if (breakBrick.Enabled && breakBrick.CollisionRectangle.Intersects(collisionRectangle))
                    {
                        Game1.CurrentLevel.BreakBricks(breakBrick.GroupName);
                    }
                }
            }
        }

        public void ShootOutOfCannon(Vector2 velocity)
        {
            this.Enabled = true;
            this.velocity = velocity;
            this.WorldLocation = CannonHoldingMe.WorldLocation;
            CannonHoldingMe.CannonballInside = null;
            CannonHoldingMe = null;
            IsShootingOutOfCannon = true;
            CanSmashBreakBricks = true;
        }
    }

}
