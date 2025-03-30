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
        public string Name { get; set; }

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

        public Sock(ContentManager content, int cellX, int cellY, Player player, Camera camera) : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\BigTextures");

            var animations = new AnimationDisplay();
            this.DisplayComponent = animations;

            var spin = new AnimationStrip(textures, Helpers.GetBigTileRect(3, 6), 8, "spin");
            spin.LoopAnimation = true;
            spin.Oscillate = false;
            spin.FrameLength = 0.33f;
            animations.Add(spin);

            animations.Play("spin");

            SetWorldLocationCollisionRectangle(14, 12);
            // Move it up slightly
            this.collisionRectangle.Y -= 8;

            IsInChest = false;

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

        public override void WhenCollected(Player player)
        {
            if (!Enabled || AlreadyCollected) return;

            this.Enabled = false;

            // Add this sock if they don't already have it.
            var levelState = Game1.StorageState.Levels[Game1.CurrentLevel.LevelNumber];
            if (!levelState.CollectedSocks.Contains(Name))
            {
                levelState.CollectedSocks.Add(Name);
                player.SockCount++;
            }

            player.Health = Player.MaxHealth;

            // take the player back to the main room. Reset tacos, health, etc. Save the game.
            GlobalEvents.FireSockCollected(this, EventArgs.Empty);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (blockFromCollectingTime > 0)
            {
                blockFromCollectingTime -= elapsed;
            }

            if (AlreadyCollected)
            {
                // They're enabled but not collectible if they are already collected. They're kind of of in a ghost mode.
                Enabled = true;
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
