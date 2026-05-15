using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Items
{
    public class HairHatItem : HatItem
    {
        public override string HatName => "Hair Hat";
        protected override Rectangle SourceRectangle => Helpers.GetTileRect(0, 1);

        public HairHatItem(ContentManager content, int cellX, int cellY, Player player) : base(content, cellX, cellY, player) { }
    }
}
