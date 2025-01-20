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

        public bool AlreadyCollected { get; set; } = false;
        
        const float fadeInTimerGoal = 3f;
        float fadeInTimer = fadeInTimerGoal;

        private Color Color = Color.White;

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

            SetCenteredCollisionRectangle(14, 14);

            IsInChest = false;

            WorldLocation = new Vector2((cellX + 1) * TileMap.TileSize, (cellY + 1) * TileMap.TileSize);
        }

        public void CheckIfAlreadyCollected(int levelNumber)
        {
            if (Game1.StorageState.Levels[levelNumber].CollectedSocks.Contains(Name))
            {
                AlreadyCollected = true;
                Color = Color.White * 0.5f;
            }
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

            if (AlreadyCollected)
            {
                // They're enabled but not collectible if they are already collected. They're kind of of in a ghos tmode.
                Enabled = true;
            }

            if (fadeInTimer < fadeInTimerGoal)
            {
                fadeInTimer += elapsed;
                this.DisplayComponent.TintColor = Color * (fadeInTimer / fadeInTimerGoal);
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
            SoundManager.PlaySound("SockReveal");
        }
    }
}
