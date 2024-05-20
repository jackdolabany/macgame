using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MacGame.Platforms;
using System;
using TileEngine;
using MacGame.Enemies;
using MacGame.DisplayComponents;
using MacGame.Items;
using System.Collections.Generic;

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
        IsKnockedDown
    }

    public class Player : GameObject
    {
        AnimationDisplay animations;

        public const int MaxHealth = 5;

        public int Health { get; set; } = MaxHealth;

        public int Tacos = 0;
        public int CricketCoinCount = 0;

        public InputManager InputManager { get; private set; }

        private DeadMenu _deadMenu;

        private const float maxAcceleration = 600;
        private const float maxSpeed = 500;

        private MacState _state = MacState.Idle;

        private bool IsRunning => _state == MacState.Running;
        private bool IsJumping => _state == MacState.Jumping;
        private bool IsSliding => _state == MacState.Sliding;
        private bool IsFalling => _state == MacState.Falling;
        private bool IsClimbingLadder => _state == MacState.ClimbingLadder;
        private bool IsClimbingVine => _state == MacState.ClimbingVine;

        // Track if the player jumped off of sand or ice so that we can maintain the adjusted
        // movement speed through the jump.
        private bool isInJumpFromSand = false;
        private bool isInJumpFromIce = false;
        private bool isInJumpFromGround = false;
        float jumpButtonHeldDownTimer = 0f;
        const float maxJumpButtonHeldDownTime = 0.5f;
        public Vector2 NormalGravity;
        public Vector2 JumpGravity;

        // Add time here to make the camera track Mac slowly for a period of time. This will help 
        // move the camera more naturally for a period when he does "snapping" actions like snapping to the
        // other side of a vine, or snapping to other objects.
        private float cameraTrackingTimer = 0f;
        private bool IsKnockedDown => _state == MacState.IsKnockedDown;

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
        private bool HasInfiniteJump
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

        private float InfiniteJumpTimer = 0f;
        public Item? CurrentItem = null;

        private bool HasShovel
        {
            get
            {
                return this.CurrentItem is Shovel;
            }
        }
        private MacShovel shovel;

        /// <summary>
        /// if Mac is using the wing, it'll render behind him.
        /// </summary>
        MacWings wings;

        public bool HasRedKey { get; set; } = false;
        public bool HasGreenKey { get; set; } = false;
        public bool HasBlueKey { get; set; } = false;

        public bool IsInvisible { get; set; } = false;

        public bool IsInCannon 
        {
            get
            {
                return CannonYouAreIn != null;
            }
        }
        /// <summary>
        /// After being shot out of a cannon you are not effected by gravity for a period of time.
        /// </summary>
        public bool IsJustShotOutOfCannon { get; set; } = false;

        public Cannon CannonYouAreIn { get; set; }

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

            Enabled = true;

            // TODO: Whatever
            DisplayComponent.DrawDepth = 0.5f;

            this.IsAbleToMoveOutsideOfWorld = true;
            this.IsAbleToSurviveOutsideOfWorld = true;
            this.IsAffectedByForces = false;
            this.isEnemyTileColliding = false;

            this.IsAffectedByGravity = true;

            this.IsAffectedByPlatforms = true;

            SetCenteredCollisionRectangle(6, 7);

            InputManager = inputManager;
            _deadMenu = deadMenu;

            // Use this one wing image to draw flapping wings.
            wings = new MacWings(this, textures);

            Apples = new ObjectPool<Apple>(2);
            Apples.AddObject(new Apple(content, 0, 0, this, Game1.Camera));
            Apples.AddObject(new Apple(content, 0, 0, this, Game1.Camera));

            shovel = new MacShovel(this, textures);

            NormalGravity = Gravity;
            JumpGravity = NormalGravity * 0.5f;
        }

        public override void SetDrawDepth(float depth)
        {
            this.DisplayComponent.DrawDepth = depth;
            this.shovel.SetDrawDepth(DrawDepth + Game1.MIN_DRAW_INCREMENT);
            this.wings.SetDrawDepth(DrawDepth + Game1.MIN_DRAW_INCREMENT);
            this.Apples.RawList.ForEach(a => a.SetDrawDepth(DrawDepth + Game1.MIN_DRAW_INCREMENT));
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            InteractButtonPressedThisFrame = false;

            _previousCollisionRectangle = this.CollisionRectangle;

            if(cameraTrackingTimer >= 0)
            {                 
                cameraTrackingTimer -= elapsed;
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
            else
            {
                HandleInputs(elapsed);
            }

            if (HasInfiniteJump)
            {
                wings.Update(gameTime, elapsed);
            }
            if (HasShovel)
            {
                shovel.Update(gameTime, elapsed);
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

            base.Update(gameTime, elapsed);
            
            foreach (var apple in Apples.RawList)
            {
                if (apple.Enabled)
                {
                    apple.Update(gameTime, elapsed);
                }
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
                HasRedKey = false;
                HasGreenKey = false;
                HasBlueKey = false;
            }

            Enabled = true;
            Velocity = Vector2.Zero;
            IsInMineCart = false;
            IsInvisible = false;
        }

        /// <summary>
        /// Send the player sliding out of a hub door after being killed or whatever.
        /// </summary>
        public void SlideOutOfDoor(Vector2 doorLocation)
        {
            this.worldLocation = doorLocation + new Vector2(0, -4);
            this.Flipped = true;
            this.velocity = new Vector2(280, 0);
            this._state = MacState.IsKnockedDown;
            // TODO: Play sound
        }

        public void CheckEnemyInteractions(Enemy enemy)
        {
            if (enemy.Alive)
            {
                // Check body collisions
                if (CollisionRectangle.Intersects(enemy.CollisionRectangle))
                {
                    enemy.HandleCustomPlayerCollision(this);

                    // Pad 1 pixel to make it a little easier
                    var wasAboveEnemy = _previousCollisionRectangle.Bottom - 8 <= enemy.CollisionRectangle.Top;

                    if (enemy.Alive && !enemy.IsInvincibleAfterHit && wasAboveEnemy && !IsClimbingLadder && !IsClimbingVine)
                    {
                        // If the player was above the enemy, the enemy was jumped on and takes a hit.
                        enemy.TakeHit(1, Vector2.Zero);
                        velocity.Y = -450;
                    }
                    else if (enemy.Alive && !enemy.IsInvincibleAfterHit)
                    {
                        TakeHit(enemy);
                    }

                }
                else
                {
                    foreach (var apple in Apples.RawList)
                    {
                        if (apple.Enabled)
                        {
                            if(apple.CollisionRectangle.Intersects(enemy.CollisionRectangle))
                            {
                                apple.Smash();
                                enemy.TakeHit(1, Vector2.Zero);
                            }
                        }
                    }
                }
            }

        }

        public void TakeHit(Enemy enemy)
        {
            if (IsInvincible) return;

            // player takes a hit.
            Health -= 1;
            if (Health <= 0)
            {
                Kill();
            }
            else
            {
                invincibleTimeRemaining = 0.75f;
                SoundManager.PlaySound("take_hit");
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
            jumpBoost = 500;

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
            if (!InputManager.CurrentAction.attack && (onGround || IsClimbingLadder))
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

            if (isInJumpFromGround && jumpButtonHeldDownTimer > 0)
            {
                this.Gravity = JumpGravity;
            }
            else
            {
                this.Gravity = NormalGravity;
            }

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
                _state = MacState.ClimbingLadder;
                this.velocity.Y = climbingSpeed;
                this.PoisonPlatforms.Add(PlatformThatThisIsOn);
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
                var belowPlayerRect = new Rectangle(this.CollisionRectangle.Left, this.CollisionRectangle.Bottom, this.CollisionRectangle.Width, 3);

                foreach (var platform in Game1.Platforms)
                {
                    if (belowPlayerRect.Intersects(platform.CollisionRectangle))
                    {
                        this.PoisonPlatforms.Add(platform);
                    }
                }
                SoundManager.PlaySound("jump");

            }
            else if (InputManager.CurrentAction.jump && !InputManager.PreviousAction.jump && OnGround)
            {
                // Regular jump.
                this.velocity.Y -= jumpBoost;
                _state = MacState.Jumping;
                SoundManager.PlaySound("jump");
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
            else if (InputManager.CurrentAction.jump && !InputManager.PreviousAction.jump && !OnGround && HasInfiniteJump && this.Velocity.Y >= 0)
            {
                // Infinite Jump Jump.
                this.velocity.Y = -jumpBoost;
                _state = MacState.Jumping;
                SoundManager.PlaySound("jump");
            }
            else if (InputManager.CurrentAction.jump
                && !InputManager.PreviousAction.jump
                && IsClimbingLadder)
            {
                // Jump off ladder
                this.velocity.Y -= (jumpBoost / 2); // weaker jump
                SoundManager.PlaySound("jump");

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

                SoundManager.PlaySound("jump");
            }

            // Unset canclimb ladders if they release up.
            if (!InputManager.CurrentAction.up || onGround)
            {
                canClimbLadders = true;
            }

            // Climbing Vine
            var tileAtCenter = Game1.CurrentMap.GetMapSquareAtPixel(this.CollisionCenter);
            var isOverVine = tileAtCenter != null && tileAtCenter.IsVine;

            if (!IsClimbingVine && canClimbVines && isOverVine && (!OnGround || InputManager.CurrentAction.up))
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
            this.InteractButtonPressedThisFrame = InputManager.CurrentAction.up && !InputManager.PreviousAction.up;

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
                    SoundManager.PlaySound("climb", 0.7f, 0.3f);
                    playClimbSoundTimer += 0.15f;
                }
            }
            else
            {
                playClimbSoundTimer = 0f;
            }

            // Limit the time the player has the infinite jump powerup. They'll only lose it after some time if they hit the ground.
            if (HasInfiniteJump)
            {
                InfiniteJumpTimer += elapsed;
                if ((InfiniteJumpTimer >= 6f && OnGround) || IsClimbingLadder || IsClimbingVine)
                {
                    InfiniteJumpTimer = 0;
                    this.CurrentItem = null;
                }
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

            // Mac throws an apple if he has them.
            if (HasApples && appleCooldownTimer < appleCooldownTime)
            {
                appleCooldownTimer += elapsed;
            }

            if (InputManager.CurrentAction.attack && !InputManager.PreviousAction.attack && HasApples && appleCooldownTimer >= appleCooldownTime)
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
                if (InputManager.CurrentAction.attack && !InputManager.PreviousAction.attack)
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
                    shovel.TryDig(digDirection);
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

            // If you land on a tile that isn't track, then exit the minecart.
            var bottomLeftTile = Game1.CurrentMap.GetMapSquareAtPixel(this.worldLocation + new Vector2(-10, -4));
            var bottomLeftIsTrack = bottomLeftTile != null && bottomLeftTile.IsMinecartTrack;
            
            var bottomRightTile = Game1.CurrentMap.GetMapSquareAtPixel(this.worldLocation + new Vector2(10, -4));
            var bottomRightIsTrack = bottomRightTile != null && bottomRightTile.IsMinecartTrack;

            if (OnGround && !bottomLeftIsTrack && !bottomRightIsTrack)
            {
                _state = MacState.Idle;
                IsInMineCart = false;
                return;
            }

            this.velocity.X = 240f;
            if (Flipped)
            {
                this.velocity.X *= -1;
            }

            // Jump
            if (InputManager.CurrentAction.jump && !InputManager.PreviousAction.jump && OnGround)
            {
                // Regular jump.
                this.velocity.Y -= 450;
                SoundManager.PlaySound("jump");
            }

            // If the tile to the right is colliding, flip the player
            if (!Flipped)
            {
                var tileToTheRight = Game1.CurrentMap.GetMapSquareAtPixel(new Vector2(this.CollisionRectangle.Right + 1, this.CollisionRectangle.Center.Y));
                if (tileToTheRight != null && !tileToTheRight.Passable)
                {
                    this.Flipped = true;
                }
            }
            else
            {
                var tileToTheLeft = Game1.CurrentMap.GetMapSquareAtPixel(new Vector2(this.CollisionRectangle.Left - 1, this.CollisionRectangle.Center.Y));
                if (tileToTheLeft != null && !tileToTheLeft.Passable)
                {
                    this.Flipped = false;
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
            this.CannonYouAreIn = cannon;
            this.IsJustShotOutOfCannon = false;
            this.IsAffectedByGravity = false;
            // TODO: Play the sound of the player entering the cannon.
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

        public void Kill()
        {
            Health = 0;
            Enabled = false;
            this.CurrentItem = null;
            EffectsManager.EnemyPop(WorldCenter, 10, Color.Yellow, 200f);
            SoundManager.PlaySound("mac_death");
            MenuManager.AddMenu(_deadMenu);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            if (IsInvisible) return;

            if (IsInCannon) return;

            if(HasInfiniteJump)
            {
                wings.Draw(spriteBatch);
            }

            if (HasShovel)
            {
                shovel.Draw(spriteBatch);
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
            if (Game1.State.UnlockedDoors.ContainsKey(Game1.CurrentLevel.LevelNumber))
            {
                if (!Game1.State.UnlockedDoors[Game1.CurrentLevel.LevelNumber].Contains(doorName))
                {
                    Game1.State.UnlockedDoors[Game1.CurrentLevel.LevelNumber].Add(doorName);
                }
            }
            else
            {
                Game1.State.UnlockedDoors.Add(Game1.CurrentLevel.LevelNumber, new HashSet<string> { doorName });
            }
        }
    }
}
