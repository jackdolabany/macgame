using System;
using MacGame.DisplayComponents;
using MacGame.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Enemies
{
    /// <summary>
    /// A box that contains a ghost. When triggered by a button action, the ghost is released.
    /// The ghost flies right, bounces off walls, slowly moves down, and can trigger buttons/switches.
    /// </summary>
    public class GhostBoxGhost : Enemy
    {
        public enum GhostState
        {
            InBoxOrChest,
            Flying
        }

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private GhostState state = GhostState.InBoxOrChest;
        private float ghostSpeed = 120f;
        private float maxDownwardSpeed = 20f;
        private float bounceCooldownTimer = 0f;
        private Vector2 originalWorldLocation;
        
        /// <summary>
        /// If the ghost triggers a button you don't want it triggered every frame.
        /// </summary>
        private float buttonTriggerCooldown = 0f;

        private const float ButtonTriggerCooldownMax = 1f;
        private float boxDrawDepth = 0f; // Draw depth for the box (stays at original location)

        public GhostBoxGhost(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            originalWorldLocation = WorldLocation;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            
            // Ghost state animation
            var fly = new AnimationStrip(textures, Helpers.GetTileRect(7, 21), 2, "fly");
            fly.LoopAnimation = true;
            fly.FrameLength = 0.14f;
            animations.Add(fly);

            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;
            CanBeJumpedOn = false; // Can't jump on this enemy
            CanBeHitWithWeapons = false; // Can't be hit with weapons, only DestroyPickupObjectField

            SetWorldLocationCollisionRectangle(8, 8);

            // Start inactive (as a box) - but still enabled so it's in the enemies list
            Enabled = true;
            state = GhostState.InBoxOrChest;

        }

        /// <summary>
        /// Releases the ghost from the box. Can be called multiple times to re-release after death.
        /// </summary>
        public void Release(Vector2 releaseLocation)
        {
            if (state == GhostState.InBoxOrChest || Dead)
            {
                state = GhostState.Flying;
                Enabled = true;
                Alive = true;
                Dead = false;
                Health = 1;
                isEnemyTileColliding = true;
                WorldLocation = releaseLocation;
                Velocity = new Vector2(ghostSpeed, maxDownwardSpeed); // Start flying right
                DisplayComponent = animations;
                animations.Play("fly");
                Flipped = false; // Face right
            }
        }

        public override void Kill()
        {
            // When killed, go back to box state
            EffectsManager.SmallEnemyPop(WorldCenter);
            state = GhostState.InBoxOrChest;
            Enabled = true; // Keep enabled so it can be re-released
            isEnemyTileColliding = false;
            Velocity = Vector2.Zero;
            WorldLocation = originalWorldLocation;
            bounceCooldownTimer = 0f;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);
            
            // Don't do anything in box state
            if (state == GhostState.InBoxOrChest)
            {
                return;
            }

            if (buttonTriggerCooldown > 0)
            {
                buttonTriggerCooldown -= elapsed;
            }

            if (!Alive || !Enabled)
            {
                return;
            }

            if (bounceCooldownTimer > 0)
            {
                bounceCooldownTimer -= elapsed;
            }

            // Handle state-specific behavior
            // Set velocity for next frame (base.Update() already ran and applied previous velocity)
            switch (state)
            {

                case GhostState.Flying:
                    // Normal flying behavior - move horizontally and slowly downward
                    Velocity = new Vector2(Flipped ? -ghostSpeed : ghostSpeed, velocity.Y);

                    // Bounce off walls (check after base.Update so OnLeftWall/OnRightWall are set)
                    if (OnLeftWall)
                    {
                        Velocity = new Vector2(ghostSpeed, velocity.Y);
                        Flipped = false;
                    }
                    else if (onRightWall)
                    {
                        Velocity = new Vector2(-ghostSpeed, velocity.Y);
                        Flipped = true;
                    }

                    if (velocity.Y < maxDownwardSpeed)
                    {
                        Velocity = new Vector2(velocity.X, velocity.Y + (100 * elapsed));
                    }

                    // Check for pick up object collisions from below (bounce up when hit by rock)
                    if (bounceCooldownTimer <= 0)
                    {
                        foreach (var pickupObject in Game1.CurrentLevel.PickupObjects)
                        {
                            var go = (GameObject)pickupObject;
                            if (!pickupObject.IsPickedUp && go.Enabled)
                            {
                                if (pickupObject.CollisionRectangle.Intersects(this.CollisionRectangle))
                                {
                                    // Check if rock is moving upward and below the ghost
                                    if (go.Velocity.Y < 0 && go.WorldCenter.Y > this.WorldCenter.Y)
                                    {
                                        Velocity = new Vector2(velocity.X, velocity.Y - 100);
                                        SoundManager.PlaySound("Bounce");
                                        bounceCooldownTimer = 1f;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // Trigger buttons when colliding (with cooldown)
                    if (buttonTriggerCooldown <= 0)
                    {
                        foreach (var gameObject in Game1.CurrentLevel.GameObjects)
                        {
                            if (gameObject is Button button && button.CollisionRectangle.Intersects(this.CollisionRectangle))
                            {
                                // Trigger button down action
                                if (button.animations.CurrentAnimationName == "up")
                                {
                                    button.animations.Play("down");
                                    SoundManager.PlaySound("Click");
                                    
                                    foreach (var action in button.DownActions)
                                    {
                                        Game1.CurrentLevel.ExecuteButtonAction(button, action.ActionName, action.Args);
                                    }
                                    buttonTriggerCooldown = ButtonTriggerCooldownMax;
                                }
                            }
                        }
                    }
                    break;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw the ghost if it's active (not in box state)
            if (state != GhostState.InBoxOrChest && Enabled && Alive)
            {
                base.Draw(spriteBatch);
            }
        }

        public override void ReleasedFromChest(Chest chest)
        {
            base.ReleasedFromChest(chest);

            // Pop him up a little so he doesn't hit the player immediately
            Release(chest.WorldLocation + new Vector2(0, -8));
        }

    }
}
