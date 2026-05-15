using MacGame.DisplayComponents;
using MacGame.Enemies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
    public abstract class PlayerHat : GameObject
    {
        protected Player _player;
        protected StaticImageDisplay FrontDisplay { get; set; }
        protected StaticImageDisplay BackDisplay { get; set; }
        public abstract string HatName { get; }

        protected abstract Rectangle frontSource { get; }
        protected abstract Rectangle backSource { get; }

        protected PlayerHat(Player player, ContentManager content)
        {
            _player = player;
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
