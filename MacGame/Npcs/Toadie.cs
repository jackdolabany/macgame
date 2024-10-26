﻿using MacGame.Behaviors;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using TileEngine;

namespace MacGame.Npcs
{
    public class Toadie : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public Toadie(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var idle = new AnimationStrip(textures, Helpers.GetTileRect(8, 13), 1, "idle");
            idle.LoopAnimation = false;
            idle.FrameLength = 0.5f;
            animations.Add(idle);

            SetCenteredCollisionRectangle(8, 8);

            Behavior = new JustIdle("idle");
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(0, 1);

        /// <summary>
        /// The dog gives you hints to where the next sock is.
        /// </summary>
        public override void InitiateConversation()
        {
            ConversationManager.AddMessage("Toadie", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
        }
    }
}