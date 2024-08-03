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

            var levelNumber = Game1.CurrentLevel.LevelNumber;

            var coinInfos = CoinIndex.LevelNumberToCoins[levelNumber];

            var collectedCoins = Game1.State.Levels[levelNumber].CollectedCoins;

            // He'll say the hint for first coin with a hint that you don't have.
            foreach (var coinInfo in coinInfos)
            {
                var hintText = coinInfo.Hint;
                
                if (collectedCoins == null || !collectedCoins.Contains(coinInfo.Name))
                {
                    ConversationManager.AddMessage(hintText, ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                    return;
                }
            }

            ConversationManager.AddMessage("Nice work collecting coins. Don't skip chest day.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
        }
    }
}
