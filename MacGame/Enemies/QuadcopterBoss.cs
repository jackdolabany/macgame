﻿using System;
using System.Collections.Generic;
using MacGame.DisplayComponents;
using MacGame.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class QuadcopterBoss : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        const int MaxHealth = 4;
        float explosionTimer = 0f;
        float dyingTimer = 0f;
        
        public enum QuadState
        {
            Attacking,
            Dying,
            Dead
        }

        public bool IsMovingToTargetLocation = true;

        public QuadState state = QuadState.Attacking;

        /// <summary>
        /// After death the boss will reveal a sock.
        /// </summary>
        private Sock Sock;

        private bool _isInitialized = false;

        /// <summary>
        /// The quadcopter bounces up and down a bit.
        /// </summary>
        public Vector2 bounceOffset;

        /// <summary>
        ///  The quadcopter will try to go here.
        /// </summary>
        private Vector2 currentTargetLocation;

        // He moves between these locations
        private Vector2 middleLocation;
        private Vector2 leftLocation;
        private Vector2 rightLocation;

        // When he turns red and attacks he'll move way off screen and then sweep over
        private Vector2 wayOffScreenLocation;
        private Vector2 sweepStartLocation;
        private Vector2 sweepEndLoaction;
        private float sweepStartX;
        private float sweepEndX;

        float moveTimer = 0f;

        /// <summary>
        /// Turns angry and invincible for a bit after being hit.
        /// </summary>
        bool isAngry;

        public List<Bomb> Bombs = new List<Bomb>();
        float bombTimer = 0f;

        /// <summary>
        /// Once this boss is dead and you have the sock we'll count this down and then exit the map.
        /// </summary>
        private float leaveLevelTimer = 4f;

        public QuadcopterBoss(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\MegaTextures");
            var flying = new AnimationStrip(textures, Helpers.GetMegaTileRect(0, 0), 2, "flying");
            flying.LoopAnimation = true;
            flying.Oscillate = true;
            flying.FrameLength = 0.05f;
            animations.Add(flying);

            animations.Play("flying");

            var red = new AnimationStrip(textures, Helpers.GetMegaTileRect(2, 0), 2, "red");
            red.LoopAnimation = true;
            red.Oscillate = true;
            red.FrameLength = 0.05f;
            animations.Add(red);

            animations.Play("red");

            isEnemyTileColliding = false;
            Attack = 1;

            Health = MaxHealth;

            IsAffectedByGravity = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            isTileColliding = false;
            CanBeJumpedOn = false;

            this.CollisionRectangle = new Rectangle(-28 * Game1.TileScale, -42 * Game1.TileScale, 56 * Game1.TileScale, 21 * Game1.TileScale);

            moveTimer = 4f;

            for (int i = 0; i < 4; i++)
            {
                var bomb = new Bomb(content, 0, 0, player, camera);
                bomb.Enabled = false;
                Bombs.Add(bomb);
            }

            ExtraEnemiesToAddAfterConstructor.AddRange(Bombs);
        }

        public float Speed
        {
            get
            {
                float speed = 200;
                if (Health < 5)
                {
                    speed *= 1.1f;
                }
                if (Health < 4)
                {
                    speed *= 1.1f;
                }
                if (Health < 3)
                {
                    speed *= 1.1f;
                }
                if (Health < 2)
                {
                    speed *= 1.1f;
                }

                if (isAngry)
                {
                    speed *= 2f;
                }

                return speed;
            }
        }

        public float BombFrequency
        {
            get
            {
                float frequency = 2f;
                if (Health < 5)
                {
                    frequency *= 0.9f;
                }
                if (Health < 4)
                {
                    frequency *= 0.9f;
                }
                if (Health < 3)
                {
                    frequency *= 0.9f;
                }
                if (Health < 2)
                {
                    frequency *= 0.9f;
                }
                return frequency;
            }
        }

        public float MoveFrequency
        {
            get
            {
                float frequency = 8f;
                if (Health < 5)
                {
                    frequency *= 0.8f;
                }
                if (Health < 4)
                {
                    frequency *= 0.8f;
                }
                if (Health < 3)
                {
                    frequency *= 0.8f;
                }
                if (Health < 2)
                {
                    frequency *= 0.8f;
                }
                return frequency;
            }
        }

        public override void TakeHit(GameObject attacker, int damage, Vector2 force)
        {
            if (IsTempInvincibleFromBeingHit || isAngry)
            {
                return;
            }

            if (attacker is Cannonball)
            {
                var cannonball = attacker as Cannonball;
                if (!cannonball.IsShootingOutOfCannon)
                {
                    // Cannon balls only hurt if shot out of a cannon.
                    return;
                }
            }

            Health -= damage;

            SoundManager.PlaySound("ShootFromCannon", 0.6f, -0.2f);

            if (!IsTempInvincibleFromBeingHit)
            {
                InvincibleTimer += 2f;
            }

            if (Health <= 0)
            {
                // DEATH!!!
                state = QuadState.Dying;
                Dead = true;
                this.velocity = Vector2.Zero;
                currentTargetLocation = Sock.WorldCenter;

                foreach (var bomb in Bombs)
                {
                    bomb.Kill();
                }

            }
            else
            {
                isAngry = true;

                // we're going to randomly reverse the x position of the sweep locations so that the quadcopter doesn't always sweep in the same direction.
                var reverse = Game1.Randy.NextBool();
                if (reverse)
                {
                    wayOffScreenLocation.X = sweepEndX;
                    sweepStartLocation.X = sweepEndX;
                    sweepEndLoaction.X = sweepStartX;
                }
                else
                {
                    wayOffScreenLocation.X = sweepStartX;
                    sweepStartLocation.X = sweepStartX;
                    sweepEndLoaction.X = sweepEndX;
                }

                currentTargetLocation = wayOffScreenLocation;
                bombTimer = 0f;
            }
        }

        public bool IsCloseToCurrentMoveTarget()
        {
            return Vector2.Distance(WorldCenter, currentTargetLocation) < 10;
        }

        public override void Kill()
        {
            Enabled = false;
            base.Kill();
        }

        public void Initialize()
        {

            this.WorldLocation += new Vector2(2, 0);

            foreach (var item in Game1.CurrentLevel.Items)
            {
                if (item is Sock)
                {
                    Sock = item as Sock;
                }
            }

            if (Sock == null)
            {
                throw new Exception("You need a sock in the level!");
            }

            Sock.Enabled = false;
            
            middleLocation = this.CollisionCenter;
            leftLocation = middleLocation + new Vector2(-148, 54);
            rightLocation = middleLocation + new Vector2(148, 54);
            currentTargetLocation = middleLocation;

            wayOffScreenLocation = middleLocation + new Vector2(-500, -200);
            sweepStartLocation = middleLocation + new Vector2(-500, 224);
            sweepEndLoaction = middleLocation + new Vector2(500, 224);
            sweepStartX = sweepStartLocation.X;
            sweepEndX = sweepEndLoaction.X;

        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!_isInitialized)
            {
                Initialize();
                _isInitialized = true;
            }

            base.Update(gameTime, elapsed);

            Game1.DrawBossHealth = true;
            Game1.MaxBossHealth = MaxHealth;
            Game1.BossHealth = Health;
            Game1.BossName = "Chopper";

            if (isAngry || state == QuadState.Dying)
            {
                animations.PlayIfNotAlreadyPlaying("red");
            }
            else
            {
                animations.PlayIfNotAlreadyPlaying("flying");
            }

            if (state == QuadState.Attacking)
            {
                // Move towards the current location
                if (IsMovingToTargetLocation)
                {
                    GoToLocation(Speed, currentTargetLocation);
                }

                moveTimer += elapsed;

                if (!isAngry)
                {

                    bombTimer += elapsed;

                    if (bombTimer >= BombFrequency)
                    {
                        bombTimer = 0f;
                        // Drop a bomb
                        foreach (var bomb in Bombs)
                        {
                            if (!bomb.Enabled)
                            {
                                bomb.Reset();
                                bomb.WorldLocation = WorldCenter + new Vector2(16, 0);

                                // Shoot towards the player
                                var xVelocity = Game1.Randy.NextFloat() * 300f;

                                // If the player is to the left, flip x velocity.
                                if (Player.WorldCenter.X < WorldCenter.X)
                                {
                                    xVelocity *= -1;
                                }

                                bomb.Velocity = new Vector2(xVelocity, -600);

                                break;
                            }
                        }
                    }

                    if (moveTimer >= MoveFrequency)
                    {
                        moveTimer = 0f;
                        IsMovingToTargetLocation = true;

                        if (currentTargetLocation == middleLocation)
                        {
                            currentTargetLocation = rightLocation;
                        }
                        else if (currentTargetLocation == rightLocation)
                        {
                            currentTargetLocation = leftLocation;
                        }
                        else if (currentTargetLocation == leftLocation)
                        {
                            currentTargetLocation = middleLocation;
                        }
                        else
                        {
                            if (Game1.IS_DEBUG)
                            {
                                throw new Exception("Unhandled quadcopter location");
                            }
                        }
                    }

                    if (IsCloseToCurrentMoveTarget())
                    {
                        IsMovingToTargetLocation = false;
                    }

                    if (!IsMovingToTargetLocation)
                    {
                        // Oscillate up and down around the targetLocation
                        var frequency = 5f;
                        var amplitude = 1f;
                        this.velocity = Vector2.Zero;
                        bounceOffset.Y = (float)Math.Sin(moveTimer * frequency) * amplitude;
                        this.worldLocation += bounceOffset;
                    }

                }
                else
                {
                    // Angry mode.
                    IsMovingToTargetLocation = true; // never stop moving!
                    if (IsCloseToCurrentMoveTarget())
                    {
                        if (currentTargetLocation == wayOffScreenLocation)
                        {
                            currentTargetLocation = sweepStartLocation;
                        }
                        else if (currentTargetLocation == sweepStartLocation)
                        {
                            currentTargetLocation = sweepEndLoaction;
                        }
                        else if (currentTargetLocation == sweepEndLoaction)
                        {
                            currentTargetLocation = middleLocation;
                            isAngry = false;
                        }
                    }
                }
            }

            if (state == QuadState.Dying)
            {
                GoToLocation(100f, currentTargetLocation);

                // Add random explosions
                explosionTimer += elapsed;
                if (explosionTimer >= 0.2f)
                {
                    explosionTimer = 0f;

                    // Make explosions slightly larger than the collision rect
                    int explosionBuffer = 20;

                    // Get a random location over this collision rectangle
                    var randomX = Game1.Randy.Next(CollisionRectangle.Width + (explosionBuffer * 2));
                    var randomY = Game1.Randy.Next(CollisionRectangle.Height + (explosionBuffer * 2));

                    var randomLocation = new Vector2(CollisionRectangle.X + randomX - explosionBuffer, CollisionRectangle.Y + randomY - explosionBuffer);
                    EffectsManager.AddExplosion(randomLocation);
                }

                dyingTimer += elapsed;
                if (dyingTimer >= 4f)
                {

                    this.Kill();
                    state = QuadState.Dead;
                    Sock.FadeIn();
                }
            }

            if (state == QuadState.Dead)
            {
                // Take them to wherever you need to take them. Once we figure out where that is.
                if (this.Sock.IsCollected)
                {
                    leaveLevelTimer -= elapsed;
                    if (leaveLevelTimer <= 0)
                    {
                        // Leave the level.
                        GlobalEvents.FireDoorEntered(null, "World3", "QuadcopterBossDoor", "");
                    }
                }
            }

        }
    }
}