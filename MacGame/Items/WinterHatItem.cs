using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Items
{
    public class WinterHatItem : HatItem
    {
        public override string HatName => "Winter Hat";
        protected override Rectangle SourceRectangle => Helpers.GetTileRect(4, 0);

        public WinterHatItem(ContentManager content, int cellX, int cellY, Player player) : base(content, cellX, cellY, player) { }
    }
}
