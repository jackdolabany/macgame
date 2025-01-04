using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
    public class Explosion : GameObject
    {

        private AnimationDisplay AnimationDisplay => (AnimationDisplay)DisplayComponent;
        public Explosion(ContentManager content) : base()
        {
            var explosion = new AnimationStrip(content.Load<Texture2D>(@"Textures\BigTextures"), Helpers.GetBigTileRect(0, 8), 8, "explosion");
            explosion.LoopAnimation = false;
            explosion.FrameLength = 0.09f;

            var ad = new AnimationDisplay();
            this.DisplayComponent = ad;
            ad.Add(explosion);
            Enabled = false;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);
            if (AnimationDisplay.CurrentAnimation.FinishedPlaying)
            {
                Enabled = false;
            }
        }

        public void Explode(Vector2 location)
        {
            Enabled = true;
            AnimationDisplay.Play("explosion");
            WorldLocation = location + new Vector2(0, this.AnimationDisplay.CurrentAnimation.FrameRectangle.Height / 2);
            this.Rotation = Game1.Randy.GetRandomFourWayRotation();
            SoundManager.PlaySound("Explosion");
        }
    }
}
