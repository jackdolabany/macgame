using MacGame.DisplayComponents;
using MacGame.Platforms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// Kinda like Jason from Friday the 13th, but certainly not him.
    /// 
    /// This guy will appear in multiple places, so set up Murderer Rectangles on the map
    /// Add Object Modifiers with the property IsMurdererRectangle = true.
    /// 
    /// There's only 1 murderer so his health is stored in LevelState. When you kill him 
    /// it should be saved in StorageState and he won't come back!
    /// </summary>
    public class Murderer : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        // Margin in pixels to spawn off-screen
        const int SpawnMargin = 32;

        private List<Rectangle> _murdererRectangles = new List<Rectangle>(); 

        /// <summary>
        /// This is the Rectangle that Mac needs to be inside for the murderer to appear.
        /// In Tiled place a rectangle and call it "MurdererRectangle"
        /// 
        /// There's multiple on the map so whatever one Mac steps into last is the current murderer rectangle.
        /// </summary>
        Rectangle currentMurdererRectangle;

        private bool _isInitialized = false;
        private Player _player;

        private MurdererState _state;

        private float _speed = 60f;

        // When attacking he'll jump every few seconds.
        private float _jumpTimer = 0;
        private float _jumpTimerGoal = 3;

        // Don't turn more than once per second so he doesn't spaz.
        private float _turnAroundTimer = 0f;
        private float _turnAroundTimerGoal = 1f;

        private float _throwSickleTimer = 0f;
        private float _throwSickleTimerGoal = 4f;

        /// <summary>
        /// Sickle that he randomly throws.
        /// </summary>
        private Sickle _sickle;

        // Count up until he appears when the player is in the location
        private float appearTimer = 0f;
        bool hasAppeared = false;

        // He'll appear after some short time.
        const float firstAppearanceTimerGoal = 10f;

        // Harder to get him back once he's appeared once
        const float reappearTimerGoal = 30f;

        // He'll attack longer if you have apples and can actually defeat him.
        private float attackingTimer = 0f;
        const float attackingTimeWithApplesGoal = 20f;
        const float attackingTimeWithoutApplesGoal = 10f;

        private float explosionTimer = 0f;

        private float dyingTimer = 0f;
        private float dyingTimerGoal = 5f;

        private enum MurdererState
        {
            Hiding, 
            Attacking,
            ReadyToRunAway,
            RunningAway,
            Dying,
            Dead
        }

        public Murderer(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            _player = player;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\ReallyBigTextures");
            
            var walk = new AnimationStrip(textures, Helpers.GetReallyBigTileRect(0, 3), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.14f;
            animations.Add(walk);

            var toss = new AnimationStrip(textures, Helpers.GetReallyBigTileRect(2, 3), 1, "toss");
            toss.LoopAnimation = false;
            toss.FrameLength = 0.14f;
            animations.Add(toss);

            // Face the player. Either to die or to turn around.
            var face = new AnimationStrip(textures, Helpers.GetReallyBigTileRect(3, 3), 1, "face");
            face.LoopAnimation = false;
            face.FrameLength = 0.2f;
            animations.Add(face);

            animations.Play("walk");

            isEnemyTileColliding = true;
            Attack = 1;

            if (Game1.LevelState.MurdererHealth.HasValue)
            {
                Health = Game1.LevelState.MurdererHealth.Value;
            }
            else
            {
                Health = 20;
                Game1.LevelState.MurdererHealth = Health;
            }

            IsAffectedByGravity = true;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            CanBeJumpedOn = false;

            SetWorldLocationCollisionRectangle(10, 23);

            this.Enabled = false;

            if (!Game1.StorageState.HasKilledMurderer)
            {
                _state = MurdererState.Hiding;
            }
            else
            {
                // Once dead in game state, he'll never come back.
                _state = MurdererState.Dead;
                Dead = true;
            }
            
            _sickle = new Sickle(content, cellX, cellY, player, camera);
            _sickle.Enabled = false;
            ExtraEnemiesToAddAfterConstructor.Add(_sickle);
            ExtraEnemiesToAddAfterConstructor.Add(_sickle);
        }

        private void Initialize()
        {
            // Find the murderer rectangle in the map.
            foreach (var obj in Game1.CurrentMap.ObjectModifiers)
            {
                if (obj.Properties.ContainsKey("IsMurdererRectangle") && obj.Properties["IsMurdererRectangle"].ToBoolean())
                {
                    _murdererRectangles.Add(obj.GetScaledRectangle());
                }
            }

            if (!_murdererRectangles.Any())
            {
                throw new Exception("The murderer needs at least one rectangle with the property 'IsMurdererRectangle' on the map set to true.");
            }
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!_isInitialized)
            {
                Initialize();
                _isInitialized = true;
            }

            base.Update(gameTime, elapsed);

            // Only handle spawn logic if not enabled and alive
            if (_state == MurdererState.Hiding)
            {
                // Be way off the map so we don't collide with the player or anything.
                this.worldLocation = new Vector2(-10000, -10000);

                foreach (var rectangle in _murdererRectangles)
                {
                    // Check if player is inside the murderer rectangle
                    if (rectangle.Contains(_player.CollisionCenter))
                    {
                        currentMurdererRectangle = rectangle;
                    }
                }

                // Check if player is inside the murderer rectangle
                if (currentMurdererRectangle.Contains(_player.CollisionCenter))
                {
                    appearTimer += elapsed;
                    if (appearTimer >= (hasAppeared ? reappearTimerGoal : firstAppearanceTimerGoal))
                    {
                        // Determine which side to spawn on
                        int spawnX;

                        // Y is tricky, just spawn him in the middle of the bounding rect, he's off screen so he'll just fall to the ground.
                        int spawnY = currentMurdererRectangle.Bottom - 8;

                        // Figure out if the player is in the left, middle, or right third of the bounding rect
                        bool isLeftSpawn = false;

                        if (_player.WorldLocation.X < (currentMurdererRectangle.X + (currentMurdererRectangle.Width / 3f)))
                        {
                            // You're too close to the left, spawn on the right.
                            isLeftSpawn = false;
                        }
                        else if (_player.WorldLocation.X > (currentMurdererRectangle.X + currentMurdererRectangle.Width * (2f / 3f)))
                        {                             
                            // You're too close to the right, spawn on the left.
                            isLeftSpawn = true;
                        }
                        else
                        {
                            // You're in the middle, spawn is random.
                            isLeftSpawn = Game1.Randy.NextBool();
                        }

                        if (isLeftSpawn)
                        {
                            // Player is to the right, spawn on left
                            spawnX = Game1.Camera.ViewPort.X - SpawnMargin - (CollisionRectangle.Width / 2);
                        }
                        else
                        {
                            // Player is to the left, spawn on right
                            spawnX = Game1.Camera.ViewPort.Right + SpawnMargin + (CollisionRectangle.Width / 2);
                        }

                        // Set position
                        this.WorldLocation = new Vector2(spawnX, spawnY);
                        Enabled = true;
                        Alive = true;
                        _state = MurdererState.Attacking;
                        animations.Play("walk");
                        hasAppeared = true;
                        appearTimer = 0f;
                    }
                }
                else
                {
                    appearTimer = 0f;
                }
            }

            if (_state == MurdererState.Attacking)
            {
                // Walk towards the player
                _turnAroundTimer += elapsed;
                if (_turnAroundTimer >= _turnAroundTimerGoal)
                {
                    _turnAroundTimer = 0f;
                    var oldFlipped = Flipped;
                    Flipped = (Player.WorldLocation.X <= this.WorldLocation.X);

                    if (oldFlipped != Flipped)
                    {
                        // If we flipped, play the face animation.
                        animations.Play("face").FollowedBy("walk");
                    }
                }

                if (Flipped)
                {
                    Velocity = new Vector2(-_speed, Velocity.Y);
                }
                else
                {
                    Velocity = new Vector2(_speed, Velocity.Y);
                }

                // Jump at some interval.
                _jumpTimer += elapsed;
                if (_jumpTimer >= _jumpTimerGoal)
                {
                    Velocity = new Vector2(Velocity.X, -500);
                    _jumpTimer = 0f;
                    SoundManager.PlaySound("JumpQuick");
                }

                // Check if the player is still in the rectangle
                if (!currentMurdererRectangle.Contains(_player.CollisionCenter))
                {
                    // Player left, go back to hiding
                    _state = MurdererState.ReadyToRunAway;
                    attackingTimer = 0f;
                }

                // Throw a sickle at the player.
                if (!_sickle.Enabled)
                {
                    _throwSickleTimer += elapsed;
                    if (_throwSickleTimer >= _throwSickleTimerGoal)
                    {
                        animations.Play("toss").FollowedBy("walk");
                        _throwSickleTimer = 0f;
                        _sickle.Toss(this, !Flipped);
                    }
                }

                attackingTimer += elapsed;

                if (attackingTimer >= (Game1.Player.HasApples ? attackingTimeWithApplesGoal : attackingTimeWithoutApplesGoal))
                {
                    _state = MurdererState.ReadyToRunAway;
                    attackingTimer = 0f;
                }

            }

            if (_state == MurdererState.ReadyToRunAway)
            {
                if (onGround)
                {
                    // Run away!
                    animations.Play("walk");
                    // Just run to the left or right, stop colliding, just play the run animation and move!
                    var isCloserToLeftOfBoundingBox = currentMurdererRectangle.Center.X < this.WorldLocation.X;
                    var runawaySpeed = _speed * 5;
                    if (isCloserToLeftOfBoundingBox)
                    {
                        Flipped = true;
                        this.Velocity = new Vector2(-runawaySpeed, 0f);
                    }
                    else
                    {
                        this.Velocity = new Vector2(runawaySpeed, 0f);
                        Flipped = false;
                    }
                    _state = MurdererState.RunningAway;
                    IsAffectedByGravity = false;
                    isTileColliding = false;
                }
            }

            if (_state == MurdererState.RunningAway)
            { 
                if (camera.IsWayOffscreen(this.CollisionRectangle))
                {
                    _state = MurdererState.Hiding;
                    // No more running through walls.
                    IsAffectedByGravity = true;
                    isTileColliding = true;
                }
            }

            if (_state == MurdererState.Dying)
            {
                
                Velocity = Vector2.Zero;

                // random explosions
                explosionTimer += elapsed;
                if (explosionTimer >= 0.2f)
                {
                    explosionTimer = 0f;
                    // Get a random location over this collision rectangle
                    var randomX = Game1.Randy.Next(CollisionRectangle.Width);
                    var randomY = Game1.Randy.Next(CollisionRectangle.Height);

                    var randomLocation = new Vector2(CollisionRectangle.X + randomX, CollisionRectangle.Y + randomY);
                    EffectsManager.AddExplosion(randomLocation);


                }

                // Become more transparent as time goes on
                var deadPercentage = dyingTimer / dyingTimerGoal;
                DisplayComponent.TintColor = Color.Lerp(Color.White, Color.Transparent, deadPercentage);

                dyingTimer += elapsed;
                if (dyingTimer >= dyingTimerGoal)
                {
                    // Time to die.
                    Dead = true;
                    Enabled = false;
                    _state = MurdererState.Dead;

                    // TODO: Reveal dracula part.
                }
            }

           
        }

        public override void PlayTakeHitSound()
        {
            SoundManager.PlaySound("Hurt");
        }

        public override void TakeHit(GameObject attacker, int damage, Vector2 force)
        {

            if (_state != MurdererState.Attacking)
            {
                return;
            }

            Health -= damage;

            Game1.LevelState.MurdererHealth = Health;

            if (Health > 0)
            {
                PlayTakeHitSound();

                if (!IsTempInvincibleFromBeingHit)
                {
                    InvincibleTimer += InvincibleTimeAfterBeingHit;
                }
            }
            else
            {
                _state = MurdererState.Dying;
                animations.Play("face");
                _sickle.Enabled = false;
                EffectsManager.SmallEnemyPop(_sickle.CollisionCenter);
                SoundManager.PlaySound("MurdererDeath");
                Dead = true;
                Game1.StorageState.HasKilledMurderer = true;
                StorageManager.TrySaveGame();

            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_state != MurdererState.Hiding)
            {
                base.Draw(spriteBatch);
            }
        }
    }
}