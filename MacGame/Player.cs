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

                    if (enemy.Alive && !enemy.IsInvincibleAfterHit && wasAboveEnemy)
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
            if (InputManager.CurrentAction.right && !InputManager.CurrentAction.left)
            {
                this.velocity.X += acceleration * elapsed;
                if (velocity.X > maxWalkingSpeed)
                {
                    velocity.X = maxWalkingSpeed;
                }
                isRunning = true;
                isSliding = false;
                flipped = false;
            }

            // Walk left
            if (InputManager.CurrentAction.left && !InputManager.CurrentAction.right)
            {
                this.velocity.X -= acceleration * elapsed;
                if (velocity.X < -maxWalkingSpeed)
                {
                    velocity.X = -maxWalkingSpeed;
                }
                isRunning = true;
                isSliding = false;
                flipped = true;
            }


            if (!isRunning)
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

            if (OnGround)
            {
                PoisonPlatforms.Clear();
            }

            if (InputManager.CurrentAction.jump && !InputManager.PreviousAction.jump && InputManager.CurrentAction.down && OnPlatform)
            {
                // Jump down from platform(s). Find every platform below the player and mark them all as poison.
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
            }

            // slightly sliding is not sliding, so we want to see the idle animation.
            if (velocity.X < 20 && velocity.X > -20 && isSliding)
            {
                isRunning = false;
                isSliding = false;
            }

            // stop the player if they are nearly stopped so you don't get weird 1px movement.
            if (velocity.X < 6 && velocity.X > -6 && !isRunning)
            {
                velocity.X = 0;
            }

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
            // EffectsManager.RisingText("Dead", WorldCenter);
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
    }
}
