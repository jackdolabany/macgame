using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Platforms
{
    public class BreakingPlatform : Platform
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        float reEnableTimer = 0;

        public BreakingPlatform(ContentManager content, int cellX, int cellY)
           : base(content, cellX, cellY)
        {
            this.DisplayComponent = new StaticImageDisplay(content.Load<Texture2D>(@"Textures/Textures"), Helpers.GetTileRect(10, 13));
            DisplayComponent = new AnimationDisplay();
            var textures = content.Load<Texture2D>(@"Textures\Textures");

            var idle = new AnimationStrip(textures, Helpers.GetTileRect(10, 13), 1, "idle");
            idle.LoopAnimation = false;
            idle.FrameLength = 0.14f;
            animations.Add(idle);

            animations.Play("idle");


            var breakDown = new AnimationStrip(textures, Helpers.GetTileRect(10, 13), 3, "break");
            breakDown.LoopAnimation = false;
            breakDown.FrameLength = 0.1f;
            animations.Add(breakDown);


            var reform = (AnimationStrip)breakDown.Clone();
            reform.Reverse = true;
            reform.Name = "reform";
            animations.Add(reform);

            SetWorldLocationCollisionRectangle(8, 8);

        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (animations.CurrentAnimationName == "idle" && Game1.Player.PlatformThatThisIsOn == this)
            {
                animations.Play("break");
                SoundManager.PlaySound("PlatformBreak");
            }

            if (Enabled && animations.CurrentAnimationName == "break" && animations.CurrentAnimation!.FinishedPlaying)
            {
                Enabled = false;
                reEnableTimer = 0f;
            }

            if (!Enabled)
            {
                reEnableTimer += elapsed;
                if (reEnableTimer > 1.1f)
                {
                    Enabled = true;
                    animations.Play("reform")!.FollowedBy("idle");
                    reEnableTimer = 0;
                }
            }

            base.Update(gameTime, elapsed);
        }
    }
}
