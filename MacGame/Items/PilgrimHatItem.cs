using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace MacGame.Items
{
    public class PilgrimHatItem : HatItem
    {

        protected override Rectangle SourceRectangle => Helpers.GetTileRect(0, 0);

        public PilgrimHatItem(ContentManager content, int cellX, int cellY, Player player) : base(content, cellX, cellY, player)
        {
          
        }

        protected override bool IsAlreadyCollected() => Game1.StorageState.HasPilgrimHat;

        public override void Collect(Player player)
        {
            Game1.StorageState.HasPilgrimHat = true;
            base.Collect(player);
        }
    }
}
