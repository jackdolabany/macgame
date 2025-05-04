using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
    /// <summary>
    /// Smoke puff from a crashed car.
    /// </summary>
    public abstract class BaseSmoke : GameObject
    {

        private AnimationDisplay AnimationDisplay => (AnimationDisplay)DisplayComponent;

        private float noDrawTimer = 0f;
        protected float NoDrawTimerGoal = 0.5f;
        bool drawEnabled = true;


        public BaseSmoke(ContentManager content)
        {
            // We'll draw the wings all custom like.
            var ad = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");
            var smoke = new AnimationStrip(textures, GetTileRect(), 2, "smoke");
            smoke.LoopAnimation = false;
            smoke.FrameLength = 0.1f;

            ad.Add(smoke);

            this.DisplayComponent = ad;

            ad.Play("smoke");

            Enabled = true;
        }

        protected abstract Rectangle GetTileRect();

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);

            if (drawEnabled && AnimationDisplay.CurrentAnimation!.FinishedPlaying)
            {
                noDrawTimer = 0f;
                drawEnabled = false;
            }
            else
            {
                noDrawTimer += elapsed;
                if (noDrawTimer > NoDrawTimerGoal)
                {
                    noDrawTimer = 0f;
                    AnimationDisplay.Play("smoke");
                    drawEnabled = true;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (drawEnabled)
            {
                base.Draw(spriteBatch);
            }
        }

    }

}
