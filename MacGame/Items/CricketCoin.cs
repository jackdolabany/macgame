using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MacGame.Items
{
    public class CricketCoin : Item
    {

        public string Name { get; set; }

        public bool AlreadyCollected { get; set; } = false;

        public bool IsTacoCoin { get; set; } = false;

        public CricketCoin(ContentManager content, int cellX, int cellY, Player player, Camera camera) : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\BigTextures");

            var animations = new AnimationDisplay();
            this.DisplayComponent = animations;

            var spin = new AnimationStrip(textures, Helpers.GetBigTileRect(0, 1), 3, "spin");
            spin.LoopAnimation = true;
            spin.Oscillate = true;
            spin.FrameLength = 0.25f;
            animations.Add(spin);

            animations.Play("spin");

            SetCenteredCollisionRectangle(14, 14);

            IsInChest = false;
        }

        public void CheckIfAlreadyCollected(int levelNumber)
        {
            if (Game1.State.LevelsToCoins.ContainsKey(levelNumber))
            {
                var coins = Game1.State.LevelsToCoins[levelNumber];
                if (coins.Contains(Name))
                {
                    AlreadyCollected = true;
                    this.DisplayComponent.TintColor = Color.White * 0.5f;
                }
            }
        }

        public override void WhenCollected(Player player)
        {
            if (!Enabled || AlreadyCollected) return;

            // Add this cricket coin if they don't already have it.
            if (Game1.State.LevelsToCoins.ContainsKey(Game1.CurrentLevel.LevelNumber))
            {
                var coins = Game1.State.LevelsToCoins[Game1.CurrentLevel.LevelNumber];
                if (!coins.Contains(Name))
                {
                    coins.Add(Name);
                    player.CricketCoinCount++;
                }
            }
            else
            {
                Game1.State.LevelsToCoins.Add(Game1.CurrentLevel.LevelNumber, new HashSet<string> { Name });
                player.CricketCoinCount++;
            }

            // take the player back to the main room. Reset tacos, health, etc. Save the game.
            GlobalEvents.FireCricketCoinCollected(this, EventArgs.Empty);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (AlreadyCollected)
            {
                // They're enabled but not collectible if they are already collected. They're kind of of in a ghos tmode.
                Enabled = true;
            }

            base.Update(gameTime, elapsed);
        }
    }
}
