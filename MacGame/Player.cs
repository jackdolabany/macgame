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
using MacGame.Behaviors;

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
        NPC,

        /// <summary>
        /// Mac will be idle for a little bit while he's disabling the water bombs which are only
        /// on the TMNT like water bomb level.
        /// </summary>
        DisablingWaterBomb,

        /// <summary>
        /// Mac is a space ship in shooting levels.
        /// </summary>
        SpaceShip,
    }

    /// <summary>
    /// How strong Mac's shot is in his spaceship.
    /// </summary>
    public enum ShotPower
    {
        Single,
        Double,
        Charge
    }

    public class Player : GameObject
    {
        AnimationDisplay animations;

        public const int MaxHealth = 5;

        public ShotPower ShotPower { get; private set; } = ShotPower.Single; 

        public int Health { get; set; } = MaxHealth;

        public int Tacos = 0;
        public int SockCount = 0;

        public InputManager InputManager { get; private set; }

        private DeadMenu _deadMenu;

        private const float maxAcceleration = 600;
        private const float maxSpeed = 500;
        private const float maxFlyingSpeed = 450;

        private MacState _state = MacState.Idle;

        /// <summary>
        /// If Mac is an NPC he'll walk towards this target.
        /// </summary>
        public Vector2 NpcModeTarget { get; set; }

        /// <summary>
        /// NPC behavior so Mac can go to a location.
        /// </summary>
        private MoveToLocation _moveToLocation { get; set; }
        private JustIdle _justIdle { get; set; }

        private bool IsRunning => _state == MacState.Running;
        private bool IsJumping => _state == MacState.Jumping;
        private bool IsSliding => _state == MacState.Sliding;
        private bool IsFalling => _state == MacState.Falling;
        private bool IsClimbingLadder => _state == MacState.ClimbingLadder;
        private bool IsClimbingVine => _state == MacState.ClimbingVine;
        private bool IsDisablingWaterBomb => _state == MacState.DisablingWaterBomb;
        private bool IsInSpaceShip => _state == MacState.SpaceShip;

        IPickupObject? pickedUpObject;

        // Track a pickup object that you recently dropped so you can kick it out.
        IPickupObject? recentlyDropped;
        float kickTimer = 0;

        // Can't pick up Item. Use this so Mac can't insta pick up an item he just dropped or kicked.
        float pickUpAgainTimer = 0;

        // Track if the player jumped off of ice so that we can maintain the adjusted
        // movement speed through the jump.
        private bool isInJumpFromIce = false;
        private bool isInJumpFromGround = false;
        float jumpButtonHeldDownTimer = 0f;
        const float maxJumpButtonHeldDownTime = 0.5f;

        public bool IsInWater = false;
        public bool IsJumpingOutOfWater = false;

        // Set smoothMoveCameraToTarget to true to make the camera track Mac slowly for a period of time. This will help 
        // move the camera more naturally for a period when he does "snapping" actions like snapping to the
        // other side of a vine, or snapping to other objects.
        private bool smoothMoveCameraToTarget = false;
        float maxCameraVelocity = 3000;
        const float minCameraVelocity = 200;
        float cameraAcceleration = 600f; 
        private float cameraVelocity = minCameraVelocity;

        private float noMoveTimer = 0f;

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
        const int mineCartVelocity = 350;
        public bool IsInSub = false;
        private Rectangle normalCollisionRectangle;

        private Submarine subPlayerIsIn = null;

        private bool HasWings
        {
            get
            {
                return this.CurrentItem is InfiniteJump;
            }
        }

        public ObjectPool<Apple> Apples;

        public bool HasApples
        {
            get
            {
                return this.CurrentItem is Apples;
            }
        }

        private float appleCooldownTimer = 0f;
        private const float appleCooldownTime = 0.3f;

        public ObjectPool<Harpoon> Harpoons;
        private float harpoonCooldownTimer = 0f;
        private const float harpoonCooldownTime = 0.3f;

        public CircularBuffer<Bubble> Bubbles;
        float bubbleTimer = 0;

        public ObjectPool<SpaceshipShot> Shots;
        private float spaceshipShotCooldownTimer = 0f;

        ChargedSpaceshipShot chargedShot;

        // When shot is fully charged the ship will alternate between being white or not.
        bool isShipWhite = false;
        float shipFlashingWhiteTimer = 0f;
        float shipFlashingWhiteTimerGoal = 0.1f;

        /// <summary>
        /// If you have the charge shot powerup the shot will charge up when you aren't shooting.
        /// </summary>
        private float chargeShotTimer = 0f;
        private const float chargeShotTimerGoal = 1.85f;

        public float chargePercentage
        {
            get
            {
                return chargeShotTimer / chargeShotTimerGoal;
            }
        }

        /// <summary>
        /// Controls how fast you can shoot.
        /// </summary>
        private float spaceshipShotCooldownTime
        {
            get
            {
                if (ShotPower == ShotPower.Single)
                {
                    return 0.225f;
                }
                else
                {
                    return 0.15f;
                }
            }
        }

        Vector2 gunOffset = new Vector2(0, 8);
        public CircularBuffer<ShipFire> ShipFires;
        float shipFireTimer = 0f;

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

        /// <summary>
        /// For things like doors that may need to make Mac temporarily invisible/disabled.
        /// </summary>
        public bool IsInvisibleAndCantMove { get; set; } = false;

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

        public bool IsHoldingObject
        {
            get
            {
                return pickedUpObject != null;
            }
        }

        public void BreakPickupObject()
        {
            if (pickedUpObject != null)
            {
                pickedUpObject.BreakAndReset();
                pickedUpObject = null;
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

        /// <summary>
        /// A half size collision rectangle to check collisions with Cannons or other
        /// objects where a point can be missed, but the regular collision rect is too big.
        /// </summary>
        public Rectangle SmallerCollisionRectangle
        {
            get
            {
                var collisionRect = this.CollisionRectangle;
                return new Rectangle(collisionRect.X + 8, collisionRect.Y + 8, collisionRect.Width - 16, collisionRect.Height - 16);
            }
        }

        Texture2D textures;

        /// <summary>
        /// Track whether or not the player is in a camera offset zone so that the camera smooth scrolls in and out of these zones.
        /// </summary>
        private bool isInCameraOffsetZone;

        // Dracula parts rotation state
        private bool isRotatingDracParts = false;
        private float rotatingDracPartsTimer = 0f;
        private const float rotatingDracPartsDuration = 5f;
        private float dracPartsRotationAngle = 0f;
        private const float dracPartsRotationSpeed = 2f; // radians per second
        private const float dracPartsMaxDistance = 60f;
        private float dracPartsCurrentDistance = 0f;
        private const float dracPartsExpansionDuration = 1.5f; // Time to expand outward
        private const float dracPartsExpansionSpeed = dracPartsMaxDistance / dracPartsExpansionDuration;
        private bool dracPartsFullyExpanded = false;
        private Texture2D textures2;

        public Player(ContentManager content, InputManager inputManager, DeadMenu deadMenu)
        {
            animations = new AnimationDisplay();
            this.DisplayComponent = animations;

            textures = content.Load<Texture2D>(@"Textures\Textures");
            var bigTextures = content.Load<Texture2D>(@"Textures\BigTextures");
            var spaceTextures = content.Load<Texture2D>(@"Textures\SpaceTextures");
            textures2 = content.Load<Texture2D>(@"Textures\Textures2");

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

            var sub = new AnimationStrip(bigTextures, Helpers.GetBigTileRect(7, 3), 2, "sub");
            sub.LoopAnimation = true;
            sub.FrameLength = 0.25f;
            animations.Add(sub);

            var knockedDown = new AnimationStrip(textures, Helpers.GetTileRect(4, 0), 1, "knockedDown");
            knockedDown.LoopAnimation = false;
            knockedDown.FrameLength = 0.1f;
            animations.Add(knockedDown);

            var swim = new AnimationStrip(textures, Helpers.GetTileRect(6, 4), 2, "swim");
            swim.LoopAnimation = true;
            swim.FrameLength = 0.25f;
            animations.Add(swim);

            var disableWaterBomb = new AnimationStrip(textures, Helpers.GetTileRect(7, 0), 2, "disableWaterBomb");
            disableWaterBomb.LoopAnimation = true;
            disableWaterBomb.FrameLength = 0.25f;
            animations.Add(disableWaterBomb);

            var spaceShip = new AnimationStrip(spaceTextures, Helpers.GetTileRect(5, 2), 1, "spaceShip");
            spaceShip.LoopAnimation = false;
            spaceShip.FrameLength = 1f;
            animations.Add(spaceShip);

            // Ship will flash white when the shot is fully charged.
            var spaceShipWhite = new AnimationStrip(spaceTextures, Helpers.GetTileRect(5, 0), 1, "spaceShipWhite");
            spaceShipWhite.LoopAnimation = false;
            spaceShipWhite.FrameLength = 1f;
            animations.Add(spaceShipWhite);

            Enabled = true;

            // This gets set later when the level loads.
            DisplayComponent.DrawDepth = 0.5f;

            this.IsAbleToMoveOutsideOfWorld = true;
            this.IsAbleToSurviveOutsideOfWorld = true;
            this.IsAffectedByForces = false;
            this.isEnemyTileColliding = false;

            this.IsAffectedByGravity = true;

            this.IsAffectedByPlatforms = true;

            SetWorldLocationCollisionRectangle(5, 6);
            normalCollisionRectangle = this.collisionRectangle;

            InputManager = inputManager;
            _deadMenu = deadMenu;

            // Use this one wing image to draw flapping wings.
            wings = new MacWings(this, textures);

            Apples = new ObjectPool<Apple>(2);
            Apples.AddObject(new Apple(content, 0, 0, this));
            Apples.AddObject(new Apple(content, 0, 0, this));
            Apples.AddObject(new Apple(content, 0, 0, this));

            Harpoons = new ObjectPool<Harpoon>(4);
            Harpoons.AddObject(new Harpoon(content, 0, 0, this, Game1.Camera));
            Harpoons.AddObject(new Harpoon(content, 0, 0, this, Game1.Camera));
            Harpoons.AddObject(new Harpoon(content, 0, 0, this, Game1.Camera));
            Harpoons.AddObject(new Harpoon(content, 0, 0, this, Game1.Camera));

            Bubbles = new CircularBuffer<Bubble>(10);
            for (int i = 0; i < 10; i++)
            {
                Bubbles.SetItem(i, new Bubble(textures));
            }

            Shots = new ObjectPool<SpaceshipShot>(10);
            for (int i = 0; i < 10; i++)
            {
                Shots.AddObject(new SpaceshipShot(content, 0, 0, this, Game1.Camera));
            }
            chargedShot = new ChargedSpaceshipShot(content, 0, 0, this, Game1.Camera);
            ShipFires = new CircularBuffer<ShipFire>(10);
            for (int i = 0; i < 10; i++)
            {
                ShipFires.SetItem(i, new ShipFire(spaceTextures));
            }

            _shovel = new MacShovel(this, textures);

            _moveToLocation = new MoveToLocation(this, 250, "idle", "run", "jump", "climbLadder");
            _justIdle = new JustIdle("idle");
        }

        public void SmoothMoveCameraToTarget(int initialVelocity = 0)
        {
            smoothMoveCameraToTarget = true;
            cameraVelocity = initialVelocity;
        }

        public override void SetDrawDepth(float depth)
        {
            this.DisplayComponent.DrawDepth = depth;
            this._shovel.SetDrawDepth(DrawDepth + Game1.MIN_DRAW_INCREMENT);
            this.wings.SetDrawDepth(DrawDepth + Game1.MIN_DRAW_INCREMENT);
            this.Apples.RawList.ForEach(a => a.SetDrawDepth(DrawDepth + Game1.MIN_DRAW_INCREMENT));
            this.Harpoons.RawList.ForEach(a => a.SetDrawDepth(DrawDepth + Game1.MIN_DRAW_INCREMENT));
            this.Shots.RawList.ForEach(a => a.SetDrawDepth(DrawDepth + Game1.MIN_DRAW_INCREMENT));
            chargedShot.SetDrawDepth(DrawDepth + Game1.MIN_DRAW_INCREMENT);
            for (int i = 0; i < Bubbles.Length; i++)
            {
                Bubbles.GetItem(i).SetDrawDepth(DrawDepth + Game1.MIN_DRAW_INCREMENT);
            }

            for (int i = 0; i < ShipFires.Length; i++)
            {
                ShipFires.GetItem(i).SetDrawDepth(DrawDepth + Game1.MIN_DRAW_INCREMENT);
            }
        }

        public void BecomeNpc()
        {
            this._state = MacState.NPC;
        }

        public void GoToLocation(Vector2 location)
        {
            _moveToLocation.SetTargetLocation(location);
        }

        public bool IsAtLocation()
        {
            return _moveToLocation.IsAtFinalLocation;
        }

        public void HandleShotPowerupCollected()
        {
            if (ShotPower == ShotPower.Single)
            {
                ShotPower = ShotPower.Double;
            }
            else if (ShotPower == ShotPower.Double)
            {
                ShotPower = ShotPower.Charge;
            }
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            InteractButtonPressedThisFrame = false;

            _previousCollisionRectangle = this.CollisionRectangle;

            if (noMoveTimer > 0)
            {
                noMoveTimer -= elapsed;
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

            if (!ConsoleManager.ShowConsole)
            {
                if (IsInMineCart)
                {
                    HandleMineCartInputs(elapsed);
                }
                else if (IsInSub)
                {
                    HandleSubInputs(elapsed);
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
                else if (IsDisablingWaterBomb)
                {
                    // Do nothing, a timer will release mac from this state.
                }
                else if (IsInWater)
                {
                    HandleWaterInputs(elapsed);
                }
                else if (IsNpcMode)
                {
                    if (_moveToLocation.TargetLocation == Vector2.Zero)
                    {
                        _justIdle.Update(this, gameTime, elapsed);
                    }
                    else
                    {
                        _moveToLocation.Update(this, gameTime, elapsed);
                    }
                }
                else if (IsInSpaceShip)
                {
                    HandleSpaceshipInputs(elapsed);
                }
                else
                {
                    HandleRegularInputs(elapsed);
                }
            }

            if (this.Enabled && CollisionRectangle.Top > Game1.CurrentMap.GetWorldRectangle().Bottom && !IsInSpaceShip)
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

            var velocityBeforeUpdate = this.Velocity;

            if (HasWings)
            {
                // Cap max velocity
                this.Velocity = new Vector2(
                    MathHelper.Clamp(this.Velocity.X, -maxFlyingSpeed, maxFlyingSpeed),
                    MathHelper.Clamp(this.Velocity.Y, -maxFlyingSpeed * 3, maxFlyingSpeed)); // Less of a cap on upwards speed than falling.
            }

            base.Update(gameTime, elapsed);

            if (Landed && PlatformThatThisIsOn != null)
            {
                // Since the platform will pull the player along with it, cut his velocity
                // by the platform velocity as he lands on it.
                this.velocity.X -= PlatformThatThisIsOn.Velocity.X;
            }

            // Bounce off walls in the Minecart instead of stopping.
            if (IsInMineCart)
            {
                Flipped = false;

                if (this.OnRightWall)
                {
                    this.velocity.X = velocityBeforeUpdate.X * - 1;
                    
                    if (this.velocity.X >= (mineCartVelocity / 2))
                    {
                        SoundManager.PlaySound("Bounce");
                    }
                }
            }

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

            foreach (var harpoon in Harpoons.RawList)
            {
                if (harpoon.Enabled)
                {
                    harpoon.Update(gameTime, elapsed);
                }
            }

            for (int i = 0; i < Bubbles.Length; i++)
            {
                var bubble = Bubbles.GetItem(i);
                if (bubble.Enabled)
                {
                    bubble.Update(gameTime, elapsed);
                }
            }

            if (IsInSpaceShip)
            {
                foreach (var shot in Shots.RawList)
                {
                    if (shot.Enabled)
                    {
                        shot.Update(gameTime, elapsed);
                    }
                }

                chargedShot.Update(gameTime, elapsed);

                for (int i = 0; i < ShipFires.Length; i++)
                {
                    var fire = ShipFires.GetItem(i);
                    if (fire.Enabled)
                    {
                        fire.Update(gameTime, elapsed);
                    }
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

            // Handle Dracula parts rotation
            if (isRotatingDracParts)
            {
                rotatingDracPartsTimer += elapsed;

                // First phase: expand the parts outward from the center
                if (!dracPartsFullyExpanded)
                {
                    dracPartsCurrentDistance += dracPartsExpansionSpeed * elapsed;
                    if (dracPartsCurrentDistance >= dracPartsMaxDistance)
                    {
                        dracPartsCurrentDistance = dracPartsMaxDistance;
                        dracPartsFullyExpanded = true;
                    }
                }
                else
                {
                    // Second phase: rotate the parts around the player
                    dracPartsRotationAngle += dracPartsRotationSpeed * elapsed;
                }

                // After 5 seconds, transition to the Dracula fight
                if (rotatingDracPartsTimer >= rotatingDracPartsDuration)
                {
                    // Trigger transition to Dracula fight with slow dramatic fade
                    GlobalEvents.FireDoorEntered(this, "Dracula", "", "FromPlayer", Game1.TransitionType.SlowFade);
                }
            }
        }

        private void HandleSpaceshipInputs(float elapsed)
        {
            if (isShipWhite)
            {
                animations.PlayIfNotAlreadyPlaying("spaceShipWhite");
            }
            else
            {
                animations.PlayIfNotAlreadyPlaying("spaceShip");
            }
            this.IsAbleToMoveOutsideOfWorld = false;
            this.IsAbleToSurviveOutsideOfWorld = true;
            this.isTileColliding = false;

            IsAffectedByGravity = false;
            Flipped = false;

            float moveSpeed = 200f;

            bool isScrolledAllTheWayOver = false;

            // We scrolled all the way to the right.
            if (Game1.CurrentLevel.AutoScrollSpeed.X > 0 && Game1.Camera.ViewPort.Right >= Game1.Camera.WorldRectangle.Right)
            {
                isScrolledAllTheWayOver = true;
            }

            // Can't scroll left anymore
            if (Game1.CurrentLevel.AutoScrollSpeed.X < 0 && Game1.Camera.ViewPort.Left <= 0)
            {
                isScrolledAllTheWayOver = true;
            }

            Vector2 initialSpeed = Vector2.Zero;
            if (!isScrolledAllTheWayOver)
            {
                initialSpeed = Game1.CurrentLevel.AutoScrollSpeed;
            }

            this.Velocity = initialSpeed;

            Vector2 moveVelocity = Vector2.Zero;

            if (InputManager.CurrentAction.up)
            {
                moveVelocity.Y += -1;
            }
            else if (InputManager.CurrentAction.down)
            {
                moveVelocity.Y += 1;
            }
            if (InputManager.CurrentAction.left)
            {
                moveVelocity.X += -1;
            }
            else if (InputManager.CurrentAction.right)
            {
                moveVelocity.X += 1;
            }

            // we don't want diagonals to be faster than straight.
            if (moveVelocity != Vector2.Zero)
            {
                moveVelocity.Normalize();
                moveVelocity *= moveSpeed;
                this.Velocity += moveVelocity;
            }

            // Don't let the player move outside the camera
            if (this.WorldLocation.X - 16 < Game1.Camera.ViewPort.Left)
            {
                this.worldLocation.X = Game1.Camera.ViewPort.Left + 16;
            }
            else if (this.WorldLocation.X + 16 > Game1.Camera.ViewPort.Right)
            {
                this.worldLocation.X = Game1.Camera.ViewPort.Right - 16;
            }
            if (this.WorldLocation.Y - 32 < Game1.Camera.ViewPort.Top)
            {
                this.worldLocation.Y = Game1.Camera.ViewPort.Top + 32;
            }
            else if (this.WorldLocation.Y > Game1.Camera.ViewPort.Bottom)
            {
                this.worldLocation.Y = Game1.Camera.ViewPort.Bottom;
            }

            // Shooting
            if (spaceshipShotCooldownTimer < spaceshipShotCooldownTime)
            {
                spaceshipShotCooldownTimer += elapsed;
            }

            if (InputManager.CurrentAction.action && spaceshipShotCooldownTimer >= spaceshipShotCooldownTime)
            {

                if (ShotPower == ShotPower.Charge && chargeShotTimer >= chargeShotTimerGoal)
                {
                    // Handle charge shot
                    chargedShot.Reset();
                    chargedShot.WorldLocation = this.WorldLocation + new Vector2(-20, 16);
                    chargedShot.Velocity = new Vector2(500, 0);
                    spaceshipShotCooldownTimer = 0;
                    chargeShotTimer = 0;
                    SoundManager.PlaySound("ChargedShot", 1f, -0.2f);
                }
                else
                {
                    
                    var shot = Shots.TryGetObject();
                    if (shot != null)
                    {
                        shot.Enabled = true;

                        Vector2 offset = Vector2.Zero;

                        if (ShotPower != ShotPower.Single)
                        {
                            offset = gunOffset;

                            // This will make the bullet alternate from the top of the ship to the bottom as if
                            // both guns are producing shots.
                            gunOffset *= -1;
                        }

                        shot.WorldLocation = this.WorldLocation + offset;

                        shot.Velocity = new Vector2(500, 0);
                        if (Flipped)
                        {
                            shot.Velocity *= -1;
                        }
                        spaceshipShotCooldownTimer = 0;
                        chargeShotTimer = 0;
                        SoundManager.PlaySound("Shoot", 1f, -0.2f);
                    }
                }
            }

            // Shot charges if you have charge shot and you aren't shooting.
            if (ShotPower == ShotPower.Charge && !InputManager.CurrentAction.action)
            {
                chargeShotTimer = Math.Min(chargeShotTimerGoal, chargeShotTimer + elapsed);


                if (chargeShotTimer >= chargeShotTimerGoal)
                {
                    SoundManager.StopCharging();
                    SoundManager.PlayFullyCharged();
                    shipFlashingWhiteTimer += elapsed;
                    if (shipFlashingWhiteTimer >= shipFlashingWhiteTimerGoal)
                    {
                        isShipWhite = !isShipWhite;
                        shipFlashingWhiteTimer = 0f;
                    }
                }
                else if (chargeShotTimer >= (chargeShotTimerGoal * 0.3) && chargeShotTimer <= (chargeShotTimerGoal * 0.7f))
                {
                    SoundManager.PlayCharging();
                }
           
            }
            else
            {
                SoundManager.StopCharging();
                SoundManager.StopFullyCharged();
                isShipWhite = false;
            }

            float fireTimerGoal = 0.1f;

            // Make a rocket flame every so often.
            if (shipFireTimer < fireTimerGoal)
            {
                shipFireTimer += elapsed;
            }
            else
            {
                shipFireTimer = 0f;
                var fire = ShipFires.GetNextObject();
                fire.Reset();
                fire.SetDrawDepth(this.DrawDepth + Game1.MIN_DRAW_INCREMENT);
                fire.WorldLocation = this.WorldLocation + new Vector2(-12, 0);
                fire.Velocity = initialSpeed + new Vector2(-120, 0);
            }

            // When you're the ship you can fly through solid objects but
            // the corners of your collision rectangle will cause you to take a hit.
            
            // Check top left
            var topLeftPixel = new Vector2(this.CollisionRectangle.Left, this.CollisionRectangle.Top);
            var topLeftMapSquare = Game1.CurrentMap.GetMapSquareAtPixel(topLeftPixel);
            if (topLeftMapSquare != null && !topLeftMapSquare.Passable)
            {
                TakeHit(1, Vector2.Zero);
            }
            else
            {
                // Check top right
                var topRightPixel = new Vector2(this.CollisionRectangle.Right, this.CollisionRectangle.Top);
                var topRightMapSquare = Game1.CurrentMap.GetMapSquareAtPixel(topRightPixel);
                if (topRightMapSquare != null && !topRightMapSquare.Passable)
                {
                    TakeHit(1, Vector2.Zero);
                }
                else
                {
                    // Check bottom left
                    var bottomLeftPixel = new Vector2(this.CollisionRectangle.Left, this.CollisionRectangle.Bottom);
                    var bottomLeftMapSquare = Game1.CurrentMap.GetMapSquareAtPixel(bottomLeftPixel);
                    if (bottomLeftMapSquare != null && !bottomLeftMapSquare.Passable)
                    {
                        TakeHit(1, Vector2.Zero);
                    }
                    else
                    {
                        // Check bottom right
                        var bottomRightPixel = new Vector2(this.CollisionRectangle.Right, this.CollisionRectangle.Bottom);
                        var bottomRightMapSquare = Game1.CurrentMap.GetMapSquareAtPixel(bottomRightPixel);
                        if (bottomRightMapSquare != null && !bottomRightMapSquare.Passable)
                        {
                            TakeHit(1, Vector2.Zero);
                        }
                    }
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
            }

            isTileColliding = true;
            Enabled = true;
            Velocity = Vector2.Zero;
            IsInMineCart = false;
            IsInSub = false;
            subPlayerIsIn = null;
            pickedUpObject = null;
            CollisionRectangle = normalCollisionRectangle;
            IsAffectedByGravity = true;
            
            IsInvisibleAndCantMove = false;
            _state = MacState.Idle;
            this.IsInWater = false;
            IsJumpingOutOfWater = false;
            this.invincibleTimeRemaining = 0f;
            this.IsInvisibleAndCantMove = false;
            this.IsJustShotOutOfCannon = false;
            this.PlatformThatThisIsOn = null;
            ShotPower = ShotPower.Single;
            isRotatingDracParts = false;

            SoundManager.StopMinecart(); 
            SoundManager.StopCharging();
            SoundManager.StopFullyCharged();
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
            if (IsClimbingLadder || IsClimbingVine || IsInWater || IsInMineCart || IsInSub || IsInSpaceShip)
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

            if (!enemy.HasCollisionRectangle) return;

            // Check body collisions
            if (CollisionRectangle.Intersects(enemy.CollisionRectangle))
            {
                // Make sure you're moving towards the enemy, or they're moving towards you.
                var isMovingDown = ((this.velocity.Y - enemy.Velocity.Y) > 0);

                if (isMovingDown && enemy.CanBeJumpedOn && JumpedOnEnemyRectangle(enemy.CollisionRectangle))
                {
                    // If the player was above the enemy, the enemy was jumped on and takes a hit.
                    enemy.TakeHit(this, 1, Vector2.Zero);
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
                foreach (var harpoon in Harpoons.RawList)
                {
                    if (harpoon.Enabled)
                    {
                        if (harpoon.CollisionRectangle.Intersects(enemy.CollisionRectangle))
                        {
                            harpoon.Break();
                            enemy.TakeHit(harpoon, 1, Vector2.Zero);
                        }
                    }
                }
                if (IsInSpaceShip)
                {
                    foreach (var shot in Shots.RawList)
                    {
                        if (shot.Enabled)
                        {
                            if (shot.CollisionRectangle.Intersects(enemy.CollisionRectangle))
                            {
                                shot.Break();
                                enemy.TakeHit(shot, 1, Vector2.Zero);
                            }
                        }
                    }

                    if (chargedShot.Enabled)
                    {
                        if (chargedShot.CollisionRectangle.Intersects(enemy.CollisionRectangle))
                        {
                            var alreadyHit = false;
                            foreach(var alreadyHitEnemy in chargedShot.EnemiesHit)
                            {
                                if (alreadyHitEnemy == enemy)
                                {
                                    alreadyHit = true;
                                    break;
                                }
                            }
                            if (!alreadyHit)
                            {
                                chargedShot.EnemiesHit.Add(enemy);
                                enemy.TakeHit(chargedShot, chargedShot.Strength, Vector2.Zero);
                            }
                        }
                    }
                }
            }

        }

        public void TakeHit(Enemy enemy)
        {
            TakeHit(enemy.Attack, enemy.GetHitBackBoost(this));
        }

        public void TakeHit(int attack, Vector2 hitBackBoost)
        {
            if (IsInvincible) return;
            if (Health <= 0) return;
            if (attack == 0) return;

            // player takes a hit.
            Health -= attack;
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

                if (IsInSpaceShip)
                {
                    if (ShotPower == ShotPower.Charge)
                    {
                        ShotPower = ShotPower.Double;
                    }
                    else if (ShotPower == ShotPower.Double)
                    {
                        ShotPower = ShotPower.Single;
                        CurrentItem = null;
                    }
                }

                invincibleTimeRemaining = 1.5f;
                SoundManager.PlaySound("TakeHit");

                this.Velocity = hitBackBoost;
            }
        }

        private void HandleRegularInputs(float elapsed)
        {
            float friction;
            float jumpBoost;

            var mapSquareBelow = Game1.CurrentMap.GetMapSquareAtPixel(this.worldLocation + new Vector2(0, 1));
            var isIce = mapSquareBelow != null && mapSquareBelow.IsIce || (!OnGround && isInJumpFromIce);
            
            var environmentMaxWalkSpeed = maxSpeed;
            var acceleration = maxAcceleration;

            friction = 1.5f;
            jumpBoost = 390;

            if (isIce)
            {
                friction /= 2f;
                environmentMaxWalkSpeed *= 1.25f;
            }

            // If they aren't running max walk speed is cut down.
            if (!InputManager.CurrentAction.action && (onGround || IsClimbingLadder))
            {
                environmentMaxWalkSpeed /= 3;
            }

            // Cut the acceleration in half if they are in the air.
            if (!OnGround && !IsClimbingLadder && !IsClimbingVine)
            {
                acceleration /= 2;
            }

            float airMovementSpeed = 250f;

            // If they aren't running cut the air move speed.
            if (!InputManager.CurrentAction.action)
            {
                airMovementSpeed /= 3;
            }

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
                
                // Need to subtract 1 here. Rectangles are problematic for collision detection. If the width is 10 the left and right pixels will be
                // 10 and 20. But pixels 10 through 20 are 11 pixels. 10 through 19 would be 10.
                var tileAtBottomRight = Game1.CurrentMap.GetMapSquareAtPixel(this.CollisionRectangle.Right - 1, this.WorldLocation.Y.ToInt());

                var canGoDown = ((tileAtBottomLeft == null || tileAtBottomLeft.Passable) && (tileAtBottomRight == null || tileAtBottomRight.Passable));
                if (canGoDown)
                {
                    _state = MacState.ClimbingLadder;
                    this.velocity.Y = climbingSpeed;
                    this.PoisonPlatforms.Add(PlatformThatThisIsOn);
                    this.PlatformThatThisIsOn = null;
                }
            }

            // When you climb up a ladder but a block is in the way and you stop it's annoying. Move the player towards the center of the ladder.
            // this seems stupid but it helps a lot.
            if (IsClimbingLadder && OnCeiling && !InputManager.CurrentAction.left && !InputManager.CurrentAction.right)
            {
                var blockTopLeft = Game1.CurrentMap.GetMapSquareAtPixel(this.CollisionRectangle.Left - 4, this.CollisionRectangle.Top - 4);
                var blockTopRight = Game1.CurrentMap.GetMapSquareAtPixel(this.CollisionRectangle.Right - 1, this.CollisionRectangle.Top - 4);
                
                if (blockTopLeft != null && !blockTopLeft.Passable && blockTopRight != null && blockTopRight.Passable)
                {
                    this.velocity.X = 50;
                }
                else if (blockTopLeft != null && blockTopLeft.Passable && blockTopRight != null && !blockTopRight.Passable)
                {
                    this.velocity.X = -50;
                }
            }

            // Clear out weird state fields.
            if (OnGround || IsClimbingLadder || IsClimbingVine)
            {
                PoisonPlatforms.Clear();
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
                if (isIce)
                {
                    isInJumpFromIce = true;
                }
                isInJumpFromGround = true;
                jumpButtonHeldDownTimer = maxJumpButtonHeldDownTime;
                onGround = false;

                if (PlatformThatThisIsOn != null)
                {
                    velocity.X += PlatformThatThisIsOn.Velocity.X;
                    PlatformThatThisIsOn = null;
                }

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
            var autoGrabOnToLadder = !OnGround && !HasWings;
            if (!IsClimbingVine && canClimbVines && isOverVine && (autoGrabOnToLadder || InputManager.CurrentAction.up) && pickedUpObject == null)
            {
                _state = MacState.ClimbingVine;
            }

            if (IsClimbingVine)
            {

                Vector2 vineTile;
                if (!Flipped)
                {
                    vineTile = Game1.CurrentMap.GetCellByPixel(new Vector2(this.CollisionRectangle.Right - 1, this.CollisionCenter.Y));
                }
                else
                {
                    vineTile = Game1.CurrentMap.GetCellByPixel(new Vector2(this.CollisionRectangle.Left, this.CollisionCenter.Y));
                }

                // You can't move left and right on the vine, but Mac can flip.
                if (!Flipped && InputManager.CurrentAction.left)
                {
                    SmoothMoveCameraToTarget();
                    Flipped = true;
                }
                else if (Flipped && InputManager.CurrentAction.right)
                {
                    SmoothMoveCameraToTarget();
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
                (InputManager.CurrentAction.up && !InputManager.PreviousAction.up);

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
                    _state = MacState.Falling;
                }
                else if (!OnGround && velocity.Y < 0)
                {
                    _state = MacState.Jumping;
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
                    SoundManager.PlaySound("Climb", 1f, 0.3f);
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

            var isAbleToPickup = !IsClimbingLadder && !IsClimbingVine && !IsInMineCart && !HasWings && !IsInCannon && !IsInWater && !IsInSub;

            bool didPickUpObject = false;
            bool didKickObject = false;

            // If you jump off a spring I don't want you to pick it up as you're jumping. So as a hack around this
            // you can't pick anything up for a short period after you jump
            if (InputManager.CurrentAction.jump && !InputManager.PreviousAction.jump)
            {
                pickUpAgainTimer = Math.Max(pickUpAgainTimer, 0.2f);
            }

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
                DropItem();
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
                    SoundManager.PlaySound("Kick");
                    apple.Enabled = true;
                    apple.WorldLocation = this.WorldLocation;
                    apple.Velocity = new Vector2(400, 0);
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

        }

        private void DropItem()
        {
            if (pickedUpObject == null) return;
            pickedUpObject.Drop();
            recentlyDropped = pickedUpObject;
            kickTimer = 0f;
            pickUpAgainTimer = 1f;
            pickedUpObject = null;
        }

        public void EnterMineCart()
        {
            this.IsInMineCart = true;
            SmoothMoveCameraToTarget();
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

            // Check for minecart destroying tiles
            var centerTile = Game1.CurrentMap.GetMapSquareAtPixel(this.WorldCenter);
            if (centerTile != null && centerTile.IsDestroyMinecart)
            {
                _state = MacState.Idle;
                IsInMineCart = false;
                SoundManager.PlaySound("Break");
                SoundManager.StopMinecart();
                SmoothMoveCameraToTarget();
                return;
            }

            Flipped = false;

            this.velocity.X += 600f * elapsed;
            this.velocity.X = Math.Min(this.velocity.X, mineCartVelocity);

            // Jump
            if (InputManager.CurrentAction.jump && !InputManager.PreviousAction.jump && OnGround)
            {
                // Regular jump.
                this.velocity.Y -= 550;
                SoundManager.PlayMinecartJump();
                SoundManager.StopMinecart();
            }
        }

        private void HandleSubInputs(float elapsed)
        {
            animations.PlayIfNotAlreadyPlaying("sub");

            IsAffectedByGravity = false;

            float subVelocity = 200f;

            Velocity = Vector2.Zero;

            // Disable the inputs for a short time so the camera can catch up to Mac's new location.
            if (noMoveTimer > 0) return;

            var isPixelInWater = (Vector2 pixel) =>
            {
                return Game1.CurrentMap.GetMapSquareAtPixel(pixel)?.IsWater ?? false;
            };

            if (InputManager.CurrentAction.up && isPixelInWater(this.WorldCenter))
            {
                this.velocity.Y = -subVelocity;
            }
            else if (InputManager.CurrentAction.down && isPixelInWater(this.WorldLocation))
            {
                this.velocity.Y = subVelocity;
            }
            if (InputManager.CurrentAction.left && isPixelInWater(new Vector2(this.CollisionRectangle.Left, this.WorldLocation.Y)))
            {
                this.velocity.X = -subVelocity;
                this.Flipped = true;
            }
            else if (InputManager.CurrentAction.right && isPixelInWater(new Vector2(this.CollisionRectangle.Right, this.WorldLocation.Y)))
            {
                this.velocity.X = subVelocity;
                this.Flipped = false;
            }

            // we don't want diagonals to be faster than straight.
            if (velocity != Vector2.Zero)
            {
                var tempVelocity = this.velocity;
                tempVelocity.Normalize();
                tempVelocity *= subVelocity;
                this.Velocity = tempVelocity;
            }

            // Shoot harpoons.
            if (harpoonCooldownTimer < harpoonCooldownTime)
            {
                harpoonCooldownTimer += elapsed;
            }

            var isInWater = isPixelInWater(this.WorldLocation);

            if (isInWater && InputManager.CurrentAction.action && !InputManager.PreviousAction.action && harpoonCooldownTimer >= harpoonCooldownTime)
            {
                var harpoon = Harpoons.TryGetObject();
                if (harpoon != null)
                {
                    harpoon.Enabled = true;
                    harpoon.WorldLocation = this.WorldLocation + new Vector2(0, -5);
                    harpoon.Velocity = new Vector2(280, 0);
                    if (Flipped)
                    {
                        harpoon.Velocity *= -1;
                    }
                    harpoonCooldownTimer = 0;
                    SoundManager.PlaySound("Shoot");
                }
            }

            if (InputManager.CurrentAction.jump && !InputManager.PreviousAction.jump)
            {
                subPlayerIsIn.PlayerExit();
                this.ExitSub();
            }

            float bubbleTimerGoal = 0.5f;

            // Slow bubbles if you're not moving.
            if (velocity == Vector2.Zero)
            {
                bubbleTimerGoal = 1.8f;
            }

            // Make a bubble every so often.
            if (bubbleTimer < bubbleTimerGoal)
            {
                if (isInWater)
                {
                    bubbleTimer += elapsed;
                }
            }
            else
            {
                bubbleTimer = 0f;
                var bubble = Bubbles.GetNextObject();
                bubble.Reset();
                bubble.WorldLocation = this.WorldLocation + new Vector2(-32 * (Flipped ? -1 : 1), 8);
                bubble.Velocity = new Vector2(-50, -50);
                if (velocity.X != 0)
                {
                    bubble.Velocity *= new Vector2(1.5f, 1);
                }

                if (Flipped)
                {
                    bubble.Velocity *= new Vector2(-1, 1);
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

        public void EnterSpaceship()
        {
            _state = MacState.SpaceShip;
            this.IsAffectedByGravity = false;
            animations.Play("spaceShip");
            SetCenteredCollisionRectangle(8, 8, 5, 5);
        }

        /// <summary>
        /// Initiated by the sub.
        /// </summary>
        /// <param name="sub"></param>
        public void EnterSub(Submarine sub)
        {
            // Slowly track the player for a short period to avoid jerky movement
            SmoothMoveCameraToTarget();
            noMoveTimer = 0.2f;
            IsInSub = true;
            IsAffectedByGravity = false;
            subPlayerIsIn = sub;
            this.WorldLocation = sub.WorldLocation;
            this.CollisionRectangle = sub.RelativeCollisionRectangle;
            // TODO: reset collision rect
        }

        /// <summary>
        /// Initiated by the player.
        /// </summary>
        public void ExitSub()
        {
            IsInSub = false;
            this.WorldLocation = new Vector2(subPlayerIsIn.WorldLocation.X, subPlayerIsIn.CollisionRectangle.Bottom - 4);
            SmoothMoveCameraToTarget();
            IsAffectedByGravity = true;
            subPlayerIsIn = null;
            this.collisionRectangle = normalCollisionRectangle;
            animations.Play("swim");
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

            SmoothMoveCameraToTarget();
        }

        public void ShootOutOfCannon(Cannon cannon, Vector2 velocity)
        {
            this.velocity = velocity;
            this.IsJustShotOutOfCannon = true;
            this.CannonYouAreIn = null;

            this.IsJustShotOutOfCannon = true;

            // A regular cannon fires Mac for fraction of a second. A supershot leaves
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

            DropItem();

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

                    SoundManager.PlaySound("Swim", 1f, pitch);
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

            const float swimAcceleration = 250f;
            const float maxSwimSpeed = 100f;

            if (InputManager.CurrentAction.right && !InputManager.CurrentAction.left)
            {
                this.velocity.X += swimAcceleration * elapsed;
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
                this.velocity.Y += swimAcceleration * elapsed;
            }
            else if (InputManager.CurrentAction.up && !InputManager.CurrentAction.down)
            {
                this.velocity.Y -= swimAcceleration * elapsed;
            }
            else
            {
                // They slowly float down if you don't press anything.
                this.velocity.Y += 30 * elapsed;
            }

            if (isHeadUnderWater)
            {
                this.velocity.Y = MathHelper.Clamp(this.velocity.Y, -maxSwimSpeed, maxSwimSpeed);
                this.velocity.X = MathHelper.Clamp(this.velocity.X, -maxSwimSpeed, maxSwimSpeed);
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

            if (IsInSub || IsInSpaceShip)
            {
                EffectsManager.AddExplosion(this.WorldCenter);
            }

            Health = 0;
            Enabled = false;
            this.CurrentItem = null;
            EffectsManager.EnemyPop(WorldCenter, 10, Color.Yellow, 200f);
            SoundManager.PlaySound("MacDeath");

            // Pause for a bit before adding the dead menu
            TimerManager.AddNewTimer(2f, () => MenuManager.AddMenu(_deadMenu));

            if (IsInMineCart)
            {
                IsInMineCart = false;
                SmoothMoveCameraToTarget();
            }

            SoundManager.StopMinecart();
            SoundManager.StopCharging();
            SoundManager.StopFullyCharged();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsInvisibleAndCantMove) return;

            if (IsInCannon) return;

            if (!Enabled) return;

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

            foreach (var harpoon in Harpoons.RawList)
            {
                if (harpoon.Enabled)
                {
                    harpoon.Draw(spriteBatch);
                }
            }

            for (int i = 0; i < Bubbles.Length; i++)
            {
                if (Bubbles.GetItem(i).Enabled)
                {
                    Bubbles.GetItem(i).Draw(spriteBatch);
                }
            }

            if (IsInSub)
            {
                // the sub sprite doesn't contain Mac so we're going to just draw his idle image behind the sub.
                Vector2 position = this.WorldLocation + new Vector2(-16, -40);
                var depth = this.DisplayComponent.DrawDepth + Game1.MIN_DRAW_INCREMENT;
                var effect = this.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                spriteBatch.Draw(textures, position, Helpers.GetTileRect(1, 0), Color.White, 0f, Vector2.Zero, 1f, effect, depth);
            }

            if (IsInSpaceShip)
            {
                foreach (var shot in Shots.RawList)
                {
                    if (shot.Enabled)
                    {
                        shot.Draw(spriteBatch);
                    }
                }
                chargedShot.Draw(spriteBatch);
                for (int i = 0; i < ShipFires.Length; i++)
                {
                    if (ShipFires.GetItem(i).Enabled)
                    {
                        ShipFires.GetItem(i).Draw(spriteBatch);
                    }
                }
            }

            // Draw the rotating Dracula parts behind the player
            if (isRotatingDracParts)
            {
                // Draw depth just behind the player
                var depth = this.DisplayComponent.DrawDepth - Game1.MIN_DRAW_INCREMENT;

                // Define the parts and their tile positions
                var parts = new[]
                {
                    new { TileX = 3, TileY = 35 }, // Heart
                    new { TileX = 4, TileY = 35 }, // Skull
                    new { TileX = 5, TileY = 35 }, // Rib
                    new { TileX = 6, TileY = 35 }, // Eye
                    new { TileX = 7, TileY = 35 }  // Teeth
                };

                // Draw each part at its position around the player
                for (int i = 0; i < parts.Length; i++)
                {
                    // Calculate angle for this part (evenly spaced around the circle)
                    float angleOffset = (float)(i * (MathHelper.TwoPi / parts.Length));
                    float totalAngle = dracPartsRotationAngle + angleOffset;

                    // Calculate position offset from player center
                    float offsetX = (float)Math.Cos(totalAngle) * dracPartsCurrentDistance;
                    float offsetY = (float)Math.Sin(totalAngle) * dracPartsCurrentDistance;

                    // Calculate world position for this part
                    Vector2 partPosition = this.CollisionCenter + new Vector2(offsetX, offsetY);

                    // Adjust for tile size (center the sprite)
                    partPosition -= new Vector2(16, 16);

                    // Draw the part
                    spriteBatch.Draw(
                        textures2,
                        partPosition,
                        Helpers.GetTileRect(parts[i].TileX, parts[i].TileY),
                        Color.White,
                        0f,
                        Vector2.Zero,
                        1f,
                        SpriteEffects.None,
                        depth
                    );
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

        public void SetCameraTarget(Camera camera, float elapsed)
        {
            var targetPosition = this.WorldLocation + new Vector2(Game1.CurrentLevel.CameraXOffset, 0);

            if (IsInMineCart)
            {
                // Track behind the player
                targetPosition = this.WorldLocation + new Vector2(80, 0);
            }

            if (!IsInSpaceShip)
            {
                // Track just above the player so we see more up than down.
                targetPosition.Y -= Game1.TileSize * 1.5f;
            }

            var wasInCameraOffsetZone = isInCameraOffsetZone;
            isInCameraOffsetZone = false;
            foreach (var offsetZone in Game1.CurrentLevel.CameraOffsetZones)
            {
                if (offsetZone.CollisionRectangle.Intersects(this.CollisionRectangle))
                {
                    targetPosition += offsetZone.Offset;
                    isInCameraOffsetZone = true;
                    break;
                }
            }

            // Smooth move the camera if Mac goes in or out of an offset zone.
            if (wasInCameraOffsetZone != isInCameraOffsetZone)
            {
                smoothMoveCameraToTarget = true;
            }

            if (!smoothMoveCameraToTarget)
            {
                // Snap to the target.
                camera.Position = targetPosition;
            }
            else
            {
                // Move at an accelerating rate to catch up to the target.
                // Note: An improvement here would be to track the character directly and
                // have a velocity vector. Not track x and y separately. Or if I was to track
                // them separate, have 2 velocities. This is the amature channel.
                Vector2 positionToReturn = Vector2.Zero;
                var distanceNeededToMoveX = targetPosition.X - camera.Position.X;
                var distanceToMoveX = cameraVelocity * elapsed;

                if (distanceNeededToMoveX < 0)
                {
                    distanceToMoveX = -distanceToMoveX;
                }

                if (Math.Abs(distanceNeededToMoveX) < Math.Abs(distanceToMoveX))
                {
                    positionToReturn.X = targetPosition.X;
                }
                else
                {
                    positionToReturn.X = camera.Position.X + distanceToMoveX;
                }

                // Now do the y direction.
                var distanceNeededToMoveY = targetPosition.Y - camera.Position.Y;
                var distanceToMoveY = cameraVelocity * elapsed;

                if (distanceNeededToMoveY < 0)
                {
                    distanceToMoveY = -distanceToMoveY;
                }

                if (Math.Abs(distanceNeededToMoveY) < Math.Abs(distanceToMoveY))
                {
                    positionToReturn.Y = targetPosition.Y;
                }
                else
                {
                    positionToReturn.Y = camera.Position.Y + distanceToMoveY;
                }

                if (positionToReturn == targetPosition)
                {
                    // we've reached or overlapped the target positon. Just hard track the target.
                    smoothMoveCameraToTarget = false;
                    cameraVelocity = minCameraVelocity;
                }
                else
                {
                    // increase speed by a percentage.
                    cameraVelocity += cameraAcceleration * elapsed;
                    cameraVelocity = Math.Max(Math.Min(cameraVelocity, maxCameraVelocity), minCameraVelocity);
                }

                camera.Position = positionToReturn;

                // Kind of weird, but another check if the camera made it to it's target. We consider it there if the x or y position didn't move because the 
                // camera blocked it from being at the extact target. This might happen if the camera is too close to the edge of the level or if it's locked
                // because you are fighting a boss.
                var xWasSetToTargetOrBlocked = camera.Position.X == targetPosition.X || camera.Position.X != positionToReturn.X;
                var yWasSetToTargetOrBlocked = camera.Position.Y == targetPosition.Y || camera.Position.Y != positionToReturn.Y;
                if (xWasSetToTargetOrBlocked && yWasSetToTargetOrBlocked)
                {
                    smoothMoveCameraToTarget = false;
                    cameraVelocity = minCameraVelocity;
                }
            }

        }

        public void RotateDracParts()
        {
            isRotatingDracParts = true;
            rotatingDracPartsTimer = 0f;
            dracPartsRotationAngle = 0f;
            dracPartsCurrentDistance = 0f;
            dracPartsFullyExpanded = false;
            this.Velocity = Vector2.Zero;
            animations.Play("idle");
            SoundManager.PlaySound("DracPart1");
            TimerManager.AddNewTimer(1.5f, () =>
            {
                SoundManager.PlaySound("DracPart2");
            });
        }

        public void AddUnlockedDoor(string doorName)
        {
            var doors = Game1.StorageState.Levels[Game1.CurrentLevel.LevelNumber].UnlockedDoors;
            if (!doors.Contains(doorName))
            {
                doors.Add(doorName);
            }
        }

        public void StartDisableWaterBomb(WaterBomb waterBomb)
        {
            var state = this._state;
            if (state != MacState.DisablingWaterBomb)
            {
                animations.Play("disableWaterBomb");
                _state = MacState.DisablingWaterBomb;
                this.Velocity = Vector2.Zero;
                SmoothMoveCameraToTarget();
                TimerManager.AddNewTimer(2f, () =>
                {
                    animations.Play("idle");
                    waterBomb.Disable();
                    _state = state;
                });
            }
        }
    }
}
