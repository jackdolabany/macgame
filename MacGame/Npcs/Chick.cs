using MacGame.Behaviors;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Npcs
{
    public class Chick : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public Chick(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");

            var idle = new AnimationStrip(textures, Helpers.GetTileRect(4, 10), 1, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.2f;
            animations.Add(idle);

            var walk = new AnimationStrip(textures, Helpers.GetTileRect(4, 10), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.2f;
            animations.Add(walk);

            SetWorldLocationCollisionRectangle(8, 8);

            Behavior = new WalkRandomlyBehavior("idle", "walk");
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(0, 4);

        public override void InitiateConversation()
        {
            ConversationManager.AddMessage("Chiky!", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
        }
    }
}