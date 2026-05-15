using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Items
{
    public class NinjaHatItem : HatItem
    {
        protected override Rectangle SourceRectangle => Helpers.GetTileRect(2, 0);

        public NinjaHatItem(ContentManager content, int cellX, int cellY, Player player) : base(content, cellX, cellY, player)
        {

        }

        protected override bool IsAlreadyCollected() => Game1.StorageState.HasNinjaHat;

        public override void Collect(Player player)
        {
            Game1.StorageState.HasNinjaHat = true;
            base.Collect(player);
        }
    }
}
