using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Items
{
    public class BaseballHatItem : HatItem
    {
        public override string HatName => "Baseball Hat";
        protected override Rectangle SourceRectangle => Helpers.GetTileRect(4, 1);

        public BaseballHatItem(ContentManager content, int cellX, int cellY, Player player) : base(content, cellX, cellY, player) { }
    }
}
