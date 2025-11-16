using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MacGame
{
    public class TNT : PickupObject
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        const float ExplosionTime = 3f;
        private float timeRemaining = 0.0f;
        private bool isArmed = false;
        const float originalAnimationFrameLength = 0.5f;

        public TNT(ContentManager content, int x, int y, Player player) : base(content, x, y, player)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");
            
            DisplayComponent = new AnimationDisplay();

            var idle = new AnimationStrip(textures, Helpers.GetTileRect(14, 12), 1, "idle");
            idle.LoopAnimation = true;
            animations.Add(idle);

            var flash = new AnimationStrip(textures, Helpers.GetTileRect(14, 12), 2, "flash");
            flash.LoopAnimation = true;
            flash.FrameLength = originalAnimationFrameLength; // Start with slower flash
            animations.Add(flash);
            
            animations.Play("idle");
            
            Enabled = true;
            IsAffectedByGravity = true;
            this.SetWorldLocationCollisionRectangle(8, 8);
        }

        public override void Pickup()
        {
            base.Pickup();
            
            // Start the countdown when picked up
            if (!isArmed)
            {
                isArmed = true;
                timeRemaining = ExplosionTime;

                animations.Play("flash");
            }
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (isArmed && timeRemaining > 0)
            {
                timeRemaining -= elapsed;
                
                // Speed up the flash animation as time runs out
                var percentTimeRemaining = timeRemaining / ExplosionTime;
                // Flash faster and faster - from 0.5f down to 0.05f
                if (animations.CurrentAnimation != null)
                {
                    animations.CurrentAnimation.FrameLength = MathHelper.Lerp(0.05f, originalAnimationFrameLength, percentTimeRemaining);
                }
                
                if (timeRemaining <= 0)
                {
                    Explode();
                }
            }
            
            base.Update(gameTime, elapsed);
        }

        private void Explode()
        {
            var explosionRectangle = new Rectangle((int)WorldCenter.X - 48, (int)WorldCenter.Y - 48, 96, 96);
            
            // Play explosion effect
            EffectsManager.AddExplosion(this.WorldCenter);
            
            // Harm the player if they're in the explosion radius
            if (_player.CollisionRectangle.Intersects(explosionRectangle))
            {
                _player.TakeHit(1, Vector2.Zero);
            }
            
            // Kill enemies in the explosion radius
            foreach (var enemy in Game1.CurrentLevel.Enemies)
            {
                if (enemy.Alive && enemy.Enabled && enemy.CollisionRectangle.Intersects(explosionRectangle))
                {
                    enemy.TakeHit(this, 10, Vector2.Zero);
                }
            }
            
            // Flip buttons in the explosion radius
            foreach (var gameObject in Game1.CurrentLevel.GameObjects)
            {
                if (gameObject is Button button && button.CollisionRectangle.Intersects(explosionRectangle))
                {
                    button.Trigger();
                }
                else if (gameObject is GameObjects.CrystalSwitch crystalSwitch && crystalSwitch.CollisionRectangle.Intersects(explosionRectangle))
                {
                    // Flip red/blue switches
                    crystalSwitch.Trigger();
                }
                else if (gameObject is BreakBrick breakBrick && breakBrick.CollisionRectangle.Intersects(explosionRectangle))
                {
                    breakBrick.Break();
                }
            }
            
            // Reset the TNT
            BreakAndReset();
        }

        public override void BreakAndReset()
        {
            base.BreakAndReset();
            
            // Reset TNT state
            isArmed = false;
            timeRemaining = 0f;

            animations.Play("idle");

            // Reset animation speed and pause it
            if (animations.CurrentAnimation != null)
            {
                animations.CurrentAnimation.FrameLength = originalAnimationFrameLength;
            }
        }
    }
}

