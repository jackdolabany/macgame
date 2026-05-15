using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MacGame.Items
{
    public class TopHatItem : HatItem
    {
        public override string HatName => "Top Hat";
        protected override Rectangle SourceRectangle => Helpers.GetTileRect(2, 1);

        public TopHatItem(ContentManager content, int cellX, int cellY, Player player) : base(content, cellX, cellY, player) { }
    }
}
