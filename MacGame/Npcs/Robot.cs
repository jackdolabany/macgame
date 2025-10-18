using MacGame.Behaviors;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Npcs
{
    public class Robot : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public Robot(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");

            var idle = new AnimationStrip(textures, Helpers.GetTileRect(8, 12), 2, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.15f;
            animations.Add(idle);

            SetWorldLocationCollisionRectangle(8, 8);
            Behavior = new JustIdle("idle");
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(0, 5);

        public override void InitiateConversation()
        {
            ConversationManager.AddMessage("Danger! Danger!", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
        }
    }
}
