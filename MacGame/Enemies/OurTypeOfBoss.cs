using System;
using System.Collections.Generic;
using System.Linq;
using MacGame.DisplayComponents;
using MacGame.Items;
using MacGame.Platforms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using static MacGame.Enemies.Blowfish;

namespace MacGame.Enemies
{
    public enum OurTypeOfBossState
    {
        /// <summary>
        /// Don't do anything until you've been seen.
        /// </summary>
        Unseen,
        Alive,
        /// <summary>
        /// Dying is two states, first his tail starts blowing up like a bomb towards the body.
        /// </summary>
        DyingTailBlowingUp,
        /// <summary>
        /// Second state of dying afer the tail is blown up and the whole body starts exploding.
        /// </summary>
        DyingBodyBlowingUp,
        Dead
    }

    /// <summary>
    /// Sort of looks like the first boss from R-Type, but that's just a coincidence.
    /// </summary>
    public class OurTypeOfBoss : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        OurTypeOfBossState _state = OurTypeOfBossState.Unseen;

        float openAndCloseMouthTimer = 0;
        float openAndCloseMouthTimerGoal = 2.5f;

        private Player _player;

        private int MaxHealth = 100;
        //private int MaxHealth = 3;

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

        List<SpaceEyeball> spaceEyeballs = new List<SpaceEyeball>();
        List<SpaceMouth> spaceMouths = new List<SpaceMouth>();

        float sendEnemyTimer = 0f;
        float sendEnemyTimerGoal = 1f;

        OurTypeOfBossShot _shot;
        float shotTimer = 0f;
        float shotTimerGoal = 4f;
        private float explosionTimer;
        private Rectangle explosionRectangle;
        private float dyingTimer;
        private float dyingTimerGoal = 4f;

        /// <summary>
        /// After death reveal the sock.
        /// </summary>
        private Sock Sock;

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
            for (int i = 0; i < 12; i++)
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

            InvincibleTimeAfterBeingHit = 0f;
            
            // Hack, but the last tail pieces moves around and the other pieces kind of follow but wave around and such, 
            // so they can't help but get out of sync a bit. And for that reason we hide the last tail piece and make it not
            // collide. It makes the tail look smoother.
            _tailPieces.Last().CollisionRectangle = new Rectangle();
            _tailPieces.Last().Attack = 0;
            _tailPieces.Last().DisplayComponent = new NoDisplay();

            // He'll randomly send some mouths and eyeballs at you too.
            for (int i = 0; i < 10; i++)
            {
                var eyeball = new SpaceEyeball(content, cellX, cellY, player, camera);
                eyeball.Enabled = false;
                spaceEyeballs.Add(eyeball);
                AddEnemyInConstructor(eyeball);
            }

            for (int i = 0; i < 10; i++)
            {
                var mouth = new SpaceMouth(content, cellX, cellY, player, camera);
                mouth.Enabled = false;
                spaceMouths.Add(mouth);
                AddEnemyInConstructor(mouth);
            }

            _shot = new OurTypeOfBossShot(content, cellX, cellY, player, camera);
            _shot.Enabled = false;
            AddEnemyInConstructor(_shot);
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

            explosionRectangle = new Rectangle((int)this.worldLocation.X - 100, (int)this.worldLocation.Y - 250, 200, 250);

            foreach (var item in Game1.CurrentLevel.Items)
            {
                if (item is Sock)
                {
                    Sock = (Sock)item;
                }
            }

            if (Sock == null)
            {
                throw new Exception("You need a sock in the level!");
            }

            Sock.Enabled = false;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!isInitialized)
            {
                Initialize();
            }

            if (_state == OurTypeOfBossState.Unseen)
            {
                // Go by the last tail piece because it'll be the first thing visible.
                var lastTailPiece = _tailPieces.Last();
                if (Game1.Camera.IsPointVisible(new Vector2(lastTailPiece.CollisionRectangle.Left - 10, lastTailPiece.CollisionRectangle.Center.Y)))
                {
                    _state = OurTypeOfBossState.Alive;
                }
                else
                {
                    return;
                }
            }

            Game1.DrawBossHealth = true;
            Game1.MaxBossHealth = MaxHealth;
            Game1.BossHealth = Health;
            Game1.BossName = "Grok";

            if (_state == OurTypeOfBossState.Alive)
            {
                openAndCloseMouthTimer += elapsed;
                if (openAndCloseMouthTimer > openAndCloseMouthTimerGoal)
                {
                    openAndCloseMouthTimer = 0;
                    animations.Play("openMouth").FollowedBy("closeMouth");
                }

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
                    float percentage = (float)(i) / (float)(_tailPieces.Count - 1);
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
                for (int i = 0; i < _tailPieces.Count - 1; i++)
                {
                    // how close is it to the center piece, get a number 0 to 1
                    var centerIndex = _tailPieces.Count / 2;
                    var distanceFromCenter = Math.Abs(centerIndex - i);
                    var percentage = 1 - ((float)distanceFromCenter / (float)(centerIndex));

                    // This smooths out the curve of the tail.
                    percentage = (float)Math.Sin(percentage * MathHelper.PiOver2);

                    // convert the percentage into a smooth curve using half of a sin wave
                    //var offset = (float)Math.Sin(percentage * MathHelper.PiOver2);

                    _tailPieces[i].WorldLocation = new Vector2(_tailPieces[i].WorldLocation.X, _tailPieces[i].WorldLocation.Y + percentage * middleOffset);
                }

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

                sendEnemyTimer += elapsed;
                if (sendEnemyTimer >= sendEnemyTimerGoal)
                {
                    sendEnemyTimer = 0;

                    var randomY = this.WorldLocation.Y + Game1.Randy.Next(-200, 32);

                    if (Game1.Randy.NextBool())
                    {
                        // Search for a disabled eyeball to reuse.
                        foreach (var eyeBall in spaceEyeballs)
                        {
                            if (!eyeBall.Enabled)
                            {
                                eyeBall.Reset();
                                eyeBall.WorldLocation = new Vector2(this.camera.ViewPort.Right + 100, randomY);
                                break;
                            }
                        }

                    }
                    else
                    {
                        // Search for a disabled mouth to reuse.
                        foreach (var mouth in spaceMouths)
                        {
                            if (!mouth.Enabled)
                            {
                                mouth.Reset();
                                mouth.WorldLocation = new Vector2(this.camera.ViewPort.Right + 100, randomY);
                                break;
                            }
                        }
                    }
                }

                // Disable eyes and mouths as they go off the left side of the screen or die.
                foreach (var eyeBall in spaceEyeballs)
                {
                    if (eyeBall.CollisionRectangle.Right + 10 < camera.ViewPort.Left)
                    {
                        eyeBall.Enabled = false;
                    }
                }

                foreach (var mouth in spaceMouths)
                {
                    if (mouth.Enabled && mouth.CollisionRectangle.Right + 10 < camera.ViewPort.Left)
                    {
                        mouth.Enabled = false;
                    }
                }

                // Shoot every so often
                shotTimer += elapsed;
                if (shotTimer >= shotTimerGoal)
                {
                    shotTimer = 0;
                    // Search for a disabled shot to reuse.
                    _shot.WorldLocation = this.WorldLocation + new Vector2(-78, -158);
                    _shot.Enabled = true;
                    // Shoot towards the player
                    var direction = _player.CollisionCenter - _shot.CollisionCenter;
                    direction.Normalize();
                    _shot.Velocity = direction * 300;
                }
            }

            base.Update(gameTime, elapsed);

            if (_state == OurTypeOfBossState.DyingBodyBlowingUp)
            {
                // random explosions
                explosionTimer += elapsed;
                if (explosionTimer >= 0.13f)
                {
                    explosionTimer = 0f;
                    // Get a random location over this collision rectangle
                    var randomX = Game1.Randy.Next(explosionRectangle.Width);
                    var randomY = Game1.Randy.Next(explosionRectangle.Height);

                    var randomLocation = new Vector2(explosionRectangle.X + randomX, explosionRectangle.Y + randomY);
                    EffectsManager.AddExplosion(randomLocation);
                }

                dyingTimer += elapsed;

                var deadPercentage = dyingTimer / dyingTimerGoal;
                DisplayComponent.TintColor = Color.Lerp(Color.White, Color.Transparent, deadPercentage);

                if (dyingTimer >= dyingTimerGoal)
                {
                    this.Kill();
                    _head.Kill();
                    _state = OurTypeOfBossState.Dead;

                    TimerManager.AddNewTimer(1f, () => { Sock.FadeIn(); });

                    // break these bricks so that Mac can leave.
                    Game1.CurrentLevel.BreakBricks("Puff");
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // draw head and tail
            //_head.Draw(spriteBatch);
            //foreach (var tailPiece in _tailPieces)
            //{
            //    tailPiece.Draw(spriteBatch);
            //}

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

        public override void TakeHit(GameObject attacker, int damage, Vector2 force)
        {
            Health -= damage;

            SoundManager.PlaySound("HitEnemy2");

            if (Health <= 0)
            {
                _state = OurTypeOfBossState.DyingTailBlowingUp;

                // Blow up the tail like a wick on a bomb.
                float tailExplosionTime = 0.2f;
                for (int i = _tailPieces.Count - 1; i >= 0; i--)
                {
                    var tailPiece = _tailPieces[i];
                    TimerManager.AddNewTimer(tailExplosionTime, () => { tailPiece.Kill(); });
                    tailExplosionTime += 0.2f;
                }

                TimerManager.AddNewTimer(tailExplosionTime, () => _state = OurTypeOfBossState.DyingBodyBlowingUp);

                _head.Kill();

                foreach(var eyeball in spaceEyeballs)
                {
                    eyeball.Kill();
                }

                foreach (var mouth in spaceMouths)
                {
                    mouth.Kill();
                }

                _shot.Kill();
            }
        }
    }
}