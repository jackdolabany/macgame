using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Items
{
    public class NinjaHatItem : HatItem
    {
        public override string HatName => "Ninja";
        protected override Rectangle SourceRectangle => Helpers.GetTileRect(2, 0);

        public NinjaHatItem(ContentManager content, int cellX, int cellY, Player player) : base(content, cellX, cellY, player)
        {
        }
    }
}
