using MacGame.Behaviors;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Npcs
{
    public class Spaceman : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public Spaceman(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\BigTextures");
            var idle = new AnimationStrip(textures, Helpers.GetBigTileRect(3, 3), 1, "idle");
            idle.LoopAnimation = false;
            idle.FrameLength = 0.2f;
            animations.Add(idle);

            var disappear = new AnimationStrip(textures, Helpers.GetBigTileRect(3, 3), 4, "disappear");
            disappear.LoopAnimation = false;
            disappear.FrameLength = 0.2f;
            animations.Add(disappear);

            SetCenteredCollisionRectangle(10, 16);

            Enabled = true;
            animations.Play("idle");
        }

        // TODO: something? do they talk?
        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(4, 0);

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (animations.CurrentAnimationName == "disappear" && animations.CurrentAnimation!.FinishedPlaying)
            {
                Enabled = false;
            }

            base.Update(gameTime, elapsed);
        }

        public override void InitiateConversation()
        {
            if (animations.CurrentAnimationName != "disappear")
            {
                animations.Play("disappear");
                SoundManager.PlaySound("AlienDisappear");
            }
        }
    }
}
