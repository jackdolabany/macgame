using MacGame.Behaviors;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using TileEngine;

namespace MacGame.Npcs
{
    public class Dog : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public Dog(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\BigTextures");
            var idle = new AnimationStrip(textures, Helpers.GetBigTileRect(4, 1), 2, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.5f;
            animations.Add(idle);

            SetCenteredCollisionRectangle(8, 8);

            Behavior = new JustIdle("idle");
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(6, 0);

        /// <summary>
        /// The dog gives you hints to where the next coin is.
        /// </summary>
        public override void InitiateConversation()
        {
            var hints = Game1.CoinHints;

            HashSet<int>? collectedCoins = null;

            if (Game1.State.LevelsToCoins.ContainsKey(Game1.CurrentLevel.LevelNumber))
            {
                collectedCoins = Game1.State.LevelsToCoins[Game1.CurrentLevel.LevelNumber];
            }

            // He'll say the hint for first coin with a hint that you don't have.
            foreach (var hint in hints.OrderBy(h => h.Key))
            {
                var coinNumber = hint.Key;
                var hintText = hint.Value;
                
                if (collectedCoins == null || !collectedCoins.Contains(coinNumber))
                {
                    ConversationManager.AddMessage(hintText, ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                    return;
                }
            }

            ConversationManager.AddMessage("Nice work collecting coins. Don't skip chest day.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
        }
    }
}
