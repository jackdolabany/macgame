using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
    /// <summary>
    /// Anything Mac wears on his head. Drawn from the 16x16 tiles in the Hats texture with a front and back view.
    /// </summary>
    public abstract class Headwear : GameObject
    {
        protected StaticImageDisplay FrontDisplay { get; set; }
        protected StaticImageDisplay BackDisplay { get; set; }

        protected abstract Rectangle frontSource { get; }
        protected abstract Rectangle backSource { get; }

        protected Headwear(ContentManager content)
        {
            var hatTexture = content.Load<Texture2D>(@"Textures\Hats");
            FrontDisplay = new StaticImageDisplay(hatTexture, frontSource);
            BackDisplay = new StaticImageDisplay(hatTexture, backSource);
            DisplayComponent = FrontDisplay;
            Enabled = true;
        }

        public void Front()
        {
            DisplayComponent = FrontDisplay;
        }

        public void Back()
        {
            DisplayComponent = BackDisplay;
        }

        public override void SetDrawDepth(float depth)
        {
            FrontDisplay.DrawDepth = depth;
            BackDisplay.DrawDepth = depth;
        }
    }
}
