using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Items
{
    public abstract class HatItem : Item
    {

        protected abstract Rectangle SourceRectangle { get; }

        protected HatItem(ContentManager content, int cellX, int cellY, Player player) : base(content, cellX, cellY, player)
        {
            var hatsTexture = content.Load<Texture2D>(@"Textures\Hats");
            DisplayComponent = new StaticImageDisplay(hatsTexture, SourceRectangle);
            SetWorldLocationCollisionRectangle(8, 8);
        }

        protected override void Initialize()
        {
            if (IsAlreadyCollected())
            {
                this.Enabled = false;
            }
        }

        protected abstract bool IsAlreadyCollected();

        public override void Collect(Player player)
        {
            this.Enabled = false;
            StorageManager.TrySaveGame();
            base.Collect(player);
        }
    }
}
