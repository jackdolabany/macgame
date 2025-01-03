﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MacGame.Platforms;
using System;
using TileEngine;
using MacGame.Enemies;
using MacGame.DisplayComponents;
using MacGame.Items;
using System.Collections.Generic;
using MacGame.Behaviors;
using System.Threading.Tasks.Dataflow;
using System.Net.Http;

namespace MacGame
{
    public enum MacState
    {
        Idle,
        Running,
        Jumping,
        Falling,
        Sliding,
        ClimbingLadder,
        ClimbingVine,
        Dead,
        IsKnockedDown,
        /// <summary>
        /// Set Mac to this state for some kind of custom programming for cut scenes or whatever.
        /// </summary>
        NPC
    }

    public class Player : GameObject
    {
        AnimationDisplay animations;

        public const int MaxHealth = 5;

        public int Health { get; set; } = MaxHealth;

        public int Tacos = 0;
        public int SockCount = 0;

        public InputManager InputManager { get; private set; }

        private DeadMenu _deadMenu;

        private const float maxAcceleration = 600;
        private const float maxSpeed = 500;

        private MacState _state = MacState.Idle;

        /// <summary>
        /// If Mac is an NPC he'll walk towards this target.
        /// </summary>
        public Vector2 NpcModeTarget { get; set; }

        /// <summary>
        /// NPC behavior so Mac can go to a location.
        /// </summary>
        private MoveToLocation _moveToLocation { get; set; } 

        private bool IsRunning => _state == MacState.Running;
        private bool IsJumping => _state == MacState.Jumping;
        private bool IsSliding => _state == MacState.Sliding;
        private bool IsFalling => _state == MacState.Falling;
        private bool IsClimbingLadder => _state == MacState.ClimbingLadder;
        private bool IsClimbingVine => _state == MacState.ClimbingVine;

        IPickupObject? pickedUpObject;

        // Track a pickup object that you recently dropped so you can kick it out.
        IPickupObject? recentlyDropped;
        float kickTimer = 0;

        // Can't pick up Item. Use this so Mac can't insta pick up an item he just dropped or kicked.
        float pickUpAgainTimer = 0;

        // Track if the player jumped off of sand or ice so that we can maintain the adjusted
        // movement speed through the jump.
        private bool isInJumpFromSand = false;
        private bool isInJumpFromIce = false;
        private bool isInJumpFromGround = false;
        float jumpButtonHeldDownTimer = 0f;
        const float maxJumpButtonHeldDownTime = 0.5f;
        public Vector2 NormalGravity;
        public Vector2 JumpGravity;

        public bool IsInWater = false;
        public bool IsJumpingOutOfWater = false;

        // Add time here to make the camera track Mac slowly for a period of time. This will help 
        // move the camera more naturally for a period when he does "snapping" actions like snapping to the
        // other side of a vine, or snapping to other objects.
        private float cameraTrackingTimer = 0f;
        private bool IsKnockedDown => _state == MacState.IsKnockedDown;
        private bool IsNpcMode => _state == MacState.NPC;

        private float invincibleTimeRemaining = 0.0f;
        private float invincibleFlashTimer = 0.0f;

        public bool InteractButtonPressedThisFrame = false;

        public bool IsInvincible => invincibleTimeRemaining > 0.0f;

        private Rectangle _previousCollisionRectangle;

        // Ladder climbing stuff
        AnimationStrip climbingLadderAnimation;
        private const int climbingSpeed = 120;

        // Used to temporarily prevent you from climbing ladders if you jump while holding up
        // until you release up and press it again. This way you don't just insta-climb the ladder above you.
        private bool canClimbLadders = true;

        private float playClimbSoundTimer = 0f;

        // Vine climbing stuff
        AnimationStrip climbingVineAnimation;

        /// <summary>
        /// When you jump off a vine, we temporarily prevent you from grabbing onto another 
        /// vine until your y position has moved away from the vine you are currently on.
        /// </summary>
        private bool canClimbVines = true;
        private float yPositionWhenLastOnVine = 0;

        public bool IsInMineCart = false;
        private bool HasWings
        {
            get
            {
                return this.CurrentItem is InfiniteJump;
            }
        }

        public ObjectPool<Apple> Apples;

        private bool HasApples
        {
            get
            {
                return this.CurrentItem is Apples;
            }
        }

        private float appleCooldownTimer = 0f;
        private const float appleCooldownTime = 0.3f;

        public Item? CurrentItem = null;

        private bool HasShovel
        {
            get
            {
                return this.CurrentItem is Shovel;
            }
        }
        private MacShovel _shovel;

        /// <summary>
        /// if Mac is using the wing, it'll render behind him.
        /// </summary>
        MacWings wings;

        public bool IsInvisible { get; set; } = false;

        public override Vector2 Gravity
        {
            get
            {
                if (isInJumpFromGround && jumpButtonHeldDownTimer > 0)
                {
                    return base.Gravity * 0.333f;
                }
                else
                {
                    return base.Gravity;
                }
            }
        }

        public bool IsInCannon 
        {
            get
            {
                return CannonYouAreIn != null;
            }
        }

        public bool CanEnterCannon
        {
            get
            {
                return this.CannonYouAreIn == null && pickedUpObject == null;
            }
        }

        /// <summary>
        /// After being shot out of a cannon you are not effected by gravity for a period of time, 
        /// you can't enter inputs, and you smash through sand.
        /// </summary>
        public bool IsJustShotOutOfCannon { get; set; } = false;

        /// <summary>
        ///  If this has a positive value then you are only in the 'shooting out of cannon' state until it runs out.
        ///  Set to 0 to be shot out forever! Like for a SuperShot cannon.
        /// </summary>
        private float timeRemainingToBeShotOutOfCannon = 0.0f;

        public Cannon CannonYouAreIn { get; set; }

        /// <summary>
        /// A special rectangle so that when you talk to NPCs you don't have to be right on top of them.
        /// </summary>
        public Rectangle NpcRectangle
        {
            get
            {
                var collisionRect = this.CollisionRectangle;

                if (Flipped)
                {
                    // extend the rectangle to the left
                    return new Rectangle(collisionRect.X - collisionRect.Width, collisionRect.Y, collisionRect.Width * 2, collisionRect.Height);
                }
                else
                {
                    // Extend the rectangle to the right
                    return new Rectangle(collisionRect.X, collisionRect.Y, collisionRect.Width * 2, collisionRect.Height);
                }
            }
        }

        public Player(ContentManager content, InputManager inputManager, DeadMenu deadMenu)
        {
            animations = new AnimationDisplay();
            this.DisplayComponent = animations;

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var idle = new AnimationStrip(textures, Helpers.GetTileRect(1, 0), 1, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.1f;
            animations.Add(idle);

            var walk = new AnimationStrip(textures, Helpers.GetTileRect(1, 0), 2, "run");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.14f;
            animations.Add(walk);

            var slide = new AnimationStrip(textures, Helpers.GetTileRect(3, 0), 1, "slide");
            slide.LoopAnimation = false;
            slide.FrameLength = 0.1f;
            animations.Add(slide);

            var jump = new AnimationStrip(textures, Helpers.GetTileRect(1, 1), 1, "jump");
            jump.FrameLength = 0.1f;
            animations.Add(jump);

            var fall = new AnimationStrip(textures, Helpers.GetTileRect(2, 1), 1, "fall");
            fall.LoopAnimation = true;
            fall.FrameLength = 0.1f;
            animations.Add(fall);

            climbingLadderAnimation = new AnimationStrip(textures, Helpers.GetTileRect(5, 3), 2, "climbLadder");
            climbingLadderAnimation.LoopAnimation = true;
            climbingLadderAnimation.FrameLength = 0.14f;
            animations.Add(climbingLadderAnimation);

            climbingVineAnimation = new AnimationStrip(textures, Helpers.GetTileRect(6, 2), 2, "climbVine");
            climbingVineAnimation.LoopAnimation = true;
            climbingVineAnimation.FrameLength = 0.14f;
            animations.Add(climbingVineAnimation);

            var mineCart = new AnimationStrip(textures, Helpers.GetTileRect(2, 8), 1, "mineCart");
            mineCart.LoopAnimation = false;
            mineCart.FrameLength = 0.1f;
            animations.Add(mineCart);

            var knockedDown = new AnimationStrip(textures, Helpers.GetTileRect(4, 0), 1, "knockedDown");
            knockedDown.LoopAnimation = false;
            knockedDown.FrameLength = 0.1f;
            animations.Add(knockedDown);

            var swim = new AnimationStrip(textures, Helpers.GetTileRect(6, 4), 2, "swim");
            swim.LoopAnimation = true;
            swim.FrameLength = 0.25f;
            animations.Add(swim);

            Enabled = true;

            // This gets set later when the level loads.
            DisplayComponent.DrawDepth = 0.5f;

            this.IsAbleToMoveOutsideOfWorld = true;
            this.IsAbleToSurviveOutsideOfWorld = true;
            this.IsAffectedByForces = false;
            this.isEnemyTileColliding = false;

            this.IsAffectedByGravity = true;

            this.IsAffectedByPlatforms = true;

            SetCenteredCollisionRectangle(5, 6);

            InputManager = inputManager;
            _deadMenu = deadMenu;

            // Use this one wing image to draw flapping wings.
            wings = new MacWings(this, textures);

            Apples = new ObjectPool<Apple>(2);
            Apples.AddObject(new Apple(content, 0, 0, this, Game1.Camera));
            Apples.AddObject(new Apple(content, 0, 0, this, Game1.Camera));

            _shovel = new MacShovel(this, textures);

            _moveToLocation = new MoveToLocation(Vector2.Zero, 150, "idle", "run", "jump", "climbLadder");
        }

        public override void SetDrawDepth(float depth)
        {
            this.DisplayComponent.DrawDepth = depth;
            this._shovel.SetDrawDepth(DrawDepth + Game1.MIN_DRAW_INCREMENT);
            this.wings.SetDrawDepth(DrawDepth + Game1.MIN_DRAW_INCREMENT);
            this.Apples.RawList.ForEach(a => a.SetDrawDepth(DrawDepth + Game1.MIN_DRAW_INCREMENT));
        }

        public void BecomeNpc()
        {
            this._state = MacState.NPC;
        }

        public void GoToLocation(Vector2 location)
        {
            _moveToLocation.TargetLocation = location;
        }

        public bool IsAtLocation()
        {
            return _moveToLocation.IsAtLocation();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            InteractButtonPressedThisFrame = false;

            _previousCollisionRectangle = this.CollisionRectangle;

            if(cameraTrackingTimer >= 0)
            {                 
                cameraTrackingTimer -= elapsed;
            }

            var wasInWater = IsInWater;

            if (!IsJumpingOutOfWater)
            {
                if (wasInWater)
                {
                    // Water is kind of sticky, once in you'll stay in water until your top and bottom pixel are out of water.
                    var topPixel = new Vector2(this.WorldCenter.X, this.CollisionRectangle.Top);
                    var isTopInWater = Game1.CurrentMap?.GetMapSquareAtPixel(topPixel)?.IsWater ?? false;
                    var bottomPixel = new Vector2(this.WorldCenter.X, this.CollisionRectangle.Bottom);
                    var isBottomInWater = Game1.CurrentMap?.GetMapSquareAtPixel(bottomPixel)?.IsWater ?? false;
                    IsInWater = isTopInWater || isBottomInWater;
                }
                else
                {
                    // If you weren't in water, you're in water when your center pixel is in water.
                    IsInWater = Game1.CurrentMap?.GetMapSquareAtPixel(this.WorldCenter)?.IsWater ?? false;
                }
            }

            if (!wasInWater && IsInWater)
            {
                if (velocity.Y > 200)
                {
                    EffectsManager.AddSplash(new Vector2(this.CollisionRectangle.Left, this.CollisionRectangle.Bottom), new Vector2(0, this.velocity.Y));
                    EffectsManager.AddSplash(new Vector2(this.CollisionRectangle.Right, this.CollisionRectangle.Bottom), new Vector2(0, this.velocity.Y));
                }
                SoundManager.PlaySound("Splash");
            }

            if (IsJumpingOutOfWater && this.Velocity.Y >= 0)
            {
                IsJumpingOutOfWater = false;
            }

            if (!IsInMineCart)
            {
                SoundManager.StopMinecart();
            }

            if (IsInMineCart)
            {
                HandleMineCartInputs(elapsed);
            }
            else if (IsKnockedDown)
            {
                HandleKnockedDownInputs(elapsed);
            }
            else if (IsInCannon)
            {
                HandleCannonInputs(elapsed);
            }
            else if (IsJustShotOutOfCannon)
            {
                HandleShotOutOfCannonInputs(elapsed);
            }
            else if (IsInWater)
            {
                HandleWaterInputs(elapsed);
            }
            else if (IsNpcMode)
            {
                _moveToLocation.Update(this, gameTime, elapsed);
            }
            else
            {
                HandleInputs(elapsed);
            }

            if (this.Enabled && CollisionRectangle.Top > Game1.CurrentMap.GetWorldRectangle().Bottom)
            {
                // player fell down a bottomless pit
                Kill();
            }

            if (invincibleTimeRemaining > 0)
            {
                invincibleTimeRemaining -= elapsed;
                invincibleFlashTimer -= elapsed;

                if (invincibleFlashTimer < 0)
                {
                    this.DisplayComponent.TintColor = Color.White * 0.4f;
                }
                else
                {
                    this.DisplayComponent.TintColor = Color.White;
                }
                if (invincibleFlashTimer <= -0.1f)
                {
                    invincibleFlashTimer = 0.1f;
                }
            }
            else
            {
                DisplayComponent.TintColor = Color.White;
            }

            if (IsInMineCart && Landed)
            {
                SoundManager.PlayMinecartLanded();
            }

            base.Update(gameTime, elapsed);

            if (HasWings)
            {
                wings.Update(gameTime, elapsed);
            }
            if (HasShovel)
            {
                _shovel.Update(gameTime, elapsed);
            }

            foreach (var apple in Apples.RawList)
            {
                if (apple.Enabled)
                {
                    apple.Update(gameTime, elapsed);
                }
            }

            // When climbing a vine, to make it look more natural, offset the sprite from the CollisionRectangle a bit.
            if (IsClimbingVine)
            {
                var climbingOffset = new Vector2(-4, 0);
                if (this.Flipped)
                {
                    climbingOffset *= -1;
                }
                this.animations.Offset = climbingOffset;
            }
            else
            {
                this.animations.Offset = Vector2.Zero;
            }
        }

        /// <summary>
        /// Reset health and items and stuff once Mac changes levels or dies or whatever.
        /// </summary>
        /// <param name="isNewLevel">True if you are transitioning to or from the hub. False if you are just changing rooms in a level.</param>
        public void ResetStateForLevelTransition(bool isNewLevel)
        {
            if (isNewLevel)
            {
                Health = Player.MaxHealth;
                CurrentItem = null;
                Tacos = 0;
            }

            Enabled = true;
            Velocity = Vector2.Zero;
            IsInMineCart = false;
            IsInvisible = false;
            _state = MacState.Idle;
            this.IsInWater = false;
            IsJumpingOutOfWater = false;
            this.invincibleTimeRemaining = 0f;
            this.IsInvisible = false;
            this.IsJustShotOutOfCannon = false;
            this.PlatformThatThisIsOn = null;
    }

        /// <summary>
        /// Send the player sliding out of a hub door after being killed or whatever.
        /// </summary>
        public void SlideOutOfDoor(Vector2 doorLocation)
        {
            PositionForSlideOutOfDoor(doorLocation);
            this.velocity = new Vector2(280, 0);
            this._state = MacState.IsKnockedDown;
        }

        public void PositionForSlideOutOfDoor(Vector2 doorLocation)
        {
            this.worldLocation = doorLocation + new Vector2(0, -4);
            this.Flipped = true;
        }

        public bool JumpedOnEnemyRectangle(Rectangle rectangle)
        {
            if (IsClimbingLadder || IsClimbingVine || IsInWater || IsInMineCart)
            {
                return false;
            }

            // Pad 1 pixel to make it a little easier
            var wasAboveEnemy = _previousCollisionRectangle.Bottom - 8 <= rectangle.Top;

            return wasAboveEnemy;
        }

        public void CheckEnemyInteractions(Enemy enemy)
        {
            if (!enemy.Enabled) return;

            if (!enemy.Alive) return;

            if (enemy.IsTempInvincibleFromBeingHit) return;

            // Check body collisions
            if (CollisionRectangle.Intersects(enemy.CollisionRectangle))
            {
                if (enemy.CanBeJumpedOn && JumpedOnEnemyRectangle(enemy.CollisionRectangle))
                {
                    // If the player was above the enemy, the enemy was jumped on and takes a hit.
                    enemy.TakeHit(this,1, Vector2.Zero);
                    velocity.Y = -450;
                }
                else if (enemy.Attack > 0)
                {
                    TakeHit(enemy);
                    enemy.AfterHittingPlayer();
                }
            }
            else if (enemy.CanBeHitWithWeapons)
            {
                foreach (var apple in Apples.RawList)
                {
                    if (apple.Enabled)
                    {
                        if(apple.CollisionRectangle.Intersects(enemy.CollisionRectangle))
                        {
                            apple.Smash();
                            enemy.TakeHit(apple, 1, Vector2.Zero);
                        }
                    }
                }
            }
        }

        public void TakeHit(Enemy enemy)
        {
            if (IsInvincible) return;
            if (Health <= 0) return;
            if (enemy.Attack == 0) return;

            // player takes a hit.
            Health -= enemy.Attack;
            if (Health <= 0)
            {
                Kill();
            }
            else
            {
                if (HasWings || HasApples)
                {
                    CurrentItem = null;
                }
                invincibleTimeRemaining = 1.5f;
                SoundManager.PlaySound("TakeHit");
                var hitBackBoost = new Vector2(100, -200);
                if (CollisionCenter.X < enemy.CollisionCenter.X)
                {
                    hitBackBoost.X *= -1;
                }
                this.velocity = hitBackBoost;
            }
        }

        private void HandleInputs(float elapsed)
        {
            float friction;
            float jumpBoost;

            var mapSquareBelow = Game1.CurrentMap.GetMapSquareAtPixel(this.worldLocation + new Vector2(0, 1));
            var isSand = mapSquareBelow != null && mapSquareBelow.IsSand || (!OnGround && isInJumpFromSand);
            var isIce = mapSquareBelow != null && mapSquareBelow.IsIce || (!OnGround && isInJumpFromIce);
            
            var environmentMaxWalkSpeed = maxSpeed;
            var acceleration = maxAcceleration;

            friction = 1.5f;
            jumpBoost = 390;

            if (isSand)
            {
                friction *= 2;
                jumpBoost *= 0.6f;
                environmentMaxWalkSpeed /= 2;
            }
            else if (isIce)
            {
                friction /= 2f;
                environmentMaxWalkSpeed *= 1.25f;
            }

            // If they aren't running max walk speed is cut in half.
            if (!InputManager.CurrentAction.action && (onGround || IsClimbingLadder))
            {
                environmentMaxWalkSpeed /= 3;
            }

            // Cut the acceleration in half if they are in the air.
            if (!OnGround && !IsClimbingLadder && !IsClimbingVine)
            {
                acceleration /= 2;
            }

            const float airMovementSpeed = 250f;

            // Walk Right
            if (InputManager.CurrentAction.right && !InputManager.CurrentAction.left && !IsClimbingVine)
            {
                if (OnGround && !IsClimbingLadder)
                {
                    // Walking
                    this.velocity.X += acceleration * elapsed;
                    if (velocity.X > environmentMaxWalkSpeed)
                    {
                        velocity.X = environmentMaxWalkSpeed;
                    }
                }
                else if (IsClimbingLadder)
                {
                    this.velocity.X = climbingSpeed;
                }
                else
                {
                    // Movement in the air. You should be capped by your initial jump speed but able to accelerate a bit backwards.
                    this.velocity.X += airMovementSpeed * elapsed;
                }

                if (onGround)
                {
                    _state = MacState.Running;
                }
                Flipped = false;
            }

            // Walk left
            if (InputManager.CurrentAction.left && !InputManager.CurrentAction.right && !IsClimbingVine)
            {
                if (OnGround && !IsClimbingLadder)
                {
                    this.velocity.X -= acceleration * elapsed;
                    if (velocity.X < -environmentMaxWalkSpeed)
                    {
                        velocity.X = -environmentMaxWalkSpeed;
                    }
                }
                else if (IsClimbingLadder)
                {
                    this.velocity.X = -climbingSpeed;
                }
                else
                {
                    // Movement in the air. You should be capped by your initial jump speed but able to accelerate a bit backwards.
                    this.velocity.X -= airMovementSpeed * elapsed;
                }

                if (onGround)
                {
                    _state = MacState.Running;
                }
                Flipped = true;
            }

            if (OnGround && !IsClimbingLadder && !IsClimbingVine)
            {
                this.velocity.X -= (this.velocity.X * friction * elapsed);
                if (Math.Abs(this.velocity.X) < 1f)
                {
                    this.velocity.X = 0;
                }
            }

            // Sliding is a special state when you are still moving after walking.
            if (IsRunning
                && !InputManager.CurrentAction.left
                && !InputManager.CurrentAction.right
                && !IsFalling
                && !IsJumping)
            {
                _state = MacState.Sliding;
            }
            
            // Ladder stuff.
            var tileAtBottom = Game1.CurrentMap.GetMapSquareAtPixel(this.worldLocation);
            var tileAtTop = Game1.CurrentMap.GetMapSquareAtPixel(this.worldLocation - new Vector2(0, CollisionRectangle.Height));
            var isOverALadder = (tileAtBottom?.IsLadder ?? false) || (tileAtTop?.IsLadder ?? false);

            if (!isOverALadder && IsClimbingLadder)
            {
                _state = MacState.Idle;
            }
            
            // Climbing a ladder from standstill.
            if (isOverALadder 
                && pickedUpObject == null
                && canClimbLadders
                && (InputManager.CurrentAction.up || (IsClimbingLadder && InputManager.CurrentAction.down)) // Need to press up to latch onto a ladder. Down only if you are already climbing.
                && !(PlatformThatThisIsOn is LadderPlatform)) // Don't climb if you are standing on a ladder platform. Climbing down from atop a ladder is handled below.
            {
                _state = MacState.ClimbingLadder;
                this.velocity.X -= maxAcceleration * elapsed;
                if (velocity.X < -environmentMaxWalkSpeed)
                {
                    velocity.X = -environmentMaxWalkSpeed;
                }
                this.velocity.Y = climbingSpeed;
                if (InputManager.CurrentAction.up)
                {
                    this.velocity.Y *= -1;
                }

                // No moving left or right on the ladder unless you are not going up or down.
                this.velocity.X = 0;
            }
            this.IsAffectedByGravity = !IsClimbingLadder && !IsClimbingVine && !IsJustShotOutOfCannon && !IsInCannon;

            // Stop moving while climbing if you aren't pressing up or down.
            if ((IsClimbingLadder || IsClimbingVine) && !InputManager.CurrentAction.up && !InputManager.CurrentAction.down)
            {
                this.velocity.Y = 0;
            }

            if (IsClimbingLadder && !InputManager.CurrentAction.left && !InputManager.CurrentAction.right)
            {
                this.velocity.X = 0;
            }

            // Stop climbing if you move down towards the ground.
            if (InputManager.CurrentAction.down && onGround)
            {
                _state = MacState.Idle;
            }

            // If you are on a ladder platform you can press down to climb down through it.
            if (canClimbLadders && !InputManager.CurrentAction.jump && InputManager.CurrentAction.down && OnPlatform && PlatformThatThisIsOn is LadderPlatform)
            {
                // Also make sure Mac's collision rect isn't blocked from going down by other solid tiles.
                // Otherwise he may do a weird vibrating thing trying to climb down to ladder with a solid wall next to the top tile.
                var tileAtBottomLeft = Game1.CurrentMap.GetMapSquareAtPixel(this.CollisionRectangle.Left, this.WorldLocation.Y.ToInt());
                var tileAtBottomRight = Game1.CurrentMap.GetMapSquareAtPixel(this.CollisionRectangle.Right, this.WorldLocation.Y.ToInt());

                var canGoDown = ((tileAtBottomLeft == null || tileAtBottomLeft.Passable) && (tileAtBottomRight == null || tileAtBottomRight.Passable));
                if (canGoDown)
                {
                    _state = MacState.ClimbingLadder;
                    this.velocity.Y = climbingSpeed;
                    this.PoisonPlatforms.Add(PlatformThatThisIsOn);
                    this.PlatformThatThisIsOn = null;
                }
            }

            // Clear out weird state fields.
            if (OnGround || IsClimbingLadder || IsClimbingVine)
            {
                PoisonPlatforms.Clear();
                isInJumpFromSand = false;
                isInJumpFromIce = false;
                isInJumpFromGround = false;
                jumpButtonHeldDownTimer = 0f;
                IsJustShotOutOfCannon = false;
            }

            // Stop the jump if they let go of the button or hit a ceiling or something.
            if (!InputManager.CurrentAction.jump || OnCeiling || this.velocity.Y >= 0)
            {
                jumpButtonHeldDownTimer = 0;
            }
            else if (jumpButtonHeldDownTimer > 0)
            {
                jumpButtonHeldDownTimer -= elapsed;
            }

            // Jump down from platform(s). 
            if (InputManager.CurrentAction.jump && !InputManager.PreviousAction.jump && InputManager.CurrentAction.down && OnPlatform)
            {
                // Find every platform below the player and mark them all as poison.
                // extend left and right a bit in case the player is moving.
                var belowPlayerRect = new Rectangle(this.CollisionRectangle.Left - 32, this.CollisionRectangle.Bottom, this.CollisionRectangle.Width + 64, 3);

                foreach (var platform in Game1.Platforms)
                {
                    if (belowPlayerRect.Intersects(platform.CollisionRectangle))
                    {
                        this.PoisonPlatforms.Add(platform);
                    }
                }
                SoundManager.PlaySound("Jump");

            }
            else if (InputManager.CurrentAction.jump && !InputManager.PreviousAction.jump && OnGround)
            {
                // Regular jump.
                this.velocity.Y -= jumpBoost;
                _state = MacState.Jumping;
                SoundManager.PlaySound("Jump");
                if (isSand)
                {
                    isInJumpFromSand = true;
                }
                else if (isIce)
                {
                    isInJumpFromIce = true;
                }
                isInJumpFromGround = true;
                jumpButtonHeldDownTimer = maxJumpButtonHeldDownTime;
                onGround = false;
            }
            else if (InputManager.CurrentAction.jump && !InputManager.PreviousAction.jump && !OnGround && HasWings && this.Velocity.Y >= 0)
            {
                // Infinite Jump Jump.
                this.velocity.Y = -jumpBoost * 1.5f;
                _state = MacState.Jumping;
                SoundManager.PlaySound("Jump");
                isInJumpFromGround = true;
            }
            else if (InputManager.CurrentAction.jump
                && !InputManager.PreviousAction.jump
                && IsClimbingLadder)
            {
                // Jump off ladder
                this.velocity.Y -= (jumpBoost / 2); // weaker jump
                SoundManager.PlaySound("Jump");

                // block their ability to climb ladders until they release up. This prevents you from
                // insta-climbing the ladder above you.
                if (IsClimbingLadder && InputManager.CurrentAction.up)
                {
                    canClimbLadders = false;
                }
                _state = MacState.Jumping;
            }
            else if (InputManager.CurrentAction.jump
                && !InputManager.PreviousAction.jump
                && IsClimbingVine)
            {
                // Jump off a vine.
                canClimbVines = false;
                yPositionWhenLastOnVine = this.worldLocation.Y;
                _state = MacState.Jumping;
                this.velocity = new Vector2(300, -250);
                if (Flipped)
                {
                    this.velocity.X *= -1;
                }

                SoundManager.PlaySound("Jump");
            }

            // Unset canclimb ladders if they release up.
            if (!InputManager.CurrentAction.up || onGround)
            {
                canClimbLadders = true;
            }

            // Climbing Vine
            var tileAtCenter = Game1.CurrentMap.GetMapSquareAtPixel(this.CollisionCenter);
            var isOverVine = tileAtCenter != null && tileAtCenter.IsVine;

            if (!IsClimbingVine && canClimbVines && isOverVine && (!OnGround || InputManager.CurrentAction.up) && pickedUpObject == null)
            {
                _state = MacState.ClimbingVine;
            }

            if (IsClimbingVine)
            {

                Vector2 vineTile;
                if (!Flipped)
                {
                    vineTile = Game1.CurrentMap.GetCellByPixel(new Vector2(this.CollisionRectangle.Right, this.CollisionCenter.Y));
                }
                else
                {
                    vineTile = Game1.CurrentMap.GetCellByPixel(new Vector2(this.CollisionRectangle.Left, this.CollisionCenter.Y));
                }

                // You can't move left and right on the vine, but Mac can flip.
                if (!Flipped && InputManager.CurrentAction.left)
                {
                    cameraTrackingTimer = 0.2f;
                    Flipped = true;
                }
                else if (Flipped && InputManager.CurrentAction.right)
                {
                    cameraTrackingTimer = 0.2f;
                    Flipped = false;
                }

                if (!Flipped)
                {
                    this.worldLocation.X = (TileMap.TileSize * vineTile.X) + 8;
                }
                else
                {
                    this.worldLocation.X = TileMap.TileSize * vineTile.X + 24;
                }

                this.velocity.X = 0;

                if (InputManager.CurrentAction.up)
                {
                    this.velocity.Y = -climbingSpeed;
                }
                else if (InputManager.CurrentAction.down)
                {
                    this.velocity.Y = climbingSpeed;
                }
                else
                {
                    this.velocity.Y = 0;
                }

                // Don't let them climb too high on the vine.
                if ((tileAtTop == null || !tileAtTop.IsVine) && InputManager.CurrentAction.up)
                {
                    this.velocity.Y = 0;
                }
            }

            // Unset canClimbVines if they move enough away from the vine.
            if (!canClimbVines && Math.Abs(this.worldLocation.Y - yPositionWhenLastOnVine) > 20)
            {
                canClimbVines = true;
            }

            // Just in case.
            if (tileAtCenter != null && !tileAtCenter.IsVine && IsClimbingVine)
            {
                _state = MacState.Idle;
            }

            // Level.cs will check door collisions if this is true.
            this.InteractButtonPressedThisFrame = 
                (InputManager.CurrentAction.up && !InputManager.PreviousAction.up) || 
                (InputManager.CurrentAction.action && !InputManager.PreviousAction.action);

            // slightly sliding is not sliding, so we want to see the idle animation.
            if (velocity.X < 80 && velocity.X > -80 && IsSliding)
            {
                _state = MacState.Idle;
            }

            // stop the player if they are nearly stopped so you don't get weird 1px movement.
            if (velocity.X < 24 && velocity.X > -24 && !IsRunning && onGround)
            {
                velocity.X = 0;
            }

            if (!IsClimbingLadder && !IsClimbingVine)
            {
                if (!OnGround && velocity.Y > 0)
                {
                    _state = MacState.Jumping;
                }
                else if (!OnGround && velocity.Y < 0)
                {
                    _state = MacState.Falling;
                }
                else if ((IsJumping || IsFalling) && OnGround && this.Velocity.Y >= 0)
                {
                    _state = MacState.Idle;
                }
            }
            
            var isClimbingAnimationPlaying = (IsClimbingLadder || IsClimbingVine) && velocity != Vector2.Zero;

            if (isClimbingAnimationPlaying)
            {
                playClimbSoundTimer -= elapsed;
                if (playClimbSoundTimer <= 0f)
                {
                    SoundManager.PlaySound("Climb", 0.3f, 0.3f);
                    playClimbSoundTimer += 0.15f;
                }
            }
            else
            {
                playClimbSoundTimer = 0f;
            }

            // Bound the player to the map left and right.
            if (CollisionRectangle.X < Game1.CurrentMap.GetWorldRectangle().X && velocity.X < 0)
            {
                velocity.X = 0;
            }
            else if (CollisionRectangle.Right > Game1.CurrentMap.GetWorldRectangle().Right && velocity.X > 0)
            {
                velocity.X = 0;
            }

            var isAbleToPickup = !IsClimbingLadder && !IsClimbingVine && !IsInMineCart && !HasWings && !IsInCannon && !IsInWater;

            bool didPickUpObject = false;
            bool didKickObject = false;

            // Pick up objects
            if (pickedUpObject == null && pickUpAgainTimer <= 0 && isAbleToPickup && InputManager.CurrentAction.action)
            {
                var bottomPickUpRectangle = new Rectangle(this.CollisionRectangle.Left, this.CollisionRectangle.Bottom, this.collisionRectangle.Width, 8);
                Rectangle frontPickUpRectangle;
                if (IsFacingRight())
                {
                    frontPickUpRectangle = new Rectangle(this.CollisionRectangle.Left + 8, this.CollisionRectangle.Top, CollisionRectangle.Width, this.CollisionRectangle.Height);
                }
                else
                {
                    frontPickUpRectangle = new Rectangle(this.CollisionRectangle.Left - 8, this.CollisionRectangle.Top, CollisionRectangle.Width, this.CollisionRectangle.Height);
                }
                foreach (var puo in Game1.CurrentLevel.PickupObjects)
                {
                    if (puo.CanBePickedUp && (frontPickUpRectangle.Intersects(puo.CollisionRectangle) || bottomPickUpRectangle.Intersects(puo.CollisionRectangle)))
                    {
                        this.pickedUpObject = puo;
                        puo.Pickup();
                        didPickUpObject = true;
                        break;
                    }
                }
            }

            // Drop it
            if (pickedUpObject != null && (!InputManager.CurrentAction.action || !isAbleToPickup))
            {
                pickedUpObject.Drop();
                recentlyDropped = pickedUpObject;
                kickTimer = 0f;
                pickUpAgainTimer = 1f;
                pickedUpObject = null;
            }

            // If Mac recently dropped a block he can kick it away for a short time.
            if (recentlyDropped != null)
            {
                // Check if Mac kicked it
                if (InputManager.CurrentAction.action && !InputManager.PreviousAction.action)
                {
                    // Kick the item

                    var isStraightUp = false;
                    if (InputManager.CurrentAction.up && !InputManager.CurrentAction.left && !InputManager.CurrentAction.right)
                    {
                        isStraightUp = true;
                    }

                    recentlyDropped.Kick(isStraightUp);
                    recentlyDropped = null;
                    didKickObject = true;
                }

                // Item is only kickable for a short time.
                kickTimer += elapsed;
                if (kickTimer >= 0.15f)
                {
                    kickTimer = 0f;
                    recentlyDropped = null;
                }

            }

            // Don't let Mac pick up objects for a short time. Otherwise he can re-pick it up mid-air.
            if (pickUpAgainTimer > 0)
            {
                pickUpAgainTimer -= elapsed;
            }

            // Mac throws an apple if he has them.
            if (HasApples && appleCooldownTimer < appleCooldownTime)
            {
                appleCooldownTimer += elapsed;
            }

            if (HasApples && InputManager.CurrentAction.action && !InputManager.PreviousAction.action && appleCooldownTimer >= appleCooldownTime && !didPickUpObject && !didKickObject)
            {
                var apple = Apples.TryGetObject();
                if (apple != null)
                {
                    apple.Enabled = true;
                    apple.WorldLocation = this.WorldLocation;
                    apple.Velocity = new Vector2(280, 0);
                    if (Flipped)
                    {
                        apple.Velocity *= -1;
                    }
                    appleCooldownTimer = 0;
                }
            }

            if (HasShovel)
            {
                if (InputManager.CurrentAction.action && !InputManager.PreviousAction.action && !didPickUpObject && !didKickObject)
                {
                    var digDirection = Flipped ? DigDirection.Left : DigDirection.Right;
                    if(InputManager.CurrentAction.up)
                    {                         
                        digDirection = DigDirection.Up;
                    }
                    else if (InputManager.CurrentAction.down)
                    {
                        digDirection = DigDirection.Down;
                    }
                    else if (InputManager.CurrentAction.left)
                    {
                        digDirection = DigDirection.Left;
                    }
                    else if (InputManager.CurrentAction.right)
                    {
                        digDirection = DigDirection.Right;
                    }
                    _shovel.TryDig(digDirection);
                }
            }

            string nextAnimation;
            if (IsJumping)
            {
                nextAnimation = "jump";
            }
            else if (IsFalling)
            {
                nextAnimation = "fall";
            }
            else if (IsRunning)
            {
                nextAnimation = "run";
            }
            else if (IsSliding)
            {
                nextAnimation = "slide";
            }
            else if (IsClimbingLadder)
            {
                nextAnimation = "climbLadder";
                climbingLadderAnimation.IsPaused = !isClimbingAnimationPlaying;
            }
            else if (IsClimbingVine)
            {
                nextAnimation = "climbVine";
                climbingVineAnimation.IsPaused = !isClimbingAnimationPlaying;
            }
            else
            {
                nextAnimation = "idle";
            }

            if (animations.CurrentAnimationName != nextAnimation)
            {
                animations.Play(nextAnimation);
            }

            if (!Game1.Camera.CanScrollLeft)
            {
                // If you aren't allowed to scroll left (boss fight) your left movement becomes blocked by the camera
                if (this.CollisionRectangle.Left < Game1.Camera.ViewPort.Left && this.velocity.X < 0)
                {
                    this.velocity.X = 0;
                    this.worldLocation.X = Game1.Camera.ViewPort.Left + (CollisionRectangle.Width / 2) - 1;
                }
            }

        }

        private void HandleMineCartInputs(float elapsed)
        {
            if (animations.CurrentAnimationName != "mineCart")
            {
                animations.Play("mineCart");
            }

            if (OnGround)
            {
                SoundManager.PlayMinecart();
            }

            // Cert
            var centerTile = Game1.CurrentMap.GetMapSquareAtPixel(this.WorldCenter);
            if (centerTile != null && centerTile.IsDestroyMinecart)
            {
                _state = MacState.Idle;
                IsInMineCart = false;
                SoundManager.PlaySound("Break");
                SoundManager.StopMinecart();
                return;
            }

            this.velocity.X = 350;
            if (Flipped)
            {
                this.velocity.X *= -1;
            }

            // Jump
            if (InputManager.CurrentAction.jump && !InputManager.PreviousAction.jump && OnGround)
            {
                // Regular jump.
                this.velocity.Y -= 550;
                SoundManager.PlayMinecartJump();
                SoundManager.StopMinecart();
            }

            // If the tile to the right is colliding, flip the player
            if (!Flipped)
            {
                var tileToTheRight = Game1.CurrentMap.GetMapSquareAtPixel(new Vector2(this.CollisionRectangle.Right + 1, this.CollisionRectangle.Center.Y));
                if (tileToTheRight != null && !tileToTheRight.Passable && !tileToTheRight.IsSlope())
                {
                    this.Flipped = true;
                    SoundManager.PlaySound("Bounce");
                }
            }
            else
            {
                var tileToTheLeft = Game1.CurrentMap.GetMapSquareAtPixel(new Vector2(this.CollisionRectangle.Left - 1, this.CollisionRectangle.Center.Y));
                if (tileToTheLeft != null && !tileToTheLeft.Passable && !tileToTheLeft.IsSlope())
                {
                    this.Flipped = false;
                    SoundManager.PlaySound("Bounce");
                }
            }
        }

        private void HandleKnockedDownInputs(float elapsed)
        {

            // Friction
            if (OnGround)
            {
                this.velocity.X -= (this.velocity.X * 2.5f * elapsed);
            }
            animations.Play("knockedDown");
            // When you're knocked down you can't do anything until some time passes.
            TimerManager.AddNewTimer(0.5f, () =>
            {
                _state = MacState.Idle;
            }); 
        }

        public void EnterCannon(Cannon cannon)
        {
            this.WorldLocation = cannon.WorldLocation;
            this.Velocity = Vector2.Zero;
            this.CannonYouAreIn = cannon;
            this.IsJustShotOutOfCannon = false;
            this.IsAffectedByGravity = false;
            this._state = MacState.Idle;
            this.animations.Play("idle");
        }

        public void ShootOutOfCannon(Cannon cannon, Vector2 velocity)
        {
            this.velocity = velocity;
            this.IsJustShotOutOfCannon = true;
            this.CannonYouAreIn = null;

            this.IsJustShotOutOfCannon = true;

            // A regular cannon fires Mac for half a second. A supershot leaves
            // him in the air until he hits something.
            if (cannon.IsSuperShot)
            {
                timeRemainingToBeShotOutOfCannon = 0;
            }
            else
            { 
                timeRemainingToBeShotOutOfCannon = 0.1f;
            }

            if (this.velocity.X > 0)
            {
                this.Flipped = false;
            }
            else if (this.velocity.X < 0)
            {
                this.Flipped = true;
            }
        }

        private void HandleCannonInputs(float elapsed)
        {
            this.WorldLocation = CannonYouAreIn.WorldLocation;
            if (this.CannonYouAreIn.PlayerCanShootOut && InputManager.CurrentAction.jump && !InputManager.PreviousAction.jump)
            {
                this.CannonYouAreIn.Shoot();
                this.IsJustShotOutOfCannon = true;

                TimerManager.AddNewTimer(0.5f, () =>
                {
                    IsJustShotOutOfCannon = false;
                });
            }
        }

        /// <summary>
        ///  When you are shot out of a cannon you will smash through sand and not be effected
        ///  by gravity for a period of time. It stops if you hit a wall.
        /// </summary>
        private void HandleShotOutOfCannonInputs(float elapsed)
        {
            // Check the pixel in the direction Mac is being shot.
            int pixelToCheckX = this.CollisionRectangle.Center.X;
            if (this.velocity.X > 0)
            {
                pixelToCheckX = this.CollisionRectangle.Right + 12;
            }
            else if (this.velocity.X < 0)
            {
                pixelToCheckX = this.CollisionRectangle.Left - 12;
            }
            int pixelToCheckY = this.CollisionRectangle.Center.Y;
            if (this.velocity.Y > 0)
            {
                pixelToCheckY = this.CollisionRectangle.Bottom + 12;
            }
            else if (this.velocity.Y < 0)
            {
                pixelToCheckY = this.CollisionRectangle.Top - 12;
            }
            var tile = Game1.CurrentMap.GetMapSquareAtPixel(new Vector2(pixelToCheckX, pixelToCheckY));
            if (tile != null && tile.IsSand)
            {
                tile.DigSand();
            }
            else if (tile == null || !tile.Passable)
            {
                // If you hit a wall or something you stop being shot out of the cannon.
                this.velocity = Vector2.Zero;
                this.IsJustShotOutOfCannon = false;
                timeRemainingToBeShotOutOfCannon = 0;
            }

            // Check if you are shot out for a limited time.
            if (timeRemainingToBeShotOutOfCannon > 0)
            {
                timeRemainingToBeShotOutOfCannon -= elapsed;
                if (timeRemainingToBeShotOutOfCannon <= 0)
                {
                    this.IsJustShotOutOfCannon = false;
                }
            }

            // Not sure what happened here but if you aren't moving you can't 
            // be shooting out of a cannon now can you?
            if (this.velocity == Vector2.Zero)
            {
                this.IsJustShotOutOfCannon = false;
                timeRemainingToBeShotOutOfCannon = 0;
            }
        }

        private void HandleWaterInputs(float elapsed)
        {

            IsAffectedByGravity = false;

            if (animations.CurrentAnimationName != "swim")
            {
                animations.Play("swim");
            }

            Vector2 headLocation = new Vector2(this.CollisionRectangle.Center.X, this.CollisionRectangle.Top);
            bool isHeadUnderWater = Game1.CurrentMap?.GetMapSquareAtPixel(headLocation + new Vector2(0, -20))?.IsWater ?? false;
            bool justBelowHeadUnderWater = Game1.CurrentMap?.GetMapSquareAtPixel(headLocation)?.IsWater ?? false;

            // If you are in water you can't jump very high.
            if (InputManager.CurrentAction.jump && !InputManager.PreviousAction.jump)
            {
                if (isHeadUnderWater)
                {
                    // weak water 'jump'
                    this.velocity.Y -= 100;

                    // random pitch
                    var pitch = Game1.Randy.NextFloat() / 2f;

                    SoundManager.PlaySound("Swim", 0.5f, pitch);
                }
                else
                {
                    IsJumpingOutOfWater = true;
                    IsInWater = false;
                    SoundManager.PlaySound("Jump");
                    // TODO: Too much!
                    this.velocity.Y =- 500;
                }
                
            }

            const float swimSpeed = 250f;
            const float maxSpeed = 100f;

            if (InputManager.CurrentAction.right && !InputManager.CurrentAction.left)
            {
                this.velocity.X += swimSpeed * elapsed;
                Flipped = false;
            }
            else if (InputManager.CurrentAction.left && !InputManager.CurrentAction.right)
            {
                this.velocity.X -= 100 * elapsed;
                Flipped = true;
            }
            else
            {
                this.velocity.X -= (this.velocity.X * 2.5f * elapsed);
            }

            if (InputManager.CurrentAction.down && !InputManager.CurrentAction.up)
            {
                this.velocity.Y += swimSpeed * elapsed;
            }
            else if (InputManager.CurrentAction.up && !InputManager.CurrentAction.down)
            {
                this.velocity.Y -= swimSpeed * elapsed;
            }
            else
            {
                // They slowly float down if you don't press anything.
                this.velocity.Y += 30 * elapsed;
            }

            if (isHeadUnderWater)
            {
                this.velocity.Y = MathHelper.Clamp(this.velocity.Y, -maxSpeed, maxSpeed);
                this.velocity.X = MathHelper.Clamp(this.velocity.X, -maxSpeed, maxSpeed);
            }

            // If they are near the top, stop their movement so that they have a head above water but they don't pop out
            // this makes it easier to jump out of the water.
            if (!IsJumpingOutOfWater && !justBelowHeadUnderWater)
            {
                this.velocity.Y = MathHelper.Clamp(this.velocity.Y, 0, this.velocity.Y);
            }
        }

        public void Kill()
        {
            Health = 0;
            Enabled = false;
            this.CurrentItem = null;
            EffectsManager.EnemyPop(WorldCenter, 10, Color.Yellow, 200f);
            SoundManager.PlaySound("MacDeath", 0.5f);
            MenuManager.AddMenu(_deadMenu);
            IsInMineCart = false;
            SoundManager.StopMinecart();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsInvisible) return;

            if (IsInCannon) return;

            if (HasWings)
            {
                wings.Draw(spriteBatch);
            }

            if (HasShovel)
            {
                _shovel.Draw(spriteBatch);
            }

            foreach (var apple in Apples.RawList)
            {
                if (apple.Enabled)
                {
                    apple.Draw(spriteBatch);
                }
            }
            base.Draw(spriteBatch);

        }

        public bool IsFacingRight()
        {
            return !this.Flipped;
        }

        public bool IsFacingLeft()
        {
            return !IsFacingRight();
        }

        public Vector2 GetCameraPosition(Camera camera)
        {
            // For a brief time the camera will slowly track Mac so that it doesn't adjust too quickly after he does some kind of 
            // snapping or quick moving action.
            if (cameraTrackingTimer >= 0)
            {
                var cameraPosition = camera.Position + ((this.worldLocation - camera.Position) * 0.1f);
                return cameraPosition;
            }
            
            // Normally the Camera tracks the player
            return this.worldLocation;
        }

        public void AddUnlockedDoor(string doorName)
        {
            var doors = Game1.StorageState.Levels[Game1.CurrentLevel.LevelNumber].UnlockedDoors;
            if (!doors.Contains(doorName))
            {
                doors.Add(doorName);
            }
        }
    }
}
