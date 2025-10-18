using MacGame.Behaviors;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Npcs
{
    public class Chipmunk : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public Chipmunk(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");

            var idle = new AnimationStrip(textures, Helpers.GetTileRect(6, 11), 2, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.2f;
            animations.Add(idle);

            SetWorldLocationCollisionRectangle(8, 8);
            Behavior = new JustIdle("idle");
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(4, 4);

        public override void InitiateConversation()
        {
            ConversationManager.AddMessage("awwww yeah!", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
        }
    }
}
