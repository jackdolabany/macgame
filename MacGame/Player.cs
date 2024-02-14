using MacGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MacGame.Platforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Runtime.Intrinsics.X86;
using Microsoft.Xna.Framework.Audio;
using System.Net.Http;
using System.Diagnostics;
using TileEngine;

namespace MacGame
{
    public class Player : GameObject
    {
        AnimationDisplay animations;

        public const int MaxHealth = 5;
        
        public int Health { get; set; } = MaxHealth;
        
        public InputManager InputManager { get; private set; }

        private DeadMenu _deadMenu;

        private const float acceleration = 150;
        private const float maxSpeed = 80;

        private bool isRunning = false;
        private bool isJumping = false;
        private bool isSliding = false;
        private bool isFalling = false;

        private float invincibleTimeRemaining = 0.0f;
        private float invincibleFlashTimer = 0.0f;

        public bool IsInvincible => invincibleTimeRemaining > 0.0f;

        private Rectangle _previousCollisionRectangle;

        // Ladder climbing stuff
        private bool isClimbingLadder = false;
        AnimationStrip climbingLadderAnimation;
        private const int ladderSpeed = 30;
        
        // Used to temporarily prevent you from climbing ladders if you jump while holding up
        // until you release up and press it again. This way you don't just insta-climb the ladder above you.
        private bool canClimbLadders = true;

        private float playClimbSoundTimer = 0f;

        // Vine climbing stuff
        AnimationStrip climbingVineAnimation;
        private bool isClimbingVine;

        public Player(ContentManager content, InputManager inputManager, DeadMenu deadMenu)
        {
            animations = new AnimationDisplay();
            this.DisplayComponent = animations;

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var idle = new AnimationStrip(textures, new Rectangle(8, 0, 8, 8), 1, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.1f;
            animations.Add(idle);

            var walk = new AnimationStrip(textures, new Rectangle(8, 0, 8, 8), 2, "run");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.14f;
            animations.Add(walk);

            var slide = new AnimationStrip(textures, new Rectangle(24, 0, 8, 8), 1, "slide");
            slide.LoopAnimation = false;
            slide.FrameLength = 0.1f;
            animations.Add(slide);

            var jump = new AnimationStrip(textures, new Rectangle(8, 8, 8, 8), 1, "jump");
            jump.FrameLength = 0.1f;
            animations.Add(jump);

            var fall = new AnimationStrip(textures, new Rectangle(16, 8, 8, 8), 1, "fall");
            fall.LoopAnimation = true;
            fall.FrameLength = 0.1f;
            animations.Add(fall);

            climbingLadderAnimation = new AnimationStrip(textures, new Rectangle(40, 24, 8, 8), 2, "climbLadder");
            climbingLadderAnimation.LoopAnimation = true;
            climbingLadderAnimation.FrameLength = 0.14f;
            animations.Add(climbingLadderAnimation);


            climbingVineAnimation = new AnimationStrip(textures, new Rectangle(48, 16, 8, 8), 2, "climbVine");
            climbingVineAnimation.LoopAnimation = true;
            climbingVineAnimation.FrameLength = 0.14f;
            animations.Add(climbingVineAnimation);

            Enabled = true;

            // TODO: Whatever
            DisplayComponent.DrawDepth = 0.5f;

            this.IsAbleToMoveOutsideOfWorld = true;
            this.IsAbleToSurviveOutsideOfWorld = true;
            this.IsAffectedByForces = false;
            this.isEnemyTileColliding = false;

            this.IsAffectedByGravity = true;

            this.IsAffectedByPlatforms = true;

            this.CollisionRectangle = new Rectangle(-3, -7, 6, 7);

            InputManager = inputManager;
            _deadMenu = deadMenu;

        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            _previousCollisionRectangle = this.CollisionRectangle;

            HandleInputs(elapsed);

            if (this.Enabled && CollisionRectangle.Top > Game1.CurrentMap.GetWorldRectangle().Bottom)
            {
                // player fell down a bottomless pit
                Kill();
            }

            if (invincibleTimeRemaining > 0)
            {
                invincibleTimeRemaining -= elapsed;
                invincibleFlashTimer -= elapsed;

                if(invincibleFlashTimer < 0)
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
                    var wasAboveEnemy = _previousCollisionRectangle.Bottom - 1 <= enemy.CollisionRectangle.Top;

                    if (enemy.Alive && !enemy.IsInvincibleAfterHit && wasAboveEnemy && !isClimbingLadder && !isClimbingVine)
                    {
                        // If the player was above the enemy, the enemy was jumped on and takes a hit.
                        enemy.TakeHit(1, Vector2.Zero);
                        velocity.Y = -120;
                    }
                    else if (enemy.Alive && !enemy.IsInvincibleAfterHit)
                    {
                        TakeHit(enemy);
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
                var hitBackBoost = new Vector2(50, -100);
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
            var isSand = mapSquareBelow != null && mapSquareBelow.IsSand;
            var isIce = mapSquareBelow != null && mapSquareBelow.IsIce;

            var maxWalkingSpeed = maxSpeed;
            if (isSand)
            {
                friction = 5f;
                jumpBoost = 100;
                maxWalkingSpeed /= 2;
            }
            else if (isIce)
            {
                friction = 0.95f;
                jumpBoost = 150;
            }
            else
            {
                // Normal
                friction = 2.5f;
                jumpBoost = 150f;
            }

            if (!InputManager.CurrentAction.attack)
            {
                maxWalkingSpeed /= 2;
            }

            // Walk Right
            if (InputManager.CurrentAction.right && !InputManager.CurrentAction.left && !isClimbingVine)
            {
                this.velocity.X += acceleration * elapsed;
                if (velocity.X > maxWalkingSpeed)
                {
                    velocity.X = maxWalkingSpeed;
                }
                isRunning = !isClimbingLadder;
                isSliding = false;
                flipped = false;
            }

            // Walk left
            if (InputManager.CurrentAction.left && !InputManager.CurrentAction.right && !isClimbingVine)
            {
                this.velocity.X -= acceleration * elapsed;
                if (velocity.X < -maxWalkingSpeed)
                {
                    velocity.X = -maxWalkingSpeed;
                }
                isRunning = !isClimbingLadder;
                isSliding = false;
                flipped = true;
            }


            if (!isRunning && !isClimbingLadder && !isClimbingVine)
            {
                this.velocity.X -= (this.velocity.X * friction * elapsed);
            }

            // Sliding is a special state when you are still moving after walking.
            if (isRunning
                && !InputManager.CurrentAction.left
                && !InputManager.CurrentAction.right
                && !isFalling
                && !isJumping)
            {
                isRunning = false;
                isSliding = true;
            }

            
            // Ladder stuff.
            var tileAtBottom = Game1.CurrentMap.GetMapSquareAtPixel(this.worldLocation);
            var tileAtTop = Game1.CurrentMap.GetMapSquareAtPixel(this.worldLocation - new Vector2(0, CollisionRectangle.Height));
            var isOverALadder = (tileAtBottom?.IsLadder ?? false) || (tileAtTop?.IsLadder ?? false);

            if (!isOverALadder)
            {
                isClimbingLadder = false;
            }
            
            // Climbing a ladder from standstill.
            if (isOverALadder 
                && canClimbLadders
                && (InputManager.CurrentAction.up || (isClimbingLadder && InputManager.CurrentAction.down)) // Need to press up to latch onto a ladder. Down only if you are already climbing.
                && !(PlatformThatThisIsOn is LadderPlatform)) // Don't climb if you are standing on a ladder platform. Climbing down from atop a ladder is handled below.
            {
                isClimbingLadder = true;
                isJumping = false;
                isFalling = false;
                this.velocity.X -= acceleration * elapsed;
                if (velocity.X < -maxWalkingSpeed)
                {
                    velocity.X = -maxWalkingSpeed;
                }
                this.velocity.Y = ladderSpeed;
                if (InputManager.CurrentAction.up)
                {
                    this.velocity.Y *= -1;
                }

                // No moving left or right on the ladder unless you are not going up or down.
                this.velocity.X = 0;
            }
            this.IsAffectedByGravity = !isClimbingLadder && !isClimbingVine;

            // Stop moving while climbing if you aren't pressing up or down.
            if ((isClimbingLadder || isClimbingVine) && !InputManager.CurrentAction.up && !InputManager.CurrentAction.down)
            {
                this.velocity.Y = 0;
            }

            if (isClimbingLadder && !InputManager.CurrentAction.left && !InputManager.CurrentAction.right)
            {
                this.velocity.X = 0;
            }

            // Stop climbing if you move down towards the ground.
            if (InputManager.CurrentAction.down && onGround)
            {
                isClimbingLadder = false;
                isClimbingVine = false;
            }

            // If you are on a ladder platform you can press down to climb down through it.
            if (canClimbLadders && !InputManager.CurrentAction.jump && InputManager.CurrentAction.down && OnPlatform && PlatformThatThisIsOn is LadderPlatform)
            {
                isClimbingLadder = true;
                isJumping = false;
                isFalling = false;
                this.velocity.Y = ladderSpeed;
                this.PoisonPlatforms.Add(PlatformThatThisIsOn);
            }

            if (OnGround)
            {
                PoisonPlatforms.Clear();
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
                isSliding = false;
                SoundManager.PlaySound("jump");
                isClimbingLadder = false;
            }
            else if (InputManager.CurrentAction.jump
                && !InputManager.PreviousAction.jump
                && isClimbingLadder)
            {
                // Jump off ladder
                this.velocity.Y -= (jumpBoost / 2); // weaker jump
                SoundManager.PlaySound("jump");

                // block their ability to climb ladders until they release up. This prevents you from
                // insta-climbing the ladder above you.
                if (isClimbingLadder && InputManager.CurrentAction.up)
                {
                    canClimbLadders = false;
                }
                isClimbingLadder = false;
            }
            else if (InputManager.CurrentAction.jump
                && !InputManager.PreviousAction.jump
                && isClimbingVine)
            {
                // Jump off Vine
                this.velocity.Y -= (jumpBoost / 2); // weaker jump

                // Give them a little boost in the direction they are facing.
                this.velocity.X = 30;
                if (flipped)
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
            Vector2 currentVineCell = Vector2.Zero;

            if (!isClimbingVine && isOverVine && (!OnGround || InputManager.CurrentAction.up))
            {
                isClimbingVine = true;
                currentVineCell = Game1.CurrentMap.GetCellByPixel(this.CollisionCenter);

                //// Snap the player to the vine cell
                //if (!flipped)
                //{
                //    this.worldLocation.X = (TileMap.TileSize * currentVineCell.X) + 4;
                //}
                //else
                //{
                //    this.worldLocation.X = (TileMap.TileSize * currentVineCell.X) + 4;
                //}
            }

            if (isClimbingVine)
            {

                Vector2 vineTile;
                if (!flipped)
                {
                    vineTile = Game1.CurrentMap.GetCellByPixel(new Vector2(this.CollisionRectangle.Right, this.CollisionCenter.Y));
                }
                else
                {
                    vineTile = Game1.CurrentMap.GetCellByPixel(new Vector2(this.CollisionRectangle.Left, this.CollisionCenter.Y));
                }

                // You can't move left and right on the vine, but Mac can flip.
                if (InputManager.CurrentAction.left)
                {
                    flipped = true;
                }
                else if (InputManager.CurrentAction.right)
                {
                    flipped = false;
                }

                if (!flipped)
                {
                    this.worldLocation.X = (TileMap.TileSize * vineTile.X) + 2;
                }
                else
                {
                    this.worldLocation.X = TileMap.TileSize * vineTile.X + 6;
                }

                // snap to vine.
                isClimbingVine = true;
                isFalling = false;
                isJumping = false;

                this.velocity.X = 0;

                if (InputManager.CurrentAction.up)
                {
                    this.velocity.Y = -ladderSpeed;
                }
                else if (InputManager.CurrentAction.down)
                {
                    this.velocity.Y = ladderSpeed;
                }
                else
                {
                    this.velocity.Y = 0;
                }

                if (tileAtTop == null || !tileAtTop.IsVine)
                {
                    // You reached the top of the vine.
                    this.velocity.Y = ladderSpeed;
                    //this.worldLocation.Y += 3;
                }

                if (InputManager.CurrentAction.jump)
                {
                    isClimbingVine = false;
                    isJumping = true;
                }
            }

            // temp??
            if (!tileAtCenter.IsVine)
            {
                isClimbingVine = false;
            }

            // slightly sliding is not sliding, so we want to see the idle animation.
            if (velocity.X < 20 && velocity.X > -20 && isSliding)
            {
                isRunning = false;
                isSliding = false;
            }

            // stop the player if they are nearly stopped so you don't get weird 1px movement.
            if (velocity.X < 6 && velocity.X > -6 && !isRunning && !isClimbingLadder)
            {
                velocity.X = 0;
            }

            if (!isClimbingLadder && !isClimbingVine)
            {
                if (!OnGround && velocity.Y > 0)
                {
                    isJumping = true;
                    isFalling = false;
                    isRunning = false;
                }
                else if (!OnGround && velocity.Y < 0)
                {
                    isJumping = false;
                    isFalling = true;
                    isRunning = false;
                }
                else
                {
                    isJumping = false;
                    isFalling = false;
                }
            }
            
            var isClimbingAnimationPlaying = (isClimbingLadder || isClimbingVine) && velocity != Vector2.Zero;

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

            // Bound the player to the map left and right.
            if (CollisionRectangle.X < Game1.CurrentMap.GetWorldRectangle().X && velocity.X < 0)
            {
                velocity.X = 0;
            }
            else if (CollisionRectangle.Right > Game1.CurrentMap.GetWorldRectangle().Right && velocity.X > 0)
            {
                velocity.X = 0;
            }

            string nextAnimation;
            if (isJumping)
            {
                nextAnimation = "jump";
            }
            else if (isFalling)
            {
                nextAnimation = "fall";
            }
            else if (isRunning)
            {
                nextAnimation = "run";
            }
            else if (isSliding)
            {
                nextAnimation = "slide";
            }
            else if (isClimbingLadder)
            {
                nextAnimation = "climbLadder";
                climbingLadderAnimation.IsPaused = !isClimbingAnimationPlaying;
            }
            else if (isClimbingVine)
            {
                nextAnimation = "climbVine";
                climbingVineAnimation.IsPaused = !isClimbingAnimationPlaying;
            }
            else
            {
                nextAnimation = "idle";
            }

            if (animations.currentAnimationName != nextAnimation)
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

        public void Kill()
        {
            Health = 0;
            Enabled = false;
            EffectsManager.EnemyPop(WorldCenter, 10, Color.Yellow, 50f);
            SoundManager.PlaySound("mac_death");
            MenuManager.AddMenu(_deadMenu);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public bool IsFacingRight()
        {
            return !this.flipped;
        }

        public bool IsFacingLeft()
        {
            return !IsFacingRight();
        }

        public Vector2 GetCameraPosition(Camera camera)
        {
            // Whe climbing a vine, the player's position may snap to the vine, or snap when he faces left and right
            // so we need to more slowly move the camera to track the player.
            if(isClimbingVine)
            {
                var cameraPosition = camera.Position + ((this.worldLocation - camera.Position) * 0.1f);
                return cameraPosition;
            }
            
            // Normally the Camera tracks the player
            return this.worldLocation;
        }
    }
}
