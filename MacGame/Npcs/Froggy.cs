using MacGame.DisplayComponents;
using MacGame.Enemies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using System.Linq;
using System.IO;

namespace MacGame.Npcs
{
    public class Froggy : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public Waypath? RacePath { get; set; }

        private enum State
        {
            IdleStart,
            Racing,
            IdleEnd
        }

        private enum Speed
        {
            Slow,
            Medium,
            Fast
        }
        private enum LastRaceResult
        {
            DidNotRaceYet,
            FroggyWon,
            MacWon
        }

        private State _state = State.IdleStart;
        private Speed _speed = Speed.Slow;
        private LastRaceResult _result = LastRaceResult.DidNotRaceYet;

        private List<ConversationChoice> raceChoices;

        private Rectangle _raceVictoryZone { get; set; }

        /// <summary>
        /// Tracks whether or not you already spoke to Froggy so you can get a shorter message next time.
        /// </summary>
        private bool _hasSpoken;

        List<string> boasts;

        float moveSpeed = 200f;

        private Vector2 _startLocation;
        private Rectangle _startCollisionRect;

        public Froggy(ContentManager content, int cellX, int cellY, Player player, Camera camera) 
            : base(content, cellX, cellY, player, camera)
        {

            Enabled = true;

            _startLocation = this.WorldLocation;
            _startCollisionRect = this.CollisionRectangle;

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

            raceChoices = new List<ConversationChoice>();
            raceChoices.Add(new ConversationChoice("Yes", () =>
            {
                _state = State.Racing;
                animations.Play("walk");
            }));
            raceChoices.Add(new ConversationChoice("No", () =>
            {
                // Do nothing;
            }));

            boasts = new List<string>()
            {
                "Too slow buddy. They should call you Molasses Mac.",
                "I've seen dead slugs move faster than that.",
                "I'm as fast as the wind, you're as slow as a fart.",
                "You're not frog enough to race me.",
                "Handsome frogs never lose a race.",
                "I'd say slow oooooooooooiorooodddpoke but you're more of a slow joke.",
                "You're as slow as a sloth stuck in quicksand.",
                "Man, you really need to work on your speed.",
                "You can't teach speed.",
                "Practice running fast and come back.",
                "You're like a snail and I'm like a racing snail"
            };
        }

        public void SetVictoryZone(Rectangle rectangle)
        {
            this._raceVictoryZone = rectangle;
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

            if (_raceVictoryZone == Rectangle.Empty)
            {
                // You need to add a rectangle GameObject to the map with the
                // name "RaceVictoryZone". Forggy's final waypoint should be inside it.
                throw new Exception("Race victory zone wasn't set.");
            }
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (_state == State.Racing)
            {
                if (RacePath == null)
                {
                    InitializeRacePath();
                }

                if (RacePath!.Waypoints.Any())
                {
                    var nextWaypoint = RacePath.Waypoints.First();

                    // As you hit waypoints look for the next one. Until the last one, we'll go to that until we stop moving.
                    if (Vector2.Distance(this.WorldCenter, nextWaypoint.Location) <= Game1.TileSize && RacePath.Waypoints.Count > 1)
                    {
                        // Remove the waypoints as we hit them.
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
                        this.velocity.X = moveSpeed;

                        if (onGround && animations.CurrentAnimationName != "walk")
                        {
                            animations.Play("walk");
                        }
                        this.Flipped = false;
                    }
                    else if (nextWaypoint.Location.X <= this.CollisionRectangle.Left)
                    {
                        this.velocity.X = -moveSpeed;
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
                    var tileAtHead = Game1.CurrentMap?.GetMapSquareAtPixel(this.WorldCenter.X.ToInt(), this.CollisionRectangle.Top);
                    var tileAtFeet = Game1.CurrentMap?.GetMapSquareAtPixel(this.WorldLocation);
                    var onLadder = (tileAtHead != null && tileAtHead.IsLadder) || (tileAtFeet != null && tileAtFeet.IsLadder);
                    if (onLadder)
                    {
                        if (animations.CurrentAnimationName != "climb")
                        {
                            animations.Play("climb");
                        }
                        this.IsAffectedByGravity = false;

                        // Move towards the center of the ladder
                        var targetX = ((int)this.WorldLocation.X / Game1.TileSize * Game1.TileSize) + (Game1.TileSize / 2);

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
                            this.velocity.Y = moveSpeed;
                        }
                        else if (nextWaypoint.Location.Y < this.CollisionCenter.Y)
                        {
                            this.velocity.Y = -moveSpeed;
                        }
                    }
                    else
                    {
                        this.IsAffectedByGravity = true;
                    }
                }
                else
                {
                    // No more waypoints
                    _state = State.IdleEnd;
                }

                // Check if someone won.
                if (_result == LastRaceResult.DidNotRaceYet)
                {
                    if (_raceVictoryZone.Contains(this.WorldLocation))
                    {
                        _result = LastRaceResult.FroggyWon;
                        _hasSpoken = false;
                    }
                    else if (_raceVictoryZone.Contains(Game1.Player.WorldLocation))
                    {
                        _result = LastRaceResult.MacWon;
                        _hasSpoken = false;
                    }
                }

                if (OnGround && this.Velocity == Vector2.Zero && RacePath.Waypoints.Count == 1)
                {
                    // we hit the last waypoint
                    animations.Play("idle");
                    _state = State.IdleEnd;
                    _hasSpoken = false;
                }
            }
            else if (_state == State.IdleEnd)
            {
                animations.Play("idle");
                this.velocity = Vector2.Zero;
                IsAffectedByGravity = true;

                if (_hasSpoken && Game1.Camera.IsWayOffscreen(this.CollisionRectangle) && Game1.Camera.IsWayOffscreen(this._startCollisionRect))
                {
                    // If both the start location and the current frog are off camera, put the frog back.
                    this.WorldLocation = _startLocation;
                    _state = State.IdleStart;
                    _result = LastRaceResult.DidNotRaceYet;
                    RacePath = null;
                }
            }

            base.Update(gameTime, elapsed);
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(7, 0);

        public override void InitiateConversation()
        {
            if (_state == State.IdleStart)
            {
                if (!_hasSpoken)
                {
                    _hasSpoken = true;
                    ConversationManager.AddMessage("I'm fast.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                    ConversationManager.AddMessage("Hi Fast, I'm Mac.", PlayerConversationRectangle, ConversationManager.ImagePosition.Left);
                    ConversationManager.AddMessage("What? My name is Froggy, and I'm the fastest Frog in America. Honestly, you look ridiculous and slow.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                }
                ConversationManager.AddMessage("Want to race?", ConversationSourceRectangle, ConversationManager.ImagePosition.Right, raceChoices);
            }
            else if (_state == State.IdleEnd)
            {
                _hasSpoken = true;
                if (_result == LastRaceResult.FroggyWon)
                {
                    // Pick a random boast.
                    var boastIndex = Game1.Randy.Next(0, boasts.Count);
                    ConversationManager.AddMessage(boasts[boastIndex], ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                }
                else
                {
                    // Mac won!
                    ConversationManager.AddMessage("No fair! I wasn't going my fastest. Come try that again and see what happens buddy.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
           
            if (Game1.DrawAllCollisisonRects)
            {
                // Draw the waypoints for debugging
                foreach (var waypoint in Game1.CurrentLevel.Waypoints)
                {
                    spriteBatch.Draw(Game1.TileTextures, new Rectangle((int)waypoint.Location.X - 2, (int)waypoint.Location.Y - 2, 4, 4), Game1.WhiteSourceRect, Color.Red);
                }
            }

            base.Draw(spriteBatch);
        }
    }
}
