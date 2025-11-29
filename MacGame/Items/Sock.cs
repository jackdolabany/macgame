using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TileEngine;

namespace MacGame.Items
{
    public class Sock : Item
    {
        /// <summary>
        /// Whether or not the sock was collected when the map was loaded. This is used to show a transparent sock.
        /// Not accurate if you just collected it on the same map. Use IsCollected for that.
        /// </summary>
        public bool AlreadyCollected { get; set; } = false;
        
        const float fadeInTimerGoal = 3f;
        float fadeInTimer = fadeInTimerGoal;

        private Color Color = Color.White;

        /// <summary>
        /// If this time is > 0 you can't collect the sock. This gives it a small period of time after fading in where you
        /// can't collect it so you don't insta-collect it if you're already on it and it's weird.
        /// </summary>
        float blockFromCollectingTime = 0f;

        private bool _isInitialized = false;

        private AnimationDisplay _animationDisplay => (AnimationDisplay)DisplayComponent;

        public Sock(ContentManager content, int cellX, int cellY, Player player) : base(content, cellX, cellY, player)
        {
            var textures = content.Load<Texture2D>(@"Textures\BigTextures");

            var animations = new AnimationDisplay();
            this.DisplayComponent = animations;

            var spin = new AnimationStrip(textures, Helpers.GetBigTileRect(3, 6), 8, "spin");
            spin.LoopAnimation = true;
            spin.Oscillate = false;
            spin.FrameLength = 0.33f;
            animations.Add(spin);

            var idle = new AnimationStrip(textures, Helpers.GetBigTileRect(3, 6), 1, "idle");
            idle.LoopAnimation = false;
            animations.Add(idle);

            animations.Play("spin");

            SetWorldLocationCollisionRectangle(14, 12);
            // Move it up slightly
            this.collisionRectangle.Y -= 8;

            WorldLocation = new Vector2((cellX + 1) * TileMap.TileSize, (cellY + 1) * TileMap.TileSize);
        }

        /// <summary>
        /// Called after the level is set up.
        /// </summary>
        public void CheckIfAlreadyCollected(int levelNumber)
        {
            if (Game1.StorageState.Levels[levelNumber].CollectedSocks.Contains(Name))
            {
                AlreadyCollected = true;
                Color = Color.White * 0.5f;
            }
        }

        /// <summary>
        /// True if you already collected this sock on the current map.
        /// </summary>
        public bool IsCollected
        {
            get
            {
                return Game1.StorageState.Levels[Game1.CurrentLevel.LevelNumber].CollectedSocks.Contains(Name);
            }
        }

        public override void PlayCollectedSound()
        {
            // Do nothing.
        }

        /// <summary>
        /// After the music plays and the "you got a sock!" alert menu pops up, call this to make the sock go away.
        /// </summary>
        public void HideSock()
        {
            this.Enabled = false;
            EffectsManager.EnemyPop(this.CollisionCenter, 10, Color.White, 200);
        }

        // Call this to stop the sock from spinning
        public void StopSpinning()
        {
            _animationDisplay.Play("idle");
        }

        public override void WhenCollected(Player player)
        {
            if (!Enabled || AlreadyCollected) return;

            AlreadyCollected = true;

            // Add this sock if they don't already have it.
            var levelState = Game1.StorageState.Levels[Game1.CurrentLevel.LevelNumber];
            if (!levelState.CollectedSocks.Contains(Name))
            {
                levelState.CollectedSocks.Add(Name);
                player.SockCount++;
            }

            player.Health = Player.MaxHealth;

            // Show the sock up front once it's collected
            this.DisplayComponent.DrawDepth = TileMap.FRONTMOST_DRAW_DEPTH;

            // Speed it up
            _animationDisplay.CurrentAnimation!.FrameLength /= 4;

            // take the player back to the main room. Reset tacos, health, etc. Save the game.
            GlobalEvents.FireSockCollected(this, EventArgs.Empty);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!_isInitialized)
            {
                _isInitialized = true;

                if (AlreadyCollected)
                {
                    // They're enabled but not collectible if they are already collected. They're kind of of in a ghost mode.
                    Enabled = true;
                }
            }

            if (blockFromCollectingTime > 0)
            {
                blockFromCollectingTime -= elapsed;
            }

            if (fadeInTimer < fadeInTimerGoal)
            {
                fadeInTimer += elapsed;
                // showly fade in
                this.DisplayComponent.TintColor = Color.Lerp(Color.Transparent, Color, fadeInTimer / fadeInTimerGoal);
            }
            else
            {
                this.DisplayComponent.TintColor = Color;
            }

            base.Update(gameTime, elapsed);
        }

        public void FadeIn()
        {
            Enabled = true;
            fadeInTimer = 0f;
            this.DisplayComponent.TintColor = Color.Transparent;
            SoundManager.PlaySound("SockReveal");
            blockFromCollectingTime = fadeInTimerGoal + 0.5f;
        }

        protected override void Collect(Player player)
        {
            if (blockFromCollectingTime <= 0)
            {
                base.Collect(player);
            }
        }
    }
}
