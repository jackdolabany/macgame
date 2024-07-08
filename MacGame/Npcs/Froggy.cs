using MacGame.Behaviors;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using TileEngine;

namespace MacGame.Npcs
{
    public class Froggy : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public Waypath RacePath { get; set; }

        public Froggy(ContentManager content, int cellX, int cellY, Player player, Camera camera) 
            : base(content, cellX, cellY, player, camera)
        {

            Enabled = true;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var idle = new AnimationStrip(textures, Helpers.GetTileRect(12, 9), 1, "idle");
            idle.LoopAnimation = false;
            idle.FrameLength = 0.2f;
            animations.Add(idle);

            var walk = new AnimationStrip(textures, Helpers.GetTileRect(12, 9), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.2f;
            animations.Add(walk);

            var climb = new AnimationStrip(textures, Helpers.GetTileRect(14, 9), 2, "climb");
            climb.LoopAnimation = true;
            climb.FrameLength = 0.2f;
            animations.Add(climb);

            var jump = new AnimationStrip(textures, Helpers.GetTileRect(12, 10), 1, "jump");
            jump.LoopAnimation = false;
            jump.FrameLength = 0.2f;
            animations.Add(jump);

            animations.Play("idle");

            SetCenteredCollisionRectangle(5, 7);

        }

        public void InitializeRacePath()
        {
            RacePath = new Waypath();

            var waypoints = Game1.CurrentLevel.Waypoints.ToList();

            Vector2 pointToStartFrom = this.CollisionCenter;

            // Order the waypoints by distance from the frog and then distance to each other.
            while (waypoints.Any())
            {
                var closestWaypoint = waypoints.OrderBy(w => Vector2.Distance(w.Location, pointToStartFrom)).First();
                RacePath.Waypoints.Add(closestWaypoint);
                waypoints.Remove(closestWaypoint);
                pointToStartFrom = closestWaypoint.Location;
            }
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (RacePath == null)
            {
                InitializeRacePath();
            }

            if (RacePath.Waypoints.Any())
            {
                var nextWaypoint = RacePath.Waypoints.First();

                if (Vector2.Distance(this.WorldCenter, nextWaypoint.Location) <= Game1.TileSize && RacePath.Waypoints.Count > 1)
                {
                    RacePath.Waypoints.Remove(nextWaypoint);
                    nextWaypoint = RacePath.Waypoints.First();
                }

                // Go to the next waypoint.
                


                Vector2 inFrontOfCenter = new Vector2(Game1.TileSize, 0);
                Vector2 inFrontBelow = new Vector2(0, collisionRectangle.Height / 2 + 2);
                if (Flipped)
                {
                    inFrontOfCenter.X *= -1;
                    inFrontBelow.X *= -1;
                }

                var tileInFront = Game1.CurrentMap?.GetMapSquareAtPixel(this.WorldCenter + inFrontOfCenter);
                var tileAtFrontBelow = Game1.CurrentMap?.GetMapSquareAtPixel(this.WorldCenter + inFrontBelow);

                if (nextWaypoint.Location.X >= this.CollisionRectangle.Right)
                {
                    this.velocity.X = 100f;

                    if (onGround && animations.CurrentAnimationName != "walk")
                    {
                        animations.Play("walk");
                    }
                    this.Flipped = false;
                }
                else if (nextWaypoint.Location.X <= this.CollisionRectangle.Left)
                {
                    this.velocity.X = -100f;
                    if (onGround && animations.CurrentAnimationName != "walk")
                    {
                        animations.Play("walk");
                    }
                    this.Flipped = true;
                }
                else
                {
                    this.velocity.X = 0;
                }

                // Jump before you walk into a wall.
                if (tileInFront != null && !tileInFront.Passable && OnGround && this.velocity.X != 0)
                {
                    this.velocity.Y -= 600;
                    animations.Play("jump");
                }

                // Jump before a cliff, unless the waypoint is below you.
                if (tileAtFrontBelow != null && tileAtFrontBelow.Passable && OnGround && nextWaypoint.Location.Y < this.CollisionRectangle.Bottom)
                {
                    this.velocity.Y -= 600;
                    animations.Play("jump");
                }

                // Ladder climbing.
                var tileAtHead = Game1.CurrentMap?.GetMapSquareAtPixel((int)this.WorldCenter.X, this.CollisionRectangle.Top);
                var tileAtFeet = Game1.CurrentMap?.GetMapSquareAtPixel(this.WorldLocation);
                var onLadder = (tileAtHead != null && tileAtHead.IsLadder) || (tileAtFeet != null && tileAtFeet.IsLadder);
                if (onLadder)
                {
                    animations.Play("climb");
                    this.IsAffectedByGravity = false;

                    // Move towards the center of the ladder
                    var targetX = ((int)(this.WorldLocation.X / Game1.TileSize)) * Game1.TileSize + Game1.TileSize / 2;

                    if (targetX > this.WorldLocation.X)
                    {
                        this.velocity.X = 20;
                    }
                    else if (targetX <= this.WorldLocation.X)
                    {
                        this.velocity.X = -20;
                    }

                    // Climb up or down.
                    if (nextWaypoint.Location.Y > this.CollisionCenter.Y)
                    {
                        this.velocity.Y = 100f;
                    }
                    else if (nextWaypoint.Location.Y < this.CollisionCenter.Y)
                    {
                        this.velocity.Y = -100f;
                    }
                }
                else
                {
                    this.IsAffectedByGravity = true;
                }

            }

            if (OnGround && this.Velocity == Vector2.Zero)
            {
                animations.Play("idle");
            }

            base.Update(gameTime, elapsed);
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(7, 0);

        public override void InitiateConversation()
        {
            ConversationManager.AddMessage("Ribbet", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
        }
    }
}
