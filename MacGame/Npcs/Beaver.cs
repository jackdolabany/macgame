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
    public class Beaver : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public Beaver(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\BigTextures");
            var idle = new AnimationStrip(textures, Helpers.GetBigTileRect(0, 11), 4, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.15f;
            animations.Add(idle);

            SetCenteredCollisionRectangle(8, 8);

            Behavior = new JustIdle("idle");
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(3, 2);

        /// <summary>
        /// The dog gives you hints to where the next sock is.
        /// </summary>
        public override void InitiateConversation()
        {
            ConversationManager.AddMessage("Not so fast water!", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
        }
    }
}
