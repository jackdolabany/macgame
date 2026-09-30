using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Items
{
    public class PilgrimHatItem : HatItem
    {
        public override string HatName => "Pilgrim";
        protected override Rectangle SourceRectangle => Helpers.GetBigTileRect(0, 0);

        public PilgrimHatItem(ContentManager content, int cellX, int cellY, Player player) : base(content, cellX, cellY, player)
        {
        }
    }
}
