using MacGame.Behaviors;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Npcs
{
    public class Fox : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public Fox(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");

            var idle = new AnimationStrip(textures, Helpers.GetTileRect(4, 12), 1, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.2f;
            animations.Add(idle);

            var walk = new AnimationStrip(textures, Helpers.GetTileRect(4, 12), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.2f;
            animations.Add(walk);

            SetWorldLocationCollisionRectangle(8, 8);

            Behavior = new WalkRandomlyBehavior("idle", "walk");
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(6, 4);

        public override void InitiateConversation()
        {
            ConversationManager.AddMessage("Yip yip!", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
        }
    }
}
