using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;
using MacGame.GameObjects;
using System.Collections.Generic;

namespace MacGame.Enemies
{
    /// <summary>
    /// A box that contains a ghost. When triggered by a button action, the ghost is released.
    /// The ghost flies right, bounces off walls, slowly moves down, and can trigger buttons/switches.
    /// </summary>
    public class GhostBox : Enemy
    {
        private enum GhostState
        {
            InBox,
            Flying,
            Bouncing
        }

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;
        StaticImageDisplay boxImage;

        private GhostState state = GhostState.InBox;
        private float ghostSpeed = 120f;
        private float downwardSpeed = 20f;
        private float bounceSpeed = 300f;
        private float bounceDuration = 0.3f;
        private float bounceTimer = 0f;
        private Vector2 originalWorldLocation;
        
        /// <summary>
        /// If the ghost triggers a button you don't want it triggered every frame.
        /// </summary>
        private float buttonTriggerCooldown = 0f;

        private const float ButtonTriggerCooldownMax = 1f;
        private float boxDrawDepth = 0f; // Draw depth for the box (stays at original location)

        public GhostBox(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            originalWorldLocation = WorldLocation;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            
            // Box state display
            boxImage = new StaticImageDisplay(textures, Helpers.GetTileRect(6, 20));
            
            // Ghost state animation
            var fly = new AnimationStrip(textures, Helpers.GetTileRect(7, 21), 2, "fly");
            fly.LoopAnimation = true;
            fly.FrameLength = 0.14f;
            animations.Add(fly);

            isEnemyTileColliding = true;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;
            CanBeJumpedOn = false; // Can't jump on this enemy
            CanBeHitWithWeapons = false; // Can't be hit with weapons, only DestroyPickupObjectField

            SetWorldLocationCollisionRectangle(8, 8);

            // Start inactive (as a box) - but still enabled so it's in the enemies list
            Enabled = true;
            state = GhostState.InBox;
            // Don't collide as a box
            isEnemyTileColliding = false;
        }

        public override void SetDrawDepth(float depth)
        {
            // The box draws at the front (original depth)
            boxDrawDepth = depth;

            // The ghost (this enemy) draws behind the box
            base.SetDrawDepth(depth + Game1.MIN_DRAW_INCREMENT);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Always draw the box at its original location (even when ghost is active)
            // Calculate the box's draw rectangle at its original position
            var boxDrawRect = new Rectangle(
                (int)originalWorldLocation.X - CollisionRectangle.Width / 2,
                (int)originalWorldLocation.Y - CollisionRectangle.Height,
                CollisionRectangle.Width,
                CollisionRectangle.Height);
            
            if (camera.IsObjectVisible(boxDrawRect))
            {
                boxImage.DrawDepth = boxDrawDepth;
                boxImage.Draw(spriteBatch, originalWorldLocation, false);
            }

            // Draw the ghost if it's active (not in box state)
            if (state != GhostState.InBox && Enabled && Alive)
            {
                base.Draw(spriteBatch);
            }
        }

        /// <summary>
        /// Releases the ghost from the box. Can be called multiple times to re-release after death.
        /// </summary>
        public void Release()
        {
            if (state == GhostState.InBox || Dead)
            {
                state = GhostState.Flying;
                Enabled = true;
                Alive = true;
                Dead = false;
                Health = 1;
                isEnemyTileColliding = true;
                WorldLocation = originalWorldLocation;
                Velocity = new Vector2(ghostSpeed, downwardSpeed); // Start flying right
                DisplayComponent = animations;
                animations.Play("fly");
                Flipped = false; // Face right
            }
        }

        public override void Kill()
        {
            // When killed, go back to box state
            EffectsManager.SmallEnemyPop(WorldCenter);
            state = GhostState.InBox;
            Enabled = true; // Keep enabled so it can be re-released
            isEnemyTileColliding = false;
            Velocity = Vector2.Zero;
            WorldLocation = originalWorldLocation;
            bounceTimer = 0f;
            base.Kill();
            
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);
            
            // Don't do anything in box state
            if (state == GhostState.InBox)
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

            // Handle state-specific behavior
            // Set velocity for NEXT frame (base.Update() already ran and applied previous velocity)
            switch (state)
            {
                case GhostState.Bouncing:
                    // Update bounce timer
                    bounceTimer -= elapsed;
                    if (bounceTimer <= 0)
                    {
                        // Transition back to flying state
                        state = GhostState.Flying;
                        Velocity = new Vector2(Flipped ? -ghostSpeed : ghostSpeed, downwardSpeed);
                    }
                    else
                    {
                        // Continue moving upward during bounce
                        Velocity = new Vector2(Flipped ? -ghostSpeed : ghostSpeed, -bounceSpeed);
                    }
                    break;

                case GhostState.Flying:
                    // Normal flying behavior - move horizontally and slowly downward
                    Velocity = new Vector2(Flipped ? -ghostSpeed : ghostSpeed, downwardSpeed);

                    // Bounce off walls (check after base.Update so OnLeftWall/OnRightWall are set)
                    if (OnLeftWall)
                    {
                        Velocity = new Vector2(ghostSpeed, downwardSpeed);
                        Flipped = false;
                    }
                    else if (onRightWall)
                    {
                        Velocity = new Vector2(-ghostSpeed, downwardSpeed);
                        Flipped = true;
                    }

                    // Check for pick up object collisions from below (bounce up when hit by rock)
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
                                    // Enter bouncing state
                                    state = GhostState.Bouncing;
                                    bounceTimer = bounceDuration;
                                    Velocity = new Vector2(Flipped ? -ghostSpeed : ghostSpeed, -bounceSpeed);
                                    SoundManager.PlaySound("Bounce");
                                    break;
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
    }
}

