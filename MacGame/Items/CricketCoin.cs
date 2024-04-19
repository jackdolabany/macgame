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

        public int Number { get; set; }
        public string Hint { get; set; } = "";
        public bool AlreadyCollected { get; set; } = false;

        public bool IsTacoCoin { get; set; } = false;

        public CricketCoin(ContentManager content, int cellX, int cellY, Player player, Camera camera) : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");

            var animations = new AnimationDisplay();
            this.DisplayComponent = animations;

            var spin = new AnimationStrip(textures, new Rectangle(10 * Game1.TileSize, 2 * Game1.TileSize, 16, 16), 3, "spin");
            spin.LoopAnimation = true;
            spin.Oscillate = true;
            spin.FrameLength = 0.25f;
            animations.Add(spin);

            animations.Play("spin");

            SetCenteredCollisionRectangle(14, 14);

            IsInChest = false;
        }
        public void InitializeAlreadyCollected(Level level)
        {
            if (Game1.State.LevelsToCoins.ContainsKey(level.LevelNumber))
            {
                var coins = Game1.State.LevelsToCoins[level.LevelNumber];
                if (coins.Contains(Number))
                {
                    AlreadyCollected = true;
                }
            }

            if (AlreadyCollected)
            {
                this.DisplayComponent.TintColor = Color.White * 0.5f;
            }
        }

        public override void WhenCollected(Player player)
        {
            // Set max tacos for this level.
            if (Game1.State.MaxTacosPerLevel.ContainsKey(Game1.CurrentLevel.LevelNumber))
            {
                Game1.State.MaxTacosPerLevel[Game1.CurrentLevel.LevelNumber] = Math.Max(Game1.State.MaxTacosPerLevel[Game1.CurrentLevel.LevelNumber], player.Tacos);
            }
            else
            {
                Game1.State.MaxTacosPerLevel.Add(Game1.CurrentLevel.LevelNumber, player.Tacos);
            }

            // Add this cricket coin if they don't already have it.
            if (Game1.State.LevelsToCoins.ContainsKey(Game1.CurrentLevel.LevelNumber))
            {
                var coins = Game1.State.LevelsToCoins[Game1.CurrentLevel.LevelNumber];
                if (!coins.Contains(Number))
                {
                    coins.Add(Number);
                    player.CricketCoinCount++;
                }
            }
            else
            {
                Game1.State.LevelsToCoins.Add(Game1.CurrentLevel.LevelNumber, new HashSet<int> { Number });
                player.CricketCoinCount++;
            }

            // take the player back to the main room. Reset tacos, health, etc. Save the game.
            GlobalEvents.FireCricketCoinCollected(this, EventArgs.Empty);

            // TODO: Play sound
            //SoundManager.PlaySound("CricketCoinCollected");
        }
    }
}
