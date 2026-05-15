using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Items
{
    public class YellowHatItem : HatItem
    {
        public override string HatName => "Yellow Hat";
        protected override Rectangle SourceRectangle => Helpers.GetTileRect(6, 1);

        public YellowHatItem(ContentManager content, int cellX, int cellY, Player player) : base(content, cellX, cellY, player) { }
    }
}
