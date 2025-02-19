using MacGame.Behaviors;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Npcs
{
    public class Daisy : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public Daisy(ContentManager content, int cellX, int cellY, Player player, Camera camera) 
            : base(content, cellX, cellY, player, camera)
        {

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");
            var idle = new AnimationStrip(textures, Helpers.GetTileRect(0, 11), 1, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.2f;
            animations.Add(idle);

            var walk = new AnimationStrip(textures, Helpers.GetTileRect(0, 11), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.2f;
            animations.Add(walk);

            SetCenteredCollisionRectangle(8, 8);
           
            Behavior = new WalkRandomlyBehavior("idle", "walk");
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(7, 1);

        public override void InitiateConversation()
        {
            ConversationManager.AddMessage("Hissss.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
        }
    }
}
