using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Items
{
    public class RussianHatItem : HatItem
    {
        public override string HatName => "Russian Hat";
        protected override Rectangle SourceRectangle => Helpers.GetTileRect(0, 2);

        public RussianHatItem(ContentManager content, int cellX, int cellY, Player player) : base(content, cellX, cellY, player) { }
    }
}
