﻿using System;
using System.Collections.Generic;
using System.Linq;
using MacGame.DisplayComponents;
using MacGame.Items;
using MacGame.Platforms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Enemies
{
    /// <summary>
    /// Sort of looks like the first boss from R-Type, but that's just a coincidence.
    /// </summary>
    public class OurTypeOfBoss : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        /// <summary>
        /// Don't do anything until you've been seen.
        /// </summary>
        private bool _hasBeenSeen = false;

        float openAndCloseMouthTimer = 0;
        float openAndCloseMouthTimerGoal = 2.5f;

        private Player _player;

        private int MaxHealth = 50;

        private bool isInitialized = false;

        private OurTypeOfBossHead _head;
        private List<OurTypeOfBossTailPiece> _tailPieces = new List<OurTypeOfBossTailPiece>();

        Waypoint tailStart;
        Waypoint tailMidway;
        Waypoint tailEnd;
        Waypoint nextTailWaypoint;
        bool isWaypointComingFromStart = true;
        float middleOffset = 0f;
        float offsetMoveSpeed = 60f;
        float maxMiddleOffset = 70f;

        List<Rectangle> collisionRectangles = new List<Rectangle>();

        public OurTypeOfBoss(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            _player = player;

            isEnemyTileColliding = false;
            IsAbleToMoveOutsideOfWorld = true;
            isTileColliding = false;
            IsAbleToSurviveOutsideOfWorld = true;
            IsAffectedByForces = false;
            IsAffectedByGravity = false;
            IsAffectedByPlatforms = false;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = true;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\MegaTextures");
            
            var idle = new AnimationStrip(textures, Helpers.GetMegaTileRect(2, 3), 1, "idle");
            idle.LoopAnimation = false;
            idle.FrameLength = 0.15f;
            animations.Add(idle);

            var closeMouth = new AnimationStrip(textures, Helpers.GetMegaTileRect(0, 3), 3, "closeMouth");
            closeMouth.LoopAnimation = false;
            closeMouth.FrameLength = 0.15f;
            closeMouth.Oscillate = true;
            animations.Add(closeMouth);

            var openMouth = (AnimationStrip)closeMouth.Clone();
            openMouth.Reverse = true;
            openMouth.Name = "openMouth";
            animations.Add(openMouth);

            animations.Play("idle");

            isEnemyTileColliding = false;
            Attack = 1;
            Health = MaxHealth;

            IsAffectedByGravity = false;

            SetWorldLocationCollisionRectangle(8, 8);

            _head = new OurTypeOfBossHead(content, cellX, cellY, player, camera, this);
            _head.WorldLocation = this.WorldCenter + new Vector2(16, 40);
            Vector2 initialTailSpot = this.WorldLocation + new Vector2(-40, 0);
            for (int i = 0; i < 10; i++)
            {
                var tailPiece = new OurTypeOfBossTailPiece(content, cellX, cellY, player, camera);
                _tailPieces.Add(tailPiece);
                tailPiece.WorldLocation = initialTailSpot;
                initialTailSpot.X -= 28;
            }

            // The end of the tail will move between 3 waypoints and the rest of the
            // tail will wag between that.
            tailStart = new Waypoint(_tailPieces.Last().WorldCenter);
            tailMidway = new Waypoint(tailStart.CenterLocation + new Vector2(40, -74));
            tailEnd = new Waypoint(tailMidway.CenterLocation + new Vector2(20, -74));
            nextTailWaypoint = tailMidway;

            ExtraEnemiesToAddAfterConstructor.AddRange(_tailPieces);
            ExtraEnemiesToAddAfterConstructor.Add(_head);

            InvincibleTimeAfterBeingHit = 0.1f;
        }

        private void Initialize()
        {
            _head.SetDrawDepth(this.DrawDepth + Game1.MIN_DRAW_INCREMENT);

            for (int i = 0; i < _tailPieces.Count; i++)
            {
                _tailPieces[i].SetDrawDepth(this.DrawDepth + ((i + 1) * Game1.MIN_DRAW_INCREMENT));
            }

            isInitialized = true;

            // Set up custom collion rectangles
            collisionRectangles.Add(new Rectangle((int)this.worldLocation.X - 8, (int)this.worldLocation.Y - 200, 100, 200));
            collisionRectangles.Add(new Rectangle((int)this.worldLocation.X - 100, (int)this.worldLocation.Y - 220, 125, 50));
            collisionRectangles.Add(new Rectangle((int)this.worldLocation.X - 75, (int)this.worldLocation.Y - 250, 100, 30));
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!isInitialized)
            {
                Initialize();
            }

            if (!_hasBeenSeen)
            {
                if (Game1.Camera.IsPointVisible(new Vector2(this.CollisionRectangle.Right, this.CollisionRectangle.Center.Y)))
                {
                    _hasBeenSeen = true;
                }
                else
                {
                    return;
                }
            }

            openAndCloseMouthTimer += elapsed;
            if (openAndCloseMouthTimer > openAndCloseMouthTimerGoal)
            {
                openAndCloseMouthTimer = 0;
                animations.Play("openMouth").FollowedBy("closeMouth");
            }

            Game1.DrawBossHealth = true;
            Game1.MaxBossHealth = MaxHealth;
            Game1.BossHealth = Health;
            Game1.BossName = "Grok";

            _tailPieces.Last().GoToWaypoint(30, nextTailWaypoint);
            if (_tailPieces.Last().IsAtWaypoint(nextTailWaypoint))
            {
                if (nextTailWaypoint == tailStart)
                {
                    isWaypointComingFromStart = true;
                    nextTailWaypoint = tailMidway;
                }
                else if (nextTailWaypoint == tailMidway)
                {
                    if (isWaypointComingFromStart)
                    {
                        nextTailWaypoint = tailEnd;
                    }
                    else
                    {
                        nextTailWaypoint = tailStart;
                    }
                }
                else
                {
                    isWaypointComingFromStart = false;
                    nextTailWaypoint = tailMidway;
                }
            }

            // Lerp the tail pieces between the first and loast locations
            for (int i = 1; i < _tailPieces.Count - 1; i++)
            {
                float percentage = (float)(i ) / (float)(_tailPieces.Count - 1);  
                Vector2 location = Vector2.Lerp(_tailPieces.First().WorldLocation, _tailPieces.Last().WorldLocation, percentage);
                _tailPieces[i].WorldLocation = location;
            }


            middleOffset += elapsed * offsetMoveSpeed;
            if (middleOffset > maxMiddleOffset && offsetMoveSpeed > 0)
            {
                offsetMoveSpeed *= -1;
            }
            else if (middleOffset < -maxMiddleOffset && offsetMoveSpeed < 0)
            {
                offsetMoveSpeed *= -1;
            }

            // Then wag the center pieces up more than the end pieces
            for (int i = 1; i < _tailPieces.Count - 1; i++)
            {
                // how close is it to the center piece, get a number 0 to 1
                var centerIndex = _tailPieces.Count / 2;
                var distanceFromCenter = Math.Abs(centerIndex - i);
                var percentage = 1 - ((float)distanceFromCenter / (float)(centerIndex));

                // convert the percentage into a smooth curve using half of a sin wave
                //var offset = (float)Math.Sin(percentage * MathHelper.PiOver2);

                _tailPieces[i].WorldLocation = new Vector2(_tailPieces[i].WorldLocation.X, _tailPieces[i].WorldLocation.Y + percentage * middleOffset);
            }

            //// Move the tail pieces up and down in a sign wave
            //for (int i = 0; i < _tailPieces.Count; i++)
            //{
            //    var yPositionOffset = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds + (i / 2f)) * 15;
            //    _tailPieces[i].WorldLocation = new Vector2(_tailPieces[i].WorldLocation.X, this.WorldLocation.Y + yPositionOffset);
            //}

            // Check custom rectangle collisions
            foreach (var rect in collisionRectangles)
            {
                if (rect.Intersects(_player.CollisionRectangle))
                {
                    _player.TakeHit(this);
                }

                foreach (var shot in _player.Shots.RawList)
                {
                    if (shot.Enabled && shot.CollisionRectangle.Intersects(rect))
                    {
                        shot.Break();
                    }
                }
            }

            // Update head and tail
            _head.Update(gameTime, elapsed);
            foreach (var tailPiece in _tailPieces)
            {
                tailPiece.Update(gameTime, elapsed);
            }

            base.Update(gameTime, elapsed);

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // draw head and tail
            _head.Draw(spriteBatch);
            foreach (var tailPiece in _tailPieces)
            {
                tailPiece.Draw(spriteBatch);
            }

        

            // Draw Collision Rectangle in reddish
            if (DrawCollisionRect || Game1.DrawAllCollisionRects)
            {
                Color color = Color.Red * 0.25f;

                foreach (var rectangle in collisionRectangles)
                {
                    spriteBatch.Draw(Game1.TileTextures, rectangle, Game1.WhiteSourceRect, color);
                }

                
            }

            base.Draw(spriteBatch);
        }
    }
}