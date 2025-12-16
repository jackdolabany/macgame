using MacGame.Behaviors;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Npcs
{
    public class Penguin : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public Penguin(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");

            var idle = new AnimationStrip(textures, Helpers.GetTileRect(8, 10), 1, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.2f;
            animations.Add(idle);

            var walk = new AnimationStrip(textures, Helpers.GetTileRect(8, 10), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.2f;
            animations.Add(walk);


            SetWorldLocationCollisionRectangle(8, 8);

            Behavior = new WalkRandomlyBehavior("idle", "walk");
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(2, 4);

        public override void InitiateConversation()
        {
            ConversationManager.AddMessage("I'm lost.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
        }
    }
}
