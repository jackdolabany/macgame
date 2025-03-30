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
    public class Snake : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public Snake(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");
            var idle = new AnimationStrip(textures, Helpers.GetTileRect(2, 11), 2, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.8f;
            animations.Add(idle);

            SetWorldLocationCollisionRectangle(8, 8);

            Behavior = new JustIdle("idle");
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(6, 1);

        /// <summary>
        /// The dog gives you hints to where the next sock is.
        /// </summary>
        public override void InitiateConversation()
        {
            ConversationManager.AddMessage("Whasssssup?", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
        }
    }
}
