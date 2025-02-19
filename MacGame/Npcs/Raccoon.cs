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
    public class Raccoon : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public Raccoon(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\BigTextures");
            var idle = new AnimationStrip(textures, Helpers.GetBigTileRect(0, 10), 6, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.8f;
            animations.Add(idle);

            SetCenteredCollisionRectangle(8, 8);

            Behavior = new JustIdle("idle");
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(2, 2);

        /// <summary>
        /// The dog gives you hints to where the next sock is.
        /// </summary>
        public override void InitiateConversation()
        {

            var levelNumber = Game1.CurrentLevel.LevelNumber;

            var sockInfos = SockIndex.LevelNumberToSocks[levelNumber];

            var collectedSocks = Game1.StorageState.Levels[levelNumber].CollectedSocks;

            // He'll say the hint for first sock with a hint that you don't have.
            foreach (var sockInfo in sockInfos)
            {
                var hintText = sockInfo.Hint;
                
                if (!collectedSocks.Contains(sockInfo.Name))
                {
                    ConversationManager.AddMessage(hintText, ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                    return;
                }
            }

            ConversationManager.AddMessage("Nice work collecting socks. Don't skip chest day.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
        }
    }
}
