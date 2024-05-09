using MacGame.Behaviors;
using MacGame.DisplayComponents;
using MacGame.Enemies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TileEngine;

namespace MacGame.Npcs
{
    public class HyperBluey : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public HyperBluey(ContentManager content, int cellX, int cellY, Player player, Camera camera) 
            : base(content, cellX, cellY, player, camera)
        {

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var idle = new AnimationStrip(textures, Helpers.GetTileRect(6, 13), 1, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.2f;
            animations.Add(idle);

            var walk = new AnimationStrip(textures, Helpers.GetTileRect(6, 13), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.2f;
            animations.Add(walk);

            SetCenteredCollisionRectangle(8, 8);
           
            Behavior = new WalkRandomlyBehavior("idle", "walk");
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(5, 0);

        public override void InitiateConversation()
        {
            var rando = Game1.Randy.Next(0, 2);

            if (rando == 0)
            {
                ConversationManager.AddMessage("Wow you look like a bearded dragon like me!", PlayerConversationRectangle, ConversationManager.ImagePosition.Left);
                ConversationManager.AddMessage("Are you blind? I'm a Crested Gecko.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
            }
            else
            {
                ConversationManager.AddMessage("Hi I'm Mac", PlayerConversationRectangle, ConversationManager.ImagePosition.Left);
                ConversationManager.AddMessage("My name is Hyper Bluey", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                ConversationManager.AddMessage("If you don't like it you can go chew on walnuts buddy", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                ConversationManager.AddMessage("I love it", PlayerConversationRectangle, ConversationManager.ImagePosition.Left);
            }
        }
    }
}
